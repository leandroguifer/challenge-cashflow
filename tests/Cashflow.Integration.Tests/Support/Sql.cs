using Npgsql;

namespace Cashflow.Integration.Tests.Support;

internal static class Sql
{
    public static async Task<T?> ScalarAsync<T>(string connectionString, string sql, IReadOnlyList<string>? stores = null, params (string Name, object? Value)[] parameters)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        await SetStoresAsync(connection, transaction, stores);

        await using var command = Build(connection, transaction, sql, parameters);
        var value = await command.ExecuteScalarAsync();
        await transaction.CommitAsync();

        return value is null or DBNull ? default : (T)Convert.ChangeType(value, Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T), System.Globalization.CultureInfo.InvariantCulture);
    }

    public static async Task<int> ExecuteAsync(string connectionString, string sql, IReadOnlyList<string>? stores = null, params (string Name, object? Value)[] parameters)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        await SetStoresAsync(connection, transaction, stores);

        await using var command = Build(connection, transaction, sql, parameters);
        var affected = await command.ExecuteNonQueryAsync();
        await transaction.CommitAsync();

        return affected;
    }

    private static async Task SetStoresAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, IReadOnlyList<string>? stores)
    {
        if (stores is null)
        {
            return;
        }

        await using var command = new NpgsqlCommand("select set_config('app.stores', @stores, true)", connection, transaction);
        command.Parameters.AddWithValue("stores", string.Join(',', stores));
        await command.ExecuteNonQueryAsync();
    }

    private static NpgsqlCommand Build(NpgsqlConnection connection, NpgsqlTransaction transaction, string sql, (string Name, object? Value)[] parameters)
    {
        var command = new NpgsqlCommand(sql, connection, transaction);

        foreach (var (name, value) in parameters)
        {
            command.Parameters.AddWithValue(name, value ?? DBNull.Value);
        }

        return command;
    }
}
