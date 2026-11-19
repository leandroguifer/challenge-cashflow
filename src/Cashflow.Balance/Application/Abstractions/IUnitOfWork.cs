namespace Cashflow.Balance.Application.Abstractions;

internal interface IUnitOfWork
{
    Task BeginAsync(CancellationToken cancellationToken);

    Task CommitAsync(CancellationToken cancellationToken);
}