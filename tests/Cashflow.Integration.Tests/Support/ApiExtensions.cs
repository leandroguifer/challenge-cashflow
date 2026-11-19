using System.Net.Http.Json;
using Cashflow.Balance;
using Cashflow.Entries;

namespace Cashflow.Integration.Tests.Support;

internal static class ApiExtensions
{
    public static Task<HttpResponseMessage> OpenCashRegisterAsync(this ApiHost<EntriesModule> host, string store, TestUser? user = null) =>
        host.SendAsync(HttpMethod.Post, $"/api/v1/stores/{store}/cash-register/open", user ?? TestUser.Cashier(store));

    public static Task<HttpResponseMessage> CloseCashRegisterAsync(this ApiHost<EntriesModule> host, string store, TestUser? user = null) =>
        host.SendAsync(HttpMethod.Post, $"/api/v1/stores/{store}/cash-register/close", user ?? TestUser.Cashier(store));

    public static Task<HttpResponseMessage> PostEntryAsync(this ApiHost<EntriesModule> host, string store, string type, decimal amount, string description, TestUser? user = null) =>
        host.SendAsync(HttpMethod.Post, $"/api/v1/stores/{store}/entries", user ?? TestUser.Cashier(store), new { type, amount, description });

    public static Task<HttpResponseMessage> ListEntriesAsync(this ApiHost<EntriesModule> host, string store, TestUser user) =>
        host.SendAsync(HttpMethod.Get, $"/api/v1/stores/{store}/entries", user);

    public static Task<HttpResponseMessage> DailyBalancesRawAsync(this ApiHost<BalanceModule> host, TestUser? user, string? store, DateOnly start, DateOnly end)
    {
        var query = $"startDate={start:yyyy-MM-dd}&endDate={end:yyyy-MM-dd}" + (store is null ? string.Empty : $"&storeId={store}");
        return host.SendAsync(HttpMethod.Get, $"/api/v1/daily-balances?{query}", user);
    }

    public static Task<HttpResponseMessage> CompanyBalancesRawAsync(this ApiHost<BalanceModule> host, TestUser? user, DateOnly start, DateOnly end) =>
        host.SendAsync(HttpMethod.Get, $"/api/v1/company/daily-balances?startDate={start:yyyy-MM-dd}&endDate={end:yyyy-MM-dd}", user);

    public static async Task<IReadOnlyList<DailyBalanceDto>> DailyBalancesAsync(this ApiHost<BalanceModule> host, TestUser user, string? store, DateOnly day)
    {
        using var response = await host.DailyBalancesRawAsync(user, store, day, day);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<DailyBalanceDto>>() ?? [];
    }

    public static async Task<IReadOnlyList<CompanyBalanceDto>> CompanyBalancesAsync(this ApiHost<BalanceModule> host, TestUser user, DateOnly day)
    {
        using var response = await host.CompanyBalancesRawAsync(user, day, day);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<CompanyBalanceDto>>() ?? [];
    }
}
