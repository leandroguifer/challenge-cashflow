using System.Reflection;
using System.Runtime.CompilerServices;
using Cashflow.Architecture.Tests.Support;
using Cashflow.Core.Messaging;
using Microsoft.EntityFrameworkCore;
using NetArchTest.Rules;

namespace Cashflow.Architecture.Tests;

[Trait("Category", "Unit")]
public sealed class ArchitectureTests
{
    private static readonly string[] WriteSideSuffixes = ["Repository", "UnitOfWork"];

    [Theory(DisplayName = "Arquitetura: o domínio não depende de EF Core, Dapper, Npgsql nem ASP.NET")]
    [Trait("Requirement", "Arquitetura")]
    [InlineData(Services.Entries)]
    [InlineData(Services.Balance)]
    public void DomainHasNoInfrastructureDependency(string service)
    {
        var result = Types.InAssembly(Services.Assembly(service))
            .That().ResideInNamespace($"{Services.RootNamespace(service)}.Domain")
            .ShouldNot().HaveDependencyOnAny("Microsoft.EntityFrameworkCore", "Dapper", "Npgsql", "Microsoft.AspNetCore")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Theory(DisplayName = "RNF01: Lançamentos e Consolidado não têm ProjectReference um para o outro")]
    [Trait("Requirement", "RNF01")]
    [InlineData(Services.Entries)]
    [InlineData(Services.Balance)]
    public void ServicesDoNotReferenceEachOther(string service)
    {
        var project = Path.Combine(Repository.Root, "src", $"Cashflow.{service}", $"Cashflow.{service}.csproj");

        var content = File.ReadAllText(project);

        Assert.DoesNotContain($"Cashflow.{Services.Other(service)}.csproj", content, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "RNF01: o Core compartilhado não depende de nenhum dos serviços")]
    [Trait("Requirement", "RNF01")]
    public void CoreDoesNotDependOnServices()
    {
        var result = Types.InAssembly(typeof(Cashflow.Core.Results.Result).Assembly)
            .ShouldNot().HaveDependencyOnAny("Cashflow.Entries", "Cashflow.Balance")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Theory(DisplayName = "RNF02: handlers de consulta não usam DbContext nem repositório de escrita")]
    [Trait("Requirement", "RNF02")]
    [InlineData(Services.Entries)]
    [InlineData(Services.Balance)]
    public void QueryHandlersDoNotUseDbContextOrRepositories(string service)
    {
        var queryHandlers = Services.Assembly(service).GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && IsQueryHandler(type))
            .ToList();

        var violations = queryHandlers
            .Where(handler => handler.GetConstructors()
                .SelectMany(constructor => constructor.GetParameters())
                .Select(parameter => parameter.ParameterType)
                .Any(dependency => typeof(DbContext).IsAssignableFrom(dependency)
                    || WriteSideSuffixes.Any(suffix => dependency.Name.EndsWith(suffix, StringComparison.Ordinal))))
            .Select(handler => handler.FullName)
            .ToList();

        Assert.NotEmpty(queryHandlers);
        Assert.True(violations.Count == 0, string.Join(", ", violations));
    }

    [Theory(DisplayName = "Arquitetura: tipos concretos dos serviços são internal sealed")]
    [Trait("Requirement", "Arquitetura")]
    [InlineData(Services.Entries)]
    [InlineData(Services.Balance)]
    public void ConcreteTypesAreInternalSealed(string service)
    {
        var violations = Services.Assembly(service).GetTypes()
            .Where(type => !type.IsNested
                && (type.Namespace ?? string.Empty).StartsWith(Services.RootNamespace(service), StringComparison.Ordinal)
                && !(type.Namespace ?? string.Empty).EndsWith(".Migrations", StringComparison.Ordinal)
                && type.Name != "Program"
                && type.GetCustomAttribute<CompilerGeneratedAttribute>() is null)
            .Where(type => type.IsPublic || (type is { IsClass: true, IsAbstract: false } && !type.IsSealed))
            .Select(type => type.FullName)
            .ToList();

        Assert.True(violations.Count == 0, string.Join(", ", violations));
    }

    private static bool IsQueryHandler(Type type) =>
        type.GetInterfaces()
            .Where(candidate => candidate.IsGenericType && candidate.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
            .Select(candidate => candidate.GetGenericArguments()[0])
            .Any(request => request.GetInterfaces().Any(candidate => candidate.IsGenericType && candidate.GetGenericTypeDefinition() == typeof(IQueryRequest<>)));
}
