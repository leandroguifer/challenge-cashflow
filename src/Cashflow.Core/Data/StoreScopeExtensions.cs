using System.Data.Common;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Cashflow.Core.Data;

public static class StoreScopeExtensions
{
    private const string SetStoresSql = "select set_config('app.stores', @value, true)";

    public static async Task SetStoreAsync(this DbContext dbContext, IEnumerable<string> stores, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(stores);

        var value = string.Join(',', stores);
        await dbContext.Database.ExecuteSqlInterpolatedAsync($"select set_config('app.stores', {value}, true)", cancellationToken);
    }

    public static async Task SetStoreAsync(this DbConnection dbConnection, DbTransaction transaction, IEnumerable<string> stores, CancellationToken cancellationToken = default)
    {
        var command =  new CommandDefinition(SetStoresSql, new { value = string.Join(',', stores) }, transaction, cancellationToken: cancellationToken);
        await dbConnection.ExecuteAsync(command);
    }

}