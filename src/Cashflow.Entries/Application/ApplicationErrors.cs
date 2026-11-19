using Cashflow.Core.Results;

namespace Cashflow.Entries.Application;

internal static class ApplicationErrors
{
    public static readonly ResultError StoreForbidden = new("store.forbidden", "Você não tem acesso a esta loja.", ResultErrorKind.Forbidden);
    public static readonly ResultError CashRegisterNotFound = new("cash_register.not_found", "Não há caixa aberto hoje para esta loja.", ResultErrorKind.NotFound);
    public static readonly ResultError CashRegisterAlreadyOpened = new("cash_register.already_opened", "O caixa de hoje já foi aberto para esta loja.", ResultErrorKind.Conflict);
    public static readonly ResultError ConcurrentConflict = new("request.concurrent_conflict", "A operação conflitou com outra em andamento. Tente novamente.", ResultErrorKind.Conflict);
}