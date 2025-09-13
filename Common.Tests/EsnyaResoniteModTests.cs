using System;
using System.Reflection;
using FluentAssertions;
using Xunit;

namespace EsnyaTweaks.Common.Tests;

public sealed class EsnyaResoniteModTests
{
    public sealed class DummyMod : Common.Modding.EsnyaResoniteMod
    {
        public string GetHarmonyId()
        {
            return HarmonyId;
        }

        public void Auto(Action reg, Action unreg)
        {
            RegisterWithAutoUnregister(reg, unreg);
        }
    }

    [Fact]
    public void HarmonyId_Should_Use_DefaultPrefix_And_AssemblyName()
    {
        var mod = new DummyMod();
        var id = mod.GetHarmonyId();

        id.Should().NotBeNullOrEmpty();
        id.Should().StartWith("com.nekometer.esnya.");

        var asmName = typeof(DummyMod).Assembly.GetName().Name;
        id.Should().Contain(asmName);
    }
}

public sealed class EsnyaResoniteModAutoUnregisterTests
{
    [Fact]
    public void RegisterWithAutoUnregister_Should_Invoke_Unregister_On_BeforeHotReload()
    {
        var mod = new EsnyaResoniteModTests.DummyMod();
        var id = mod.GetHarmonyId();
        var called = 0;
        var regCalled = false;

        mod.Auto(() => regCalled = true, () => called++);
        regCalled.Should().BeTrue();
        called.Should().Be(0);

        Common.Modding.EsnyaResoniteMod.BeforeHotReload(id);

        called.Should().Be(1);

        // 2回目は登録が消費されているため呼ばれない
        Common.Modding.EsnyaResoniteMod.BeforeHotReload(id);
        called.Should().Be(1);
    }

    [Fact]
    public void Multiple_Registers_Should_All_Unregister_On_BeforeHotReload()
    {
        var mod = new EsnyaResoniteModTests.DummyMod();
        var id = mod.GetHarmonyId();
        var a = 0; var b = 0;

        mod.Auto(() => { }, () => a++);
        mod.Auto(() => { }, () => b++);

        Common.Modding.EsnyaResoniteMod.BeforeHotReload(id);

        a.Should().Be(1);
        b.Should().Be(1);
    }

    // NOTE: OnHotReload は内部で GetConfiguration() を呼ぶため、
    // テスト環境では Mod 初期化前例外となる。必要であれば別シナリオで検証する。
}
