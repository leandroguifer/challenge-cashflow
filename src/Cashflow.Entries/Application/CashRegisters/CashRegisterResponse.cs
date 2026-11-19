using Cashflow.Entries.Domain;

namespace Cashflow.Entries.Application.CashRegisters;

internal sealed record CashRegisterResponse(
    Guid Id,
    string StoreId,
    DateOnly BusinessDate,
    CashRegisterStatus Status,
    DateTimeOffset OpenedAt,
    DateTimeOffset? ClosedAt)
{
    public static CashRegisterResponse From(CashRegister cashRegister) => new(
        cashRegister.Id,
        cashRegister.StoreId,
        cashRegister.BusinessDate,
        cashRegister.Status,
        cashRegister.OpenedAt,
        cashRegister.ClosedAt);
}