using System.Reflection;
using ResoniteModLoader;
using EsnyaTweaks.Common.Reflection;

namespace EsnyaTweaks.Common.Modding;

/// <summary>
/// Base mod that auto-populates metadata (Name/Author/Version/Link) from AssemblyMetadata.
/// </summary>
public abstract class EsnyaResoniteMod : ResoniteMod
{
    /// <summary>
    /// The module assembly (derived type's assembly).
    /// </summary>
    protected Assembly ModAssembly => GetType().Assembly;

    /// <inheritdoc/>
    public override string Name => AssemblyMetadata.Read(ModAssembly).Name;

    /// <inheritdoc/>
    public override string Author => AssemblyMetadata.Read(ModAssembly).Author;

    /// <inheritdoc/>
    public override string Version => AssemblyMetadata.Read(ModAssembly).Version;

    /// <inheritdoc/>
    public override string Link => AssemblyMetadata.Read(ModAssembly).Link;
}

