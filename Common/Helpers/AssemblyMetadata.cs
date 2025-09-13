using System.Linq;
using System.Reflection;

namespace EsnyaTweaks.Common.Reflection;

/// <summary>
/// Reads assembly metadata (Title/Company/Version/RepositoryUrl) in a safe, consistent way.
/// </summary>
public static class AssemblyMetadata
{
    /// <summary>
    /// Reads key metadata values from the given assembly.
    /// </summary>
    /// <param name="asm">The assembly to read from.</param>
    /// <returns>
    /// Tuple of Name (Title), Author (Company), Version (Informational/Assembly version without build metadata), and Link (RepositoryUrl) strings.
    /// </returns>
    public static (string Name, string Author, string Version, string Link) Read(Assembly asm)
    {
        var name = asm.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? "Unknown";
        var author = asm.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "Unknown";
        var version =
            asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? asm.GetCustomAttribute<AssemblyVersionAttribute>()?.Version
            ?? "0.0.0";
        if (!string.IsNullOrEmpty(version))
        {
            var plus = version.IndexOf('+', System.StringComparison.Ordinal);
            if (plus >= 0)
            {
                version = version[..plus];
            }
        }
        var link =
            asm.GetCustomAttributes<AssemblyMetadataAttribute>()
               .FirstOrDefault(meta => meta.Key == "RepositoryUrl")?.Value
            ?? string.Empty;
        return (name, author, version!, link);
    }
}
