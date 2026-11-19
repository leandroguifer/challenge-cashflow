using Cashflow.Core.Messaging;
using Cashflow.Core.Results;
using Cashflow.Core.Security;
using Cashflow.Entries.Application.Abstractions;

namespace Cashflow.Entries.Application.Postings;

internal sealed class ListEntriesHandler(
    IEntryReader reader,
    ICurrentUser currentUser,
    BusinessClock clock) : IRequestHandler<ListEntriesQuery, IReadOnlyList<EntryResponse>>
{
    public async Task<Result<IReadOnlyList<EntryResponse>>> HandleAsync(ListEntriesQuery request, CancellationToken cancellationToken)
    {
        if (!currentUser.Stores.Contains(request.StoreId))
        {
            return Result.Failure<IReadOnlyList<EntryResponse>>(ApplicationErrors.StoreForbidden);
        }

        var entries = await reader.ListAsync(request.StoreId, request.BusinessDate ?? clock.Today, cancellationToken);

        return Result.Success(entries);
    }
}
