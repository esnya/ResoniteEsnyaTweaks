using System.Reflection;
using FluentAssertions;
using FrooxEngine;
using HarmonyLib;
using Xunit;

namespace EsnyaTweaks.PhotonDustTweaks.Tests;

internal static class WorkerInspector_BuildInspectorUI_PatchTests
{
    [Fact]
    public static void Patch_Should_Have_HarmonyPatch_Attribute()
    {
        var patchType = typeof(PhotonDustTweaksMod).Assembly.GetType(
            "EsnyaTweaks.PhotonDustTweaks.PhotonDust_WorkerInspector_BuildInspectorUI_Patch",
            true
        )!;
        patchType.GetCustomAttribute<HarmonyPatch>().Should().NotBeNull();
    }

    [Fact]
    public static void Patch_Should_Target_WorkerInspector_BuildInspectorUI()
    {
        var patchType = typeof(PhotonDustTweaksMod).Assembly.GetType(
            "EsnyaTweaks.PhotonDustTweaks.PhotonDust_WorkerInspector_BuildInspectorUI_Patch",
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
        var patchType = typeof(PhotonDustTweaksMod).Assembly.GetType(
            "EsnyaTweaks.PhotonDustTweaks.PhotonDust_WorkerInspector_BuildInspectorUI_Patch",
            true
        )!;
        var postfix = patchType.GetMethod("Postfix", BindingFlags.Static | BindingFlags.NonPublic);
        postfix.Should().NotBeNull();
        postfix!.IsStatic.Should().BeTrue();
    }
}
