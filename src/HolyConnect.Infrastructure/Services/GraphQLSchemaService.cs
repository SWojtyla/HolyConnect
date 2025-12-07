using System.Collections.Concurrent;
using System.Text;
using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;
using HolyConnect.Infrastructure.Common;
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
                Content = new StringContent(json, Encoding.UTF8, HttpConstants.MediaTypes.ApplicationJson)
            };

            // Apply authentication and headers using helpers
            HttpAuthenticationHelper.ApplyAuthentication(httpRequest, request);
            HttpAuthenticationHelper.ApplyHeaders(httpRequest, request);

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

}
