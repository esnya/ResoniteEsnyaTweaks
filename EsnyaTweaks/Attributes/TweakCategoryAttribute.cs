using System;

namespace EsnyaTweaks.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
internal sealed class TweakDescriptionAttribute : Attribute
{
    public string Description { get; }
    public bool DefaultValue { get; }

    public TweakDescriptionAttribute(string description, bool defaultValue = true)
    {
        Description = description;
        DefaultValue = defaultValue;
    }
}
