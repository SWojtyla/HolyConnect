using HolyConnect.Domain.Entities;

namespace HolyConnect.Application.Interfaces;

public interface IRequestExecutor
{
    Task<RequestResponse> ExecuteAsync(Request request);
    bool CanExecute(Request request);
}
