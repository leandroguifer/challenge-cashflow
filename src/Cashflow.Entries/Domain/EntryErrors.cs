using Cashflow.Core.Results;

namespace Cashflow.Entries.Domain;

internal static class EntryErrors
{
    public static readonly ResultError InvalidAmount = new("entry.invalid_amount", "O valor deve ser maior que zero.");
    public static readonly ResultError StoreRequired = new("entry.store_required", "A loja é obrigatória.");
}