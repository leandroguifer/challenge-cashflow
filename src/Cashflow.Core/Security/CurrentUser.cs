using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Cashflow.Core.Security;

public class CurrentUser : ICurrentUser
{
    private ClaimsPrincipal? Principal;
    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public IReadOnlyList<string> Stores
    {
        get
        {
            if(Principal is null) return [];

            return Principal.FindAll("groups")
                .Select(c => StoreFromGroup(c.Value))
                .OfType<string>()
                .Distinct()
                .ToList();
        }
    }

    public CurrentUser(IHttpContextAccessor accessor)
    {
        Principal = accessor.HttpContext?.User;
    }

    private static string? StoreFromGroup(string group)
    {
        var parts = group.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2 ? parts[^1] : null;
    }

    public bool IsInRole(string role) => Principal?.IsInRole(role) ?? false;
}