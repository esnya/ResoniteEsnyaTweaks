using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;
using SystemHelperClient;

namespace EsnyaTweaks.SystemHelperTweaks;

[HarmonyPatch(typeof(SystemHelper))]
internal static class SystemHelper_Patch
{
    private const int CONNECTION_TIMEOUT_MS = 10_000;
    private const int RETRY_DELAY_MS = 3_000;
    private const string SYSTEM_HELPER_EXECUTABLE = "SystemHelperServer.exe";

    private static string? systemHelperPath;

    private sealed class StreamManager(NetworkStream stream)
    {
        public BinaryWriter Writer { get; } = new BinaryWriter(stream, SystemHelper.StringEncoding);
        public BinaryReader Reader { get; } = new BinaryReader(stream, SystemHelper.StringEncoding);

        public void Close()
        {
            Writer?.Close();
            Reader?.Close();
        }
    }

    private static void InitializeSystemHelperInstance(SystemHelper instance)
    {
        AccessTools.PropertySetter(typeof(SystemHelper), "Current").Invoke(instance, [instance]);
        AccessTools.PropertySetter(typeof(SystemHelper), "Initialized").Invoke(instance, [true]);
    }

    private static bool ValidateExecutablePath(string path)
    {
        var executablePath = Path.Combine(path, SYSTEM_HELPER_EXECUTABLE);
        ResoniteMod.Msg("System Helper Executable: " + executablePath);

        if (!File.Exists(executablePath))
        {
            ResoniteMod.Msg("System Helper Not supported");
            return false;
        }

        return true;
    }

    private static TcpClient EstablishConnection(TcpListener tcpListener)
    {
        var blockingTask = Task.Run(() => tcpListener.AcceptTcpClient());
        return !blockingTask.Wait(CONNECTION_TIMEOUT_MS)
            ? throw new TimeoutException("SystemHelper Timeout")
            : blockingTask.Result;
    }

    private static void SetupStreamsAndReaders(SystemHelper instance, TcpClient connection)
    {
        var stream = connection.GetStream();
        var streamManager = new StreamManager(stream);

        AccessTools.Field(typeof(SystemHelper), "connection").SetValue(instance, connection);
        AccessTools.Field(typeof(SystemHelper), "stream").SetValue(instance, stream);
        AccessTools.Field(typeof(SystemHelper), "writer").SetValue(instance, streamManager.Writer);
        AccessTools.Field(typeof(SystemHelper), "reader").SetValue(instance, streamManager.Reader);
    }

    private static void RegisterShutdownHandler(
        TcpClient connection,
        System.Diagnostics.Process process
    )
    {
        if (Engine.Current == null)
        {
            return;
        }

        Engine.Current.OnShutdownRequest += (_) =>
        {
            try
            {
                var reader =
                    AccessTools.Field(typeof(SystemHelper), "reader").GetValue(null)
                    as BinaryReader;
                var writer =
                    AccessTools.Field(typeof(SystemHelper), "writer").GetValue(null)
                    as BinaryWriter;

                reader?.Close();
                writer?.Close();
                connection.Close();
                process.Kill();
            }
            catch (Exception ex)
            {
                ResoniteMod.Error("Error while closing SystemHelper connection");
                ResoniteMod.Error(ex);
            }
        };
    }

    private static bool RetryStartSystemHelper(SystemHelper instance, string path)
    {
        ResoniteMod.Msg("Retrying after 3s...");
        Task.Delay(RETRY_DELAY_MS).Wait();

        try
        {
            return StartSystemHelper(instance, path);
        }
        catch (Exception ex)
        {
            ResoniteMod.Error($"Recursive restart failed: {ex.Message}");
            return false;
        }
    }

    private static bool StartSystemHelper(SystemHelper __instance, string path)
    {
        if (__instance == null || string.IsNullOrEmpty(path))
        {
            return false;
        }

        systemHelperPath = path;
        TcpListener? tcpListener = null;
        System.Diagnostics.Process? process = null;

        try
        {
            ResoniteMod.Msg("Initializing system helper at: " + path);
            InitializeSystemHelperInstance(__instance);

            if (!ValidateExecutablePath(path))
            {
                return false;
            }

            tcpListener = new TcpListener(new IPEndPoint(IPAddress.Loopback, 0));
            tcpListener.Start();

            ResoniteMod.Msg("Listening on: " + ((IPEndPoint)tcpListener.LocalEndpoint).Port);

            process = System.Diagnostics.Process.Start(
                Path.Combine(path, SYSTEM_HELPER_EXECUTABLE),
                $"{((IPEndPoint)tcpListener.LocalEndpoint).Port}"
            );

            var connection = EstablishConnection(tcpListener);
            SetupStreamsAndReaders(__instance, connection);

            tcpListener.Stop();

            RegisterShutdownHandler(connection, process);

            ResoniteMod.Msg("System Helper initialized");
            return false;
        }
        catch (Exception ex)
        {
            ResoniteMod.Error("Error while initializing system helper");
            ResoniteMod.Error(ex);
        }
        finally
        {
            tcpListener?.Stop();
        }

        process?.Kill();
        return RetryStartSystemHelper(__instance, path);
    }

    [HarmonyPatch(MethodType.Constructor, [typeof(string)])]
    [HarmonyPrefix]
    public static bool Ctor_Prefix(ref SystemHelper __instance, string path)
    {
        StartSystemHelper(__instance, path);
        return false;
    }

    [HarmonyPatch(nameof(SystemHelper.GetClipboardData))]
    [HarmonyPrefix]
    public static void GetClipboardData_Prefix(SystemHelper __instance)
    {
        if (__instance == null || systemHelperPath is null)
        {
            return;
        }

        try
        {
            var connection =
                AccessTools.Field(typeof(SystemHelper), "connection")?.GetValue(__instance)
                as TcpClient;

            if (connection?.Connected == true)
            {
                return;
            }
        }
        catch (Exception ex)
        {
            ResoniteMod.Debug($"Connection check failed: {ex.Message}");
        }

        ResoniteMod.Error("SystemHelper connection is not available. Restarting...");

        try
        {
            StartSystemHelper(__instance, systemHelperPath);
        }
        catch (Exception ex)
        {
            ResoniteMod.Error($"Failed to restart SystemHelper: {ex.Message}");
        }
    }
}
