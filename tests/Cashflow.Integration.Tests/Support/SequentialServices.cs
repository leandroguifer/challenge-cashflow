namespace Cashflow.Integration.Tests.Support;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class SequentialServices
{
    public const string Name = "Services";
}
