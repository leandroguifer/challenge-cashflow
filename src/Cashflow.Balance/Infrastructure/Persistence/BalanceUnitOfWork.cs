using Cashflow.Balance.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Cashflow.Balance.Infrastructure.Persistence;

internal sealed class BalanceUnitOfWork(BalanceDbContext context) : IUnitOfWork, IAsyncDisposable
{
    private IDbContextTransaction? transaction;

    public async Task BeginAsync(CancellationToken cancellationToken) =>
        transaction = await context.Database.BeginTransactionAsync(cancellationToken);

    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        await context.SaveChangesAsync(cancellationToken);
        await transaction!.CommitAsync(cancellationToken);
    }

    public ValueTask DisposeAsync() => transaction?.DisposeAsync() ?? ValueTask.CompletedTask;
}