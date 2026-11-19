using System.Text.Json;
using Cashflow.Core.Security;
using Cashflow.Entries.Application;
using Cashflow.Entries.Application.Abstractions;
using Cashflow.Entries.Application.CashRegisters;
using Cashflow.Entries.Application.Postings;
using Cashflow.Entries.Domain;
using Cashflow.Entries.Infrastructure.Outbox;
using Cashflow.Entries.Infrastructure.Persistence;
using Cashflow.Integration.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cashflow.Integration.Tests;

[Trait("Category", "Integration")]
public sealed class OutboxAtomicityTests(InfrastructureFixture infrastructure) : DatabaseTestBase(infrastructure)
{
    private const string Store = "LOJA01";

    private ServiceProvider BuildServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton(sp => new BusinessClock(sp.GetRequiredService<TimeProvider>(), new ConfigurationBuilder().Build()));
        services.AddSingleton<ICurrentUser>(new TestCurrentUser("caixa.loja01", Store));
        services.AddDbContext<EntriesDbContext>(options => options.UseNpgsql(Databases.EntriesAs(TestCredentials.EntriesApp)));
        services.AddScoped<ICashRegisterRepository, CashRegisterRepository>();
        services.AddScoped<IUnitOfWork, EntriesUnitOfWork>();
        services.AddScoped<IOutbox, OutboxWriter>();
        services.AddScoped<OpenCashRegisterHandler>();
        services.AddScoped<PostEntryHandler>();

        return services.BuildServiceProvider();
    }

    private static async Task OpenCashRegisterAsync(ServiceProvider provider)
    {
        await using var scope = provider.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<OpenCashRegisterHandler>()
            .HandleAsync(new OpenCashRegisterCommand(Store), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    private static async Task<Cashflow.Core.Results.Result<EntryResponse>> PostEntryAsync(ServiceProvider provider)
    {
        await using var scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<PostEntryHandler>()
            .HandleAsync(new PostEntryCommand(Store, EntryType.Credit, 150m, "Troca de óleo"), CancellationToken.None);
    }

    private Task<int> CountAsync(string table) =>
        Sql.ScalarAsync<int>(Databases.EntriesMigrations, $"select count(*) from {table}");

    [Fact(DisplayName = "RNF05: o lançamento e o evento do outbox são gravados juntos")]
    [Trait("Requirement", "RNF05")]
    public async Task EntryAndOutboxMessageAreStoredTogether()
    {
        await using var provider = BuildServices();
        await OpenCashRegisterAsync(provider);

        var result = await PostEntryAsync(provider);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, await CountAsync("entries"));
        Assert.Equal(1, await CountAsync("outbox_messages"));

        var payload = await Sql.ScalarAsync<string>(Databases.EntriesMigrations, "select payload::text from outbox_messages");
        using var document = JsonDocument.Parse(payload!);
        Assert.Equal(result.Value!.Id, document.RootElement.GetProperty("entryId").GetGuid());
        Assert.Equal("Credit", document.RootElement.GetProperty("type").GetString());
        Assert.Equal(150m, document.RootElement.GetProperty("amount").GetDecimal());
    }

    [Fact(DisplayName = "RNF05: se a gravação do evento falha, o lançamento também não fica gravado")]
    [Trait("Requirement", "RNF05")]
    public async Task OutboxFailureLeavesNoEntry()
    {
        await using var provider = BuildServices();
        await OpenCashRegisterAsync(provider);
        await Sql.ExecuteAsync(Databases.EntriesMigrations, "alter table outbox_messages add constraint reject_every_event check (false)");

        await Assert.ThrowsAsync<DbUpdateException>(() => PostEntryAsync(provider));

        Assert.Equal(0, await CountAsync("entries"));
        Assert.Equal(0, await CountAsync("outbox_messages"));
    }
}
