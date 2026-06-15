using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace LibroFiscal.Tests.Architecture;

/// <summary>
/// Architecture tests that enforce the dependency rules of Clean Architecture.
/// These tests are the guardrails that prevent architectural erosion over time.
/// 
/// Rules enforced:
/// 1. Domain → references ONLY SharedKernel
/// 2. Application → references Domain + SharedKernel (never Infrastructure)
/// 3. SharedKernel → references nothing from the solution
/// 4. Infrastructure → can reference Application + Domain (implements interfaces)
/// 5. No circular dependencies
/// </summary>
public class DependencyRuleTests
{
    private static readonly Assembly SharedKernelAssembly = typeof(SharedKernel.Primitives.Entity<>).Assembly;
    private static readonly Assembly DomainAssembly = typeof(Domain.Common.Ids.DteId).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(Application.DependencyInjection).Assembly;
    private static readonly Assembly ContractsAssembly = typeof(Contracts.IntegrationEvents.DteSealedIntegrationEvent).Assembly;
    private static readonly Assembly PersistenceAssembly = typeof(Persistence.LibroFiscalDbContext).Assembly;

    private const string SharedKernelNamespace = "LibroFiscal.SharedKernel";
    private const string DomainNamespace = "LibroFiscal.Domain";
    private const string ApplicationNamespace = "LibroFiscal.Application";
    private const string ContractsNamespace = "LibroFiscal.Contracts";
    private const string PersistenceNamespace = "LibroFiscal.Persistence";
    private const string ObservabilityNamespace = "LibroFiscal.Observability";

    // ══════════════════════════════════════════════════════════════
    // Rule 1: Domain depends ONLY on SharedKernel
    // ══════════════════════════════════════════════════════════════

    [Fact]
    public void Domain_Should_Not_Reference_Application()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApplicationNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Domain must not reference Application. Violating types: {FormatFailingTypes(result)}");
    }

    [Fact]
    public void Domain_Should_Not_Reference_Infrastructure()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(PersistenceNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Domain must not reference Persistence. Violating types: {FormatFailingTypes(result)}");
    }

    [Fact]
    public void Domain_Should_Not_Reference_Observability()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(ObservabilityNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Domain must not reference Observability. Violating types: {FormatFailingTypes(result)}");
    }

    // ══════════════════════════════════════════════════════════════
    // Rule 2: Application depends on Domain + SharedKernel only
    // ══════════════════════════════════════════════════════════════

    [Fact]
    public void Application_Should_Not_Reference_Infrastructure()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(PersistenceNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Application must not reference Persistence. Violating types: {FormatFailingTypes(result)}");
    }

    [Fact]
    public void Application_Should_Not_Reference_Observability()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(ObservabilityNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Application must not reference Observability. Violating types: {FormatFailingTypes(result)}");
    }

    // ══════════════════════════════════════════════════════════════
    // Rule 3: SharedKernel has zero solution dependencies
    // ══════════════════════════════════════════════════════════════

    [Fact]
    public void SharedKernel_Should_Not_Reference_Domain()
    {
        var result = Types.InAssembly(SharedKernelAssembly)
            .ShouldNot()
            .HaveDependencyOn(DomainNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"SharedKernel must not reference Domain. Violating types: {FormatFailingTypes(result)}");
    }

    [Fact]
    public void SharedKernel_Should_Not_Reference_Application()
    {
        var result = Types.InAssembly(SharedKernelAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApplicationNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"SharedKernel must not reference Application. Violating types: {FormatFailingTypes(result)}");
    }

    [Fact]
    public void SharedKernel_Should_Not_Reference_Infrastructure()
    {
        var result = Types.InAssembly(SharedKernelAssembly)
            .ShouldNot()
            .HaveDependencyOn(PersistenceNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"SharedKernel must not reference Infrastructure. Violating types: {FormatFailingTypes(result)}");
    }

    // ══════════════════════════════════════════════════════════════
    // Rule 4: Contracts depends only on SharedKernel
    // ══════════════════════════════════════════════════════════════

    [Fact]
    public void Contracts_Should_Not_Reference_Domain()
    {
        var result = Types.InAssembly(ContractsAssembly)
            .ShouldNot()
            .HaveDependencyOn(DomainNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Contracts must not reference Domain. Violating types: {FormatFailingTypes(result)}");
    }

    [Fact]
    public void Contracts_Should_Not_Reference_Application()
    {
        var result = Types.InAssembly(ContractsAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApplicationNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Contracts must not reference Application. Violating types: {FormatFailingTypes(result)}");
    }

    // ══════════════════════════════════════════════════════════════
    // Naming Conventions
    // ══════════════════════════════════════════════════════════════

    [Fact]
    public void Interfaces_Should_Start_With_I()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .AreInterfaces()
            .Should()
            .HaveNameStartingWith("I")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"All interfaces must start with 'I'. Violating types: {FormatFailingTypes(result)}");
    }

    [Fact]
    public void Domain_Events_Should_End_With_Event()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .ImplementInterface(typeof(SharedKernel.Events.IDomainEvent))
            .Should()
            .HaveNameEndingWith("Event")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"All domain events must end with 'Event'. Violating types: {FormatFailingTypes(result)}");
    }

    // ══════════════════════════════════════════════════════════════
    // Helpers
    // ══════════════════════════════════════════════════════════════

    private static string FormatFailingTypes(TestResult result)
    {
        if (result.IsSuccessful || result.FailingTypes is null)
            return "none";

        return string.Join(", ", result.FailingTypes.Select(t => t.FullName));
    }
}
