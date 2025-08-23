using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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
    private const string FIX_LABEL = "[Mod] Fix issues by sorting (no epsilon) (Not Undoable)";

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

    private static void PatchSliderUI(LODGroup lodGroup, UIBuilder ui)
    {
        var root = ui.Root;
        foreach (var listEditor in ui.Canvas.Slot.GetComponentsInChildren<ListEditor>())
        {
            lodGroup.StartTask(
                async delegate
                {
                    var sliders = Pool.BorrowList<SliderMemberEditor>();

                    try
                    {
                        while (!root.IsDestroyed)
                        {
                            root.GetComponentsInChildren(sliders);

                            if (sliders.Count > 0)
                            {
                                foreach (var slider in sliders)
                                {
                                    slider.TextFormat = "F4";
                                }

                                return;
                            }

                            await default(NextUpdate);
                        }
                    }
                    finally
                    {
                        Pool.Return(ref sliders);
                    }
                }
            );
        }
    }

    private static void BuildInspectorUI(LODGroup lodGroup, UIBuilder ui)
    {
        Button(ui, ADD_LABEL, button => SetupFromChildren(button, lodGroup));
        Button(ui, SETUP_LABEL, button => SetupByParts(button, lodGroup));
        Button(ui, REMOVE_LABEL, button => RemoveFromChildren(button, lodGroup));
        Button(ui, SCAN_LABEL, _ => ScanAllLODGroupsAndSpawnReport(lodGroup));
        Button(ui, FIX_LABEL, _ => FixLODGroupsBySortingAndShowResult(lodGroup));
    }

    // (moved) Use Slot.PositionInFrontOfUser extension

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

    private static void ScanAllLODGroupsAndSpawnReport(LODGroup context)
    {
        var world = context.Slot.World;
        var root = world.RootSlot;

        var groups = Pool.BorrowList<LODGroup>();
        var problematic = Pool.BorrowList<(LODGroup Group, float[] Heights, int[] Violations)>();
        try
        {
            root.GetComponentsInChildren(groups);

            foreach (var g in groups)
            {
                if (g.LODs.Count <= 1)
                {
                    continue;
                }

                var heights = g.LODs
                    .Select(l => l.ScreenRelativeTransitionHeight.Value)
                    .ToArray();

                var violations = new List<int>();
                for (var i = 0; i < heights.Length - 1; i++)
                {
                    // Report strictly ascending and equal values as issues.
                    if (heights[i] <= heights[i + 1])
                    {
                        violations.Add(i);
                    }
                }

                if (violations.Count > 0)
                {
                    problematic.Add((g, heights, violations.ToArray()));
                }
            }

            var sb = new StringBuilder();
            sb.AppendLine("LOD issues report (ascending or equal threshold issues)");
            sb.AppendLine(CultureInfo.InvariantCulture, $"Scanned: {groups.Count}, Problems: {problematic.Count}");

            foreach (var (group, heights, violations) in problematic)
            {
                var slot = group.Slot;
                var slotName = slot.Name;
                // Build path manually from root
                var names = new Stack<string>();
                for (var s = slot; s != null; s = s.Parent)
                {
                    names.Push(s.Name);
                }
                var path = string.Join('/', names);

                var heightsText = string.Join(
                    ", ",
                    heights.Select(h => h.ToString("F6", CultureInfo.InvariantCulture))
                );
                var idxText = string.Join(", ", violations);
                sb.AppendLine(CultureInfo.InvariantCulture, $"- {slotName} ({path})");
                sb.AppendLine(CultureInfo.InvariantCulture, $"  Heights: [{heightsText}]");
                sb.AppendLine(CultureInfo.InvariantCulture, $"  Violations at indices: [{idxText}] (h[i] <= h[i+1])");
            }

            var report = sb.ToString();
            if (problematic.Count == 0)
            {
                ResoniteMod.Msg("[LODGroupTweaks] No LOD issues found.");
            }
            else
            {
                ResoniteMod.Msg($"[LODGroupTweaks] Found {problematic.Count} LODGroup issues. Report spawned under {context.Slot}");
            }
            var parentForReport = world.LocalUserSpace;
            if (parentForReport == null)
            {
                ResoniteMod.Warn("[LODGroupTweaks] LocalUserSpace not available. Report will not be spawned (no fallback).");
                return;
            }
            var container = parentForReport.AddSlot("[LODGroupTweaks] LOD issues report");
            container.PositionInFrontOfUser();

            // Spawn text via UniversalImporter inside the container
            UniversalImporter.SpawnText(
                container,
                "[LODGroupTweaks] LOD issues report",
                report,
                16f,
                null,
                allowRTF: false
            );

            // No fix button here; fix is available as a separate inspector button
        }
        finally
        {
            Pool.Return(ref problematic);
            Pool.Return(ref groups);
        }
    }

    // Sort-only fix across all LODGroups; equal values are considered OK and won't be modified.
    private static void FixLODGroupsBySortingAndShowResult(LODGroup context, Slot container)
    {
        var world = context.Slot.World;
        var root = world.RootSlot;

        var groups = Pool.BorrowList<LODGroup>();
        try
        {
            root.GetComponentsInChildren(groups);

            var changedCount = 0;
            var attempted = 0;

            // Note: Not undoable (list/ordering changes aren't undo-tracked reliably)
            foreach (var g in groups)
            {
                if (g.LODs.Count <= 1)
                {
                    continue;
                }

                attempted++;
                if (ReorderBySortingOnly(g))
                {
                    changedCount++;
                }
            }

            // Re-scan to compute remaining issues (strictly ascending only)
            var remainingIssues = 0;
            foreach (var g in groups)
            {
                if (g.LODs.Count <= 1)
                {
                    continue;
                }

                var heights = g.LODs.Select(l => l.ScreenRelativeTransitionHeight.Value).ToArray();
                for (var i = 0; i < heights.Length - 1; i++)
                {
                    if (heights[i] <= heights[i + 1])
                    {
                        remainingIssues++;
                        break;
                    }
                }
            }

            var summary = $"Fixed groups: {changedCount} / {attempted}; Remaining problematic groups: {remainingIssues}";
            ResoniteMod.Msg($"[LODGroupTweaks] {summary}");

            // Spawn a summary text into the provided container only if available (no fallback)
            if (container != null)
            {
                UniversalImporter.SpawnText(
                    container,
                    "[LODGroupTweaks] LOD fix result",
                    summary,
                    16f,
                    null,
                    allowRTF: false
                );
            }
            else
            {
                ResoniteMod.Warn("[LODGroupTweaks] LocalUserSpace not available. Fix result will not be spawned (no fallback).");
            }
        }
        finally
        {
            Pool.Return(ref groups);
        }
    }

    // Overload for inspector button: create a result container automatically
    private static void FixLODGroupsBySortingAndShowResult(LODGroup context)
    {
        var parent = context.Slot.World.LocalUserSpace;
        if (parent == null)
        {
            ResoniteMod.Warn("[LODGroupTweaks] LocalUserSpace not available. Fix result will not be spawned (no fallback).");
            return;
        }
        var container = parent.AddSlot("[LODGroupTweaks] LOD fix result");
        container.PositionInFrontOfUser();
        FixLODGroupsBySortingAndShowResult(context, container);
    }

    private static bool ReorderBySortingOnly(LODGroup g)
    {
        try
        {
            var lods = g.LODs;

            var items = lods.ToArray();
            var heights = items.Select(l => l.ScreenRelativeTransitionHeight.Value).ToArray();
            var sorted = heights.OrderByDescending(v => v).ToArray();

            var changed = false;
            for (var i = 0; i < heights.Length; i++)
            {
                if (heights[i] != sorted[i])
                {
                    changed = true;
                    break;
                }
            }
            if (!changed)
            {
                return false;
            }

            for (var i = 0; i < items.Length; i++)
            {
                if (heights[i] != sorted[i])
                {
                    items[i].ScreenRelativeTransitionHeight.Value = sorted[i];
                }
            }

            ResoniteMod.DebugFunc(() =>
                $"[LODGroupTweaks] Sorted LOD heights on {g.Slot}. Before: [{string.Join(", ", heights.Select(h => h.ToString("F6", CultureInfo.InvariantCulture)))}], After: [{string.Join(", ", sorted.Select(h => h.ToString("F6", CultureInfo.InvariantCulture)))}]"
            );
            return true;
        }
        catch (Exception ex)
        {
            ResoniteMod.Warn($"[LODGroupTweaks] Failed to reorder LODGroup by sorting: {ex}");
            return false;
        }
    }

    // (removed wrapper)

    private static void SetupFromChildren(Button _, LODGroup lodGroup)
    {
        if (lodGroup.UpdateOrder == 0)
        {
            lodGroup.UpdateOrder = 1000;
        }

        lodGroup.AddLOD(0.01f, lodGroup.Slot);
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

        var largeRanderers = Pool.BorrowList<KeyValuePair<MeshRenderer, float>>();
        var rendererWithScore = Pool.BorrowList<KeyValuePair<MeshRenderer, float>>();
        var renderers = Pool.BorrowList<MeshRenderer>();
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
            var thresholdIndex = rendererWithScore.FindLastIndex(p => p.Value > sizeThreshold);
            if (thresholdIndex > 0)
            {
                largeRanderers.AddRange(rendererWithScore.Take(thresholdIndex));
                AddLOD(lodGroup, 0.005f * totalSize, rendererWithScore);
                AddLOD(lodGroup, 0.005f * totalSize, largeRanderers);
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
            Pool.Return(ref largeRanderers);
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
