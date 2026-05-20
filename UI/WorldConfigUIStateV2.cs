using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using WorldConfigMod.Common;
using WorldConfigMod.Common.Systems;
using WorldConfigMod.Common.UI;
using WorldConfigMod.Core;
using WorldConfigMod.UI.Elements;

namespace WorldConfigMod.UI;

// Compact sidebar-nav redesign. Toggle via WorldGenConfig.UseV2Panel.
public class WorldConfigUIStateV2 : UIState
{
    private enum Tab { World, Shape, Features, Ores, Presets, Info }

    private const float HeaderH    = 44f;
    private const float SummaryH   = 26f;
    private const float FooterH    = 56f;
    private const float SidebarW   = 168f;
    private const float Gap        = 6f;

    private UIPanel _panel;
    private UIPanel _content;
    private UIList  _contentList;
    private UIScrollbar _contentScroll;
    private UIScrollColumn _contentScrollCol;
    private UIText _summaryText;
    private UIText _ureChangeBadge;

    private UITextInput _filter;
    private string _filterText = "";

    private Tab _tab = Tab.World;
    private readonly List<(Tab tab, UITextButton btn, UIText badge)> _navButtons = new();

    public override void OnInitialize()
    {
        _panel = new UIPanel
        {
            Width  = StyleDimension.FromPixelsAndPercent(0f, 0.82f),
            Height = StyleDimension.FromPixelsAndPercent(0f, 0.86f),
            HAlign = 0.5f,
            VAlign = 0.5f,
            BackgroundColor = new Color(14, 18, 36) * 0.97f,
            BorderColor = new Color(90, 115, 180),
            PaddingTop = 0f, PaddingBottom = 0f, PaddingLeft = 0f, PaddingRight = 0f,
        };
        Append(_panel);

        BuildHeader();
        BuildSummary();
        BuildSidebarAndContent();
        BuildFooter();
        Refresh();
    }

    public void Refresh()
    {
        RebuildContent();
        RefreshSummary();
        RefreshNavBadges();
        Recalculate();
    }

    public void ApplyScrollWheel(int delta)
    {
        if (delta == 0 || _contentScroll == null) return;

        if (!_contentScrollCol.ContainsPoint(Main.MouseScreen)
            && _contentScroll?.IsMouseHovering != true
            && _contentList?.IsMouseHovering != true)
            return;

        const float pixelsPerTick = 48f;
        float max = Math.Max(0f, _contentScroll.MaxViewSize);
        float next = _contentScroll.ViewPosition - Math.Sign(delta) * pixelsPerTick;
        _contentScroll.ViewPosition = MathHelper.Clamp(next, 0f, max);
    }

    // ---------- Header ----------
    private void BuildHeader()
    {
        var header = new UIPanel
        {
            Width  = StyleDimension.FromPixelsAndPercent(0f, 1f),
            Height = StyleDimension.FromPixels(HeaderH),
            HAlign = 0.5f, VAlign = 0f,
            BackgroundColor = new Color(26, 32, 60),
            BorderColor = new Color(70, 90, 140),
            PaddingTop = 0f, PaddingBottom = 0f, PaddingLeft = 0f, PaddingRight = 0f,
        };
        _panel.Append(header);

        header.Append(new UIText("World Config", 1.1f, true)
        {
            HAlign = 0f, VAlign = 0.5f,
            Left = StyleDimension.FromPixels(14f),
            TextColor = new Color(220, 230, 255),
        });

        var toggle = new UITextButton(WorldGenConfig.UseCustom ? "Custom: ON" : "Custom: OFF", 130f, 28f);
        toggle.HAlign = 1f; toggle.VAlign = 0.5f;
        toggle.Left = StyleDimension.FromPixels(-12f);
        toggle.BaseColor = WorldGenConfig.UseCustom ? new Color(40, 120, 60) : new Color(120, 40, 40);
        toggle.OnClick = () =>
        {
            WorldGenConfig.UseCustom = !WorldGenConfig.UseCustom;
            toggle.Text = WorldGenConfig.UseCustom ? "Custom: ON" : "Custom: OFF";
            toggle.BaseColor = WorldGenConfig.UseCustom ? new Color(40, 120, 60) : new Color(120, 40, 40);
        };
        header.Append(toggle);
    }

    // ---------- Summary strip ----------
    private void BuildSummary()
    {
        var bar = new UIPanel
        {
            Width  = StyleDimension.FromPixelsAndPercent(0f, 1f),
            Height = StyleDimension.FromPixels(SummaryH),
            Top    = StyleDimension.FromPixels(HeaderH),
            BackgroundColor = new Color(20, 26, 48),
            BorderColor = Color.Transparent,
            PaddingTop = 0f, PaddingBottom = 0f, PaddingLeft = 12f, PaddingRight = 12f,
        };
        _panel.Append(bar);

        _summaryText = new UIText("", 0.78f)
        {
            HAlign = 0f, VAlign = 0.5f,
            TextColor = new Color(170, 190, 230),
        };
        bar.Append(_summaryText);
    }

    private void RefreshSummary()
    {
        int w = WorldGenConfig.WorldWidth;
        int h = WorldGenConfig.WorldHeight;
        string sizeLabel = SizeLabel(w, h);
        long tiles = (long)w * h;
        int genEst = EstimateGenSeconds(w, h, WorldGenConfig.OreFrequencyMul, WorldGenConfig.OreVeinSizeMul);
        int diffs = WorldGenConfig.CountChanges();

        _summaryText.SetText(
            $"{w}×{h} · {sizeLabel} · {tiles / 1000}k tiles · est gen ~{genEst}s · "
          + $"ore freq ×{WorldGenConfig.OreFrequencyMul:0.00} · vein ×{WorldGenConfig.OreVeinSizeMul:0.00} · "
          + (diffs == 0 ? "all defaults" : $"{diffs} change{(diffs == 1 ? "" : "s")}"));
    }

    private static string SizeLabel(int w, int h)
    {
        if (w <= 4500)  return "Small";
        if (w <= 6800)  return "Medium";
        if (w <= 8800)  return "Large";
        if (w <= 12400) return "XL";
        return "XXL";
    }

    private static int EstimateGenSeconds(int w, int h, float freq, float vein)
    {
        // Rough: vanilla small ~15s baseline. Scale ~linear with tile count.
        double tileFactor = (double)w * h / (4200.0 * 1200.0);
        double oreCost = 1.0 + Math.Max(0.0, (freq * vein - 1.0)) * 0.15;
        return (int)Math.Round(15.0 * tileFactor * oreCost);
    }

    // ---------- Sidebar + content shell ----------
    private void BuildSidebarAndContent()
    {
        float bodyTop = HeaderH + SummaryH + Gap;
        float bodyBot = FooterH + Gap;

        var body = new UIElement
        {
            Width  = StyleDimension.FromPixelsAndPercent(0f, 1f),
            Height = StyleDimension.FromPixelsAndPercent(-(bodyTop + bodyBot), 1f),
            Top    = StyleDimension.FromPixels(bodyTop),
        };
        _panel.Append(body);

        // Sidebar
        var sidebar = new UIPanel
        {
            Width  = StyleDimension.FromPixels(SidebarW),
            Height = StyleDimension.Fill,
            HAlign = 0f,
            BackgroundColor = new Color(18, 24, 44) * 0.95f,
            BorderColor = new Color(50, 65, 105),
            PaddingTop = 8f, PaddingBottom = 8f, PaddingLeft = 8f, PaddingRight = 8f,
        };
        body.Append(sidebar);

        AddNav(sidebar, Tab.World,    "World",    0);
        AddNav(sidebar, Tab.Shape,    "Shape",    1);
        AddNav(sidebar, Tab.Features, "Features", 2);
        AddNav(sidebar, Tab.Ores,     "Ores",     3);
        AddNav(sidebar, Tab.Presets,  "Presets",  4);
        AddNav(sidebar, Tab.Info,     "Info",     5);

        // Sidebar bottom actions
        float btnH = 28f, gap = 4f;
        float bottomY = -btnH * 3 - gap * 2 - 4;
        sidebar.Append(MakeSidebarAction("Reset Defaults", bottomY,
            () => { WorldGenConfig.Reset(); Refresh(); }, new Color(120, 60, 60)));
        sidebar.Append(MakeSidebarAction("DEBUG: Tiny ×20", bottomY + btnH + gap,
            () => { WorldGenConfig.ApplyDebugWorldGenPreset();
                    ToastManager.Push(
                        $"DEBUG: {WorldGenConfig.WorldWidth}×{WorldGenConfig.WorldHeight}, ore ×{WorldGenConfig.OreFrequencyMul:0}",
                        new Color(255, 180, 80), 4.0);
                    Refresh(); }, new Color(160, 70, 40)));
        sidebar.Append(MakeSidebarAction("← Legacy Panel", bottomY + (btnH + gap) * 2,
            () => { WorldGenConfig.UseV2Panel = false; UIInjectSystem.ReopenConfigMenu(); },
            new Color(60, 60, 90)));

        // Content area
        _content = new UIPanel
        {
            Width  = StyleDimension.FromPixelsAndPercent(-(SidebarW + Gap), 1f),
            Height = StyleDimension.Fill,
            HAlign = 1f,
            BackgroundColor = new Color(22, 28, 52) * 0.95f,
            BorderColor = new Color(60, 75, 120),
            PaddingTop = 8f, PaddingBottom = 8f, PaddingLeft = 10f, PaddingRight = 8f,
        };
        body.Append(_content);

        _contentScrollCol = new UIScrollColumn
        {
            Width  = StyleDimension.Fill,
            Height = StyleDimension.Fill,
            BackgroundColor = Color.Transparent,
            BorderColor = Color.Transparent,
        };
        _content.Append(_contentScrollCol);

        _contentList = new UIList
        {
            Width = StyleDimension.FromPixelsAndPercent(-24f, 1f),
            Height = StyleDimension.Fill,
            ListPadding = 3f,
        };
        _contentList.ManualSortMethod = _ => { };
        _contentScrollCol.Append(_contentList);

        _contentScroll = new UIScrollbar
        {
            Height = StyleDimension.Fill,
            HAlign = 1f,
        };
        _contentScrollCol.Append(_contentScroll);
        _contentScrollCol.List = _contentList;
        _contentScrollCol.Scrollbar = _contentScroll;
        _contentList.SetScrollbar(_contentScroll);
    }

    private void AddNav(UIElement parent, Tab tab, string label, int index)
    {
        float btnH = 30f, gap = 3f;
        var btn = new UITextButton(label, SidebarW - 16f, btnH, () =>
        {
            _tab = tab;
            _contentScroll.ViewPosition = 0f;
            RebuildContent();
            RefreshNavBadges();
            Recalculate();
        });
        btn.Top = StyleDimension.FromPixels(index * (btnH + gap));
        btn.BaseColor = (_tab == tab) ? new Color(60, 90, 160) : new Color(28, 36, 64);
        btn.HoverColor = new Color(80, 120, 200);
        parent.Append(btn);

        var badge = new UIText("", 0.72f)
        {
            HAlign = 1f, VAlign = 0.5f,
            Top = StyleDimension.FromPixels(index * (btnH + gap)),
            Left = StyleDimension.FromPixels(-10f),
            TextColor = new Color(255, 200, 80),
        };
        var holder = new UIElement
        {
            Width = StyleDimension.FromPixels(SidebarW - 16f),
            Height = StyleDimension.FromPixels(btnH),
            Top = StyleDimension.FromPixels(index * (btnH + gap)),
        };
        // Append badge over btn — easier to just append to parent at same offset.
        parent.Append(badge);
        _navButtons.Add((tab, btn, badge));
    }

    private void RefreshNavBadges()
    {
        foreach (var (tab, btn, badge) in _navButtons)
        {
            btn.BaseColor = (_tab == tab) ? new Color(60, 90, 160) : new Color(28, 36, 64);
            int n = tab switch
            {
                Tab.World    => WorldGenConfig.CountWorldChanges() == 0 ? 0 : WorldGenConfig.CountWorldChanges(),
                Tab.Features => WorldGenConfig.CountFeatureChanges(),
                Tab.Ores     => WorldGenConfig.CountOreChanges(),
                _ => 0,
            };
            badge.SetText(n > 0 ? $"●{n}" : "");
        }
    }

    private UITextButton MakeSidebarAction(string label, float topOffsetFromBottom, Action onClick, Color color)
    {
        var b = new UITextButton(label, SidebarW - 16f, 28f, onClick);
        b.VAlign = 1f;
        b.Top = StyleDimension.FromPixels(topOffsetFromBottom);
        b.BaseColor = color;
        b.HoverColor = new Color(
            (byte)Math.Min(255, color.R + 50),
            (byte)Math.Min(255, color.G + 50),
            (byte)Math.Min(255, color.B + 50));
        return b;
    }

    // ---------- Content rebuild per tab ----------
    private void RebuildContent()
    {
        _contentList.Clear();
        switch (_tab)
        {
            case Tab.World:    BuildWorldTab();    break;
            case Tab.Shape:    BuildShapeTab();    break;
            case Tab.Features: BuildFeaturesTab(); break;
            case Tab.Ores:     BuildOresTab();     break;
            case Tab.Presets:  BuildPresetsTab();  break;
            case Tab.Info:     BuildInfoTab();     break;
        }
    }

    private void BuildWorldTab()
    {
        AddHeading("World Size");
        _contentList.Add(new UICompactSliderRow("Width",
            WorldGenConfig.MinWidth, WorldGenConfig.MaxWidth,
            WorldGenConfig.WorldWidth, 100f, isInt: true,
            v => $"{(int)v}",
            v => { WorldGenConfig.WorldWidth = (int)v; RefreshSummary(); RefreshNavBadges(); },
            defaultValue: WorldGenConfig.DefaultWorldWidth));
        _contentList.Add(new UICompactSliderRow("Height",
            WorldGenConfig.MinHeight, WorldGenConfig.MaxHeight,
            WorldGenConfig.WorldHeight, 100f, isInt: true,
            v => $"{(int)v}",
            v => { WorldGenConfig.WorldHeight = (int)v; RefreshSummary(); RefreshNavBadges(); },
            defaultValue: WorldGenConfig.DefaultWorldHeight));

        AddHeading("Quick Size");
        _contentList.Add(MakePresetButtonRow());

        AddHeading("Quick Info");
        _contentList.Add(MakeInfoLine(
            $"Selected: {WorldGenConfig.WorldWidth} × {WorldGenConfig.WorldHeight}  ({SizeLabel(WorldGenConfig.WorldWidth, WorldGenConfig.WorldHeight)})"));
        _contentList.Add(MakeInfoLine(
            WorldGenConfig.UseCustom ? "Custom generation: ON" : "Custom generation: OFF"));
    }

    private void BuildShapeTab()
    {
        AddHeading("Cave System");
        _contentList.Add(new UICompactSliderRow("Cave Depth", 0.5f, 2.0f,
            WorldGenConfig.CaveDepthMul, 0.05f,
            formatter: v => $"×{v:0.00}",
            onChanged: v => { WorldGenConfig.CaveDepthMul = v; RefreshSummary(); RefreshNavBadges(); },
            defaultValue: WorldGenConfig.DefaultCaveDepthMul,
            showVanillaBadge: true));

        AddHeading("Dungeon");
        _contentList.Add(MakeDungeonSideRow());
    }

    private void BuildFeaturesTab()
    {
        AddHeading("World Feature Multipliers");
        AddFeature("Gems",            v => WorldGenConfig.GemsMul = v,            () => WorldGenConfig.GemsMul,            WorldGenConfig.DefaultGemsMul);
        AddFeature("Life Crystals",   v => WorldGenConfig.LifeCrystalsMul = v,    () => WorldGenConfig.LifeCrystalsMul,    WorldGenConfig.DefaultLifeCrystalsMul);
        AddFeature("Chests",          v => WorldGenConfig.ChestsMul = v,          () => WorldGenConfig.ChestsMul,          WorldGenConfig.DefaultChestsMul);
        AddFeature("Floating Islands",v => WorldGenConfig.FloatingIslandsMul = v, () => WorldGenConfig.FloatingIslandsMul, WorldGenConfig.DefaultFloatingIslandsMul);
        AddFeature("Marble/Granite",  v => WorldGenConfig.MarbleGraniteMul = v,   () => WorldGenConfig.MarbleGraniteMul,   WorldGenConfig.DefaultMarbleGraniteMul);
    }

    private void AddFeature(string label, Action<float> setter, Func<float> getter, float def)
    {
        _contentList.Add(new UICompactSliderRow(label, 0f, 5f,
            getter(), 0.05f,
            formatter: v => $"×{v:0.00}",
            onChanged: v => { setter(v); RefreshNavBadges(); RefreshSummary(); },
            defaultValue: def,
            showVanillaBadge: true));
    }

    private void BuildOresTab()
    {
        // Filter bar
        var filterRow = new UIElement
        {
            Width  = StyleDimension.Fill,
            Height = StyleDimension.FromPixels(26f),
        };
        var lbl = new UIText("Filter:", 0.8f) { HAlign = 0f, VAlign = 0.5f, TextColor = new Color(160, 180, 220) };
        filterRow.Append(lbl);
        _filter = new UITextInput("type to filter ores…", s => { _filterText = s; RebuildContent(); RefreshNavBadges(); }, 220f, 22f);
        _filter.Text = _filterText;
        _filter.Left = StyleDimension.FromPixels(54f);
        _filter.VAlign = 0.5f;
        filterRow.Append(_filter);

        var clr = new UITextButton("×", 24f, 22f, () => { _filterText = ""; RebuildContent(); });
        clr.Left = StyleDimension.FromPixels(280f);
        clr.VAlign = 0.5f;
        clr.BaseColor = new Color(60, 40, 40);
        filterRow.Append(clr);
        _contentList.Add(filterRow);

        AddHeading("Global Ore Multipliers");
        _contentList.Add(new UICompactSliderRow("Vein Size", 0.25f, 25f,
            WorldGenConfig.OreVeinSizeMul, 0.05f,
            formatter: v => $"×{v:0.00}",
            onChanged: v => { WorldGenConfig.OreVeinSizeMul = v; RefreshSummary(); RefreshNavBadges(); },
            defaultValue: WorldGenConfig.DefaultOreVeinSizeMul,
            showVanillaBadge: true));
        _contentList.Add(new UICompactSliderRow("Frequency", 0.1f, 25f,
            WorldGenConfig.OreFrequencyMul, 0.05f,
            formatter: v => $"×{v:0.00}",
            onChanged: v => { WorldGenConfig.OreFrequencyMul = v; RefreshSummary(); RefreshNavBadges(); },
            defaultValue: WorldGenConfig.DefaultOreFrequencyMul,
            showVanillaBadge: true));

        AddHeading($"Per-Ore (19 world-gen)");

        string f = (_filterText ?? "").Trim().ToLowerInvariant();
        int shown = 0;
        foreach (var entry in OreCatalog.WithMultipliers)
        {
            if (f.Length > 0 && !entry.DisplayName.ToLowerInvariant().Contains(f))
                continue;
            string captured = entry.Key;
            _contentList.Add(new UICompactSliderRow(entry.DisplayName, 0f, 5f,
                WorldGenConfig.OreMul[captured], 0.05f,
                formatter: v => $"×{v:0.00}",
                onChanged: v => { WorldGenConfig.OreMul[captured] = v; RefreshNavBadges(); },
                defaultValue: WorldGenConfig.DefaultOreMul,
                showVanillaBadge: true));
            shown++;
        }
        if (shown == 0 && f.Length > 0)
            _contentList.Add(MakeInfoLine($"No ores match '{_filterText}'."));

        AddHeading("Not World-Gen (wiki)");
        foreach (var entry in OreCatalog.WithoutMultipliers)
            _contentList.Add(MakeInfoLine($"{entry.DisplayName} — {entry.Notes}"));
    }

    private void BuildPresetsTab()
    {
        AddHeading("Size Presets");
        _contentList.Add(MakePresetButtonRow());

        AddHeading("Debug Preset");
        _contentList.Add(MakeInfoLine("Tiny + 20× ore — fast iteration. Forces Custom: ON."));
        var dbg = new UITextButton("Apply DEBUG: Tiny + 20× Ore", 280f, 30f, () =>
        {
            WorldGenConfig.ApplyDebugWorldGenPreset();
            ToastManager.Push(
                $"DEBUG: {WorldGenConfig.WorldWidth}×{WorldGenConfig.WorldHeight}, ore ×{WorldGenConfig.OreFrequencyMul:0}",
                new Color(255, 180, 80), 4.0);
            Refresh();
        });
        dbg.BaseColor = new Color(160, 55, 35);
        dbg.HoverColor = new Color(220, 85, 45);
        var w = new UIElement { Width = StyleDimension.Fill, Height = StyleDimension.FromPixels(34f) };
        w.Append(dbg);
        _contentList.Add(w);
    }

    private void BuildInfoTab()
    {
        AddHeading("About");
        _contentList.Add(MakeInfoLine("World Config Mod — modifies vanilla world generation."));
        _contentList.Add(MakeInfoLine("ESC closes panel. Scroll-wheel scrolls the content column."));
        _contentList.Add(MakeInfoLine("Yellow dot = value differs from vanilla default."));
        _contentList.Add(MakeInfoLine("Green % = above vanilla. Red % = below vanilla."));

        AddHeading("Ore Phases (wiki)");
        foreach (var entry in OreCatalog.All)
        {
            _contentList.Add(MakeInfoLine(
                $"{entry.DisplayName,-12}  {entry.Phase}"
              + (entry.HasWorldGenMultiplier ? "" : "  (no multiplier)")));
        }
    }

    // ---------- Helpers ----------
    private void AddHeading(string title)
    {
        var row = new UIElement
        {
            Width = StyleDimension.Fill,
            Height = StyleDimension.FromPixels(22f),
            PaddingTop = 6f,
        };
        row.Append(new UIText(title, 0.85f, true)
        {
            HAlign = 0f, VAlign = 0.5f,
            TextColor = new Color(150, 175, 230),
        });
        _contentList.Add(row);
    }

    private static UIElement MakeInfoLine(string text)
    {
        var row = new UIPanel
        {
            Width = StyleDimension.Fill,
            Height = StyleDimension.FromPixels(20f),
            BackgroundColor = new Color(28, 36, 60),
            BorderColor = Color.Transparent,
            PaddingTop = 0f, PaddingBottom = 0f, PaddingLeft = 6f, PaddingRight = 6f,
        };
        row.Append(new UIText(text, 0.74f)
        {
            HAlign = 0f, VAlign = 0.5f,
            TextColor = new Color(190, 200, 220),
        });
        return row;
    }

    private UIElement MakePresetButtonRow()
    {
        string[] names = { "Small", "Med", "Large", "XL", "XXL" };
        float btnW = 64f, btnH = 26f, gap = 4f;
        var row = new UIElement
        {
            Width = StyleDimension.Fill,
            Height = StyleDimension.FromPixels(btnH),
        };
        for (int i = 0; i < 5; i++)
        {
            int preset = i + 1;
            int idx = i;
            var b = new UITextButton(names[idx], btnW, btnH, () =>
            {
                WorldGenConfig.ApplyPresetSize(preset);
                Refresh();
            });
            b.Left = StyleDimension.FromPixels(i * (btnW + gap));
            row.Append(b);
        }
        return row;
    }

    private UIElement MakeDungeonSideRow()
    {
        float btnW = 70f, btnH = 26f, gap = 4f;
        var row = new UIElement
        {
            Width = StyleDimension.Fill,
            Height = StyleDimension.FromPixels(btnH),
        };
        (string text, int side)[] opts = { ("Left", -1), ("Random", 0), ("Right", 1) };
        for (int i = 0; i < opts.Length; i++)
        {
            int captured = opts[i].side;
            var b = new UITextButton(opts[i].text, btnW, btnH, () =>
            {
                WorldGenConfig.DungeonSide = captured;
                RefreshNavBadges();
            });
            b.Left = StyleDimension.FromPixels(i * (btnW + gap));
            b.BaseColor = (WorldGenConfig.DungeonSide == captured) ? new Color(60, 90, 160) : new Color(40, 50, 90);
            row.Append(b);
        }
        return row;
    }

    // ---------- Footer ----------
    private void BuildFooter()
    {
        var footer = new UIPanel
        {
            Width = StyleDimension.FromPixelsAndPercent(0f, 1f),
            Height = StyleDimension.FromPixels(FooterH),
            VAlign = 1f,
            BackgroundColor = new Color(20, 26, 48),
            BorderColor = new Color(60, 75, 120),
            PaddingTop = 0f, PaddingBottom = 0f, PaddingLeft = 0f, PaddingRight = 0f,
        };
        _panel.Append(footer);

        const float btnH = 36f;
        float top = (FooterH - btnH) * 0.5f;

        var back = new UITextButton("Back", 110f, btnH, UIInjectSystem.CloseConfigMenu);
        back.Top = StyleDimension.FromPixels(top);
        back.Left = StyleDimension.FromPixels(12f);
        footer.Append(back);

        var apply = new UITextButton("Apply & Back", 170f, btnH, UIInjectSystem.CloseConfigMenu);
        apply.Top = StyleDimension.FromPixels(top);
        apply.HAlign = 1f;
        apply.Left = StyleDimension.FromPixels(-12f);
        apply.BaseColor = new Color(40, 120, 60);
        apply.HoverColor = new Color(70, 160, 90);
        footer.Append(apply);
    }
}
