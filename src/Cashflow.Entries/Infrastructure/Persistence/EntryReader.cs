using Cashflow.Core.Data;
using Cashflow.Core.Security;
using Cashflow.Entries.Application.Abstractions;
using Cashflow.Entries.Application.Postings;
using Dapper;
using Npgsql;

namespace Cashflow.Entries.Infrastructure.Persistence;

internal sealed class EntryReader(NpgsqlDataSource dataSource, ICurrentUser currentUser) : IEntryReader
{
    public async Task<IReadOnlyList<EntryResponse>> ListAsync(string storeId, DateOnly businessDate, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await connection.SetStoreAsync(transaction, currentUser.Stores, cancellationToken);

        var items = await connection.QueryAsync<EntryResponse>(new CommandDefinition(
            EntrySql.ListByStoreAndDate,
            new { StoreId = storeId, BusinessDate = businessDate },
            transaction,
            cancellationToken: cancellationToken));

        await transaction.CommitAsync(cancellationToken);

        return items.AsList();
    }
}
