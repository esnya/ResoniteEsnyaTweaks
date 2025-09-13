using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using ResoniteModLoader;
// using EsnyaTweaks.UIX; // not used
using EsnyaTweaks.SceneAuditor.Rules;

namespace EsnyaTweaks.SceneAuditor.Editor;

internal static class SceneAuditorPanel
{
    private const string PanelTitle = "Scene Auditor";

    private sealed class RuleToggles
    {
        public bool CheckLODOrder = true;
        public bool CheckLODDuplicates = true;
        public bool CheckRequiredEditorFields = true;
    }

    public static void Spawn(Slot context, Slot? initialRoot = null)
    {
        var world = context.World;
        var container = world.LocalUserSpace.AddSlot("[SceneAuditor] Panel");
        container.PersistentSelf = false;
        container.Tag = "Developer";
        // Place panel directly in front of the local user's view using engine helper
        var viewRot = world.LocalUserViewRotation;
        var faceDir = viewRot * float3.Forward;
        SlotPositioning.PositionInFrontOfUser(
            container,
            faceDirection: faceDir,
            offset: null,
            distance: 1.0f,
            user: world.LocalUser,
            scale: false,
            checkOcclusion: true,
            preserveUp: true
        );
        container.LocalScale = float3.One * 0.0005f;

        var size = new float2(1400f, 900f);
        var ui = RadiantUI_Panel.SetupPanel(container, PanelTitle, size, pinButton: true);
        ui.Style.TextAutoSizeMin = 0f;
        ui.Style.TextAutoSizeMax = 24f;
        ui.Style.TextColor = RadiantUI_Constants.TEXT_COLOR;

        // Root layout
        ui.VerticalLayout(6f, childAlignment: Alignment.TopCenter, forceExpandHeight: false);

        // Build columns directly via layouts (no extra Slot/Panel wrappers)
        var (leftColumn, resultsContainer) = BuildSplit(ui);

        // Build left pane: controls + search handler
        var results = new List<(IWorldElement Target, string Rule, string Detail)>();
        var (getRules, resolveScanRoot, searchBtn) = BuildLeftPane(ui, leftColumn, initialRoot);
        searchBtn.LocalPressed += (_, __) =>
        {
            results.Clear();
            var scanRoot = resolveScanRoot();
            if (scanRoot == null)
            {
                ResoniteMod.Warn("Search root is not set. Please select a root.");
                BuildResults(ui, resultsContainer, results, context);
                return;
            }
            try
            {
                var toggles = getRules();
                RunSelectedRules(scanRoot, toggles, results);
                ResoniteMod.Msg($"Scan complete. Found {results.Count} item(s).");
            }
            catch (Exception ex)
            {
                ResoniteMod.Warn($"Scan failed: {ex.Message}");
            }
            BuildResults(ui, resultsContainer, results, context);
        };

        // Initial render
        BuildResults(ui, resultsContainer, results, context);
    }

    private static (Slot leftColumn, Slot rightResultsContainer) BuildSplit(UIBuilder ui)
    {
        // Use HorizontalLayout as the row container; create columns directly.
        ui.HorizontalLayout(8f);

        // Left column directly as a VerticalLayout child
        ui.PushStyle();
        ui.Style.FlexibleWidth = 0.4f;
        var leftColumn = ui.VerticalLayout(8f, childAlignment: Alignment.TopLeft, forceExpandHeight: false).Slot;
        // Return to the horizontal container before building the right side
        ui.NestOut();
        ui.PopStyle();

        // Right column: ScrollArea then results container
        ui.PushStyle();
        ui.Style.FlexibleWidth = 0.6f;
        ui.PushStyle();
        ui.Style.FlexibleHeight = 1f;
        ui.ScrollArea(Alignment.TopLeft);
        // Ensure scroll content grows to preferred vertical size so items are visible
        ui.FitContent(SizeFit.Disabled, SizeFit.PreferredSize);
        ui.PopStyle();
        var rightResultsContainer = ui.VerticalLayout(6f, childAlignment: Alignment.TopLeft, forceExpandHeight: false).Slot;
        // Return to the horizontal container after creating the right column contents
        ui.NestOut(); // out of VerticalLayout
        ui.NestOut(); // out of ScrollArea content
        ui.PopStyle();

        return (leftColumn, rightResultsContainer);
    }

    private static (Func<RuleToggles> getRules, Func<Slot?> resolveScanRoot, Button searchButton) BuildLeftPane(
        UIBuilder ui,
        Slot leftColumn,
        Slot? initialRoot
    )
    {
        ui.NestInto(leftColumn);

        // Root selector (no Panel wrapper)
        ui.Text("Search Root");
        ui.Next("RootRefEditor");
        var rootRefSlot = ui.Current;
        var rootProxy = rootRefSlot.AttachComponent<ReferenceProxy>();
        var rootRef = rootProxy.Reference;
        // Initial value should be null unless explicitly provided
        if (initialRoot != null)
        {
            rootRef.Target = initialRoot;
        }
        // Inspector-consistent minimum height for the reference editor
        var rootLayout = rootRefSlot.GetComponent<LayoutElement>() ?? rootRefSlot.AttachComponent<LayoutElement>();
        rootLayout.MinHeight.Value = 24f;
        rootRefSlot.AttachComponent<RefEditor>().Setup(rootRef);

        // Rule toggles (vertical, inspector-style checkboxes)
        ui.Text("Rules");
        var toggles = new RuleToggles();
        ui.PushStyle();
        ui.Style.MinHeight = 24f;
        // Checkboxes stacked vertically
        var orderChk = ui.Checkbox("LOD: Non-descending order", toggles.CheckLODOrder);
        var dupChk = ui.Checkbox("LOD: Duplicate renderer owners", toggles.CheckLODDuplicates);
        var requiredEditorsChk = ui.Checkbox("Inspector: Required editor fields assigned", toggles.CheckRequiredEditorFields);
        ui.PopStyle();

        // Search button (disabled until root is selected)
        ui.PushStyle();
        ui.Style.MinHeight = 24f; // Inspector-consistent
        var searchBtn = ui.Button("Search");
        searchBtn.Enabled = rootRef.Target != null;
        // Toggle enabled state on root reference changes
        rootRef.OnTargetChange += _ =>
        {
            searchBtn.Enabled = rootRef.Target != null;
        };
        ui.PopStyle();

        // Exit left pane nesting
        ui.NestOut();

        Slot? Resolve()
        {
            return rootRef.Target as Slot;
        }
        RuleToggles GetRules()
        {
            return new RuleToggles
            {
                CheckLODOrder = orderChk.IsChecked,
                CheckLODDuplicates = dupChk.IsChecked,
                CheckRequiredEditorFields = requiredEditorsChk.IsChecked,
            };
        }
        return (GetRules, Resolve, searchBtn);
    }

    private static void BuildResults(UIBuilder ui, Slot container, List<(IWorldElement Target, string Rule, string Detail)> results, Slot inspectorContext)
    {
        container.DestroyChildren();
        ui.NestInto(container);
        ui.VerticalLayout(6f, childAlignment: Alignment.TopLeft);

        // Header row with inspector-consistent height
        ui.PushStyle();
        ui.Style.MinHeight = 24f;
        ui.HorizontalLayout(6f, 0f, Alignment.MiddleLeft);
        ui.Text($"Results: {results.Count}");
        ui.Spacer(1);
        ui.PushStyle();
        ui.Style.MinHeight = 24f; // Inspector-consistent
        var openBtn = ui.Button("Open all WorkerInspectors");
        ui.PopStyle();
        openBtn.LocalPressed += (_, __) =>
        {
            var opened = 0;
            foreach (var (target, _, _) in results)
            {
                if (target is Worker w)
                {
                    w.OpenInspectorForTarget(inspectorContext, openWorkerOnly: true);
                    opened++;
                }
                else if (target is Slot s)
                {
                    s.OpenInspectorForTarget(inspectorContext, openWorkerOnly: true);
                    opened++;
                }
            }
            ResoniteMod.Msg($"Opened inspectors for {opened} item(s).");
        };
        ui.NestOut();
        ui.PopStyle();

        ui.VerticalLayout(4f);
        foreach (var (target, rule, detail) in results)
        {
            // Row container without Panel; simple Slot + HorizontalLayout
            ui.Next("Row");
            var rowSlot = ui.Current;
            // Guard: ensure visible height on the row slot itself
            var rowLayout = rowSlot.GetComponent<LayoutElement>() ?? rowSlot.AttachComponent<LayoutElement>();
            rowLayout.MinHeight.Value = 24f;
            ui.NestInto(rowSlot);
            // Ensure each row has a minimum height via style before layout creation
            ui.PushStyle();
            ui.Style.MinHeight = 24f;
            ui.Style.ForceExpandHeight = true;
            ui.HorizontalLayout(6f, 0f, Alignment.MiddleLeft);
            // Left: component RefEditor (fixed width)
            ui.PushStyle();
            ui.Style.FlexibleWidth = 0f;
            ui.Style.MinWidth = 320f;
            var refSlot = CreateRefEditor(ui, target);
            ui.PopStyle();
            // Right: rule text aligned to the right end
            ui.Spacer(1);
            ui.Text(rule);
            ui.NestOut(); // HorizontalLayout
            ui.PopStyle();
            ui.NestOut(); // Row
        }
        ui.NestOut();
    }

    private static void RunSelectedRules(Slot root, RuleToggles toggles, List<(IWorldElement Target, string Rule, string Detail)> results)
    {
        var groups = Pool.BorrowList<LODGroup>();
        try
        {
            if (toggles.CheckLODOrder || toggles.CheckLODDuplicates)
            {
                root.GetComponentsInChildren(groups);
            }

            if (toggles.CheckLODOrder)
            {
                foreach (var g in groups)
                {
                    if (g?.LODs == null || g.LODs.Count <= 1)
                    {
                        continue;
                    }
                    var heights = GetHeights(g);
                    var violations = DetectionPrimitives.FindNonDescendingIndices(heights);
                    if (violations.Count > 0)
                    {
                        results.Add((g, "LOD: Non-descending order", $"Heights=[{string.Join(", ", heights.Select(h => h.ToString("F6", CultureInfo.InvariantCulture)))}], Violations=[{string.Join(", ", violations)}]"));
                    }
                }
            }

            if (toggles.CheckLODDuplicates)
            {
                var index = DetectionPrimitives.BuildDuplicateOwnersIndex(
                    groups.Select(g => (g, EnumerateRenderers(g)))
                );
                foreach (var (renderer, owners) in index)
                {
                    if (owners.Count > 1)
                    {
                        foreach (var g in owners)
                        {
                            results.Add((g, "LOD: Duplicate renderer owners", renderer.ToString() ?? "Renderer"));
                        }
                    }
                }
            }

            if (toggles.CheckRequiredEditorFields)
            {
                DetectMissingRequiredEditorFields(root, results);
            }
        }
        finally
        {
            Pool.Return(ref groups);
        }
    }

    private static void DetectMissingRequiredEditorFields(Slot root, List<(IWorldElement Target, string Rule, string Detail)> results)
    {
        // Check MemberEditor-derived components for required assignments without using reflection on public API.
        // For protected/private internals, use reflection with exact member names based on local analysis.
        var components = Pool.BorrowList<Component>();
        try
        {
            root.GetComponentsInChildren(components);
            foreach (var c in components)
            {
                var t = c?.GetType();
                if (t == null)
                {
                    continue;
                }

                // PrimitiveMemberEditor checks: UI wiring used by TextChanged must be present
                if (c is PrimitiveMemberEditor pme)
                {
                    // Use public sync accessor via GetSyncMember index 7 => _textEditor (SyncRef<TextEditor>)
                    if (pme.GetSyncMember(7) is not SyncRef<TextEditor> teRef)
                    {
                        // Cannot evaluate; skip
                        continue;
                    }
                    // Only flag when TextEditor itself is missing. Text (IText) missing is out of scope.
                    if (teRef.Target == null)
                    {
                        results.Add((c!, "Inspector: Primitive editor TextEditor missing", nameof(PrimitiveMemberEditor)));
                    }
                }
            }
        }
        finally
        {
            Pool.Return(ref components);
        }
    }

    // No reflection helpers needed for public GetSyncMember access

    private static float[] GetHeights(LODGroup group)
    {
        var lods = group?.LODs;
        if (lods == null)
        {
            return [];
        }
        var arr = new float[lods.Count];
        for (var i = 0; i < lods.Count; i++)
        {
            arr[i] = lods[i]?.ScreenRelativeTransitionHeight.Value ?? 0f;
        }
        return arr;
    }

    private static IEnumerable<MeshRenderer> EnumerateRenderers(LODGroup group)
    {
        var lods = group?.LODs;
        if (lods == null)
        {
            yield break;
        }
        foreach (var lod in lods)
        {
            var list = lod?.Renderers;
            if (list == null)
            {
                continue;
            }
            foreach (var r in list)
            {
                if (r != null)
                {
                    yield return r;
                }
            }
        }
    }

    private static Slot CreateRefEditor(UIBuilder ui, IWorldElement? value)
    {
        ui.Next("Ref");
        var editorSlot = ui.Current;
        var layout = editorSlot.GetComponent<LayoutElement>() ?? editorSlot.AttachComponent<LayoutElement>();
        layout.MinHeight.Value = 24f; // Inspector-consistent control height
        var proxy = editorSlot.AttachComponent<ReferenceProxy>();
        var referenceRef = proxy.Reference;
        if (value is not null)
        {
            referenceRef.Target = value;
        }
        ui.Current.AttachComponent<RefEditor>().Setup(referenceRef);
        return editorSlot;
    }
}
