using Cashflow.Balance.Application.Abstractions;

namespace Cashflow.Balance.Tests.Support;

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int BeginCount { get; private set; }
    public int CommitCount { get; private set; }

    public Task BeginAsync(CancellationToken cancellationToken)
    {
        BeginCount++;
        return Task.CompletedTask;
    }

    public Task CommitAsync(CancellationToken cancellationToken)
    {
        CommitCount++;
        return Task.CompletedTask;
    }
}
