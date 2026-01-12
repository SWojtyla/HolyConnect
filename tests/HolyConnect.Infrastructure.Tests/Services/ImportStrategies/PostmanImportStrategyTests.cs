using HolyConnect.Domain.Entities;
using HolyConnect.Infrastructure.Services.ImportStrategies;
using HttpMethod = HolyConnect.Domain.Entities.HttpMethod;

namespace HolyConnect.Infrastructure.Tests.Services.ImportStrategies;

public class PostmanImportStrategyTests
{
    private readonly PostmanImportStrategy _strategy;

    public PostmanImportStrategyTests()
    {
        _strategy = new PostmanImportStrategy();
    }

    [Fact]
    public void Source_ShouldReturnPostman()
    {
        // Assert
        Assert.Equal(ImportSource.Postman, _strategy.Source);
    }

    [Fact]
    public void Parse_WithEmptyContent_ShouldReturnNull()
    {
        // Arrange
        var emptyContent = "";

        // Act
        var result = _strategy.Parse(emptyContent, null, null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Parse_WithInvalidJson_ShouldReturnNull()
    {
        // Arrange
        var invalidJson = "not a valid json";

        // Act
        var result = _strategy.Parse(invalidJson, null, null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Parse_WithValidGetRequest_ShouldReturnRestRequest()
    {
        // Arrange
        var postmanJson = @"{
            ""name"": ""Get Users"",
            ""request"": {
                ""method"": ""GET"",
                ""url"": ""https://api.example.com/users""
            }
        }";

        // Act
        var result = _strategy.Parse(postmanJson, null, null);

        // Assert
        Assert.NotNull(result);
        var restRequest = Assert.IsType<RestRequest>(result);
        Assert.Equal("Get Users", restRequest.Name);
        Assert.Equal("https://api.example.com/users", restRequest.Url);
        Assert.Equal(HttpMethod.Get, restRequest.Method);
    }

    [Fact]
    public void Parse_WithPostRequestAndJsonBody_ShouldReturnRestRequestWithBody()
    {
        // Arrange
        var postmanJson = @"{
            ""name"": ""Create User"",
            ""request"": {
                ""method"": ""POST"",
                ""url"": ""https://api.example.com/users"",
                ""body"": {
                    ""mode"": ""raw"",
                    ""raw"": ""{\""name\"":\""John Doe\"",\""email\"":\""john@example.com\""}"",
                    ""options"": {
                        ""raw"": {
                            ""language"": ""json""
                        }
                    }
                }
            }
        }";

        // Act
        var result = _strategy.Parse(postmanJson, null, null);

        // Assert
        Assert.NotNull(result);
        var restRequest = Assert.IsType<RestRequest>(result);
        Assert.Equal(HttpMethod.Post, restRequest.Method);
        Assert.NotNull(restRequest.Body);
        Assert.Contains("John Doe", restRequest.Body);
        Assert.Equal(BodyType.Json, restRequest.BodyType);
        Assert.Equal("application/json", restRequest.ContentType);
    }

    [Fact]
    public void Parse_WithCustomName_ShouldUseCustomName()
    {
        // Arrange
        var postmanJson = @"{
            ""name"": ""Original Name"",
            ""request"": {
                ""method"": ""GET"",
                ""url"": ""https://api.example.com/users""
            }
        }";
        var customName = "My Custom Request";

        // Act
        var result = _strategy.Parse(postmanJson, null, customName);

        // Assert
        Assert.NotNull(result);
        var restRequest = Assert.IsType<RestRequest>(result);
        Assert.Equal(customName, restRequest.Name);
    }

    [Fact]
    public void Parse_WithBearerAuth_ShouldSetAuthentication()
    {
        // Arrange
        var postmanJson = @"{
            ""name"": ""Get Protected"",
            ""request"": {
                ""method"": ""GET"",
                ""url"": ""https://api.example.com/protected"",
                ""auth"": {
                    ""type"": ""bearer"",
                    ""bearer"": [
                        {
                            ""key"": ""token"",
                            ""value"": ""my-secret-token"",
                            ""type"": ""string""
                        }
                    ]
                }
            }
        }";

        // Act
        var result = _strategy.Parse(postmanJson, null, null);

        // Assert
        Assert.NotNull(result);
        var restRequest = Assert.IsType<RestRequest>(result);
        Assert.Equal(AuthenticationType.BearerToken, restRequest.AuthType);
        Assert.Equal("my-secret-token", restRequest.BearerToken);
    }

    [Fact]
    public void Parse_WithBasicAuth_ShouldSetAuthentication()
    {
        // Arrange
        var postmanJson = @"{
            ""name"": ""Get Protected"",
            ""request"": {
                ""method"": ""GET"",
                ""url"": ""https://api.example.com/protected"",
                ""auth"": {
                    ""type"": ""basic"",
                    ""basic"": [
                        {
                            ""key"": ""username"",
                            ""value"": ""testuser"",
                            ""type"": ""string""
                        },
                        {
                            ""key"": ""password"",
                            ""value"": ""testpass"",
                            ""type"": ""string""
                        }
                    ]
                }
            }
        }";

        // Act
        var result = _strategy.Parse(postmanJson, null, null);

        // Assert
        Assert.NotNull(result);
        var restRequest = Assert.IsType<RestRequest>(result);
        Assert.Equal(AuthenticationType.Basic, restRequest.AuthType);
        Assert.Equal("testuser", restRequest.BasicAuthUsername);
        Assert.Equal("testpass", restRequest.BasicAuthPassword);
    }

    [Fact]
    public void Parse_WithHeaders_ShouldSetHeaders()
    {
        // Arrange
        var postmanJson = @"{
            ""name"": ""Get Users"",
            ""request"": {
                ""method"": ""GET"",
                ""url"": ""https://api.example.com/users"",
                ""header"": [
                    {
                        ""key"": ""Content-Type"",
                        ""value"": ""application/json"",
                        ""type"": ""text""
                    },
                    {
                        ""key"": ""X-Custom-Header"",
                        ""value"": ""custom-value"",
                        ""type"": ""text""
                    }
                ]
            }
        }";

        // Act
        var result = _strategy.Parse(postmanJson, null, null);

        // Assert
        Assert.NotNull(result);
        var restRequest = Assert.IsType<RestRequest>(result);
        Assert.Equal(2, restRequest.Headers.Count);
        Assert.Equal("application/json", restRequest.Headers["Content-Type"]);
        Assert.Equal("custom-value", restRequest.Headers["X-Custom-Header"]);
    }

    [Fact]
    public void Parse_WithDisabledHeader_ShouldSkipHeader()
    {
        // Arrange
        var postmanJson = @"{
            ""name"": ""Get Users"",
            ""request"": {
                ""method"": ""GET"",
                ""url"": ""https://api.example.com/users"",
                ""header"": [
                    {
                        ""key"": ""Content-Type"",
                        ""value"": ""application/json"",
                        ""type"": ""text""
                    },
                    {
                        ""key"": ""X-Disabled-Header"",
                        ""value"": ""should-not-appear"",
                        ""type"": ""text"",
                        ""disabled"": true
                    }
                ]
            }
        }";

        // Act
        var result = _strategy.Parse(postmanJson, null, null);

        // Assert
        Assert.NotNull(result);
        var restRequest = Assert.IsType<RestRequest>(result);
        Assert.Single(restRequest.Headers);
        Assert.False(restRequest.Headers.ContainsKey("X-Disabled-Header"));
    }

    [Fact]
    public void Parse_WithUrlObject_ShouldConstructUrl()
    {
        // Arrange
        var postmanJson = @"{
            ""name"": ""Get Users"",
            ""request"": {
                ""method"": ""GET"",
                ""url"": {
                    ""raw"": ""https://api.example.com/users?page=1"",
                    ""protocol"": ""https"",
                    ""host"": [""api"", ""example"", ""com""],
                    ""path"": [""users""],
                    ""query"": [
                        {
                            ""key"": ""page"",
                            ""value"": ""1""
                        }
                    ]
                }
            }
        }";

        // Act
        var result = _strategy.Parse(postmanJson, null, null);

        // Assert
        Assert.NotNull(result);
        var restRequest = Assert.IsType<RestRequest>(result);
        Assert.Equal("https://api.example.com/users?page=1", restRequest.Url);
    }

    [Fact]
    public void Parse_WithGraphQLRequest_ShouldReturnGraphQLRequest()
    {
        // Arrange
        var postmanJson = @"{
            ""name"": ""Get User"",
            ""request"": {
                ""method"": ""POST"",
                ""url"": ""https://api.example.com/graphql"",
                ""body"": {
                    ""mode"": ""graphql"",
                    ""graphql"": {
                        ""query"": ""query GetUser { user(id: 1) { name email } }"",
                        ""variables"": ""{\""id\"": 1}""
                    }
                }
            }
        }";

        // Act
        var result = _strategy.Parse(postmanJson, null, null);

        // Assert
        Assert.NotNull(result);
        var graphqlRequest = Assert.IsType<GraphQLRequest>(result);
        Assert.Equal("Get User", graphqlRequest.Name);
        Assert.Equal("https://api.example.com/graphql", graphqlRequest.Url);
        Assert.Contains("GetUser", graphqlRequest.Query);
        Assert.Equal(GraphQLOperationType.Query, graphqlRequest.OperationType);
    }

    [Fact]
    public void Parse_WithGraphQLMutation_ShouldSetCorrectOperationType()
    {
        // Arrange
        var postmanJson = @"{
            ""name"": ""Create User"",
            ""request"": {
                ""method"": ""POST"",
                ""url"": ""https://api.example.com/graphql"",
                ""body"": {
                    ""mode"": ""graphql"",
                    ""graphql"": {
                        ""query"": ""mutation CreateUser { createUser(name: \""John\"") { id name } }""
                    }
                }
            }
        }";

        // Act
        var result = _strategy.Parse(postmanJson, null, null);

        // Assert
        Assert.NotNull(result);
        var graphqlRequest = Assert.IsType<GraphQLRequest>(result);
        Assert.Equal(GraphQLOperationType.Mutation, graphqlRequest.OperationType);
    }

    [Fact]
    public void ParseCollection_WithValidCollection_ShouldReturnRequestsAndCollections()
    {
        // Arrange
        var postmanCollection = @"{
            ""info"": {
                ""name"": ""Test Collection"",
                ""schema"": ""https://schema.getpostman.com/json/collection/v2.1.0/collection.json""
            },
            ""item"": [
                {
                    ""name"": ""Get Users"",
                    ""request"": {
                        ""method"": ""GET"",
                        ""url"": ""https://api.example.com/users""
                    }
                },
                {
                    ""name"": ""Create User"",
                    ""request"": {
                        ""method"": ""POST"",
                        ""url"": ""https://api.example.com/users"",
                        ""body"": {
                            ""mode"": ""raw"",
                            ""raw"": ""{\""name\"":\""John\""}"",
                            ""options"": {
                                ""raw"": {
                                    ""language"": ""json""
                                }
                            }
                        }
                    }
                }
            ]
        }";

        // Act
        var (requests, collections, environments) = _strategy.ParseCollection(postmanCollection, null);

        // Assert
        Assert.Single(collections);
        Assert.Equal("Test Collection", collections[0].Name);
        Assert.Equal(2, requests.Count);
        Assert.Equal("Get Users", requests[0].Name);
        Assert.Equal("Create User", requests[1].Name);
    }

    [Fact]
    public void ParseCollection_WithFolders_ShouldCreateSubcollections()
    {
        // Arrange
        var postmanCollection = @"{
            ""info"": {
                ""name"": ""Test Collection""
            },
            ""item"": [
                {
                    ""name"": ""Users Folder"",
                    ""item"": [
                        {
                            ""name"": ""Get User"",
                            ""request"": {
                                ""method"": ""GET"",
                                ""url"": ""https://api.example.com/users/1""
                            }
                        }
                    ]
                }
            ]
        }";

        // Act
        var (requests, collections, environments) = _strategy.ParseCollection(postmanCollection, null);

        // Assert
        Assert.Equal(2, collections.Count); // Main collection + subfolder
        Assert.Equal("Test Collection", collections[0].Name);
        Assert.Equal("Users Folder", collections[1].Name);
        Assert.Single(requests);
        Assert.Equal("Get User", requests[0].Name);
    }

    [Fact]
    public void ParseCollection_WithCollectionVariables_ShouldParseVariables()
    {
        // Arrange
        var postmanCollection = @"{
            ""info"": {
                ""name"": ""Test Collection""
            },
            ""variable"": [
                {
                    ""key"": ""baseUrl"",
                    ""value"": ""https://api.example.com"",
                    ""type"": ""string""
                },
                {
                    ""key"": ""apiKey"",
                    ""value"": ""secret123"",
                    ""type"": ""string""
                }
            ],
            ""item"": []
        }";

        // Act
        var (requests, collections, environments) = _strategy.ParseCollection(postmanCollection, null);

        // Assert
        Assert.Single(collections);
        Assert.Equal(2, collections[0].Variables.Count);
        Assert.Equal("https://api.example.com", collections[0].Variables["baseUrl"]);
        Assert.Equal("secret123", collections[0].Variables["apiKey"]);
    }

    [Fact]
    public void ParseEnvironment_WithValidEnvironment_ShouldReturnEnvironment()
    {
        // Arrange
        var postmanEnvironment = @"{
            ""name"": ""Production"",
            ""values"": [
                {
                    ""key"": ""baseUrl"",
                    ""value"": ""https://api.example.com"",
                    ""type"": ""default"",
                    ""enabled"": true
                },
                {
                    ""key"": ""apiKey"",
                    ""value"": ""prod-secret"",
                    ""type"": ""secret"",
                    ""enabled"": true
                }
            ]
        }";

        // Act
        var environment = _strategy.ParseEnvironment(postmanEnvironment);

        // Assert
        Assert.NotNull(environment);
        Assert.Equal("Production", environment.Name);
        Assert.Equal(2, environment.Variables.Count);
        Assert.Equal("https://api.example.com", environment.Variables["baseUrl"]);
        Assert.Equal("prod-secret", environment.Variables["apiKey"]);
        Assert.Contains("apiKey", environment.SecretVariableNames);
    }

    [Fact]
    public void ParseEnvironment_WithCustomName_ShouldUseCustomName()
    {
        // Arrange
        var postmanEnvironment = @"{
            ""name"": ""Original Name"",
            ""values"": [
                {
                    ""key"": ""baseUrl"",
                    ""value"": ""https://api.example.com"",
                    ""type"": ""default""
                }
            ]
        }";
        var customName = "Custom Environment Name";

        // Act
        var environment = _strategy.ParseEnvironment(postmanEnvironment, customName);

        // Assert
        Assert.NotNull(environment);
        Assert.Equal(customName, environment.Name);
    }

    [Fact]
    public void ParseEnvironment_WithInvalidJson_ShouldReturnNull()
    {
        // Arrange
        var invalidJson = "not a valid json";

        // Act
        var environment = _strategy.ParseEnvironment(invalidJson);

        // Assert
        Assert.Null(environment);
    }

    [Fact]
    public void ParseEnvironment_WithoutValuesProperty_ShouldReturnNull()
    {
        // Arrange
        var postmanJson = @"{
            ""name"": ""Test Environment""
        }";

        // Act
        var environment = _strategy.ParseEnvironment(postmanJson);

        // Assert
        Assert.Null(environment);
    }

    [Fact]
    public void Parse_WithFormDataBody_ShouldConvertToString()
    {
        // Arrange
        var postmanJson = @"{
            ""name"": ""Submit Form"",
            ""request"": {
                ""method"": ""POST"",
                ""url"": ""https://api.example.com/form"",
                ""body"": {
                    ""mode"": ""urlencoded"",
                    ""urlencoded"": [
                        {
                            ""key"": ""username"",
                            ""value"": ""john"",
                            ""type"": ""text""
                        },
                        {
                            ""key"": ""password"",
                            ""value"": ""secret"",
                            ""type"": ""text""
                        }
                    ]
                }
            }
        }";

        // Act
        var result = _strategy.Parse(postmanJson, null, null);

        // Assert
        Assert.NotNull(result);
        var restRequest = Assert.IsType<RestRequest>(result);
        Assert.Equal("username=john&password=secret", restRequest.Body);
        Assert.Equal("application/x-www-form-urlencoded", restRequest.ContentType);
    }

    [Fact]
    public void Parse_WithXmlBody_ShouldSetCorrectBodyType()
    {
        // Arrange
        var postmanJson = @"{
            ""name"": ""Send XML"",
            ""request"": {
                ""method"": ""POST"",
                ""url"": ""https://api.example.com/xml"",
                ""body"": {
                    ""mode"": ""raw"",
                    ""raw"": ""<user><name>John</name></user>"",
                    ""options"": {
                        ""raw"": {
                            ""language"": ""xml""
                        }
                    }
                }
            }
        }";

        // Act
        var result = _strategy.Parse(postmanJson, null, null);

        // Assert
        Assert.NotNull(result);
        var restRequest = Assert.IsType<RestRequest>(result);
        Assert.Equal(BodyType.Xml, restRequest.BodyType);
        Assert.Equal("application/xml", restRequest.ContentType);
        Assert.Contains("<user>", restRequest.Body);
    }

    [Fact]
    public void Parse_WithCollectionFirstItem_ShouldParseFirstRequest()
    {
        // Arrange
        var postmanCollection = @"{
            ""info"": {
                ""name"": ""Test Collection""
            },
            ""item"": [
                {
                    ""name"": ""First Request"",
                    ""request"": {
                        ""method"": ""GET"",
                        ""url"": ""https://api.example.com/first""
                    }
                },
                {
                    ""name"": ""Second Request"",
                    ""request"": {
                        ""method"": ""GET"",
                        ""url"": ""https://api.example.com/second""
                    }
                }
            ]
        }";

        // Act
        var result = _strategy.Parse(postmanCollection, null, null);

        // Assert
        Assert.NotNull(result);
        var restRequest = Assert.IsType<RestRequest>(result);
        Assert.Equal("First Request", restRequest.Name);
        Assert.Equal("https://api.example.com/first", restRequest.Url);
    }

    [Fact]
    public void ParseCollection_WithNestedFoldersAndRequests_ShouldAssignCorrectParentIds()
    {
        // Arrange - Structure: Main Collection > Folder1 > Folder2 > Request
        var postmanCollection = @"{
            ""info"": {
                ""name"": ""Main Collection""
            },
            ""item"": [
                {
                    ""name"": ""Folder1"",
                    ""item"": [
                        {
                            ""name"": ""Folder2"",
                            ""item"": [
                                {
                                    ""name"": ""Nested Request"",
                                    ""request"": {
                                        ""method"": ""GET"",
                                        ""url"": ""https://api.example.com/nested""
                                    }
                                }
                            ]
                        },
                        {
                            ""name"": ""Request in Folder1"",
                            ""request"": {
                                ""method"": ""GET"",
                                ""url"": ""https://api.example.com/folder1""
                            }
                        }
                    ]
                }
            ]
        }";

        // Act
        var (requests, collections, environments) = _strategy.ParseCollection(postmanCollection, null);

        // Assert
        Assert.Equal(3, collections.Count); // Main Collection + Folder1 + Folder2
        Assert.Equal(2, requests.Count);
        
        var mainCollection = collections[0];
        var folder1 = collections[1];
        var folder2 = collections[2];
        
        Assert.Equal("Main Collection", mainCollection.Name);
        Assert.Equal("Folder1", folder1.Name);
        Assert.Equal("Folder2", folder2.Name);
        
        // Verify parent relationships
        Assert.Null(mainCollection.ParentCollectionId);
        Assert.Equal(mainCollection.Id, folder1.ParentCollectionId);
        Assert.Equal(folder1.Id, folder2.ParentCollectionId);
        
        // Verify request assignments
        var nestedRequest = requests.First(r => r.Name == "Nested Request");
        var folder1Request = requests.First(r => r.Name == "Request in Folder1");
        
        Assert.Equal(folder2.Id, nestedRequest.CollectionId);
        Assert.Equal(folder1.Id, folder1Request.CollectionId);
    }
}
