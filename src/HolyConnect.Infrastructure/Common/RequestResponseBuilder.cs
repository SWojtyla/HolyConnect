using System.Diagnostics;
using System.Net.Http.Headers;
using HolyConnect.Domain.Entities;

namespace HolyConnect.Infrastructure.Common;

/// <summary>
/// Fluent builder for constructing RequestResponse objects with a consistent API.
/// Centralizes response construction logic used across all request executors.
/// </summary>
public class RequestResponseBuilder
{
    private readonly RequestResponse _response;
    private readonly Stopwatch _stopwatch;

    private RequestResponseBuilder()
    {
        _response = new RequestResponse
        {
            Timestamp = DateTime.UtcNow
        };
        _stopwatch = new Stopwatch();
    }

    /// <summary>
    /// Creates a new builder instance and starts timing.
    /// </summary>
    public static RequestResponseBuilder Create()
    {
        var builder = new RequestResponseBuilder();
        builder._stopwatch.Start();
        return builder;
    }

    /// <summary>
    /// Creates a new builder instance for streaming responses and starts timing.
    /// </summary>
    public static RequestResponseBuilder CreateStreaming()
    {
        var builder = new RequestResponseBuilder();
        builder._response.IsStreaming = true;
        builder._stopwatch.Start();
        return builder;
    }

    /// <summary>
    /// Sets the sent request details.
    /// </summary>
    public RequestResponseBuilder WithSentRequest(SentRequest sentRequest)
    {
        _response.SentRequest = sentRequest;
        return this;
    }

    /// <summary>
    /// Sets the sent request details from an HttpRequestMessage.
    /// </summary>
    public RequestResponseBuilder WithSentRequest(HttpRequestMessage httpRequest, string url, string method, string? body = null, Dictionary<string, string>? queryParameters = null)
    {
        _response.SentRequest = HttpRequestHelper.CreateSentRequest(httpRequest, url, method, body, queryParameters);
        return this;
    }

    /// <summary>
    /// Stops the timing and records the response time.
    /// </summary>
    public RequestResponseBuilder StopTiming()
    {
        _stopwatch.Stop();
        _response.ResponseTime = _stopwatch.ElapsedMilliseconds;
        return this;
    }

    /// <summary>
    /// Sets the HTTP status code and message from an HttpResponseMessage.
    /// </summary>
    public RequestResponseBuilder WithStatus(HttpResponseMessage httpResponse)
    {
        _response.StatusCode = (int)httpResponse.StatusCode;
        _response.StatusMessage = httpResponse.ReasonPhrase ?? string.Empty;
        return this;
    }

    /// <summary>
    /// Sets a custom HTTP status code and message.
    /// </summary>
    public RequestResponseBuilder WithStatus(int statusCode, string statusMessage)
    {
        _response.StatusCode = statusCode;
        _response.StatusMessage = statusMessage;
        return this;
    }

    /// <summary>
    /// Captures headers from HTTP response headers.
    /// </summary>
    public RequestResponseBuilder WithHeaders(HttpResponseHeaders headers)
    {
        ResponseHelper.CaptureHeaders(_response.Headers, headers);
        return this;
    }

    /// <summary>
    /// Captures headers from HTTP content headers.
    /// </summary>
    public RequestResponseBuilder WithHeaders(HttpContentHeaders? headers)
    {
        if (headers != null)
        {
            ResponseHelper.CaptureHeaders(_response.Headers, headers);
        }
        return this;
    }

    /// <summary>
    /// Sets the response body and calculates size.
    /// </summary>
    public RequestResponseBuilder WithBody(string body)
    {
        _response.Body = body;
        _response.Size = body.Length;
        return this;
    }

    /// <summary>
    /// Reads and sets the response body from HttpContent and captures content headers.
    /// </summary>
    public async Task<RequestResponseBuilder> WithBodyFromContentAsync(HttpContent? content)
    {
        if (content != null)
        {
            _response.Body = await content.ReadAsStringAsync();
            _response.Size = _response.Body.Length;
            ResponseHelper.CaptureHeaders(_response.Headers, content.Headers);
        }
        return this;
    }

    /// <summary>
    /// Adds a stream event to the response.
    /// </summary>
    public RequestResponseBuilder AddStreamEvent(string data, string? eventType = null)
    {
        _response.StreamEvents.Add(new StreamEvent
        {
            Timestamp = DateTime.UtcNow,
            Data = data,
            EventType = eventType
        });
        return this;
    }

    /// <summary>
    /// Adds a stream event to the response.
    /// </summary>
    public RequestResponseBuilder AddStreamEvent(StreamEvent streamEvent)
    {
        _response.StreamEvents.Add(streamEvent);
        return this;
    }

    /// <summary>
    /// Finalizes a streaming response by building the body from stream events.
    /// </summary>
    public RequestResponseBuilder FinalizeStreaming()
    {
        ResponseHelper.FinalizeStreamingResponse(_response);
        return this;
    }

    /// <summary>
    /// Handles an exception and populates error information.
    /// Stops timing if not already stopped.
    /// </summary>
    public RequestResponseBuilder WithException(Exception ex)
    {
        if (_stopwatch.IsRunning)
        {
            _stopwatch.Stop();
        }
        ResponseHelper.HandleException(_response, ex, _stopwatch.ElapsedMilliseconds);
        return this;
    }

    /// <summary>
    /// Builds and returns the final RequestResponse object.
    /// </summary>
    public RequestResponse Build()
    {
        return _response;
    }

    /// <summary>
    /// Gets the elapsed milliseconds from the stopwatch without stopping it.
    /// Useful for intermediate timing checks.
    /// </summary>
    public long ElapsedMilliseconds => _stopwatch.ElapsedMilliseconds;
}
