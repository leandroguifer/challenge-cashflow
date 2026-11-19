using Cashflow.Core.Results;
using Cashflow.Core.Security;
using Cashflow.Entries.Application;
using Cashflow.Entries.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace Cashflow.Entries.Infrastructure.Persistence;

internal sealed class EntriesUnitOfWork(EntriesDbContext context, ICurrentUser currentUser) : IUnitOfWork, IAsyncDisposable
{
    private IDbContextTransaction? transaction;

    public async Task BeginAsync(CancellationToken cancellationToken)
    {
        transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var stores = string.Join(',', currentUser.Stores);
        await context.Database.ExecuteSqlInterpolatedAsync($"select set_config('app.stores', {stores}, true)", cancellationToken);
    }

    public async Task<Result> CommitAsync(CancellationToken cancellationToken)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            await transaction!.CommitAsync(cancellationToken);

            return Result.Success();
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return Result.Failure(ApplicationErrors.ConcurrentConflict);
        }
    }

    public ValueTask DisposeAsync() => transaction?.DisposeAsync() ?? ValueTask.CompletedTask;
}