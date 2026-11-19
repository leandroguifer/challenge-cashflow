using Cashflow.Core.Security;

namespace Cashflow.Integration.Tests.Support;

internal sealed class TestCurrentUser(string userName, params string[] stores) : ICurrentUser
{
    public string UserName => userName;
    public IReadOnlyList<string> Stores => stores;
    public bool IsAuthenticated => true;
    public bool IsInRole(string role) => false;
}
