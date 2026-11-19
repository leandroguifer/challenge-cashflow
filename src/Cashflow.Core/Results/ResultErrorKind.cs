namespace Cashflow.Core.Results;

public enum ResultErrorKind
{
    Validation = 0,
    Forbidden = 1,
    NotFound = 2,
    Conflict = 3,
    Unauthorized = 4,
    Internal = 5
}