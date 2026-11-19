using System.Security.Claims;
using Cashflow.Core.Security;
using Microsoft.AspNetCore.Http;

namespace Cashflow.Entries.Tests.Core;

[Trait("Category", "Unit")]
public sealed class CurrentUserTests
{
    private static CurrentUser CreateUser(params Claim[] claims)
    {
        var identity = new ClaimsIdentity(claims, "test", "preferred_username", "roles");
        var context = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
        return new CurrentUser(new HttpContextAccessor { HttpContext = context });
    }

    [Fact(DisplayName = "RF10: a loja do usuário vem do último trecho do grupo do Keycloak")]
    [Trait("Requirement", "RF10")]
    public void StoresAreTheLastSegmentOfTheGroup()
    {
        var user = CreateUser(new Claim("groups", "/Empresa/LOJA01"), new Claim("groups", "/Empresa/LOJA02"));

        Assert.Equal(["LOJA01", "LOJA02"], user.Stores);
    }

    [Fact(DisplayName = "RF09: o papel do usuário vem do claim roles do token")]
    [Trait("Requirement", "RF09")]
    public void RolesComeFromRolesClaim()
    {
        var user = CreateUser(new Claim("roles", Roles.Cashier));

        Assert.True(user.IsInRole(Roles.Cashier));
        Assert.False(user.IsInRole(Roles.Owner));
    }
}
