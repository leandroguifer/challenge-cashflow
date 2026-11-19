using Cashflow.Balance.Application.Abstractions;
using Cashflow.Balance.Application.Reports;
using Cashflow.Core.Data;
using Cashflow.Core.Security;
using Dapper;
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
               entries_count as EntryCount
        from daily_balances
        where business_date between @StartDate and @EndDate
          and (@StoreId::text is null or store_id = @StoreId)
        order by business_date, store_id
        """;

    private const string ListCompanySql = """
        select business_date as BusinessDate,
               sum(total_credits) as TotalCredits,
               sum(total_debits) as TotalDebits,
               sum(total_credits - total_debits) as Balance,
               sum(entries_count)::int as EntryCount
        from daily_balances
        where business_date between @StartDate and @EndDate
        group by business_date
        order by business_date
        """;

    public async Task<IReadOnlyList<DailyBalanceResponse>> ListAsync(string? storeId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken) =>
        await QueryAsync<DailyBalanceResponse>(ListSql, new { StoreId = storeId, StartDate = startDate, EndDate = endDate }, cancellationToken);

    public async Task<IReadOnlyList<CompanyDailyBalanceResponse>> ListCompanyAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken) =>
        await QueryAsync<CompanyDailyBalanceResponse>(ListCompanySql, new { StartDate = startDate, EndDate = endDate }, cancellationToken);

    private async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object parameters, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await connection.SetStoreAsync(transaction, currentUser.Stores, cancellationToken);

        var items = await connection.QueryAsync<T>(new CommandDefinition(
            sql,
            parameters,
            transaction,
            cancellationToken: cancellationToken));

        await transaction.CommitAsync(cancellationToken);

        return items.AsList();
    }
}