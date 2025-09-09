using System;
using System.Collections.Generic;
using System.Globalization;
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
    private const string SCAN_LABEL = "[Mod] Scan LOD issues & spawn report";
    private const string FIX_LABEL = "[Mod] Fix LOD issues by sorting";
    private const string OPEN_INSPECTORS_LABEL = "[Mod] Open inspectors for violating LODGroups";

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
        Button(ui, SCAN_LABEL, _ => LODIssuesReport.ScanAndShow(lodGroup));
        Button(ui, FIX_LABEL, _ => FixLODGroupsBySortingAndShowResult(lodGroup));
        Button(ui, OPEN_INSPECTORS_LABEL, _ => OpenInspectorsForViolatingLODGroups(lodGroup));
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
            lodGroup.AddLOD(0.01f, renderers.ToArray());
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
        lodGroup.AddLOD(baseThreshold / levelSize, rendererWithBounds.Select(p => p.Key).ToArray());
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

    private static void FixLODGroupsBySortingAndShowResult(LODGroup lodGroup)
    {
        var lods = lodGroup.LODs;
        if (lods == null || lods.Count <= 1)
        {
            ResoniteMod.Msg("Nothing to fix (no or single LOD level).");
            return;
        }

        // Snapshot current heights
        var before = lods.Select(l => l?.ScreenRelativeTransitionHeight.Value ?? 0f).ToArray();

        // Build descending, strictly decreasing target sequence
        var target = before.OrderByDescending(h => h).ToArray();
        if (target.Length > 0)
        {
            // Enforce strict descending by nudging ties downward by 1 ULP
            var prev = float.PositiveInfinity;
            for (var i = 0; i < target.Length; i++)
            {
                var h = target[i];
                if (h >= prev)
                {
                    h = MathF.BitDecrement(prev);
                }
                if (h < 0f)
                {
                    h = 0f;
                }
                target[i] = h;
                prev = h;
            }
        }

        // Apply back to components in their current order
        for (var i = 0; i < lods.Count && i < target.Length; i++)
        {
            var l = lods[i];
            if (l != null)
            {
                l.ScreenRelativeTransitionHeight.Value = target[i];
            }
        }

        var after = lods.Select(l => l?.ScreenRelativeTransitionHeight.Value ?? 0f).ToArray();

        if (ResoniteMod.IsDebugEnabled())
        {
            var bh = string.Join(", ", before.Select(h => h.ToString("F6", CultureInfo.InvariantCulture)));
            var ah = string.Join(", ", after.Select(h => h.ToString("F6", CultureInfo.InvariantCulture)));
            ResoniteMod.Debug($"LOD thresholds fixed (descending).\nBefore: [{bh}]\nAfter : [{ah}]");
        }
        ResoniteMod.Msg("LOD thresholds normalized to descending order.");
    }

    private static void OpenInspectorsForViolatingLODGroups(LODGroup context)
    {
        var world = context.Slot.World;
        var root = world.RootSlot;

        var groups = Pool.BorrowList<LODGroup>();
        var violating = Pool.BorrowList<LODGroup>();
        var duplicateOwners = new HashSet<LODGroup>();
        try
        {
            root.GetComponentsInChildren(groups);

            // Detect duplicates across all groups
            var dupIndex = new Dictionary<MeshRenderer, HashSet<LODGroup>>();
            foreach (var g in groups)
            {
                foreach (var r in LODValidation.EnumerateRenderers(g))
                {
                    if (r == null)
                    {
                        continue;
                    }
                    if (!dupIndex.TryGetValue(r, out var set))
                    {
                        set = [];
                        dupIndex[r] = set;
                    }
                    set.Add(g);
                }
            }
            foreach (var owners in dupIndex.Values)
            {
                if (owners.Count > 1)
                {
                    foreach (var g in owners)
                    {
                        duplicateOwners.Add(g);
                    }
                }
            }

            foreach (var g in groups)
            {
                if (g == null || g.LODs == null || g.LODs.Count <= 1)
                {
                    continue;
                }
                var heights = LODValidation.GetHeights(g);
                if (LODValidation.HasOrderViolation(heights))
                {
                    violating.Add(g);
                }
            }

            foreach (var g in duplicateOwners)
            {
                if (!violating.Contains(g))
                {
                    violating.Add(g);
                }
            }

            foreach (var g in violating)
            {
                g.OpenInspectorForTarget(context.Slot, openWorkerOnly: true);
            }

            if (violating.Count == 0)
            {
                ResoniteMod.Msg("No violating LODGroups found.");
            }
            else
            {
                ResoniteMod.Msg($"Opened inspectors for {violating.Count} violating LODGroup(s).");
            }
        }
        finally
        {
            Pool.Return(ref violating);
            Pool.Return(ref groups);
        }
    }

}
