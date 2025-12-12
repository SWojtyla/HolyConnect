using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;

namespace HolyConnect.Infrastructure.Services;

/// <summary>
/// Service for importing requests from various formats using the Strategy pattern
/// </summary>
public class ImportService : IImportService
{
    private readonly IRequestService _requestService;
    private readonly ICollectionService _collectionService;
    private readonly IEnumerable<IImportStrategy> _importStrategies;

    public ImportService(
        IRequestService requestService, 
        ICollectionService collectionService,
        IEnumerable<IImportStrategy> importStrategies)
    {
        _requestService = requestService;
        _collectionService = collectionService;
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

            // Process the folder recursively
            await ProcessFolderAsync(folderPath, parentCollectionId, strategy, result);

            // Determine final success status based on results
            if (result.TotalFilesProcessed == 0)
            {
                result.Success = false;
                result.ErrorMessage = "No Bruno files (.bru) found in the specified folder.";
            }
            else if (result.FailedImports > 0 && result.SuccessfulImports == 0)
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
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = $"Error importing Bruno folder: {ex.Message}";
        }

        return result;
    }

    private async Task ProcessFolderAsync(
        string folderPath, 
        Guid? parentCollectionId, 
        IImportStrategy strategy,
        ImportResult result)
    {
        var folderName = Path.GetFileName(folderPath);
        
        // Create a collection for this folder (unless it's the root folder being imported)
        Collection? folderCollection = null;
        var brunoFiles = Directory.GetFiles(folderPath, "*.bru", SearchOption.TopDirectoryOnly);
        var subFolders = Directory.GetDirectories(folderPath);
        
        // Only create a collection if there are files or subfolders to organize
        if (brunoFiles.Length > 0 || subFolders.Length > 0)
        {
            try
            {
                folderCollection = await _collectionService.CreateCollectionAsync(
                    folderName,
                    parentCollectionId,
                    $"Imported from folder: {folderPath}");
                
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
            await ProcessFolderAsync(subFolder, effectiveParentId, strategy, result);
        }
    }
}
