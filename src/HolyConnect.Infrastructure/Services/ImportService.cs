using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;

namespace HolyConnect.Infrastructure.Services;

/// <summary>
/// Service for importing requests from various formats using the Strategy pattern
/// </summary>
public class ImportService : IImportService
{
    private readonly IRequestService _requestService;
    private readonly IEnumerable<IImportStrategy> _importStrategies;

    public ImportService(IRequestService requestService, IEnumerable<IImportStrategy> importStrategies)
    {
        _requestService = requestService;
        _importStrategies = importStrategies;
    }

    public bool CanImport(ImportSource source)
    {
        return _importStrategies.Any(s => s.Source == source);
    }

    public async Task<ImportResult> ImportFromCurlAsync(string curlCommand, Guid environmentId, Guid? collectionId = null, string? customName = null)
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

            var request = strategy.Parse(curlCommand, environmentId, collectionId, customName);
            
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

    public async Task<ImportResult> ImportFromBrunoAsync(string brunoFileContent, Guid environmentId, Guid? collectionId = null, string? customName = null)
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

            var request = strategy.Parse(brunoFileContent, environmentId, collectionId, customName);
            
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
}
