using FrooxEngine;
using FrooxEngine.UIX;

namespace EsnyaTweaks.Common.UIX;

internal static class UIBuilderExtensions
{
    internal static Button LocalButton(
        this UIBuilder ui,
        string label,
        ButtonEventHandler localAction
    )
    {
        var button = ui.Button(label);
        button.LocalPressed += localAction;
        return button;
    }
}
