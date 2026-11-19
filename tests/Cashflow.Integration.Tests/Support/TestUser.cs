namespace Cashflow.Integration.Tests.Support;

internal sealed record TestUser(string UserName, IReadOnlyList<string> Roles, IReadOnlyList<string> Stores)
{
    public const string UserHeader = "X-Test-User";
    public const string RolesHeader = "X-Test-Roles";
    public const string GroupsHeader = "X-Test-Groups";

    public static TestUser Cashier(string store) => new($"caixa.{store.ToLowerInvariant()}", ["caixa"], [store]);

    public static TestUser Assistant(string store) => new($"auxiliar.{store.ToLowerInvariant()}", ["auxiliar"], [store]);

    public static TestUser Owner() => new("proprietario", ["proprietario"], ["LOJA01", "LOJA02", "LOJA03"]);

    public void Apply(HttpRequestMessage request)
    {
        request.Headers.Add(UserHeader, UserName);
        request.Headers.Add(RolesHeader, string.Join(',', Roles));
        request.Headers.Add(GroupsHeader, string.Join(',', Stores.Select(store => $"/Empresa/{store}")));
    }
}
