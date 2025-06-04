namespace ResoniteModLoader;

public class ResoniteMod
{
    public static string? LastWarning;

    public static void Warn(string message) => LastWarning = message;
}

public class ModConfiguration { }

public class ModConfigurationKey<T>
{
    public ModConfigurationKey(string name, string description, Func<T> computeDefault) { }
}
