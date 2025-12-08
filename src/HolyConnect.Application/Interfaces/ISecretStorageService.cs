namespace HolyConnect.Application.Interfaces;

/// <summary>
/// Service for securely storing and retrieving secret values (like API keys, tokens, passwords)
/// that should not be committed to version control.
/// </summary>
public interface ISecretStorageService
{
    /// <summary>
    /// Stores a secret value for a given request and header key.
    /// </summary>
    /// <param name="requestId">The ID of the request</param>
    /// <param name="headerKey">The name of the header</param>
    /// <param name="value">The secret value to store</param>
    Task SetSecretAsync(Guid requestId, string headerKey, string value);

    /// <summary>
    /// Retrieves a secret value for a given request and header key.
    /// </summary>
    /// <param name="requestId">The ID of the request</param>
    /// <param name="headerKey">The name of the header</param>
    /// <returns>The secret value, or null if not found</returns>
    Task<string?> GetSecretAsync(Guid requestId, string headerKey);

    /// <summary>
    /// Removes a secret value for a given request and header key.
    /// </summary>
    /// <param name="requestId">The ID of the request</param>
    /// <param name="headerKey">The name of the header</param>
    Task RemoveSecretAsync(Guid requestId, string headerKey);

    /// <summary>
    /// Removes all secrets for a given request.
    /// </summary>
    /// <param name="requestId">The ID of the request</param>
    Task RemoveAllSecretsForRequestAsync(Guid requestId);
}
