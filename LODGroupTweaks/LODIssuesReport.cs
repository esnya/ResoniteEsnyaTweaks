using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using ResoniteModLoader;

namespace EsnyaTweaks.LODGroupTweaks;

/// <summary>
/// Builds and shows the LOD issues report panel and contains the scanning logic.
/// </summary>
internal static class LODIssuesReport
{
    public static void ScanAndShow(LODGroup context, Slot? scanRoot = null)
    {
        var world = context.Slot.World;
        var root = scanRoot ?? world.RootSlot;

        var groups = Pool.BorrowList<LODGroup>();
        var problematic = Pool.BorrowList<(LODGroup Group, float[] Heights, int[] Violations)>();
        var duplicateIndex = new Dictionary<MeshRenderer, HashSet<LODGroup>>();
        try
        {
            root.GetComponentsInChildren(groups);

            foreach (var g in groups)
            {
                if (g.LODs.Count <= 1)
                {
                    continue;
                }

                var heights = LODValidation.GetHeights(g);

                var violations = new List<int>();
                for (var i = 0; i < heights.Length - 1; i++)
                {
                    if (heights[i] <= heights[i + 1])
                    {
                        violations.Add(i);
                    }
                }

                if (violations.Count > 0)
                {
                    problematic.Add((g, heights, violations.ToArray()));
                }

                foreach (var r in LODValidation.EnumerateRenderers(g))
                {
                    if (r == null)
                    {
                        continue;
                    }
                    if (!duplicateIndex.TryGetValue(r, out var set))
                    {
                        set = [];
                        duplicateIndex[r] = set;
                    }
                    set.Add(g);
                }
            }

            var duplicates = duplicateIndex.Where(kvp => kvp.Value.Count > 1).ToArray();

            SpawnReportPanel(context, groups.Count, problematic, duplicates);
        }
        finally
        {
            Pool.Return(ref problematic);
            Pool.Return(ref groups);
        }
    }

    private static void SpawnReportPanel(
        LODGroup context,
        int scanned,
        List<(LODGroup Group, float[] Heights, int[] Violations)> problematic,
        KeyValuePair<MeshRenderer, HashSet<LODGroup>>[] duplicates
    )
    {
        var container = context.World.LocalUserSpace.AddSlot("[LODGroupTweaks] LOD Issues");
        container.PersistentSelf = false; // top-most slot should not persist
        container.Tag = "Developer";
        container.PositionInFrontOfUser();
        container.LocalScale = float3.One * 0.0005f;

        var panelSize = new float2(1200f, 800f);
        var ui = RadiantUI_Panel.SetupPanel(container, "LOD Issues Report", panelSize, pinButton: true);

        ui.Style.TextColor = RadiantUI_Constants.TEXT_COLOR;
        ui.Style.TextAutoSizeMin = 0f;
        ui.Style.TextAutoSizeMax = 24f;

        ui.VerticalLayout(4f, forceExpandHeight: false);

        ui.Text($"Scanned: {scanned}, Problems: {problematic.Count}, Duplicates: {duplicates.Length}");

        ui.PushStyle();
        ui.Style.FlexibleHeight = 1.0f;
        ui.ScrollArea();
        ui.PopStyle();

        var mainArea = ui.VerticalLayout(4f, childAlignment: Alignment.TopCenter, forceExpandHeight: false).Slot;

        ui.NestInto(mainArea);
        BuildOrderViolationsSection(ui, context, problematic);

        ui.NestInto(mainArea);
        BuildDuplicateRenderersSection(ui, context, duplicates);

        ResoniteMod.Msg(problematic.Count == 0 && duplicates.Length == 0
            ? "No LOD issues found."
            : $"Found {problematic.Count} order issue(s) and {duplicates.Length} duplicate renderer issue(s). Panel spawned under {context.Slot}");
    }

    private static void BuildOrderViolationsSection(
        UIBuilder ui,
        LODGroup context,
        List<(LODGroup Group, float[] Heights, int[] Violations)> problematic
    )
    {
        ui.PushStyle();
        ui.Style.MinHeight = 64.0f;

        // Section container (vertical stacking)
        ui.VerticalLayout(8f);

        // Header row
        ui.Text("Order violations (h[i] <= h[i+1])");

        // Content list
        ui.VerticalLayout(8f);
        ui.Style.MinHeight = RadiantUI_Constants.GRID_CELL_SIZE;
        ui.Style.ForceExpandWidth = true;
        foreach (var (group, heights, violations) in problematic)
        {
            var row = ui.Panel();
            row.Slot.Name = group.Slot.Name;
            ui.NestInto(row.Slot);
            ui.VerticalLayout(6f);

            // Top line: refs + actions
            ui.HorizontalLayout(4f, 0f, Alignment.MiddleLeft);
            CreateRefEditor(ui, group);
            CreateRefEditor(ui, group.Slot);
            ui.Spacer(1);
            var fixBtn = ui.Button("Fix");
            fixBtn.LocalPressed += (_, __) =>
            {
                NormalizeLODHeights(group);
                ResoniteMod.Msg($"Normalized thresholds for '{group.Slot.Name}'.");
            };
            ui.NestOut();

            // Detail line: heights/violations
            ui.HorizontalLayout(4f, 0f, Alignment.MiddleLeft);
            ui.Text("Heights: [" + string.Join(", ", heights.Select(h => h.ToString("F6", CultureInfo.InvariantCulture))) + "]  Violations: [" + string.Join(", ", violations) + "]");
            ui.NestOut();

            ui.NestOut(); // row
        }
        // End of section
        ui.PopStyle();
    }

    private static void BuildDuplicateRenderersSection(
        UIBuilder ui,
        LODGroup context,
        KeyValuePair<MeshRenderer, HashSet<LODGroup>>[] duplicates
    )
    {
        ui.PushStyle();
        ui.Style.MinHeight = 64.0f;

        // Section container (vertical stacking)
        ui.VerticalLayout(8f);

        // Header row
        ui.Text("Duplicate renderer registrations");

        // Content list (tree-like)
        ui.PushStyle();
        ui.Style.MinHeight = 32.0f;
        ui.VerticalLayout(8f);
        foreach (var (renderer, owners) in duplicates)
        {
            var row = ui.Panel();
            row.Slot.Name = renderer.Slot.Name;
            ui.NestInto(row.Slot);
            ui.VerticalLayout(6f);

            // Header line (renderer + actions)
            ui.HorizontalLayout(8f, 0f, Alignment.MiddleLeft);
            CreateRefEditor(ui, renderer);
            ui.Spacer(1);
            var openOwnersBtn = ui.Button("Open owners");
            openOwnersBtn.LocalPressed += (_, __) =>
            {
                var opened = 0;
                foreach (var g in owners)
                {
                    g.OpenInspectorForTarget(context.Slot, openWorkerOnly: true);
                    opened++;
                }
                ResoniteMod.Msg($"Opened inspectors for {opened} owner group(s).");
            };
            ui.NestOut();

            // Indented owners list
            var ownersContainer = ui.Panel();
            ui.NestInto(ownersContainer.Slot);
            ui.HorizontalLayout(4f, 24f, Alignment.MiddleLeft); // left indent via padding
            var ownersList = ui.Panel();
            ui.NestInto(ownersList.Slot);
            ui.VerticalLayout(4f);
            foreach (var owner in owners)
            {
                var ownerRow = ui.Panel();
                ui.NestInto(ownerRow.Slot);
                ui.HorizontalLayout(4f, 0f, Alignment.MiddleLeft);
                CreateRefEditor(ui, owner);
                ui.NestOut();
            }
            ui.NestOut(); // ownersList
            ui.NestOut(); // ownersContainer

            ui.NestOut(); // row
        }
        ui.PopStyle();
    }

    private static void CreateRefEditor<T>(UIBuilder ui, T? value)
        where T : class, IWorldElement
    {
        ui.Next("Ref");
        var editorSlot = ui.Current;
        var proxy = editorSlot.AttachComponent<ReferenceProxy>();

        var referenceRef = proxy.Reference;
        if (value is not null)
        {
            referenceRef.Target = value;
        }
        ui.Current.AttachComponent<RefEditor>().Setup(referenceRef);
    }

    private static void NormalizeLODHeights(LODGroup lodGroup)
    {
        var lods = lodGroup.LODs;
        if (lods == null || lods.Count <= 1)
        {
            return;
        }

        var before = lods.Select(l => l?.ScreenRelativeTransitionHeight.Value ?? 0f).ToArray();

        var target = before.OrderByDescending(h => h).ToArray();
        if (target.Length > 0)
        {
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

        for (var i = 0; i < lods.Count && i < target.Length; i++)
        {
            var l = lods[i];
            if (l != null)
            {
                l.ScreenRelativeTransitionHeight.Value = target[i];
            }
        }
    }
}
