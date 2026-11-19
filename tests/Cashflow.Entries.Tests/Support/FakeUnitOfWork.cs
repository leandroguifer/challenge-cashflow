using Cashflow.Core.Results;
using Cashflow.Entries.Application.Abstractions;

namespace Cashflow.Entries.Tests.Support;

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int BeginCount { get; private set; }
    public int CommitCount { get; private set; }
    public Result CommitResult { get; set; } = Result.Success();

    public Task BeginAsync(CancellationToken cancellationToken)
    {
        BeginCount++;
        return Task.CompletedTask;
    }

    public Task<Result> CommitAsync(CancellationToken cancellationToken)
    {
        CommitCount++;
        return Task.FromResult(CommitResult);
    }
}
