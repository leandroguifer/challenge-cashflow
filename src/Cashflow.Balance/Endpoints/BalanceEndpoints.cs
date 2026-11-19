using Cashflow.Balance.Application.Reports;
using Cashflow.Core.Messaging;
using Cashflow.Core.Results;
using Cashflow.Core.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Cashflow.Balance.Endpoints;

internal static class BalanceEndpoints
{
    public static void MapBalanceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/daily-balances", async (string? storeId, DateOnly startDate, DateOnly endDate, IDispatcher dispatcher, CancellationToken cancellationToken) =>
            (await dispatcher.SendAsync(new ListDailyBalancesQuery(storeId, startDate, endDate), cancellationToken)).ToHttpResult())
            .RequireAuthorization(Policies.BalanceReader);

        endpoints.MapGet("/api/v1/company/daily-balances", async (DateOnly startDate, DateOnly endDate, IDispatcher dispatcher, CancellationToken cancellationToken) =>
            (await dispatcher.SendAsync(new GetCompanyDailyBalancesQuery(startDate, endDate), cancellationToken)).ToHttpResult())
            .RequireAuthorization(Policies.BalanceReader);
    }
}