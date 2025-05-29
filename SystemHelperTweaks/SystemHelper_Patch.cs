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
        systemHelperPath = path;
        TcpListener? tcpListener = null;
        System.Diagnostics.Process? process = null;

        try
        {
            ResoniteMod.Msg("Initializing system helper at: " + path);
            Engine.Current.InitProgress?.SetFixedPhase("Initializing System Helper");
            AccessTools.PropertySetter(typeof(SystemHelper), "Current").Invoke(__instance, new object[] { __instance });

            string text = Path.Combine(path, "SystemHelperServer.exe");
            ResoniteMod.Msg("System Helper Executable: " + text);
            if (!File.Exists(text))
            {
                ResoniteMod.Msg("System Helper Not supported");
                return false;
            }

            AccessTools.PropertySetter(typeof(SystemHelper), "Initialized").Invoke(__instance, new object[] { true });

            tcpListener = new TcpListener(new IPEndPoint(IPAddress.Loopback, 0));
            tcpListener.Start();

            ResoniteMod.Msg("Listening on: " + ((IPEndPoint)tcpListener.LocalEndpoint).Port);

            process = System.Diagnostics.Process.Start(text, $"{((IPEndPoint)tcpListener.LocalEndpoint).Port}");

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

            ResoniteMod.Msg("System Helper initialized");
            Engine.Current.InitProgress?.SetFixedPhase("System Helper Initialized");
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
        Engine.Current.InitProgress?.SetFixedPhase("Retrying System Helper Initialization");
        Task.Delay(3_000).Wait();

        return StartSystemHelper(__instance, path);
    }
#pragma warning restore CA1031

    [HarmonyPatch(MethodType.Constructor, new[] { typeof(string) })]
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
        var connection = AccessTools.Field(typeof(SystemHelper), "connection").GetValue(__instance) as TcpClient;
        if ((connection is not null && connection.Connected) || systemHelperPath is null)
        {
            return;
        }

        ResoniteMod.Error("SystemHelper connection is not available. Restarting...");

        StartSystemHelper(__instance, systemHelperPath);
    }
}
