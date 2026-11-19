using Cashflow.Core.Results;

namespace Cashflow.Entries.Domain;

internal static class EntryErrors
{
    public static readonly ResultError InvalidAmount = new("entry.invalid_amount", "O valor deve ser maior que zero.");
    public static readonly ResultError StoreRequired = new("entry.store_required", "A loja é obrigatória.");
    public static readonly ResultError DescriptionTooShort = new("entry.description_too_short", $"A descrição deve ter no mínimo {Entry.MinDescriptionLength} caracteres.");
    public static readonly ResultError DescriptionTooLong = new("entry.description_too_long", $"A descrição deve ter no máximo {Entry.MaxDescriptionLength} caracteres.");
}