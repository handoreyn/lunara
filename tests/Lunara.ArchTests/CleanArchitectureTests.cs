using System.Reflection;
using NetArchTest.Rules;

namespace Lunara.ArchTests;

/// <summary>
/// Enforces Clean Architecture dependency rules across all Lunara modules and platform projects.
/// Rules:
///   1. Domain must NOT depend on Infrastructure.
///   2. Application must NOT depend on Infrastructure.
///   3. Infrastructure MAY depend on Application and Domain (no restriction tested here).
///   4. Api and Worker must NOT depend directly on module Infrastructure projects;
///      they must go through Lunara.Infrastructure.Host.
///   5. No assembly may reference AutoMapper, MediatR, or MassTransit.
/// </summary>
public sealed class CleanArchitectureTests
{
    // ── Assembly helpers ────────────────────────────────────────────────────

    /// <summary>Returns all Lunara assemblies currently loaded in the AppDomain.</summary>
    private static IEnumerable<Assembly> LunaraAssemblies()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name?.StartsWith("Lunara.", StringComparison.Ordinal) == true);
    }

    // ── Rule 1: Domain must not depend on Infrastructure ───────────────────

    [Fact]
    public void Domain_Should_Not_Depend_On_Infrastructure()
    {
        TestResult result = Types.InCurrentDomain()
            .That()
            .ResideInNamespaceMatching(@"^Lunara\.[A-Za-z]+\.Domain")
            .ShouldNot()
            .HaveDependencyOnAny(
                "Lunara.Identity.Infrastructure",
                "Lunara.Profiles.Infrastructure",
                "Lunara.Discovery.Infrastructure",
                "Lunara.Social.Infrastructure",
                "Lunara.Messaging.Infrastructure",
                "Lunara.Media.Infrastructure",
                "Lunara.Notifications.Infrastructure",
                "Lunara.Moderation.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Domain layer violates Infrastructure dependency rule.\n" +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    // ── Rule 2: Application must not depend on Infrastructure ──────────────

    [Fact]
    public void Application_Should_Not_Depend_On_Infrastructure()
    {
        TestResult result = Types.InCurrentDomain()
            .That()
            .ResideInNamespaceMatching(@"^Lunara\.[A-Za-z]+\.Application")
            .ShouldNot()
            .HaveDependencyOnAny(
                "Lunara.Identity.Infrastructure",
                "Lunara.Profiles.Infrastructure",
                "Lunara.Discovery.Infrastructure",
                "Lunara.Social.Infrastructure",
                "Lunara.Messaging.Infrastructure",
                "Lunara.Media.Infrastructure",
                "Lunara.Notifications.Infrastructure",
                "Lunara.Moderation.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Application layer violates Infrastructure dependency rule.\n" +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    // ── Rule 3: Domain must not depend on Application ──────────────────────

    [Fact]
    public void Domain_Should_Not_Depend_On_Application()
    {
        TestResult result = Types.InCurrentDomain()
            .That()
            .ResideInNamespaceMatching(@"^Lunara\.[A-Za-z]+\.Domain")
            .ShouldNot()
            .HaveDependencyOnAny(
                "Lunara.Identity.Application",
                "Lunara.Profiles.Application",
                "Lunara.Discovery.Application",
                "Lunara.Social.Application",
                "Lunara.Messaging.Application",
                "Lunara.Media.Application",
                "Lunara.Notifications.Application",
                "Lunara.Moderation.Application")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Domain layer must not depend on Application layer.\n" +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    // ── Rule 4a: Api must not reference module Infrastructure directly ──────

    [Fact]
    public void Api_Should_Not_Depend_On_Module_Infrastructure_Directly()
    {
        TestResult result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace("Lunara.Api")
            .ShouldNot()
            .HaveDependencyOnAny(
                "Lunara.Identity.Infrastructure",
                "Lunara.Profiles.Infrastructure",
                "Lunara.Discovery.Infrastructure",
                "Lunara.Social.Infrastructure",
                "Lunara.Messaging.Infrastructure",
                "Lunara.Media.Infrastructure",
                "Lunara.Notifications.Infrastructure",
                "Lunara.Moderation.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Lunara.Api must not depend on module Infrastructure directly. " +
            $"Use Lunara.Infrastructure.Host instead.\n" +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    // ── Rule 4b: Worker must not reference module Infrastructure directly ───

    [Fact]
    public void Worker_Should_Not_Depend_On_Module_Infrastructure_Directly()
    {
        TestResult result = Types.InCurrentDomain()
            .That()
            .ResideInNamespace("Lunara.Worker")
            .ShouldNot()
            .HaveDependencyOnAny(
                "Lunara.Identity.Infrastructure",
                "Lunara.Profiles.Infrastructure",
                "Lunara.Discovery.Infrastructure",
                "Lunara.Social.Infrastructure",
                "Lunara.Messaging.Infrastructure",
                "Lunara.Media.Infrastructure",
                "Lunara.Notifications.Infrastructure",
                "Lunara.Moderation.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Lunara.Worker must not depend on module Infrastructure directly. " +
            $"Use Lunara.Infrastructure.Host instead.\n" +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    // ── Rule 5: No assembly may reference forbidden packages ───────────────

    [Fact]
    public void No_Type_Should_Depend_On_AutoMapper()
    {
        TestResult result = Types.InCurrentDomain()
            .That()
            .ResideInNamespaceMatching(@"^Lunara\.")
            .ShouldNot()
            .HaveDependencyOn("AutoMapper")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"AutoMapper is forbidden in Lunara.\n" +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void No_Type_Should_Depend_On_MediatR()
    {
        TestResult result = Types.InCurrentDomain()
            .That()
            .ResideInNamespaceMatching(@"^Lunara\.")
            .ShouldNot()
            .HaveDependencyOnAny("MediatR", "MediatR.Contracts")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"MediatR is forbidden in Lunara.\n" +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void No_Type_Should_Depend_On_MassTransit()
    {
        TestResult result = Types.InCurrentDomain()
            .That()
            .ResideInNamespaceMatching(@"^Lunara\.")
            .ShouldNot()
            .HaveDependencyOnAny("MassTransit", "MassTransit.Contracts")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"MassTransit is forbidden in Lunara.\n" +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    // ── Rule 6: Assembly-level forbidden reference check ───────────────────
    // NetArchTest checks IL-level dependencies; the tests above cover type-level.
    // This test covers the case where a reference is added to a csproj but
    // no types are yet used — it checks the assembly references directly.

    [Fact]
    public void No_Assembly_Should_Reference_Forbidden_Libraries()
    {
        string[] forbidden = ["AutoMapper", "MediatR", "MassTransit"];

        IEnumerable<string> violations = LunaraAssemblies()
            .SelectMany(a => a.GetReferencedAssemblies(),
                        (parent, reference) => (ParentName: parent.GetName().Name, ReferenceName: reference.Name))
            .Where(t => forbidden.Any(f =>
                t.ReferenceName?.StartsWith(f, StringComparison.OrdinalIgnoreCase) == true))
            .Select(t => $"{t.ParentName} -> {t.ReferenceName}");

        Assert.False(violations.Any(),
            $"Forbidden assembly references found:\n{string.Join("\n", violations)}");
    }
}
