using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;
using HolyConnect.Infrastructure.Services.ImportStrategies;
using System.Text.Json;

namespace HolyConnect.Infrastructure.Services;

/// <summary>
/// Service for importing requests from various formats using the Strategy pattern
/// </summary>
public class ImportService : IImportService
{
    private readonly IRequestService _requestService;
    private readonly ICollectionService _collectionService;
    private readonly IEnvironmentService _environmentService;
    private readonly IEnumerable<IImportStrategy> _importStrategies;

    public ImportService(
        IRequestService requestService, 
        ICollectionService collectionService,
        IEnvironmentService environmentService,
        IEnumerable<IImportStrategy> importStrategies)
    {
        _requestService = requestService;
        _collectionService = collectionService;
        _environmentService = environmentService;
        _importStrategies = importStrategies;
    }

    public bool CanImport(ImportSource source)
    {
        return _importStrategies.Any(s => s.Source == source);
    }

    public async Task<ImportResult> ImportFromCurlAsync(string curlCommand, Guid? collectionId = null, string? customName = null)
    {
        var result = new ImportResult();

        try
        {
            var strategy = _importStrategies.FirstOrDefault(s => s.Source == ImportSource.Curl);
            if (strategy == null)
            {
                result.ErrorMessage = "Curl import strategy not found.";
                return result;
            }

            var request = strategy.Parse(curlCommand, collectionId, customName);
            
            if (request == null)
            {
                result.ErrorMessage = "Failed to parse curl command. Please check the format.";
                return result;
            }

            // Save the request
            var savedRequest = await _requestService.CreateRequestAsync(request);
            
            result.Success = true;
            result.ImportedRequest = savedRequest;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"Error importing curl command: {ex.Message}";
        }

        return result;
    }

    public async Task<ImportResult> ImportFromBrunoAsync(string brunoFileContent, Guid? collectionId = null, string? customName = null)
    {
        var result = new ImportResult();

        try
        {
            var strategy = _importStrategies.FirstOrDefault(s => s.Source == ImportSource.Bruno);
            if (strategy == null)
            {
                result.ErrorMessage = "Bruno import strategy not found.";
                return result;
            }

            var request = strategy.Parse(brunoFileContent, collectionId, customName);
            
            if (request == null)
            {
                result.ErrorMessage = "Failed to parse Bruno file. Please check the format.";
                return result;
            }

            // Save the request
            var savedRequest = await _requestService.CreateRequestAsync(request);
            
            result.Success = true;
            result.ImportedRequest = savedRequest;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"Error importing Bruno file: {ex.Message}";
        }

        return result;
    }

    public async Task<ImportResult> ImportFromBrunoEnvironmentAsync(string brunoEnvironmentContent, string environmentName)
    {
        var result = new ImportResult();

        try
        {
            var strategy = _importStrategies.FirstOrDefault(s => s.Source == ImportSource.Bruno);
            if (strategy == null)
            {
                result.Success = false;
                result.ErrorMessage = "Bruno import strategy not found.";
                return result;
            }

            if (strategy is not BrunoImportStrategy brunoStrategy)
            {
                result.Success = false;
                result.ErrorMessage = "Invalid Bruno import strategy type.";
                return result;
            }

            var environment = brunoStrategy.ParseEnvironment(brunoEnvironmentContent, environmentName);
            
            if (environment == null)
            {
                result.Success = false;
                result.ErrorMessage = "Failed to parse Bruno environment file. Please check the format.";
                return result;
            }

            // Save the environment
            var savedEnvironment = await _environmentService.CreateEnvironmentAsync(environment.Name, environment.Description);
            
            // Update the variables and secret variables
            savedEnvironment.Variables = environment.Variables;
            savedEnvironment.SecretVariableNames = environment.SecretVariableNames;
            savedEnvironment = await _environmentService.UpdateEnvironmentAsync(savedEnvironment);
            
            result.Success = true;
            result.ImportedEnvironments.Add(savedEnvironment);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = $"Error importing Bruno environment file: {ex.Message}";
        }

        return result;
    }

    public async Task<ImportResult> ImportFromBrunoFolderAsync(string folderPath, Guid? parentCollectionId = null)
    {
        var result = new ImportResult();

        try
        {
            var strategy = _importStrategies.FirstOrDefault(s => s.Source == ImportSource.Bruno);
            if (strategy == null)
            {
                result.Success = false;
                result.ErrorMessage = "Bruno import strategy not found.";
                return result;
            }

            if (!Directory.Exists(folderPath))
            {
                result.Success = false;
                result.ErrorMessage = "The specified folder does not exist.";
                return result;
            }

            // Import environments from environments/ subfolder if it exists
            var environmentsPath = Path.Combine(folderPath, "environments");
            if (Directory.Exists(environmentsPath))
            {
                await ImportEnvironmentsFromFolderAsync(environmentsPath, strategy, result);
            }

            // Process the folder recursively
            await ProcessFolderAsync(folderPath, parentCollectionId, strategy, result, isRootFolder: true);

            // Determine final success status based on results
            if (result.TotalFilesProcessed == 0 && result.ImportedEnvironments.Count == 0)
            {
                result.Success = false;
                result.ErrorMessage = "No Bruno files (.bru) or environments found in the specified folder.";
            }
            else if (result.FailedImports > 0 && result.SuccessfulImports == 0 && result.ImportedEnvironments.Count == 0)
            {
                result.Success = false;
                result.ErrorMessage = $"Failed to import all {result.FailedImports} files.";
            }
            else
            {
                // Partial or complete success
                result.Success = true;
                if (result.FailedImports > 0)
                {
                    result.Warnings.Add($"{result.FailedImports} out of {result.TotalFilesProcessed} files failed to import.");
                }
                
                // Add summary for debugging
                if (result.TotalFilesProcessed == 0 && result.ImportedCollections.Count > 0)
                {
                    result.Warnings.Add($"Import summary: {result.ImportedCollections.Count} collection(s) created, {result.ImportedEnvironments.Count} environment(s) imported, but no request files were found or processed. Check that your .bru request files are in the subfolders.");
                }
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = $"Error importing Bruno folder: {ex.Message}";
        }

        return result;
    }

    private async Task ImportEnvironmentsFromFolderAsync(
        string environmentsPath,
        IImportStrategy strategy,
        ImportResult result)
    {
        if (strategy is not BrunoImportStrategy brunoStrategy)
        {
            return;
        }

        var environmentFiles = Directory.GetFiles(environmentsPath, "*.bru", SearchOption.TopDirectoryOnly);
        
        foreach (var envFile in environmentFiles)
        {
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(envFile);
                var fileContent = await File.ReadAllTextAsync(envFile);
                
                var environment = brunoStrategy.ParseEnvironment(fileContent, fileName);
                
                if (environment == null)
                {
                    result.Warnings.Add($"Failed to parse environment file: {Path.GetFileName(envFile)}");
                    continue;
                }

                // Save the environment
                var savedEnvironment = await _environmentService.CreateEnvironmentAsync(environment.Name, environment.Description);
                
                // Update the variables and secret variables
                savedEnvironment.Variables = environment.Variables;
                savedEnvironment.SecretVariableNames = environment.SecretVariableNames;
                savedEnvironment = await _environmentService.UpdateEnvironmentAsync(savedEnvironment);
                
                result.ImportedEnvironments.Add(savedEnvironment);
            }
            catch (Exception ex)
            {
                result.Warnings.Add($"Error importing environment '{Path.GetFileName(envFile)}': {ex.Message}");
            }
        }
    }

    private async Task ProcessFolderAsync(
        string folderPath, 
        Guid? parentCollectionId, 
        IImportStrategy strategy,
        ImportResult result,
        bool isRootFolder = false)
    {
        var folderName = Path.GetFileName(folderPath);
        
        // Get all .bru files excluding collection.bru
        var allBrunoFiles = Directory.GetFiles(folderPath, "*.bru", SearchOption.TopDirectoryOnly);
        var brunoFiles = allBrunoFiles.Where(f => !Path.GetFileName(f).Equals("collection.bru", StringComparison.OrdinalIgnoreCase)).ToArray();
        
        var subFolders = Directory.GetDirectories(folderPath);
        
        // Skip the environments folder - it's already processed at the root level
        if (isRootFolder)
        {
            subFolders = subFolders.Where(f => !Path.GetFileName(f).Equals("environments", StringComparison.OrdinalIgnoreCase)).ToArray();
        }
        
        // Create a collection for this folder (unless it's the root folder being imported)
        Collection? folderCollection = null;
        
        // Only create a collection if there are request files or subfolders to organize
        if (brunoFiles.Length > 0 || subFolders.Length > 0)
        {
            try
            {
                // Check for collection name in bruno.json or use folder name
                var collectionName = folderName;
                var brunoJsonPath = Path.Combine(folderPath, "bruno.json");
                if (File.Exists(brunoJsonPath))
                {
                    try
                    {
                        var jsonContent = await File.ReadAllTextAsync(brunoJsonPath);
                        using var jsonDoc = JsonDocument.Parse(jsonContent);
                        if (jsonDoc.RootElement.TryGetProperty("name", out var nameProperty))
                        {
                            collectionName = nameProperty.GetString() ?? collectionName;
                        }
                    }
                    catch (JsonException)
                    {
                        // If bruno.json parsing fails, just use folder name
                    }
                }
                
                folderCollection = await _collectionService.CreateCollectionAsync(
                    collectionName,
                    parentCollectionId,
                    $"Imported from folder: {folderPath}");
                
                // Check for collection.bru and parse variables
                var collectionBruPath = Path.Combine(folderPath, "collection.bru");
                if (File.Exists(collectionBruPath) && strategy is BrunoImportStrategy brunoStrategy)
                {
                    try
                    {
                        var collectionBruContent = await File.ReadAllTextAsync(collectionBruPath);
                        folderCollection.Variables = brunoStrategy.ParseCollectionVariables(collectionBruContent);
                        folderCollection.SecretVariableNames = brunoStrategy.ParseCollectionSecretVariables(collectionBruContent);
                        
                        // Update the collection with variables
                        folderCollection = await _collectionService.UpdateCollectionAsync(folderCollection);
                    }
                    catch (Exception ex)
                    {
                        result.Warnings.Add($"Failed to parse collection variables from collection.bru in '{folderName}': {ex.Message}");
                    }
                }
                
                result.ImportedCollections.Add(folderCollection);
            }
            catch (Exception ex)
            {
                result.Warnings.Add($"Failed to create collection for folder '{folderName}': {ex.Message}");
                // Continue processing files even if collection creation failed
            }
        }

        // Import all .bru files in the current folder
        foreach (var filePath in brunoFiles)
        {
            result.TotalFilesProcessed++;
            
            try
            {
                var fileContent = await File.ReadAllTextAsync(filePath);
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                
                var request = strategy.Parse(fileContent, folderCollection?.Id, fileName);
                
                if (request == null)
                {
                    result.FailedImports++;
                    result.Warnings.Add($"Failed to parse Bruno file: {Path.GetFileName(filePath)}");
                    continue;
                }

                // Save the request
                var savedRequest = await _requestService.CreateRequestAsync(request);
                result.ImportedRequests.Add(savedRequest);
                result.SuccessfulImports++;
            }
            catch (Exception ex)
            {
                result.FailedImports++;
                result.Warnings.Add($"Error importing file '{Path.GetFileName(filePath)}': {ex.Message}");
            }
        }

        // Process subfolders recursively
        foreach (var subFolder in subFolders)
        {
            // Use the newly created collection as parent for subfolders, or the original parent if no collection was created
            var effectiveParentId = folderCollection?.Id ?? parentCollectionId;
            await ProcessFolderAsync(subFolder, effectiveParentId, strategy, result, isRootFolder: false);
        }
    }
}
