using FrooxEngine;
using FrooxEngine.PhotonDust;
using FrooxEngine.UIX;
using HarmonyLib;
using ResoniteModLoader;

namespace EsnyaTweaks.PhotonDustTweaks;

[HarmonyPatch(typeof(WorkerInspector), nameof(WorkerInspector.BuildInspectorUI))]
internal static class PhotonDust_WorkerInspector_BuildInspectorUI_Patch
{
    private static void Postfix(Worker worker, UIBuilder ui)
    {
        if (worker is IParticleSystemSubsystem module)
        {
            module.BuildInspectorUI(ui);
            ResoniteMod.DebugFunc(() => $"ParticleSystem module {module} found. Building inspector UI...");
        }
        if (worker is ParticleStyle style)
        {
            style.BuildInspectorUI(ui);
            ResoniteMod.DebugFunc(() => $"ParticleStyle {style} found. Building inspector UI...");
        }
    }
}
