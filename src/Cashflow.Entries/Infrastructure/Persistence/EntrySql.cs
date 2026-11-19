namespace Cashflow.Entries.Infrastructure.Persistence;

internal static class EntrySql
{
    public const string ListByStoreAndDate = """
        select id as Id,
               cash_register_id as CashRegisterId,
               store_id as StoreId,
               business_date as BusinessDate,
               type as Type,
               amount as Amount,
               description as Description,
               created_at as CreatedAt
        from entries
        where store_id = @StoreId
          and business_date = @BusinessDate
        order by created_at, id
        """;
}
