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
    private static string? systemHelperPath;

#pragma warning disable CA1031
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
            if (Engine.Current?.InitProgress != null)
            {
                Engine.Current.InitProgress.SetFixedPhase("Initializing System Helper");
            }
            AccessTools
                .PropertySetter(typeof(SystemHelper), "Current")
                .Invoke(__instance, [__instance]);

            var text = Path.Combine(path, "SystemHelperServer.exe");
            ResoniteMod.Msg("System Helper Executable: " + text);
            if (!File.Exists(text))
            {
                ResoniteMod.Msg("System Helper Not supported");
                return false;
            }

            AccessTools
                .PropertySetter(typeof(SystemHelper), "Initialized")
                .Invoke(__instance, [true]);

            tcpListener = new TcpListener(new IPEndPoint(IPAddress.Loopback, 0));
            tcpListener.Start();

            ResoniteMod.Msg("Listening on: " + ((IPEndPoint)tcpListener.LocalEndpoint).Port);

            process = System.Diagnostics.Process.Start(
                text,
                $"{((IPEndPoint)tcpListener.LocalEndpoint).Port}"
            );

            var blockingTask = Task.Run(() => tcpListener.AcceptTcpClient());
            if (!blockingTask.Wait(10_000))
            {
                throw new TimeoutException("SystemHelper Timeout");
            }

            var connection = blockingTask.Result;
            AccessTools.Field(typeof(SystemHelper), "connection").SetValue(__instance, connection);

            tcpListener.Stop();
            var stream = connection.GetStream();
            AccessTools.Field(typeof(SystemHelper), "stream").SetValue(__instance, stream);

#pragma warning disable CA2000 // Dispose objects before losing scope
            var writer = new BinaryWriter(stream, SystemHelper.StringEncoding);
            AccessTools.Field(typeof(SystemHelper), "writer").SetValue(__instance, writer);

            var reader = new BinaryReader(stream, SystemHelper.StringEncoding);
            AccessTools.Field(typeof(SystemHelper), "reader").SetValue(__instance, reader);
#pragma warning restore CA2000 // Dispose objects before losing scope

            if (Engine.Current != null)
            {
                Engine.Current.OnShutdownRequest += (_) =>
                {
                    try
                    {
                        reader.Close();
                        writer.Close();
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

            ResoniteMod.Msg("System Helper initialized");
            if (Engine.Current?.InitProgress != null)
            {
                Engine.Current.InitProgress.SetFixedPhase("System Helper Initialized");
            }
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
        ResoniteMod.Msg("Retrying after 3s...");
        if (Engine.Current?.InitProgress != null)
        {
            Engine.Current.InitProgress.SetFixedPhase("Retrying System Helper Initialization");
        }
        Task.Delay(3_000).Wait();

        try
        {
            return StartSystemHelper(__instance, path);
        }
        catch (Exception ex)
        {
            ResoniteMod.Error($"Recursive restart failed: {ex.Message}");
            return false;
        }
    }
#pragma warning restore CA1031

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
            var connectionField = AccessTools.Field(typeof(SystemHelper), "connection");
            var connection = connectionField?.GetValue(__instance) as TcpClient;

            if (connection is not null && connection.Connected)
            {
                return;
            }
        }
        catch (ObjectDisposedException ex)
        {
            ResoniteMod.Debug($"Connection was disposed: {ex.Message}");
            // Continue to restart attempt
        }
        catch (InvalidOperationException ex)
        {
            ResoniteMod.Debug($"Invalid connection operation: {ex.Message}");
            // Continue to restart attempt
        }

        ResoniteMod.Error("SystemHelper connection is not available. Restarting...");

        try
        {
            StartSystemHelper(__instance, systemHelperPath);
        }
        catch (UnauthorizedAccessException ex)
        {
            ResoniteMod.Error($"Access denied when starting SystemHelper: {ex.Message}");
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            ResoniteMod.Error($"Failed to start SystemHelper process: {ex.Message}");
        }
        catch (DirectoryNotFoundException ex)
        {
            ResoniteMod.Error($"SystemHelper directory not found: {ex.Message}");
        }
        catch (FileNotFoundException ex)
        {
            ResoniteMod.Error($"SystemHelper executable not found: {ex.Message}");
        }
    }
}
