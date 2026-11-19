using Cashflow.Balance.Application.Abstractions;
using Cashflow.Balance.Application.Reports;
using Cashflow.Core.Security;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Cashflow.Balance.Infrastructure.Persistence;

internal sealed class DailyBalanceReader(NpgsqlDataSource dataSource, ICurrentUser currentUser) : IDailyBalanceReader
{
    private const string ListSql = """
        select store_id as StoreId,
               business_date as BusinessDate,
               total_credits as TotalCredits,
               total_debits as TotalDebits,
               total_credits - total_debits as Balance,
               entry_count as EntryCount
        from daily_balances
        where business_date between @StartDate and @EndDate
          and (@StoreId::text is null or store_id = @StoreId)
        order by business_date, store_id
        """;

    public async Task<IReadOnlyList<DailyBalanceResponse>> ListAsync(string? storeId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await connection.ExecuteAsync(new CommandDefinition(
            "select set_config('app.stores', @Stores, true)",
            new { Stores = string.Join(',', currentUser.Stores) },
            transaction,
            cancellationToken: cancellationToken));

        var items = await connection.QueryAsync<DailyBalanceResponse>(new CommandDefinition(
            ListSql,
            new { StoreId = storeId, StartDate = startDate, EndDate = endDate },
            transaction,
            cancellationToken: cancellationToken));

        await transaction.CommitAsync(cancellationToken);

        return items.AsList();
    }
}