using Cashflow.Core.Results;

namespace Cashflow.Core.Messaging;

public interface IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<Result<TResponse>> HandleAsync(TRequest request, CancellationToken cancellationToken);
}
