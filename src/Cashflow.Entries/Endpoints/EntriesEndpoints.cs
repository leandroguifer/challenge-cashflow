using Cashflow.Core.Messaging;
using Cashflow.Core.Results;
using Cashflow.Core.Security;
using Cashflow.Entries.Application.CashRegisters;
using Cashflow.Entries.Application.Postings;

namespace Cashflow.Entries.Endpoints;

internal static class EntriesEndpoints
{
    public static void MapEntriesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/stores/{storeId}");

        group.MapGet("/cash-register/today", async (string storeId, IDispatcher dispatcher, CancellationToken cancellationToken) =>
            (await dispatcher.SendAsync(new GetCashRegisterDayQuery(storeId), cancellationToken)).ToHttpResult())
            .RequireAuthorization(Policies.EntriesReader);

        group.MapGet("/entries", async (string storeId, DateOnly? date, IDispatcher dispatcher, CancellationToken cancellationToken) =>
            (await dispatcher.SendAsync(new ListEntriesQuery(storeId, date), cancellationToken)).ToHttpResult())
            .RequireAuthorization(Policies.EntriesReader);

        group.MapPost("/cash-register/open", async (string storeId, IDispatcher dispatcher, CancellationToken cancellationToken) =>
            (await dispatcher.SendAsync(new OpenCashRegisterCommand(storeId), cancellationToken)).ToHttpResult())
            .RequireAuthorization(Policies.Cashier);

        group.MapPost("/cash-register/close", async (string storeId, IDispatcher dispatcher, CancellationToken cancellationToken) =>
            (await dispatcher.SendAsync(new CloseCashRegisterCommand(storeId), cancellationToken)).ToHttpResult())
            .RequireAuthorization(Policies.Cashier);

        group.MapPost("/entries", async (string storeId, PostEntryBody body, IDispatcher dispatcher, CancellationToken cancellationToken) =>
            (await dispatcher.SendAsync(new PostEntryCommand(storeId, body.Type, body.Amount, body.Description), cancellationToken)).ToHttpResult())
            .RequireAuthorization(Policies.Cashier);
    }
}