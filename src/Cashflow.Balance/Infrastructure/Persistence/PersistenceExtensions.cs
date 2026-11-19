using Cashflow.Balance.Application.Abstractions;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Cashflow.Balance.Infrastructure.Persistence;

internal static class PersistenceExtensions
{
    public static IServiceCollection AddBalancePersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        var consumerConnectionString = configuration.GetConnectionString("Consumer");
        ArgumentException.ThrowIfNullOrWhiteSpace(consumerConnectionString);

        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());

        services.AddDbContext<BalanceDbContext>(options => options.UseNpgsql(consumerConnectionString));
        services.AddSingleton(_ => NpgsqlDataSource.Create(connectionString));

        services.AddScoped<IDailyBalanceRepository, DailyBalanceRepository>();
        services.AddScoped<IUnitOfWork, BalanceUnitOfWork>();
        services.AddScoped<IDailyBalanceReader, DailyBalanceReader>();

        return services;
    }
}