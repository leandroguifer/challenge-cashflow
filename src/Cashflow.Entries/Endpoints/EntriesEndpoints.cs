using Cashflow.Core.Messaging;
using Cashflow.Core.Results;
using Cashflow.Entries.Application.CashRegisters;
using Cashflow.Entries.Application.Postings;

namespace Cashflow.Entries.Endpoints;

internal static class EntriesEndpoints
{
    public static void MapEntriesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/stores/{storeId}");

        group.MapGet("/cash-register/today", async (string storeId, IDispatcher dispatcher, CancellationToken cancellationToken) =>
            (await dispatcher.SendAsync(new GetCashRegisterDayQuery(storeId), cancellationToken)).ToHttpResult());

        group.MapPost("/cash-register/open", async (string storeId, IDispatcher dispatcher, CancellationToken cancellationToken) =>
            (await dispatcher.SendAsync(new OpenCashRegisterCommand(storeId), cancellationToken)).ToHttpResult());

        group.MapPost("/cash-register/close", async (string storeId, IDispatcher dispatcher, CancellationToken cancellationToken) =>
            (await dispatcher.SendAsync(new CloseCashRegisterCommand(storeId), cancellationToken)).ToHttpResult());

        group.MapPost("/entries", async (string storeId, PostEntryBody body, IDispatcher dispatcher, CancellationToken cancellationToken) =>
            (await dispatcher.SendAsync(new PostEntryCommand(storeId, body.Type, body.Amount, body.Description), cancellationToken)).ToHttpResult());
    }
}