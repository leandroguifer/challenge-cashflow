using Cashflow.Core.Results;

namespace Cashflow.Core.Messaging;

public interface IDispatcher
{
    Task<Result<TResponse>> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}
