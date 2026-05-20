using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using WorldConfigMod.Common;
using WorldConfigMod.Common.UI;
using WorldConfigMod.UI;

namespace WorldConfigMod.Common.Systems;

// Config panel on Main.MenuUI; New World entry = SpriteBatch in framebuffer space (mouseX/mouseY).
[Autoload(Side = ModSide.Client)]
public class UIInjectSystem : ModSystem
{
    private static WorldConfigUIState _configState;
    private static WorldConfigUIStateV2 _configStateV2;
    private static UIState _returnMenuState;
    private static int _recalcFramesRemaining;
    private static int _lastScreenW = -1;
    private static int _lastScreenH = -1;
    private static int _lastMouseScroll;
    private static uint _lastConfigUpdateFrame = uint.MaxValue;
    private static bool _hovering;
    private static bool _prevMouseLeft;
    private static bool _pressedInside;

    public override void Load()
    {
        _configState = new WorldConfigUIState();
        _configState.Activate();
        _configStateV2 = new WorldConfigUIStateV2();
        _configStateV2.Activate();
    }

    public override void Unload()
    {
        _configState = null;
        _configStateV2 = null;
        _returnMenuState = null;
    }

    public static bool IsConfigMenuOpen() =>
        Main.MenuUI?.CurrentState is WorldConfigUIState
        || Main.MenuUI?.CurrentState is WorldConfigUIStateV2;

    private static UIState ActiveConfigState() =>
        WorldGenConfig.UseV2Panel ? (UIState)_configStateV2 : _configState;

    public static bool ShouldDrawOverlay() =>
        !IsConfigMenuOpen()
        && Main.MenuUI?.CurrentState is UIWorldCreation;

    public override void UpdateUI(GameTime gameTime)
    {
        if (IsConfigMenuOpen() && Main.MenuUI != null)
        {
            Main.MenuUI.MousePosition = Main.MouseScreen;
            Main.MenuUI.Update(gameTime);
        }
    }

    public static Rectangle GetOverlayButtonRect() =>
        UiDrawSpace.BottomCenter(340, 62, fromBottom: 110, framebuffer: true);

    public static void DrawOverlayButton(SpriteBatch sb)
    {
        var rect = GetOverlayButtonRect();
        var pixel = TextureAssets.MagicPixel.Value;

        bool custom = WorldGenConfig.UseCustom;
        bool hov = _hovering;

        // ---- Background: 3-band vertical gradient for depth ----
        Color top, mid, bot;
        if (custom)
        {
            top = hov ? new Color(36, 110, 70)  : new Color(22, 76, 48);
            mid = hov ? new Color(28, 92, 58)   : new Color(18, 60, 38);
            bot = hov ? new Color(20, 70, 44)   : new Color(14, 46, 28);
        }
        else
        {
            top = hov ? new Color(58, 82, 156)  : new Color(34, 50, 96);
            mid = hov ? new Color(44, 64, 130)  : new Color(26, 38, 76);
            bot = hov ? new Color(32, 48, 100)  : new Color(20, 28, 56);
        }
        int third = rect.Height / 3;
        sb.Draw(pixel, new Rectangle(rect.X, rect.Y,                rect.Width, third),                       top);
        sb.Draw(pixel, new Rectangle(rect.X, rect.Y + third,        rect.Width, third),                       mid);
        sb.Draw(pixel, new Rectangle(rect.X, rect.Y + third * 2,    rect.Width, rect.Height - third * 2),     bot);

        // ---- Inner highlight (1px under top border) + bottom shadow (1px above bottom border) ----
        var highlight = custom
            ? new Color(120, 220, 150) * (hov ? 0.6f : 0.35f)
            : new Color(140, 180, 240) * (hov ? 0.6f : 0.35f);
        sb.Draw(pixel, new Rectangle(rect.X + 2, rect.Y + 2,           rect.Width - 4, 1), highlight);
        sb.Draw(pixel, new Rectangle(rect.X + 2, rect.Bottom - 3,      rect.Width - 4, 1), Color.Black * 0.45f);

        // ---- Left accent strip (state color, full height) ----
        var accent = custom
            ? (hov ? new Color(150, 240, 170) : new Color(90, 200, 120))
            : (hov ? new Color(180, 130, 110) : new Color(140, 90, 80));
        sb.Draw(pixel, new Rectangle(rect.X, rect.Y, 5, rect.Height), accent);

        // ---- Border ----
        var border = hov
            ? (custom ? new Color(160, 240, 180) : new Color(170, 210, 255))
            : (custom ? new Color(90, 200, 120)  : new Color(90, 130, 210));
        const int borderW = 2;
        sb.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, borderW), border);
        sb.Draw(pixel, new Rectangle(rect.X, rect.Bottom - borderW, rect.Width, borderW), border);
        sb.Draw(pixel, new Rectangle(rect.X, rect.Y, borderW, rect.Height), border);
        sb.Draw(pixel, new Rectangle(rect.Right - borderW, rect.Y, borderW, rect.Height), border);

        // ---- Cog icon (drawn from pixels — a simple plus/star shape, 16px) ----
        DrawCogIcon(sb, pixel,
            new Rectangle(rect.X + 14, rect.Y + (rect.Height - 18) / 2, 18, 18),
            hov ? Color.White : new Color(220, 230, 245));

        // ---- Title text ----
        Utils.DrawBorderString(sb, "World Config",
            new Vector2(rect.X + 40, rect.Y + 10),
            Color.White, 0.95f);

        // ---- Subtitle (state-aware) ----
        string sub = custom
            ? $"applies on next world create  ·  {WorldGenConfig.CountChanges()} change{(WorldGenConfig.CountChanges() == 1 ? "" : "s")}"
            : "click to customize world generation";
        Utils.DrawBorderString(sb, sub,
            new Vector2(rect.X + 42, rect.Y + 36),
            custom ? new Color(170, 230, 190) : new Color(170, 190, 230),
            0.72f);

        // ---- Status pill on the right ----
        DrawStatusPill(sb, pixel, rect, custom, hov);

        // ---- Live config badge below (when ON) ----
        if (custom)
            DrawCustomStatusBadge(sb, rect, pixel);
    }

    private static void DrawCogIcon(SpriteBatch sb, Microsoft.Xna.Framework.Graphics.Texture2D pixel, Rectangle r, Color col)
    {
        // 8-spoke cog: outer ring + cross + diagonals. Cheap pixel art.
        int cx = r.X + r.Width / 2;
        int cy = r.Y + r.Height / 2;
        // outer ring (4 small squares N/E/S/W and 4 diagonals)
        int sp = 6;
        sb.Draw(pixel, new Rectangle(cx - 2, r.Y,           4, 3), col);
        sb.Draw(pixel, new Rectangle(cx - 2, r.Bottom - 3,  4, 3), col);
        sb.Draw(pixel, new Rectangle(r.X,         cy - 2,   3, 4), col);
        sb.Draw(pixel, new Rectangle(r.Right - 3, cy - 2,   3, 4), col);
        // diagonals
        sb.Draw(pixel, new Rectangle(r.X + 2,         r.Y + 2,         3, 3), col);
        sb.Draw(pixel, new Rectangle(r.Right - 5,     r.Y + 2,         3, 3), col);
        sb.Draw(pixel, new Rectangle(r.X + 2,         r.Bottom - 5,    3, 3), col);
        sb.Draw(pixel, new Rectangle(r.Right - 5,     r.Bottom - 5,    3, 3), col);
        // inner hole (darker)
        sb.Draw(pixel, new Rectangle(cx - 2, cy - 2, 4, 4), col * 0.4f);
        _ = sp;
    }

    private static void DrawStatusPill(SpriteBatch sb, Microsoft.Xna.Framework.Graphics.Texture2D pixel, Rectangle btn, bool custom, bool hov)
    {
        const int pillW = 70, pillH = 26;
        var pill = new Rectangle(btn.Right - pillW - 10, btn.Y + (btn.Height - pillH) / 2, pillW, pillH);

        var pillBg = custom
            ? (hov ? new Color(30, 110, 60) : new Color(18, 70, 40))
            : (hov ? new Color(110, 40, 40) : new Color(70, 24, 24));
        sb.Draw(pixel, pill, pillBg);

        var pillBr = custom
            ? new Color(120, 230, 150)
            : new Color(220, 120, 110);
        sb.Draw(pixel, new Rectangle(pill.X, pill.Y, pill.Width, 1), pillBr);
        sb.Draw(pixel, new Rectangle(pill.X, pill.Bottom - 1, pill.Width, 1), pillBr);
        sb.Draw(pixel, new Rectangle(pill.X, pill.Y, 1, pill.Height), pillBr);
        sb.Draw(pixel, new Rectangle(pill.Right - 1, pill.Y, 1, pill.Height), pillBr);

        // status dot
        int dotR = 4;
        var dot = new Rectangle(pill.X + 8, pill.Center.Y - dotR / 2, dotR, dotR);
        sb.Draw(pixel, dot, custom ? new Color(140, 255, 170) : new Color(255, 140, 130));

        Utils.DrawBorderString(sb, custom ? "ON" : "OFF",
            new Vector2(pill.X + 22, pill.Center.Y - 8), Color.White, 0.85f);
    }

    private static void DrawCustomStatusBadge(SpriteBatch sb, Rectangle btnRect, Microsoft.Xna.Framework.Graphics.Texture2D pixel)
    {
        string text =
            $"Custom: {WorldGenConfig.WorldWidth}×{WorldGenConfig.WorldHeight}"
            + $"   ore ×{WorldGenConfig.OreFrequencyMul:0.##} freq · ×{WorldGenConfig.OreVeinSizeMul:0.##} veins";

        const float scale = 0.75f;
        var size = FontAssets.MouseText.Value.MeasureString(text) * scale;
        int padX = 14, padY = 6;
        var badge = new Rectangle(
            btnRect.Center.X - ((int)size.X + padX * 2) / 2,
            btnRect.Bottom + 8,
            (int)size.X + padX * 2,
            (int)size.Y + padY * 2);

        sb.Draw(pixel, badge, new Color(18, 36, 24) * 0.92f);
        var br = new Color(90, 200, 120);
        sb.Draw(pixel, new Rectangle(badge.X, badge.Y, badge.Width, 2), br);
        sb.Draw(pixel, new Rectangle(badge.X, badge.Bottom - 2, badge.Width, 2), br);
        sb.Draw(pixel, new Rectangle(badge.X, badge.Y, 2, badge.Height), br);
        sb.Draw(pixel, new Rectangle(badge.Right - 2, badge.Y, 2, badge.Height), br);

        Utils.DrawBorderString(sb, text,
            new Vector2(badge.Center.X, badge.Center.Y),
            new Color(200, 255, 210), scale, 0.5f, 0.5f);
    }

    public override void PostUpdateInput()
    {
        if (IsConfigMenuOpen())
            UpdateConfigInput();
        else
            HandleOverlayInput();
    }

    public static void HandleOverlayInput()
    {
        if (!ShouldDrawOverlay())
        {
            _hovering = false;
            _pressedInside = false;
            _prevMouseLeft = Main.mouseLeft;
            return;
        }

        var rect = GetOverlayButtonRect();
        _hovering = UiDrawSpace.ContainsMouse(rect);

        bool down = Main.mouseLeft;
        bool justPressed = down && !_prevMouseLeft;
        bool justReleased = !down && _prevMouseLeft;
        _prevMouseLeft = down;

        if (justPressed && _hovering)
            _pressedInside = true;

        if (justReleased)
        {
            if (_pressedInside && _hovering)
            {
                SoundEngine.PlaySound(SoundID.MenuTick);
                OpenConfigMenu();
            }
            _pressedInside = false;
        }
    }

    public static void OpenConfigMenu()
    {
        _returnMenuState = Main.MenuUI?.CurrentState;
        if (WorldGenConfig.UseV2Panel)
            _configStateV2?.Refresh();
        else
            _configState?.Refresh();
        Main.MenuUI?.SetState(ActiveConfigState());
        Main.MenuUI?.Recalculate();
        _recalcFramesRemaining = 3;
        _lastScreenW = Main.screenWidth;
        _lastScreenH = Main.screenHeight;
        _lastMouseScroll = Mouse.GetState().ScrollWheelValue;
        _lastConfigUpdateFrame = uint.MaxValue;
    }

    // Swap V1 <-> V2 without resetting return state. Called by in-panel toggle buttons.
    public static void ReopenConfigMenu()
    {
        if (WorldGenConfig.UseV2Panel)
            _configStateV2?.Refresh();
        else
            _configState?.Refresh();
        Main.MenuUI?.SetState(ActiveConfigState());
        Main.MenuUI?.Recalculate();
        _recalcFramesRemaining = 3;
    }

    public static void CloseConfigMenu()
    {
        if (_returnMenuState != null && Main.MenuUI != null)
            Main.MenuUI.SetState(_returnMenuState);
        else
            Main.MenuUI?.SetState(null);
    }

    private static void UpdateConfigInput()
    {
        if (!IsConfigMenuOpen())
            return;

        if (_lastConfigUpdateFrame == Main.GameUpdateCount)
            return;
        _lastConfigUpdateFrame = Main.GameUpdateCount;

        var ui = Main.MenuUI;
        if (ui == null)
            return;

        if (Main.screenWidth != _lastScreenW || Main.screenHeight != _lastScreenH || _recalcFramesRemaining > 0)
        {
            _lastScreenW = Main.screenWidth;
            _lastScreenH = Main.screenHeight;
            ui.Recalculate();
            if (_recalcFramesRemaining > 0)
                _recalcFramesRemaining--;
        }

        int scrollDelta = ConsumeMouseScrollDelta();
        if (WorldGenConfig.UseV2Panel)
            _configStateV2?.ApplyScrollWheel(scrollDelta);
        else
            _configState?.ApplyScrollWheel(scrollDelta);

        if (Main.keyState.IsKeyDown(Keys.Escape) && !Main.oldKeyState.IsKeyDown(Keys.Escape))
            CloseConfigMenu();
    }

    private static int ConsumeMouseScrollDelta()
    {
        int now = Mouse.GetState().ScrollWheelValue;
        int delta = now - _lastMouseScroll;
        _lastMouseScroll = now;
        return delta;
    }
}
