using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;
using HolyConnect.Infrastructure.Common;

namespace HolyConnect.Infrastructure.Services;

public class GraphQLRequestExecutor : IRequestExecutor
{
    private readonly HttpClient _httpClient;

    public GraphQLRequestExecutor(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public bool CanExecute(Request request)
    {
        return request is GraphQLRequest;
    }

    public async Task<RequestResponse> ExecuteAsync(Request request)
    {
        if (request is not GraphQLRequest graphQLRequest)
        {
            throw new ArgumentException("Request must be of type GraphQLRequest", nameof(request));
        }

        var builder = RequestResponseBuilder.Create();

        try
        {
            var json = GraphQLHelper.SerializePayload(graphQLRequest);
            var httpRequest = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, graphQLRequest.Url);
            
            // Set content with or without Content-Type based on DisabledHeaders
            HttpRequestHelper.SetContent(httpRequest, json, HttpConstants.MediaTypes.ApplicationJson, graphQLRequest);

            // Add User-Agent header by default
            HttpRequestHelper.AddUserAgentHeader(httpRequest, graphQLRequest);

            // Apply authentication and headers using helpers
            HttpAuthenticationHelper.ApplyAuthentication(httpRequest, graphQLRequest);
            HttpAuthenticationHelper.ApplyHeaders(httpRequest, graphQLRequest);

            // Capture the sent request details
            builder.WithSentRequest(httpRequest, graphQLRequest.Url, "POST", json);

            var httpResponse = await _httpClient.SendAsync(httpRequest);

            builder.StopTiming()
                .WithStatus(httpResponse)
                .WithHeaders(httpResponse.Headers);

            await builder.WithBodyFromContentAsync(httpResponse.Content);
        }
        catch (Exception ex)
        {
            builder.WithException(ex);
        }

        return builder.Build();
    }

}
