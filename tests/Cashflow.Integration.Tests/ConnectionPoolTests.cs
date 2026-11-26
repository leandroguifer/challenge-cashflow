using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using Cashflow.Entries;
using Cashflow.Integration.Tests.Support;
using Npgsql;

namespace Cashflow.Integration.Tests;

[Trait("Category", "Integration")]
[Collection(SequentialServices.Name)]
public sealed class ConnectionPoolTests(InfrastructureFixture infrastructure) : ServicesTestBase(infrastructure)
{
    private const string Store = "LOJA03";
    private const int ConcurrentEntries = 100;
    private const int NpgsqlDefaultPoolSize = 100;

    [Fact(DisplayName = "RNF01: com o Consolidado parado, 100 lançamentos simultâneos são aceitos sem esgotar as conexões do banco")]
    [Trait("Requirement", "RNF01")]
    public async Task ConcurrentEntriesDoNotExhaustDatabaseConnections()
    {
        NpgsqlConnection.ClearAllPools();

        var settings = new Dictionary<string, string>(ServiceSettings.Entries(Infrastructure, Databases));
        ApplyPoolSize(settings, "ConnectionStrings__Default", "ENTRIES_DB_DEFAULT");
        ApplyPoolSize(settings, "ConnectionStrings__Migrations", "ENTRIES_DB_MIGRATIONS");
        ApplyPoolSize(settings, "ConnectionStrings__Publisher", "ENTRIES_DB_PUBLISHER");

        await using var entries = await ApiHost<EntriesModule>.StartAsync(settings);

        using var opened = await entries.OpenCashRegisterAsync(Store);
        Assert.Equal(HttpStatusCode.OK, opened.StatusCode);

        var statuses = await Task.WhenAll(Enumerable.Range(1, ConcurrentEntries).Select(async index =>
        {
            using var response = await entries.PostEntryAsync(Store, "Credit", 1m, $"Lançamento simultâneo {index}");
            return response.StatusCode;
        }));

        Assert.All(statuses, status => Assert.Equal(HttpStatusCode.OK, status));
        Assert.Equal(ConcurrentEntries, await Sql.ScalarAsync<int>(Databases.EntriesMigrations, "select count(*) from entries"));
    }

    private static void ApplyPoolSize(Dictionary<string, string> settings, string setting, string variable)
    {
        var line = Repository.ReadText(".env.example")
            .Split('\n')
            .Single(item => item.StartsWith(variable + "=", StringComparison.Ordinal));
        var match = Regex.Match(line, @"Maximum Pool Size=(\d+)");

        settings[setting] = new NpgsqlConnectionStringBuilder(settings[setting])
        {
            MaxPoolSize = match.Success ? int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) : NpgsqlDefaultPoolSize
        }.ConnectionString;
    }
}
