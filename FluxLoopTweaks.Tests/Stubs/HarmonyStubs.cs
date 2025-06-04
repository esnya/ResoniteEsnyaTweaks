namespace HarmonyLib;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class HarmonyPatch : Attribute
{
    public HarmonyPatch(Type declaringType, string methodName)
    {
        info = new PatchInfo { declaringType = declaringType, methodName = methodName };
    }

    public HarmonyPatch(Type declaringType)
    {
        info = new PatchInfo { declaringType = declaringType };
    }

    public HarmonyPatch(string methodName)
    {
        info = new PatchInfo { methodName = methodName };
    }

    public HarmonyPatch()
    {
        info = new PatchInfo();
    }

    public PatchInfo info { get; }

    public sealed class PatchInfo
    {
        public Type? declaringType;
        public string? methodName;
    }
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class HarmonyPrefix : Attribute { }
