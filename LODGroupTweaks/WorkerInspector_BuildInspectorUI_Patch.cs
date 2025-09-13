using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using ResoniteModLoader;

namespace EsnyaTweaks.LODGroupTweaks;

[HarmonyPatch(typeof(WorkerInspector), nameof(WorkerInspector.BuildInspectorUI))]
internal static class LODGroup_WorkerInspector_BuildInspectorUI_Patch
{
    internal const string CATEGORY = "LODGroup Inspector";
    internal const string DESCRIPTION = "Add useful buttons to LODGroup inspector.";

    private const string ADD_LABEL = "[Mod] Add LOD Level from children";
    private const string SETUP_LABEL = "[Mod] Setup LOD Levels by parts";
    private const string REMOVE_LABEL = "[Mod] Remove LODGroups from children";

    private static void Postfix(Worker worker, UIBuilder ui)
    {
        if (worker is LODGroup lodGroup)
        {
            ResoniteMod.DebugFunc(() =>
                $"LODGroup on {lodGroup.Slot.Name} found. Building inspector UI..."
            );
            BuildInspectorUI(lodGroup, ui);
        }
    }

    private static void BuildInspectorUI(LODGroup lodGroup, UIBuilder ui)
    {
        Button(ui, ADD_LABEL, button => SetupFromChildren(button, lodGroup));
        Button(ui, SETUP_LABEL, button => SetupByParts(button, lodGroup));
        Button(ui, REMOVE_LABEL, button => RemoveFromChildren(button, lodGroup));
    }

    private static void Button(UIBuilder ui, string text, Action<Button> onClick)
    {
        var button = ui.Button(text);
        button.LocalPressed += (_, __) =>
        {
            onClick(button);
        };
    }

    private static void Button(UIBuilder ui, string text, ButtonEventHandler onLocalPress)
    {
        var button = ui.Button(text);
        button.LocalPressed += onLocalPress;
    }



    private static void SetupFromChildren(Button _, LODGroup lodGroup)
    {
        if (lodGroup.UpdateOrder == 0)
        {
            lodGroup.UpdateOrder = 1000;
        }
        // Gather child renderers and exclude ones already registered in other LODGroups
        var renderers = Pool.BorrowList<MeshRenderer>();
        try
        {
            lodGroup.Slot.GetComponentsInChildren(renderers);
            var assignedElsewhere = LODValidation.CollectAssignedRenderersInOtherGroups(lodGroup);
            if (assignedElsewhere.Count > 0)
            {
                var before = renderers.Count;
                renderers.RemoveAll(assignedElsewhere.Contains);
                var removed = before - renderers.Count;
                if (removed > 0)
                {
                    ResoniteMod.Msg($"Excluded {removed} renderer(s) already registered in other LODGroup(s).");
                }
            }

            if (renderers.Count == 0)
            {
                ResoniteMod.Msg("No eligible MeshRenderer found under this LODGroup.");
                return;
            }
            lodGroup.AddLOD(0.01f, [.. renderers]);
        }
        finally
        {
            Pool.Return(ref renderers);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float GetBoundingMagnitude(Slot space, MeshRenderer renderer)
    {
        return renderer.GetBoundingBoxInSpace(space).Size.Magnitude;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float GetBoundingMagnitude(Slot slot)
    {
        return slot.ComputeBoundingBox(filter: static c => c is MeshRenderer).Size.Magnitude;
    }

    private static void AddLOD(
        LODGroup lodGroup,
        float baseThreshold,
        in List<KeyValuePair<MeshRenderer, float>> rendererWithBounds
    )
    {
        var levelSize = rendererWithBounds.Last().Value;
        lodGroup.AddLOD(baseThreshold / levelSize, [.. rendererWithBounds.Select(p => p.Key)]);
    }

    private static void SetupByParts(Button _, LODGroup lodGroup)
    {
        if (lodGroup.UpdateOrder == 0)
        {
            lodGroup.UpdateOrder = 1000;
        }

        var space = lodGroup.Slot;
        var totalSize = GetBoundingMagnitude(space);
        var sizeThreshold = totalSize * 0.3333f;

        var largeRenderers = Pool.BorrowList<KeyValuePair<MeshRenderer, float>>();
        var rendererWithScore = Pool.BorrowList<KeyValuePair<MeshRenderer, float>>();
        var renderers = Pool.BorrowList<MeshRenderer>();
        HashSet<MeshRenderer>? assignedElsewhere = null;
        try
        {
            lodGroup.Slot.GetComponentsInChildren(renderers);

            rendererWithScore.AddRange(
                from r in renderers
                select new KeyValuePair<MeshRenderer, float>(
                    r,
                    GetBoundingMagnitude(space, r)
                ) into p
                orderby p.Value descending
                select p
            );
            if (ResoniteMod.IsDebugEnabled())
            {
                foreach (var pair in rendererWithScore)
                {
                    ResoniteMod.DebugFunc(() => $"{pair.Key} {pair.Value}/{totalSize}");
                }
            }

            // Exclude renderers already assigned to other LODGroups to prevent Unity warning
            assignedElsewhere = LODValidation.CollectAssignedRenderersInOtherGroups(lodGroup);
            if (assignedElsewhere.Count > 0)
            {
                var before = rendererWithScore.Count;
                rendererWithScore.RemoveAll(kv => assignedElsewhere.Contains(kv.Key));
                var removed = before - rendererWithScore.Count;
                if (removed > 0)
                {
                    ResoniteMod.Msg($"Excluded {removed} renderer(s) already registered in other LODGroup(s).");
                }
            }

            var thresholdIndex = rendererWithScore.FindLastIndex(p => p.Value > sizeThreshold);
            if (thresholdIndex > 0)
            {
                largeRenderers.AddRange(rendererWithScore.Take(thresholdIndex));
                AddLOD(lodGroup, 0.005f * totalSize, rendererWithScore);
                AddLOD(lodGroup, 0.005f * totalSize, largeRenderers);
            }
            else
            {
                AddLOD(lodGroup, 0.005f * totalSize, rendererWithScore);
            }
        }
        finally
        {
            Pool.Return(ref renderers);
            Pool.Return(ref rendererWithScore);
            Pool.Return(ref largeRenderers);
        }
    }

    private static void RemoveFromChildren(Button button, LODGroup lodGroup)
    {
        var groups = lodGroup.Slot.GetComponentsInChildren<LODGroup>(c => c != lodGroup);
        var count = 0;
        foreach (var group in groups)
        {
            group.Destroy();
            count++;
        }

        button.LabelText = $"{REMOVE_LABEL} (Removed {count} groups)";
    }



}
