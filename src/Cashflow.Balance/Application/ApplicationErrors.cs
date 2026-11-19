using Cashflow.Core.Results;

namespace Cashflow.Balance.Application;

internal static class ApplicationErrors
{
    public static readonly ResultError StoreForbidden = new("store.forbidden", "Você não tem acesso a esta loja.", ResultErrorKind.Forbidden);
    public static readonly ResultError InvalidPeriod = new("period.invalid", "A data inicial deve ser menor ou igual à data final.");
}