using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;

namespace WorldConfigMod.UI.Elements;

// Single-line text input. Focus by click; uses Main.GetInputText for IME-safe entry.
public class UITextInput : UIElement
{
    public string Text = "";
    public string Placeholder = "";
    public Action<string> OnChanged;
    public int MaxLength = 32;

    private bool _focused;
    private int _blinkFrame;

    public UITextInput(string placeholder = "", Action<string> onChanged = null, float width = 180f, float height = 22f)
    {
        Placeholder = placeholder;
        OnChanged = onChanged;
        Width = StyleDimension.FromPixels(width);
        Height = StyleDimension.FromPixels(height);
    }

    public override void LeftClick(UIMouseEvent evt) { _focused = true; }

    public void Unfocus() { _focused = false; }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        // Lose focus on click-outside.
        if (_focused && Main.mouseLeft && Main.mouseLeftRelease == false && !IsMouseHovering)
        {
            // mouseLeftRelease == false means it's not the click-frame; vanilla pattern
        }
        if (Main.mouseLeft && !IsMouseHovering)
            _focused = false;

        if (!_focused) return;

        _blinkFrame++;

        string before = Text;
        Text = Main.GetInputText(Text);
        if (Text == null) Text = "";
        if (Text.Length > MaxLength) Text = Text.Substring(0, MaxLength);

        if (Text != before)
            OnChanged?.Invoke(Text);
    }

    protected override void DrawSelf(SpriteBatch sb)
    {
        var dims = GetDimensions().ToRectangle();
        var pixel = TextureAssets.MagicPixel.Value;

        sb.Draw(pixel, dims, new Color(20, 26, 48));
        var br = _focused ? new Color(150, 200, 255) : new Color(70, 90, 140);
        sb.Draw(pixel, new Rectangle(dims.X, dims.Y, dims.Width, 1), br);
        sb.Draw(pixel, new Rectangle(dims.X, dims.Bottom - 1, dims.Width, 1), br);
        sb.Draw(pixel, new Rectangle(dims.X, dims.Y, 1, dims.Height), br);
        sb.Draw(pixel, new Rectangle(dims.Right - 1, dims.Y, 1, dims.Height), br);

        bool empty = string.IsNullOrEmpty(Text);
        string shown = empty ? Placeholder : Text;
        var col = empty ? new Color(110, 120, 145) : Color.White;

        Utils.DrawBorderString(sb, shown,
            new Vector2(dims.X + 6, dims.Y + 3), col, 0.78f);

        // Caret blink
        if (_focused && (_blinkFrame / 20) % 2 == 0)
        {
            float w = string.IsNullOrEmpty(Text) ? 0f
                : FontAssets.MouseText.Value.MeasureString(Text).X * 0.78f;
            var caret = new Rectangle(dims.X + 6 + (int)w + 1, dims.Y + 3, 1, dims.Height - 6);
            sb.Draw(pixel, caret, Color.White);
        }
    }
}
