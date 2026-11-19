using Cashflow.Entries.Application.Postings;

namespace Cashflow.Entries.Application.Abstractions;

internal interface IEntryReader
{
    Task<IReadOnlyList<EntryResponse>> ListAsync(string storeId, DateOnly businessDate, CancellationToken cancellationToken);
}
