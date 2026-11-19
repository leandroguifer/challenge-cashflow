using Cashflow.Core.Messaging;

namespace Cashflow.Entries.Application.CashRegisters;

internal sealed record CloseCashRegisterCommand(string StoreId) : ICommandRequest<CashRegisterResponse>;