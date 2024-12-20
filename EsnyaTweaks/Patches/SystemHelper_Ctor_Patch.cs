﻿using Elements.Core;
using HarmonyLib;
using ResoniteModLoader;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using SystemHelperClient;

namespace EsnyaTweaks.Patches;


[HarmonyPatchCategory("System Helper Timeout"), Description("Timeout and restart SystemHelper")]
[HarmonyPatch(typeof(SystemHelper), MethodType.Constructor, new[] { typeof(string) })]
internal static class SystemHelper_Ctor_Patch
{
    internal static bool Prefix(ref SystemHelper __instance, string path)
    {
        TcpListener? tcpListener = null;
        Process? process = null;

        try
        {
            UniLog.Log("Initializing system helper at: " + path);

            string text = Path.Combine(path, "SystemHelperServer.exe");
            UniLog.Log("System Helper Executable: " + text);
            if (!File.Exists(text))
            {
                UniLog.Log("System Helper Not supported");
                return true;
            }

            AccessTools.PropertySetter(typeof(SystemHelper), "Initialized").Invoke(__instance, new object[] { true });

            tcpListener = new TcpListener(new IPEndPoint(IPAddress.Loopback, 0));
            tcpListener.Start();
            process = Process.Start(text, $"{((IPEndPoint)tcpListener.LocalEndpoint).Port}");

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
            UniLog.Log("System Helper initialized");

            return true;
        }
        catch (Exception ex)
        {
            ResoniteMod.Error("Error while initializing system helper");
            ResoniteMod.Error(ex);
        }
        finally
        {
            tcpListener?.Stop();
            process?.Kill();
        }

        ResoniteMod.Msg("Retrying after 3s...");
        Task.Delay(3_000).Wait();

        return Prefix(ref __instance, path);
    }
}