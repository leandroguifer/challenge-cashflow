namespace Cashflow.Core.Security;

public interface ICurrentUser
{
    IReadOnlyList<string> Stores { get; }
    bool IsAuthenticated { get; }
    bool IsInRole (string role);
}