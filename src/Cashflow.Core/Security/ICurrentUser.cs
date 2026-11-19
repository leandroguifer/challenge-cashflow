namespace Cashflow.Core.Security;

public interface ICurrentUser
{
    string UserName { get; }
    IReadOnlyList<string> Stores { get; }
    bool IsAuthenticated { get; }
    bool IsInRole (string role);
}