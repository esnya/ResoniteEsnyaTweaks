using ResoniteModLoader;
#if DEBUG
using ResoniteHotReloadLib;
#endif

namespace EsnyaTweaks.SceneAuditor.Editor;

internal static class CreateNewRegistration
{
    private const string Category = "Editor";
    private const string OptionName = "Scene Auditor (Mod)";

    public static void Register()
    {
        // Do not prefill Search Root; start with null for clarity
        FrooxEngine.DevCreateNewForm.AddAction(Category, OptionName, slot => SceneAuditorPanel.Spawn(slot));
        ResoniteMod.Msg($"Registered CreateNew action: {Category}/{OptionName}");
    }

    public static void Unregister()
    {
        // Remove via HotReloadLib just before hot reload to avoid stale entries
#if DEBUG
        HotReloader.RemoveMenuOption(Category, OptionName);
        ResoniteMod.Msg($"Removed CreateNew action via HotReloadLib: {Category}/{OptionName}");
#endif
    }
}
