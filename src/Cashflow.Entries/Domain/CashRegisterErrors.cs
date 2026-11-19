using Cashflow.Core.Results;

namespace Cashflow.Entries.Domain;

internal static class CashRegisterErrors
{
    public static readonly ResultError StoreRequired = new("cash_register.store_required", "A loja é obrigatória.");
    public static readonly ResultError NotOpen = new("cash_register.not_open", "O caixa não está aberto.");
    public static readonly ResultError AlreadyClosed = new("cash_register.already_closed", "O caixa já está fechado.");
}