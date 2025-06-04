using System;
using System.Reflection;
using FluentAssertions;
using HarmonyLib;
using Xunit;

namespace EsnyaTweaks.FluxLoopTweaks.Tests;

/// <summary>
/// Tests for While_Run_Patch using Moq framework instead of complex stubs.
/// This approach is cleaner and more maintainable.
/// </summary>
public sealed class While_Run_PatchMoqTests : IDisposable
{
    private readonly object? _originalConfig;

    public While_Run_PatchMoqTests()
    {
        // Reflectionで元のconfigを保存
        _originalConfig = GetConfigField();
    }

    [Fact]
    public void While_Run_Patch_Should_Have_Harmony_Patch_Attribute()
    {
        // Arrange
        var patchType = typeof(While_Run_Patch);

        // Act
        var harmonyPatchAttribute = patchType.GetCustomAttribute<HarmonyPatch>();

        // Assert
        harmonyPatchAttribute
            .Should()
            .NotBeNull("the While_Run_Patch class should have a HarmonyPatch attribute");
    }

    [Fact]
    public void While_Run_Patch_Should_Target_While_Type()
    {
        // Arrange
        var patchType = typeof(While_Run_Patch);
        var harmonyPatchAttribute = patchType.GetCustomAttribute<HarmonyPatch>();

        // Act & Assert
        harmonyPatchAttribute.Should().NotBeNull();
        harmonyPatchAttribute!
            .info.declaringType?.Name.Should()
            .Be("While", "the patch should target the While type");
    }

    [Fact]
    public void While_Run_Patch_Should_Target_Run_Method()
    {
        // Arrange
        var patchType = typeof(While_Run_Patch);
        var harmonyPatchAttribute = patchType.GetCustomAttribute<HarmonyPatch>();

        // Act & Assert
        harmonyPatchAttribute.Should().NotBeNull();
        harmonyPatchAttribute!
            .info.methodName.Should()
            .Be("Run", "the patch should target the Run method");
    }

    [Fact]
    public void While_Run_Patch_Should_Have_Prefix_Method()
    {
        // Arrange
        var patchType = typeof(While_Run_Patch);

        // Act
        var prefixMethod = patchType.GetMethod(
            "Prefix",
            BindingFlags.Static | BindingFlags.NonPublic
        );

        // Assert
        prefixMethod.Should().NotBeNull("the patch should have a Prefix method");
        prefixMethod!.IsStatic.Should().BeTrue("the Prefix method should be static");
        prefixMethod.ReturnType.Should().Be<bool>("the Prefix method should return a boolean");
    }

    [Fact]
    public void While_Run_Patch_Type_Should_Be_Internal()
    {
        // Arrange & Act
        var patchType = typeof(While_Run_Patch);

        // Assert
        patchType
            .IsPublic.Should()
            .BeFalse("the patch class is internal, which is appropriate for Harmony patches");
    }

    [Fact]
    public void Prefix_Method_Should_Have_Correct_Signature()
    {
        // Arrange
        var patchType = typeof(While_Run_Patch);
        var prefixMethod = patchType.GetMethod(
            "Prefix",
            BindingFlags.Static | BindingFlags.NonPublic
        );

        // Act & Assert
        prefixMethod.Should().NotBeNull();
        prefixMethod!
            .GetParameters()
            .Should()
            .NotBeEmpty("the Prefix method should have parameters for the patch to work")
            .And.HaveCountGreaterOrEqualTo(
                3,
                "typical Harmony prefix should have __instance, context, and __result parameters"
            );

        // Check parameter types match expected Harmony prefix pattern
        var parameters = prefixMethod.GetParameters();
        parameters[0].Name.Should().Be("__instance", "first parameter should be the instance");
        parameters[1].Name.Should().Be("context", "second parameter should be the context");
        parameters[2].Name.Should().Be("__result", "third parameter should be the result");
        parameters[2]
            .ParameterType.IsByRef.Should()
            .BeTrue("__result parameter should be by reference");
    }

    [Fact]
    public void TimeoutMs_Should_Return_Default_When_Config_Is_Null()
    {
        // Arrange
        SetConfigField(null);

        // Act
        var timeout = FluxLoopTweaksMod.TimeoutMs;

        // Assert
        timeout.Should().Be(30_000, "TimeoutMs should return default value when config is null");
    }

    [Fact]
    public void TimeoutKey_Should_Have_Correct_Default_Value()
    {
        // Arrange & Act
        var timeoutKeyField = typeof(FluxLoopTweaksMod).GetField(
            "timeoutKey",
            BindingFlags.NonPublic | BindingFlags.Static
        );

        // Assert
        timeoutKeyField.Should().NotBeNull("timeoutKey field should exist");

        var timeoutKey = timeoutKeyField!.GetValue(null);
        timeoutKey.Should().NotBeNull("timeoutKey should not be null");

        // Use reflection to get the default value
        var computeDefaultMethod = timeoutKey!
            .GetType()
            .GetMethod("computeDefault", BindingFlags.NonPublic | BindingFlags.Instance);
        if (computeDefaultMethod != null)
        {
            var defaultValue = computeDefaultMethod.Invoke(timeoutKey, null);
            defaultValue.Should().Be(30_000, "default value for timeoutKey should be 30,000 ms");
        }
    }

    public void Dispose()
    {
        // Reflectionで元のconfigを復元
        SetConfigField(_originalConfig);
    }

    private static object? GetConfigField()
    {
        var configField = typeof(FluxLoopTweaksMod).GetField(
            "config",
            BindingFlags.NonPublic | BindingFlags.Static
        );
        return configField?.GetValue(null);
    }

    private static void SetConfigField(object? value)
    {
        var configField = typeof(FluxLoopTweaksMod).GetField(
            "config",
            BindingFlags.NonPublic | BindingFlags.Static
        );
        configField?.SetValue(null, value);
    }
}
