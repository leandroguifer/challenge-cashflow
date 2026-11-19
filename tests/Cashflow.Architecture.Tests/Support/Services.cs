using System.Reflection;
using Cashflow.Balance;
using Cashflow.Entries;

namespace Cashflow.Architecture.Tests.Support;

internal static class Services
{
    public const string Entries = "Entries";
    public const string Balance = "Balance";

    public static Assembly Assembly(string service) => service switch
    {
        Entries => typeof(EntriesModule).Assembly,
        Balance => typeof(BalanceModule).Assembly,
        _ => throw new ArgumentOutOfRangeException(nameof(service), service, null)
    };

    public static string RootNamespace(string service) => $"Cashflow.{service}";

    public static string Other(string service) => service == Entries ? Balance : Entries;
}
