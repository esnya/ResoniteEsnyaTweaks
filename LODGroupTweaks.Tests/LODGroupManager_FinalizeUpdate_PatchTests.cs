using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace EsnyaTweaks.LODGroupTweaks.Tests;

public static class LODGroupManager_FinalizeUpdate_PatchTests
{
    [Fact]
    public static void Transpiler_Should_Call_LODValidation_ValidateLODs()
    {
        var projectRoot = Path.GetFullPath(
            Path.Combine(Assembly.GetExecutingAssembly().Location, "..", "..", "..", "..", "..")
        );
        var path = Path.Combine(
            projectRoot,
            "LODGroupTweaks",
            "LODGroupManager_FinalizeUpdate_Patch.cs"
        );
        var syntax = CSharpSyntaxTree.ParseText(
            File.ReadAllText(path),
            cancellationToken: TestContext.Current.CancellationToken
        );

        var root = syntax.GetRoot(cancellationToken: TestContext.Current.CancellationToken);
        var method = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.Text == "Transpiler");

        var hasValidateRef = method
            .DescendantNodes()
            .OfType<MemberAccessExpressionSyntax>()
            .Any(ma => ma.ToString().Contains("LODValidation.ValidateLODs", System.StringComparison.Ordinal));

        hasValidateRef.Should().BeTrue("Transpiler should reference LODValidation.ValidateLODs via reflection");
    }
}
