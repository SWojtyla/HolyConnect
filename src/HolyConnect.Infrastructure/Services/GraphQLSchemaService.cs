using System.Collections.Concurrent;
using System.Text;
using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;
using Newtonsoft.Json;

namespace HolyConnect.Infrastructure.Services;

/// <summary>
/// Service for fetching and caching GraphQL schemas through introspection queries
/// </summary>
public class GraphQLSchemaService : IGraphQLSchemaService
{
    private readonly HttpClient _httpClient;
    private readonly ConcurrentDictionary<string, string> _schemaCache;

    // Standard GraphQL introspection query
    private const string IntrospectionQuery = @"
        query IntrospectionQuery {
            __schema {
                queryType { name }
                mutationType { name }
                subscriptionType { name }
                types {
                    ...FullType
                }
                directives {
                    name
                    description
                    locations
                    args {
                        ...InputValue
                    }
                }
            }
        }
        
        fragment FullType on __Type {
            kind
            name
            description
            fields(includeDeprecated: true) {
                name
                description
                args {
                    ...InputValue
                }
                type {
                    ...TypeRef
                }
                isDeprecated
                deprecationReason
            }
            inputFields {
                ...InputValue
            }
            interfaces {
                ...TypeRef
            }
            enumValues(includeDeprecated: true) {
                name
                description
                isDeprecated
                deprecationReason
            }
            possibleTypes {
                ...TypeRef
            }
        }
        
        fragment InputValue on __InputValue {
            name
            description
            type { ...TypeRef }
            defaultValue
        }
        
        fragment TypeRef on __Type {
            kind
            name
            ofType {
                kind
                name
                ofType {
                    kind
                    name
                    ofType {
                        kind
                        name
                        ofType {
                            kind
                            name
                            ofType {
                                kind
                                name
                                ofType {
                                    kind
                                    name
                                    ofType {
                                        kind
                                        name
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    ";

    public GraphQLSchemaService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _schemaCache = new ConcurrentDictionary<string, string>();
    }

    public async Task<string?> FetchSchemaAsync(string url, GraphQLRequest request)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        try
        {
            // Check cache first
            if (_schemaCache.TryGetValue(url, out var cachedSchema))
            {
                return cachedSchema;
            }

            var payload = new
            {
                query = IntrospectionQuery,
                operationName = "IntrospectionQuery"
            };

            var json = JsonConvert.SerializeObject(payload);
            var httpRequest = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            // Apply authentication from the request
            ApplyAuthentication(httpRequest, request);

            // Apply headers from the request (excluding disabled ones)
            var skipAuthorizationHeader = request.AuthType != AuthenticationType.None;
            foreach (var header in request.Headers.Where(h => !request.DisabledHeaders.Contains(h.Key)))
            {
                // Skip Authorization header if authentication is configured to avoid conflicts
                if (skipAuthorizationHeader && header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            var httpResponse = await _httpClient.SendAsync(httpRequest);

            if (!httpResponse.IsSuccessStatusCode)
            {
                return null;
            }

            var responseBody = await httpResponse.Content.ReadAsStringAsync();

            // Cache the schema
            _schemaCache[url] = responseBody;

            return responseBody;
        }
        catch
        {
            return null;
        }
    }

    public string? GetCachedSchema(string url)
    {
        return _schemaCache.TryGetValue(url, out var schema) ? schema : null;
    }

    public void ClearCache(string url)
    {
        _schemaCache.TryRemove(url, out _);
    }

    public void ClearAllCaches()
    {
        _schemaCache.Clear();
    }

    private void ApplyAuthentication(HttpRequestMessage httpRequest, Request request)
    {
        switch (request.AuthType)
        {
            case AuthenticationType.Basic:
                if (!string.IsNullOrEmpty(request.BasicAuthUsername))
                {
                    var credentials = $"{request.BasicAuthUsername}:{request.BasicAuthPassword ?? string.Empty}";
                    var encodedCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
                    httpRequest.Headers.TryAddWithoutValidation("Authorization", $"Basic {encodedCredentials}");
                }
                break;

            case AuthenticationType.BearerToken:
                if (!string.IsNullOrEmpty(request.BearerToken))
                {
                    httpRequest.Headers.TryAddWithoutValidation("Authorization", $"Bearer {request.BearerToken}");
                }
                break;

            case AuthenticationType.None:
            default:
                // No authentication
                break;
        }
    }
}
