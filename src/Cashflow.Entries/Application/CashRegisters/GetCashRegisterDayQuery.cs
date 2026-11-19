using Cashflow.Core.Messaging;

namespace Cashflow.Entries.Application.CashRegisters;

internal sealed record GetCashRegisterDayQuery(string StoreId) : IQueryRequest<CashRegisterDayResponse>;
