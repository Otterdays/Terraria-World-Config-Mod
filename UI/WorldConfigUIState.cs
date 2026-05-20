using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using WorldConfigMod.Common;
using WorldConfigMod.Core;
using WorldConfigMod.Common.Systems;
using WorldConfigMod.Common.UI;
using WorldConfigMod.UI.Elements;

namespace WorldConfigMod.UI;

public class WorldConfigUIState : UIState
{
    private const float HeaderHeight = 72f;
    private const float FooterHeight = 66f;
    private const float ColumnTitleHeight = 44f;
    private const float LayoutGap = 8f;

    private UIPanel _panel;
    private UIList _leftList;
    private UIList _rightList;
    private UIScrollbar _rightScroll;
    private UIScrollColumn _rightScrollCol;

    public override void OnInitialize()
    {
        // Near-fullscreen — scales with resolution.
        _panel = new UIPanel
        {
            Width  = StyleDimension.FromPixelsAndPercent(32f, 0.96f),
            Height = StyleDimension.FromPixelsAndPercent(32f, 0.92f),
            HAlign = 0.5f,
            VAlign = 0.5f,
            BackgroundColor = new Color(16, 20, 38) * 0.97f,
            BorderColor = new Color(90, 115, 180),
            PaddingTop = 0f,
            PaddingBottom = 0f,
            PaddingLeft = 0f,
            PaddingRight = 0f,
        };
        Append(_panel);

        BuildHeader();
        BuildColumns();
        BuildFooter();
        Refresh();
    }

    public void Refresh()
    {
        _leftList?.Clear();
        _rightList?.Clear();
        BuildLeftRows();
        BuildRightRows();
        _rightScroll.ViewPosition = 0f;
        Recalculate();
    }

    public void ApplyScrollWheel(int delta)
    {
        if (delta == 0 || _rightScroll == null || _rightScrollCol == null)
            return;

        if (!_rightScrollCol.ContainsPoint(Main.MouseScreen)
            && _rightScroll?.IsMouseHovering != true
            && _rightList?.IsMouseHovering != true)
            return;

        // Direct scrollbar move — double UIList.ScrollWheel was freezing the menu.
        const float pixelsPerTick = 48f;
        float max = Math.Max(0f, _rightScroll.MaxViewSize);
        float next = _rightScroll.ViewPosition - Math.Sign(delta) * pixelsPerTick;
        _rightScroll.ViewPosition = MathHelper.Clamp(next, 0f, max);
    }

    private void ApplyDebugPreset()
    {
        WorldGenConfig.ApplyDebugWorldGenPreset();
        ToastManager.Push(
            $"DEBUG: {WorldGenConfig.WorldWidth}×{WorldGenConfig.WorldHeight}, ore ×{WorldGenConfig.OreFrequencyMul:0} freq ×{WorldGenConfig.OreVeinSizeMul:0} veins",
            new Color(255, 180, 80),
            5.0);
        Refresh();
    }

    private void BuildHeader()
    {
        var header = new UIPanel
        {
            Width = StyleDimension.FromPixelsAndPercent(0f, 1f),
            Height = StyleDimension.FromPixels(HeaderHeight),
            HAlign = 0.5f,
            VAlign = 0f,
            BackgroundColor = new Color(28, 36, 68),
            BorderColor = new Color(70, 90, 140),
            PaddingTop = 0f,
            PaddingBottom = 0f,
            PaddingLeft = 0f,
            PaddingRight = 0f,
        };
        _panel.Append(header);

        header.Append(new UIText("World Config", 1.35f, true)
        {
            HAlign = 0f,
            VAlign = 0.5f,
            Left = StyleDimension.FromPixels(20f),
            TextColor = new Color(220, 230, 255),
        });

        var tryV2 = new UITextButton("Try New UI →", 130f, 28f, () =>
        {
            WorldGenConfig.UseV2Panel = true;
            UIInjectSystem.ReopenConfigMenu();
        });
        tryV2.HAlign = 0.5f;
        tryV2.VAlign = 0.5f;
        tryV2.BaseColor = new Color(80, 120, 60);
        tryV2.HoverColor = new Color(120, 180, 90);
        header.Append(tryV2);

        var toggleRow = new UIElement
        {
            Width = StyleDimension.FromPixels(320f),
            Height = StyleDimension.FromPixels(46f),
            HAlign = 1f,
            VAlign = 0.5f,
            Left = StyleDimension.FromPixels(-14f),
        };
        header.Append(toggleRow);
        toggleRow.Append(MakeToggleRow("Use Custom Generation",
            () => WorldGenConfig.UseCustom,
            v => WorldGenConfig.UseCustom = v));
    }

    private void BuildColumns()
    {
        float bodyTop = HeaderHeight + LayoutGap;
        float bodyPadBottom = FooterHeight + LayoutGap;

        var body = new UIElement
        {
            Width = StyleDimension.FromPixelsAndPercent(0f, 1f),
            Height = StyleDimension.FromPixelsAndPercent(-(bodyTop + bodyPadBottom), 1f),
            Top = StyleDimension.FromPixels(bodyTop),
        };
        _panel.Append(body);

        var leftCol = MakeColumnPanel("World & Size", hAlign: 0f);
        leftCol.Width = StyleDimension.FromPixelsAndPercent(-4f, 0.495f);
        body.Append(leftCol);

        var rightCol = MakeColumnPanel("Ore Generation", hAlign: 1f);
        rightCol.Width = StyleDimension.FromPixelsAndPercent(-4f, 0.495f);
        body.Append(rightCol);

        float listTop = ColumnTitleHeight + 4f;
        float listPadBottom = 8f;

        _rightScrollCol = new UIScrollColumn
        {
            Width = StyleDimension.Fill,
            Height = StyleDimension.FromPixelsAndPercent(-(listTop + listPadBottom), 1f),
            Top = StyleDimension.FromPixels(listTop),
            BackgroundColor = Color.Transparent,
            BorderColor = Color.Transparent,
        };
        rightCol.Append(_rightScrollCol);

        _leftList = new UIList
        {
            Width = StyleDimension.FromPixelsAndPercent(0f, 1f),
            Height = StyleDimension.FromPixelsAndPercent(-(listTop + listPadBottom), 1f),
            Top = StyleDimension.FromPixels(listTop),
            ListPadding = 6f,
        };
        _leftList.ManualSortMethod = _ => { };
        leftCol.Append(_leftList);

        _rightList = new UIList
        {
            Width = StyleDimension.FromPixelsAndPercent(-24f, 1f),
            Height = StyleDimension.FromPixelsAndPercent(-listPadBottom, 1f),
            Top = StyleDimension.FromPixels(0f),
            ListPadding = 5f,
        };
        // Preserve insertion order — default UIList sort scrambles section headers vs sliders.
        _rightList.ManualSortMethod = _ => { };
        _rightScrollCol.Append(_rightList);

        _rightScroll = new UIScrollbar
        {
            Height = StyleDimension.FromPixelsAndPercent(-listPadBottom, 1f),
            HAlign = 1f,
            Top = StyleDimension.FromPixels(0f),
        };
        _rightScrollCol.Append(_rightScroll);
        _rightScrollCol.List = _rightList;
        _rightScrollCol.Scrollbar = _rightScroll;
        _rightList.SetScrollbar(_rightScroll);
    }

    private static UIPanel MakeColumnPanel(string title, float hAlign)
    {
        var col = new UIPanel
        {
            Height = StyleDimension.Fill,
            HAlign = hAlign,
            BackgroundColor = new Color(22, 28, 52) * 0.95f,
            BorderColor = new Color(60, 75, 120),
            PaddingTop = 0f,
            PaddingLeft = 10f,
            PaddingRight = 10f,
            PaddingBottom = 8f,
        };

        var titleBar = new UIPanel
        {
            Width = StyleDimension.FromPixelsAndPercent(0f, 1f),
            Height = StyleDimension.FromPixels(ColumnTitleHeight),
            HAlign = 0f,
            VAlign = 0f,
            BackgroundColor = new Color(32, 40, 72) * 0.9f,
            BorderColor = Color.Transparent,
        };
        col.Append(titleBar);

        titleBar.Append(new UIText(title, 1.15f, true)
        {
            HAlign = 0f,
            VAlign = 0.5f,
            Left = StyleDimension.FromPixels(12f),
            TextColor = new Color(160, 190, 255),
        });

        return col;
    }

    private void BuildLeftRows()
    {
        _leftList.Add(MakeSpacer(2f));

        _leftList.Add(new UISliderRow("World Width",
            WorldGenConfig.MinWidth, WorldGenConfig.MaxWidth,
            WorldGenConfig.WorldWidth, 100f, isInt: true,
            v => $"{(int)v}",
            v => WorldGenConfig.WorldWidth = (int)v,
            labelWidth: 118, valuePad: 64));
        _leftList.Add(new UISliderRow("World Height",
            WorldGenConfig.MinHeight, WorldGenConfig.MaxHeight,
            WorldGenConfig.WorldHeight, 100f, isInt: true,
            v => $"{(int)v}",
            v => WorldGenConfig.WorldHeight = (int)v,
            labelWidth: 118, valuePad: 64));

        _leftList.Add(MakeSection("Size Presets"));
        AddPresetButtons(_leftList);

        _leftList.Add(MakeSection("World Shape"));
        _leftList.Add(new UISliderRow("Cave Depth", 0.5f, 2.0f,
            WorldGenConfig.CaveDepthMul, 0.05f, false,
            v => $"×{v:0.00}",
            v => WorldGenConfig.CaveDepthMul = v,
            labelWidth: 118, valuePad: 64));
        _leftList.Add(MakeSpacer(4f));
        _leftList.Add(MakeDungeonSideRow());

        _leftList.Add(MakeSection("Quick Info"));
        _leftList.Add(MakeInfoRow(
            $"Current: {WorldGenConfig.WorldWidth} × {WorldGenConfig.WorldHeight} tiles"));
        _leftList.Add(MakeInfoRow(
            WorldGenConfig.UseCustom ? "Custom generation: ON" : "Custom generation: OFF"));
    }

    private void BuildRightRows()
    {
        _rightList.Add(MakeSpacer(2f));

        _rightList.Add(MakeSection("World Features"));
        _rightList.Add(new UISliderRow("Gems", 0f, 5f,
            WorldGenConfig.GemsMul, 0.05f, false,
            v => $"×{v:0.00}",
            v => WorldGenConfig.GemsMul = v,
            labelWidth: 110, valuePad: 56));
        _rightList.Add(new UISliderRow("Life Crystals", 0f, 5f,
            WorldGenConfig.LifeCrystalsMul, 0.05f, false,
            v => $"×{v:0.00}",
            v => WorldGenConfig.LifeCrystalsMul = v,
            labelWidth: 110, valuePad: 56));
        _rightList.Add(new UISliderRow("Chests", 0f, 5f,
            WorldGenConfig.ChestsMul, 0.05f, false,
            v => $"×{v:0.00}",
            v => WorldGenConfig.ChestsMul = v,
            labelWidth: 110, valuePad: 56));
        _rightList.Add(new UISliderRow("Float. Islands", 0f, 5f,
            WorldGenConfig.FloatingIslandsMul, 0.05f, false,
            v => $"×{v:0.00}",
            v => WorldGenConfig.FloatingIslandsMul = v,
            labelWidth: 110, valuePad: 56));
        _rightList.Add(new UISliderRow("Marble/Granite", 0f, 5f,
            WorldGenConfig.MarbleGraniteMul, 0.05f, false,
            v => $"×{v:0.00}",
            v => WorldGenConfig.MarbleGraniteMul = v,
            labelWidth: 110, valuePad: 56));

        _rightList.Add(MakeSection("Global Multipliers"));
        _rightList.Add(new UISliderRow("Vein Size", 0.25f, 25f,
            WorldGenConfig.OreVeinSizeMul, 0.05f, false,
            v => $"×{v:0.00}",
            v => WorldGenConfig.OreVeinSizeMul = v,
            labelWidth: 110, valuePad: 56));
        _rightList.Add(new UISliderRow("Frequency", 0.1f, 25f,
            WorldGenConfig.OreFrequencyMul, 0.05f, false,
            v => $"×{v:0.00}",
            v => WorldGenConfig.OreFrequencyMul = v,
            labelWidth: 110, valuePad: 56));

        _rightList.Add(MakeSection("Per-Ore Multipliers (19 world-gen)"));
        foreach (var entry in OreCatalog.WithMultipliers)
        {
            string captured = entry.Key;
            _rightList.Add(new UISliderRow(entry.DisplayName, 0f, 5f,
                WorldGenConfig.OreMul[captured], 0.05f, false,
                v => $"×{v:0.00}",
                v => WorldGenConfig.OreMul[captured] = v,
                labelWidth: 100, valuePad: 52));
        }

        _rightList.Add(MakeSection("Not World-Gen (wiki)"));
        foreach (var entry in OreCatalog.WithoutMultipliers)
        {
            _rightList.Add(MakeInfoRow($"{entry.DisplayName} — {entry.Notes}"));
        }
    }

    private static UIElement MakeSection(string title)
    {
        var row = new UIElement
        {
            Width = StyleDimension.Fill,
            Height = StyleDimension.FromPixels(32f),
            PaddingTop = 8f,
        };
        row.Append(new UIText(title, 0.95f, true)
        {
            HAlign = 0f,
            VAlign = 0.5f,
            TextColor = new Color(140, 165, 220),
        });
        return row;
    }

    private static UIElement MakeSpacer(float height)
    {
        return new UIElement
        {
            Width = StyleDimension.Fill,
            Height = StyleDimension.FromPixels(height),
        };
    }

    private static UIElement MakeInfoRow(string text)
    {
        var row = new UIPanel
        {
            Width = StyleDimension.Fill,
            Height = StyleDimension.FromPixels(28f),
            BackgroundColor = new Color(30, 38, 62),
            BorderColor = Color.Transparent,
        };
        row.Append(new UIText(text, 0.78f)
        {
            HAlign = 0f,
            VAlign = 0.5f,
            Left = StyleDimension.FromPixels(8f),
            TextColor = new Color(180, 190, 210),
        });
        return row;
    }

    private UIElement MakeToggleRow(string label, System.Func<bool> get, System.Action<bool> set)
    {
        var row = new UIPanel
        {
            Width = StyleDimension.Fill,
            Height = StyleDimension.FromPixels(44f),
            BackgroundColor = new Color(40, 50, 90),
        };
        row.Append(new UIText(label, 0.9f)
        {
            HAlign = 0f, VAlign = 0.5f, Left = StyleDimension.FromPixels(10f),
        });
        var btn = new UITextButton(get() ? "ON" : "OFF", 80f, 32f);
        btn.Top = StyleDimension.FromPixels(6f);
        btn.HAlign = 1f;
        btn.Left = StyleDimension.FromPixels(-8f);
        btn.OnClick = () =>
        {
            set(!get());
            btn.Text = get() ? "ON" : "OFF";
            btn.BaseColor = get() ? new Color(40, 120, 60) : new Color(120, 40, 40);
        };
        btn.BaseColor = get() ? new Color(40, 120, 60) : new Color(120, 40, 40);
        row.Append(btn);
        return row;
    }

    private UIElement MakeDungeonSideRow()
    {
        var label = new UIText("Dungeon Side", 0.85f)
        {
            HAlign = 0f,
            VAlign = 0.5f,
            TextColor = new Color(180, 190, 210),
        };

        float btnW = 70f, btnH = 28f, gap = 6f;
        var container = new UIElement
        {
            Width  = StyleDimension.Fill,
            Height = StyleDimension.FromPixels(btnH + 4f),
        };
        container.Append(label);

        (string text, int side)[] opts = { ("Left", -1), ("Random", 0), ("Right", 1) };
        float startX = 130f;
        foreach (var (text, side) in opts)
        {
            int captured = side;
            var btn = new UITextButton(text, btnW, btnH, () =>
            {
                WorldGenConfig.DungeonSide = captured;
            });
            btn.Left    = StyleDimension.FromPixels(startX);
            btn.VAlign  = 0.5f;
            btn.BaseColor = new Color(40, 50, 90);
            container.Append(btn);
            startX += btnW + gap;
        }
        return container;
    }

    private void AddPresetButtons(UIList list)
    {
        string[] names = { "Small", "Med", "Large", "XL", "XXL" };
        float btnW = 72f;
        float btnH = 30f;
        float gap = 6f;

        for (int row = 0; row < 2; row++)
        {
            var rowEl = new UIElement
            {
                Width = StyleDimension.Fill,
                Height = StyleDimension.FromPixels(btnH),
            };
            if (row > 0)
                rowEl.Top = StyleDimension.FromPixels(gap);

            int start = row * 3;
            int count = row == 0 ? 3 : 2;
            for (int i = 0; i < count; i++)
            {
                int preset = start + i + 1;
                int idx = start + i;
                var b = new UITextButton(names[idx], btnW, btnH, () =>
                {
                    WorldGenConfig.ApplyPresetSize(preset);
                    Refresh();
                });
                b.Left = StyleDimension.FromPixels(i * (btnW + gap));
                rowEl.Append(b);
            }

            list.Add(rowEl);
        }
    }

    private void BuildFooter()
    {
        var footer = new UIPanel
        {
            Width = StyleDimension.FromPixelsAndPercent(0f, 1f),
            Height = StyleDimension.FromPixels(FooterHeight),
            VAlign = 1f,
            BackgroundColor = new Color(24, 30, 55),
            BorderColor = new Color(60, 75, 120),
            PaddingTop = 0f,
            PaddingBottom = 0f,
            PaddingLeft = 0f,
            PaddingRight = 0f,
        };
        _panel.Append(footer);

        const float footerBtnH = 40f;
        float footerBtnTop = (FooterHeight - footerBtnH) * 0.5f;

        footer.Append(new UITextButton("Back", 120f, footerBtnH, UIInjectSystem.CloseConfigMenu)
        {
            Top = StyleDimension.FromPixels(footerBtnTop),
            Left = StyleDimension.FromPixels(12f),
        });

        footer.Append(new UITextButton("Reset Defaults", 150f, footerBtnH, () =>
        {
            WorldGenConfig.Reset();
            Refresh();
        })
        {
            Top = StyleDimension.FromPixels(footerBtnTop),
            Left = StyleDimension.FromPixels(140f),
        });

        var debug = new UITextButton("DEBUG: Tiny + 20× Ore", 260f, footerBtnH, ApplyDebugPreset)
        {
            Top = StyleDimension.FromPixels(footerBtnTop),
            HAlign = 0.5f,
        };
        debug.BaseColor = new Color(160, 55, 35);
        debug.HoverColor = new Color(220, 85, 45);
        footer.Append(debug);

        var save = new UITextButton("Apply & Back", 180f, footerBtnH, UIInjectSystem.CloseConfigMenu)
        {
            Top = StyleDimension.FromPixels(footerBtnTop),
            HAlign = 1f,
            Left = StyleDimension.FromPixels(-12f),
        };
        save.BaseColor = new Color(40, 120, 60);
        save.HoverColor = new Color(70, 160, 90);
        footer.Append(save);
    }
}
