using Cashflow.Core.Data;
using Cashflow.Core.Security;
using Cashflow.Entries.Application.Abstractions;
using Cashflow.Entries.Application.CashRegisters;
using Cashflow.Entries.Application.Postings;
using Dapper;
using Npgsql;

namespace Cashflow.Entries.Infrastructure.Persistence;

internal sealed class CashRegisterDayReader(NpgsqlDataSource dataSource, ICurrentUser currentUser) : ICashRegisterDayReader
{
    private const string CashRegisterSql = """
        select id as Id,
               store_id as StoreId,
               business_date as BusinessDate,
               status as Status,
               opened_at as OpenedAt,
               closed_at as ClosedAt
        from cash_registers
        where store_id = @StoreId
          and business_date = @BusinessDate
        """;

    public async Task<CashRegisterDayResponse> GetAsync(string storeId, DateOnly businessDate, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await connection.SetStoreAsync(transaction, currentUser.Stores, cancellationToken);

        var parameters = new { StoreId = storeId, BusinessDate = businessDate };

        var cashRegister = await connection.QuerySingleOrDefaultAsync<CashRegisterResponse>(new CommandDefinition(
            CashRegisterSql,
            parameters,
            transaction,
            cancellationToken: cancellationToken));

        if (cashRegister is null)
        {
            await transaction.CommitAsync(cancellationToken);
            return new CashRegisterDayResponse(null, []);
        }

        var entries = await connection.QueryAsync<EntryResponse>(new CommandDefinition(
            EntrySql.ListByStoreAndDate,
            parameters,
            transaction,
            cancellationToken: cancellationToken));

        await transaction.CommitAsync(cancellationToken);

        return new CashRegisterDayResponse(cashRegister, entries.AsList());
    }
}
