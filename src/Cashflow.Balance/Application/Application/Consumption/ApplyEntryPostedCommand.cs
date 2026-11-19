using Cashflow.Balance.Domain;
using Cashflow.Core.Messaging;

namespace Cashflow.Balance.Application.Consumption;

internal sealed record ApplyEntryPostedCommand(
    Guid EventId,
    string StoreId,
    DateOnly BusinessDate,
    EntryType Type,
    decimal Amount) : ICommandRequest<ApplyEntryPostedResponse>;