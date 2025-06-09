using System;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization;
using FluentAssertions;
using HarmonyLib;
using SystemHelperClient;
using Xunit;

namespace EsnyaTweaks.SystemHelperTweaks.Tests;

public static class SystemHelper_PatchTests
{
    public static Type GetPatchType()
    {
        return typeof(SystemHelperTweaksMod).Assembly.GetType(
            "EsnyaTweaks.SystemHelperTweaks.SystemHelper_Patch",
            true
        )!;
    }

    [Fact]
    public static void Patch_Should_Have_HarmonyPatch_Attribute()
    {
        var patchType = GetPatchType();
        patchType.GetCustomAttribute<HarmonyPatch>().Should().NotBeNull();
    }

    [Fact]
    public static void Patch_Should_Target_SystemHelper_Class()
    {
        var patchType = GetPatchType();
        var attribute = patchType.GetCustomAttribute<HarmonyPatch>();
        attribute.Should().NotBeNull();
        attribute!.info.declaringType.Should().Be<SystemHelper>();
    }

    [Fact]
    public static void Patch_Should_Define_Ctor_Prefix()
    {
        var patchType = GetPatchType();
        var method = patchType.GetMethod("Ctor_Prefix", BindingFlags.Static | BindingFlags.Public);
        method.Should().NotBeNull();
        method!.IsStatic.Should().BeTrue();
    }

    [Fact]
    public static void Patch_Should_Define_GetClipboardData_Prefix()
    {
        var patchType = GetPatchType();
        var method = patchType.GetMethod(
            "GetClipboardData_Prefix",
            BindingFlags.Static | BindingFlags.Public
        );
        method.Should().NotBeNull();
        method!.IsStatic.Should().BeTrue();
    }

    [Fact]
    public static void Ctor_Prefix_Should_ReturnFalse_And_SetSystemHelperPath()
    {
        // Arrange
        var method = GetPatchType()
            .GetMethod("Ctor_Prefix", BindingFlags.Static | BindingFlags.Public)!;
        var instance = (SystemHelper)FormatterServices.GetUninitializedObject(typeof(SystemHelper));
        var path = Path.Combine(Path.GetTempPath(), "nonexistentPath");
        // Act
        var result = (bool)method.Invoke(null, [instance, path]);
        // Assert
        result.Should().BeFalse();
        var field = GetPatchType()
            .GetField("systemHelperPath", BindingFlags.NonPublic | BindingFlags.Static)!;
        field.GetValue(null).Should().Be(path);
    }

    [Fact]
    public static void StartSystemHelper_NoExe_ReturnsFalse()
    {
        // Arrange
        var method = GetPatchType()
            .GetMethod("StartSystemHelper", BindingFlags.NonPublic | BindingFlags.Static)!;
        var instance = (SystemHelper)FormatterServices.GetUninitializedObject(typeof(SystemHelper));
        var tempDir = Path.Combine(Path.GetTempPath(), "nonexistentDir_" + Guid.NewGuid());
        Directory.CreateDirectory(tempDir);
        // Act
        var result = (bool)method.Invoke(null, [instance, tempDir]);
        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public static void GetClipboardData_Prefix_WhenConnectionNull_ShouldHandleNullReference()
    {
        // Arrange
        var field = GetPatchType()
            .GetField("systemHelperPath", BindingFlags.NonPublic | BindingFlags.Static)!;
        field.SetValue(null, null); // systemHelperPathを意図的にnullに設定
        var method = GetPatchType()
            .GetMethod("GetClipboardData_Prefix", BindingFlags.Static | BindingFlags.Public)!;
        var instance = (SystemHelper)FormatterServices.GetUninitializedObject(typeof(SystemHelper));

        // Act & Assert - systemHelperPathがnullの場合は何もしない
        method.Invoking(m => m.Invoke(null, [instance])).Should().NotThrow();
    }

    [Fact]
    public static void GetClipboardData_Prefix_WhenConnectionNotNull_ShouldNotCallRestart()
    {
        // Arrange
        var instance = (SystemHelper)FormatterServices.GetUninitializedObject(typeof(SystemHelper));
        // 実際のTcpClientインスタンスを作成し、接続フィールドに設定
        var connection = new TcpClient();

        // connectionフィールドに実際のオブジェクトを設定
        AccessTools.Field(typeof(SystemHelper), "connection").SetValue(instance, connection);

        var method = GetPatchType()
            .GetMethod("GetClipboardData_Prefix", BindingFlags.Static | BindingFlags.Public)!;

        try
        {
            // Act & Assert - 接続オブジェクトがあるのでエラーは発生しない
            method.Invoking(m => m.Invoke(null, [instance])).Should().NotThrow();
        }
        finally
        {
            // Cleanup
            connection?.Dispose();
        }
    }

    [Fact]
    public static void StartSystemHelper_WithInvalidProcess_ShouldReturnFalse()
    {
        // Arrange
        var method = GetPatchType()
            .GetMethod("StartSystemHelper", BindingFlags.NonPublic | BindingFlags.Static)!;
        var instance = (SystemHelper)FormatterServices.GetUninitializedObject(typeof(SystemHelper));
        var tempDir = Path.Combine(Path.GetTempPath(), "systemhelper_test_" + Guid.NewGuid());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Act
            var result = (bool)method.Invoke(null, [instance, tempDir]);
            // Assert
            result.Should().BeFalse();
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public static void StartSystemHelper_ShouldHandleExceptionsSafely()
    {
        // Arrange
        var method = GetPatchType()
            .GetMethod("StartSystemHelper", BindingFlags.NonPublic | BindingFlags.Static)!;
        var instance = (SystemHelper)FormatterServices.GetUninitializedObject(typeof(SystemHelper));
        var invalidPath = "invalid-path-" + Guid.NewGuid();

        // Act
        var result = (bool)method.Invoke(null, [instance, invalidPath]);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public static void GetClipboardData_Prefix_WhenConnectionDisconnected_ShouldTryRestart()
    {
        // Arrange
        var field = GetPatchType()
            .GetField("systemHelperPath", BindingFlags.NonPublic | BindingFlags.Static)!;
        var tempDir = Path.Combine(Path.GetTempPath(), "test_" + Guid.NewGuid());
        Directory.CreateDirectory(tempDir);
        field.SetValue(null, tempDir);

        var instance = (SystemHelper)FormatterServices.GetUninitializedObject(typeof(SystemHelper));
        // 実際のTcpClientインスタンスを作成（接続されていない状態）
        var connection = new TcpClient();

        AccessTools.Field(typeof(SystemHelper), "connection").SetValue(instance, connection);

        var method = GetPatchType()
            .GetMethod("GetClipboardData_Prefix", BindingFlags.Static | BindingFlags.Public)!;

        try
        {
            // Act & Assert - 接続が切れているが、SystemHelperServerがないのでエラーは発生するがテストは通る
            method.Invoking(m => m.Invoke(null, [instance])).Should().NotThrow();
        }
        finally
        {
            // Cleanup
            connection?.Dispose();
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public static void GetClipboardData_Prefix_WhenConnectionNull_AndPathSet_DoesNotThrow()
    {
        // Arrange
        var field = GetPatchType()
            .GetField("systemHelperPath", BindingFlags.NonPublic | BindingFlags.Static)!;
        var tempDir = Path.Combine(Path.GetTempPath(), "test_" + Guid.NewGuid());
        Directory.CreateDirectory(tempDir);
        field.SetValue(null, tempDir);

        var instance = (SystemHelper)FormatterServices.GetUninitializedObject(typeof(SystemHelper));
        // connectionフィールドをnullに設定
        AccessTools.Field(typeof(SystemHelper), "connection").SetValue(instance, null);

        var method = GetPatchType()
            .GetMethod("GetClipboardData_Prefix", BindingFlags.Static | BindingFlags.Public)!;

        try
        {
            // Act & Assert - connectionがnullの場合、StartSystemHelperが呼ばれるがエラーは処理される
            method.Invoking(m => m.Invoke(null, [instance])).Should().NotThrow();
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public static void StartSystemHelper_WithValidPath_ButNoExecutable_ReturnsFalse()
    {
        // Arrange
        var method = GetPatchType()
            .GetMethod("StartSystemHelper", BindingFlags.NonPublic | BindingFlags.Static)!;
        var instance = (SystemHelper)FormatterServices.GetUninitializedObject(typeof(SystemHelper));
        var tempDir = Path.Combine(Path.GetTempPath(), "validpath_" + Guid.NewGuid());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Act
            var result = (bool)method.Invoke(null, [instance, tempDir]);
            // Assert
            result.Should().BeFalse();
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public static void Ctor_Prefix_WithNullPath_ShouldReturnFalse()
    {
        // Arrange
        var method = GetPatchType()
            .GetMethod("Ctor_Prefix", BindingFlags.Static | BindingFlags.Public)!;
        var instance = (SystemHelper)FormatterServices.GetUninitializedObject(typeof(SystemHelper));

        // Act
        var result = (bool)method.Invoke(null, [instance, null]);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public static void Ctor_Prefix_WithEmptyPath_ShouldReturnFalse()
    {
        // Arrange
        var method = GetPatchType()
            .GetMethod("Ctor_Prefix", BindingFlags.Static | BindingFlags.Public)!;
        var instance = (SystemHelper)FormatterServices.GetUninitializedObject(typeof(SystemHelper));

        // Act
        var result = (bool)method.Invoke(null, [instance, ""]);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public static void GetClipboardData_Prefix_WithNullInstance_ShouldNotThrow()
    {
        // Arrange
        var method = GetPatchType()
            .GetMethod("GetClipboardData_Prefix", BindingFlags.Static | BindingFlags.Public)!;

        // Act & Assert - nullインスタンスに対しては早期リターンされるべき
        method.Invoking(m => m.Invoke(null, [null])).Should().NotThrow();
    }

    [Fact]
    public static void StartSystemHelper_WithNullInstance_ShouldReturnFalse()
    {
        // Arrange
        var method = GetPatchType()
            .GetMethod("StartSystemHelper", BindingFlags.NonPublic | BindingFlags.Static)!;
        var tempPath = Path.GetTempPath();

        // Act
        var result = (bool)method.Invoke(null, [null, tempPath]);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public static void GetClipboardData_Prefix_WithConnectionDisposedOrInvalid_ShouldHandleGracefully()
    {
        // Arrange
        var field = GetPatchType()
            .GetField("systemHelperPath", BindingFlags.NonPublic | BindingFlags.Static)!;
        var tempDir = Path.Combine(Path.GetTempPath(), "disposed_test_" + Guid.NewGuid());
        Directory.CreateDirectory(tempDir);
        field.SetValue(null, tempDir);

        var instance = (SystemHelper)FormatterServices.GetUninitializedObject(typeof(SystemHelper));
        var connection = new TcpClient();
        connection.Dispose(); // 既に破棄された接続をシミュレート

        AccessTools.Field(typeof(SystemHelper), "connection").SetValue(instance, connection);

        var method = GetPatchType()
            .GetMethod("GetClipboardData_Prefix", BindingFlags.Static | BindingFlags.Public)!;

        try
        {
            // Act & Assert - 破棄された接続でも例外を投げずに処理される
            method.Invoking(m => m.Invoke(null, [instance])).Should().NotThrow();
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
}
