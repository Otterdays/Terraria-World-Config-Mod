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
        UiDrawSpace.BottomCenter(300, 54, fromBottom: 108, framebuffer: true);

    public static void DrawOverlayButton(SpriteBatch sb)
    {
        var rect = GetOverlayButtonRect();
        var pixel = TextureAssets.MagicPixel.Value;

        bool custom = WorldGenConfig.UseCustom;

        var bg = _hovering
            ? (custom ? new Color(40, 110, 70)  : new Color(58, 78, 148))
            : (custom ? new Color(24, 70, 44)   : new Color(28, 38, 72));
        sb.Draw(pixel, rect, bg);

        var border = _hovering
            ? (custom ? new Color(140, 240, 170) : new Color(150, 200, 255))
            : (custom ? new Color(90, 200, 120)  : new Color(90, 130, 210));
        const int borderW = 2;
        sb.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, borderW), border);
        sb.Draw(pixel, new Rectangle(rect.X, rect.Bottom - borderW, rect.Width, borderW), border);
        sb.Draw(pixel, new Rectangle(rect.X, rect.Y, borderW, rect.Height), border);
        sb.Draw(pixel, new Rectangle(rect.Right - borderW, rect.Y, borderW, rect.Height), border);

        string label = custom ? "World Config  ●  ON" : "World Config";
        Utils.DrawBorderString(sb, label,
            new Vector2(rect.Center.X, rect.Center.Y), Color.White, 0.9f, 0.5f, 0.5f);

        if (custom)
            DrawCustomStatusBadge(sb, rect, pixel);
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
