using Cashflow.Core.Results;

namespace Cashflow.Balance.Domain;

internal static class DailyBalanceErrors
{
    public static readonly ResultError StoreRequired = new("daily_balance.store_required", "A loja é obrigatória.");
    public static readonly ResultError InvalidAmount = new("daily_balance.invalid_amount", "O valor deve ser maior que zero.");
}