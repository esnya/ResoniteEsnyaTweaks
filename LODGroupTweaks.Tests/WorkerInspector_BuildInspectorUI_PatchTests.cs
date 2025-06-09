using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace EsnyaTweaks.LODGroupTweaks.Tests;

public static class WorkerInspector_BuildInspectorUI_PatchTests
{
    [Fact]
    public static void Patch_Should_Have_HarmonyPatch_Attribute()
    {
        var patchType = typeof(LODGroupTweaksMod).Assembly.GetType(
            "EsnyaTweaks.LODGroupTweaks.LODGroup_WorkerInspector_BuildInspectorUI_Patch",
            true
        )!;
        patchType.GetCustomAttribute<HarmonyPatch>().Should().NotBeNull();
    }

    [Fact]
    public static void Patch_Should_Target_WorkerInspector_BuildInspectorUI()
    {
        var patchType = typeof(LODGroupTweaksMod).Assembly.GetType(
            "EsnyaTweaks.LODGroupTweaks.LODGroup_WorkerInspector_BuildInspectorUI_Patch",
            true
        )!;
        var attribute = patchType.GetCustomAttribute<HarmonyPatch>();
        attribute.Should().NotBeNull();
        attribute!.info.declaringType.Should().Be<WorkerInspector>();
        attribute.info.methodName.Should().Be(nameof(WorkerInspector.BuildInspectorUI));
    }

    [Fact]
    public static void Patch_Should_Have_Postfix_Method()
    {
        var patchType = typeof(LODGroupTweaksMod).Assembly.GetType(
            "EsnyaTweaks.LODGroupTweaks.LODGroup_WorkerInspector_BuildInspectorUI_Patch",
            true
        )!;
        var postfix = patchType.GetMethod("Postfix", BindingFlags.Static | BindingFlags.NonPublic);
        postfix.Should().NotBeNull();
        postfix!.IsStatic.Should().BeTrue();
    }

    [Fact]
    public static void Patch_Should_Define_Category_And_Description_Constants()
    {
        var patchType = typeof(LODGroupTweaksMod).Assembly.GetType(
            "EsnyaTweaks.LODGroupTweaks.LODGroup_WorkerInspector_BuildInspectorUI_Patch",
            true
        )!;

        var category = patchType.GetField(
            "CATEGORY",
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public
        );
        var description = patchType.GetField(
            "DESCRIPTION",
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public
        );

        category.Should().NotBeNull();
        category!.IsLiteral.Should().BeTrue();
        category.GetRawConstantValue().Should().Be("LODGroup Inspector");

        description.Should().NotBeNull();
        description!.IsLiteral.Should().BeTrue();
        description.GetRawConstantValue().Should().Be("Add useful buttons to LODGroup inspector.");
    }

    [Fact]
    public static void Patch_Should_Have_Private_Static_BuildInspectorUI_Method()
    {
        var patchType = typeof(LODGroupTweaksMod).Assembly.GetType(
            "EsnyaTweaks.LODGroupTweaks.LODGroup_WorkerInspector_BuildInspectorUI_Patch",
            true
        )!;

        var method = patchType.GetMethod(
            "BuildInspectorUI",
            BindingFlags.Static | BindingFlags.NonPublic
        );

        method.Should().NotBeNull();
        method!.IsStatic.Should().BeTrue();
        method.IsPrivate.Should().BeTrue();
        var parameters = method.GetParameters();
        parameters
            .Select(p => p.ParameterType)
            .Should()
            .ContainInConsecutiveOrder(typeof(LODGroup), typeof(UIBuilder));
    }

    [Fact]
    public static void BuildInspectorUI_Should_Create_Mod_Buttons()
    {
        var projectRoot = Path.GetFullPath(
            Path.Combine(Assembly.GetExecutingAssembly().Location, "..", "..", "..", "..", "..")
        );
        var path = Path.Combine(
            projectRoot,
            "LODGroupTweaks",
            "WorkerInspector_BuildInspectorUI_Patch.cs"
        );
        var syntax = CSharpSyntaxTree.ParseText(
            File.ReadAllText(path),
            cancellationToken: TestContext.Current.CancellationToken
        );

        var root = syntax.GetRoot(cancellationToken: TestContext.Current.CancellationToken);
        var method = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.Text == "BuildInspectorUI");

        var invocations = method
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Where(i => i.Expression.ToString() == "Button")
            .ToArray();

        invocations.Should().HaveCount(3);

        var labels = new[] { "ADD_LABEL", "SETUP_LABEL", "REMOVE_LABEL" };
        var handlers = new[] { "SetupFromChildren", "SetupByParts", "RemoveFromChildren" };

        for (var i = 0; i < invocations.Length; i++)
        {
            var args = invocations[i].ArgumentList.Arguments;
            args[1].Expression.ToString().Should().Be(labels[i]);

            var lambda = args[2]
                .Expression.Should()
                .BeOfType<SimpleLambdaExpressionSyntax>()
                .Subject;
            var body = lambda.Body.Should().BeOfType<InvocationExpressionSyntax>().Subject;
            body.Expression.ToString().Should().Be(handlers[i]);
        }
    }

    [Fact]
    public static void SetupFromChildren_Should_Set_Order_And_AddLOD()
    {
        var projectRoot = Path.GetFullPath(
            Path.Combine(Assembly.GetExecutingAssembly().Location, "..", "..", "..", "..", "..")
        );
        var path = Path.Combine(
            projectRoot,
            "LODGroupTweaks",
            "WorkerInspector_BuildInspectorUI_Patch.cs"
        );
        var syntax = CSharpSyntaxTree.ParseText(
            File.ReadAllText(path),
            cancellationToken: TestContext.Current.CancellationToken
        );

        var root = syntax.GetRoot(cancellationToken: TestContext.Current.CancellationToken);
        var method = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.Text == "SetupFromChildren");

        method
            .DescendantNodes()
            .OfType<IfStatementSyntax>()
            .Any(i =>
                i.Condition.ToString() == "lodGroup.UpdateOrder == 0"
                && i.Statement.ToString().Contains("lodGroup.UpdateOrder = 1000")
            )
            .Should()
            .BeTrue();

        method
            .DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Any(i => i.Expression.ToString() == "lodGroup.AddLOD")
            .Should()
            .BeTrue();
    }

    [Fact]
    public static void RemoveFromChildren_Should_Update_LabelText()
    {
        var projectRoot = Path.GetFullPath(
            Path.Combine(Assembly.GetExecutingAssembly().Location, "..", "..", "..", "..", "..")
        );
        var path = Path.Combine(
            projectRoot,
            "LODGroupTweaks",
            "WorkerInspector_BuildInspectorUI_Patch.cs"
        );
        var syntax = CSharpSyntaxTree.ParseText(
            File.ReadAllText(path),
            cancellationToken: TestContext.Current.CancellationToken
        );

        var root = syntax.GetRoot(cancellationToken: TestContext.Current.CancellationToken);
        var method = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.Text == "RemoveFromChildren");

        method
            .DescendantNodes()
            .OfType<AssignmentExpressionSyntax>()
            .Any(a =>
                a.Left.ToString() == "button.LabelText"
                && a.Right.ToString().Contains("REMOVE_LABEL")
            )
            .Should()
            .BeTrue();
    }
}
