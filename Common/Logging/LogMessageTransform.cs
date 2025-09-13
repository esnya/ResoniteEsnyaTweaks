namespace EsnyaTweaks.Common.Logging;

/// <summary>
/// Pure transforms for log messages.
/// </summary>
public static class LogMessageTransform
{
    /// <summary>
    /// Adds a tab indent to each newline when <paramref name="addIndent"/> is true.
    /// </summary>
    /// <param name="message">Source message. Null is treated as empty string.</param>
    /// <param name="addIndent">Whether to add indent for each newline.</param>
    /// <returns>Transformed message.</returns>
    public static string ApplyIndent(string? message, bool addIndent)
    {
        var src = message ?? string.Empty;
        return addIndent ? src.Replace("\n", "\n\t", System.StringComparison.Ordinal) : src;
    }
}
