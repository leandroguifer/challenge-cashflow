using System.Net;
using System.Text.Json;
using Cashflow.Integration.Tests.Support;

namespace Cashflow.Integration.Tests;

[Trait("Category", "Integration")]
[Collection(SequentialServices.Name)]
public sealed class AuthorizationTests(InfrastructureFixture infrastructure) : ServicesTestBase(infrastructure)
{
    private static void AssertProblem(HttpResponseMessage response, HttpStatusCode status)
    {
        Assert.Equal(status, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    private static async Task<string?> ProblemCodeAsync(HttpResponseMessage response)
    {
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        return document.RootElement.TryGetProperty("code", out var code) ? code.GetString() : null;
    }

    [Fact(DisplayName = "RF09: Lançamentos sem token responde 401")]
    [Trait("Requirement", "RF09")]
    public async Task EntriesRequireAuthentication()
    {
        var entries = await StartEntriesAsync();

        using var open = await entries.SendAsync(HttpMethod.Post, "/api/v1/stores/LOJA01/cash-register/open");
        using var list = await entries.SendAsync(HttpMethod.Get, "/api/v1/stores/LOJA01/entries");

        AssertProblem(open, HttpStatusCode.Unauthorized);
        AssertProblem(list, HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "RF09 e RF11: Consolidado sem token responde 401 e o caixa não consulta saldo (403)")]
    [Trait("Requirement", "RF09")]
    [Trait("Requirement", "RF11")]
    public async Task BalanceRequiresAuthenticationAndRole()
    {
        var balance = await StartBalanceAsync();
        var today = BusinessDay.Today();

        using var anonymous = await balance.DailyBalancesRawAsync(null, null, today, today);
        using var cashier = await balance.DailyBalancesRawAsync(TestUser.Cashier("LOJA01"), "LOJA01", today, today);
        using var cashierCompany = await balance.CompanyBalancesRawAsync(TestUser.Cashier("LOJA01"), today, today);

        AssertProblem(anonymous, HttpStatusCode.Unauthorized);
        AssertProblem(cashier, HttpStatusCode.Forbidden);
        AssertProblem(cashierCompany, HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "RF11: só o caixa abre, lança e fecha; auxiliar e proprietário recebem 403")]
    [Trait("Requirement", "RF11")]
    public async Task OnlyTheCashierOperatesTheCashRegister()
    {
        var entries = await StartEntriesAsync();

        foreach (var user in new[] { TestUser.Assistant("LOJA01"), TestUser.Owner() })
        {
            using var open = await entries.OpenCashRegisterAsync("LOJA01", user);
            using var post = await entries.PostEntryAsync("LOJA01", "Credit", 10m, "Venda no balcão", user);
            using var close = await entries.CloseCashRegisterAsync("LOJA01", user);

            AssertProblem(open, HttpStatusCode.Forbidden);
            AssertProblem(post, HttpStatusCode.Forbidden);
            AssertProblem(close, HttpStatusCode.Forbidden);
        }

        using var opened = await entries.OpenCashRegisterAsync("LOJA01");

        Assert.Equal(HttpStatusCode.OK, opened.StatusCode);
    }

    [Fact(DisplayName = "RF10: o caixa de uma loja não opera outra loja (403)")]
    [Trait("Requirement", "RF10")]
    public async Task CashierCannotOperateAnotherStore()
    {
        var entries = await StartEntriesAsync();
        var cashierOfStoreTwo = TestUser.Cashier("LOJA02");

        using var open = await entries.OpenCashRegisterAsync("LOJA01", cashierOfStoreTwo);
        using var post = await entries.PostEntryAsync("LOJA01", "Credit", 10m, "Venda no balcão", cashierOfStoreTwo);

        AssertProblem(open, HttpStatusCode.Forbidden);
        AssertProblem(post, HttpStatusCode.Forbidden);
        Assert.Equal("store.forbidden", await ProblemCodeAsync(open));
    }

    [Fact(DisplayName = "RF10: auxiliar consulta só a própria loja e o proprietário consulta todas")]
    [Trait("Requirement", "RF10")]
    public async Task ReadersSeeOnlyTheirStores()
    {
        var entries = await StartEntriesAsync();
        await entries.OpenCashRegisterAsync("LOJA01");

        using var assistantOwn = await entries.ListEntriesAsync("LOJA01", TestUser.Assistant("LOJA01"));
        using var assistantOther = await entries.ListEntriesAsync("LOJA02", TestUser.Assistant("LOJA01"));
        using var ownerOther = await entries.ListEntriesAsync("LOJA02", TestUser.Owner());

        Assert.Equal(HttpStatusCode.OK, assistantOwn.StatusCode);
        AssertProblem(assistantOther, HttpStatusCode.Forbidden);
        Assert.Equal(HttpStatusCode.OK, ownerOther.StatusCode);
    }
}
