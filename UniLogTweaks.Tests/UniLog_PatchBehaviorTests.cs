using System;
using System.Reflection;
using Xunit;
using FluentAssertions;

namespace EsnyaTweaks.UniLogTweaks.Tests;

public static class UniLog_PatchBehaviorTests
{
    private static Type GetPatchType()
    {
        return typeof(UniLogTweaksMod).Assembly.GetType(
            "EsnyaTweaks.UniLogTweaks.UniLog_Patch",
            true)!;
    }

    [Fact]
    public static void Patch_Should_Add_Indent_And_Disable_StackTrace()
    {
        var patch = GetPatchType()
            .GetMethod("Patch", BindingFlags.Static | BindingFlags.NonPublic)!;
        object[] args = ["line1\nline2", true, false];
        patch.Invoke(null, args);

        args[0].Should().Be("line1\n\tline2");
        args[1].Should().Be(false);
    }

    [Fact]
    public static void Patch_Should_Leave_Message_When_No_Newline()
    {
        var patch = GetPatchType()
            .GetMethod("Patch", BindingFlags.Static | BindingFlags.NonPublic)!;
        const string original = "single line";
        object[] args = [original, true, true];
        patch.Invoke(null, args);

        args[0].Should().Be(original);
        args[1].Should().Be(true);
    }

    [Fact]
    public static void Patch_Should_Handle_Null_Message()
    {
        var patch = GetPatchType()
            .GetMethod("Patch", BindingFlags.Static | BindingFlags.NonPublic)!;
        object?[] args = [null, true, true];

        var act = () => patch.Invoke(null, args);

        act.Should().NotThrow();
        args[0].Should().Be(string.Empty);
        args[1].Should().Be(true);
    }
}
