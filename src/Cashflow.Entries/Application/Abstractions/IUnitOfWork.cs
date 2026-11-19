using Cashflow.Core.Results;

namespace Cashflow.Entries.Application.Abstractions;

internal interface IUnitOfWork
{
    Task BeginAsync(CancellationToken cancellationToken);

    Task<Result> CommitAsync(CancellationToken cancellationToken);
}