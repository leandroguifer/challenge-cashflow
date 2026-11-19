using Cashflow.Core.Messaging;

namespace Cashflow.Entries.Application.CashRegisters;

internal sealed record OpenCashRegisterCommand(string StoreId) : ICommandRequest<CashRegisterResponse>;