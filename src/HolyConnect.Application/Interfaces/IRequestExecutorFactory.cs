using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Interfaces;

/// <summary>
/// Factory for selecting the appropriate request executor based on request type.
/// </summary>
public interface IRequestExecutorFactory
{
    /// <summary>
    /// Gets the appropriate executor for the given request.
    /// </summary>
    /// <param name="request">The request to execute</param>
    /// <returns>The executor that can handle this request</returns>
    /// <exception cref="NotSupportedException">Thrown when no executor is found for the request type</exception>
    IRequestExecutor GetExecutor(Request request);
}
