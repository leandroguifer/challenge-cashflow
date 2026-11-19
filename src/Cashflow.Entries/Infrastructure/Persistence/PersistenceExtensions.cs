using Cashflow.Core.Data;
using Cashflow.Entries.Application.Abstractions;
using Cashflow.Entries.Infrastructure.Outbox;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Cashflow.Entries.Infrastructure.Persistence;

internal static class PersistenceExtensions
{
    public static IServiceCollection AddEntriesPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");
        ArgumentNullException.ThrowIfNull(connectionString, "Connection string 'Default' not found.");

        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
        SqlMapper.AddTypeHandler(new DateTimeOffsetTypeHandler());

        services.AddDbContext<EntriesDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddSingleton(_ => NpgsqlDataSource.Create(connectionString));

        services.AddScoped<ICashRegisterRepository, CashRegisterRepository>();
        services.AddScoped<IUnitOfWork, EntriesUnitOfWork>();
        services.AddScoped<IEntryReader, EntryReader>();
        services.AddScoped<ICashRegisterDayReader, CashRegisterDayReader>();
        services.AddScoped<IOutbox, OutboxWriter>();

        return services;
    }
}