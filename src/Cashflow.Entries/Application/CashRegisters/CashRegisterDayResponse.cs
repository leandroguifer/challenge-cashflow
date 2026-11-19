using Cashflow.Entries.Application.Postings;

namespace Cashflow.Entries.Application.CashRegisters;

internal sealed record CashRegisterDayResponse(
    CashRegisterResponse? CashRegister,
    IReadOnlyList<EntryResponse> Entries);
