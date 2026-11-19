using Cashflow.Core.Messaging;

namespace Cashflow.Entries.Application.Postings;

internal sealed record ListEntriesQuery(string StoreId, DateOnly? BusinessDate) : IQueryRequest<IReadOnlyList<EntryResponse>>;
