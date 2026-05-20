using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;

namespace WorldConfigMod.UI.Elements;

// Dense 22px row: [diff-dot] label  ━━●━━━━━  value  [vanilla %].
public class UICompactSliderRow : UIElement
{
    public string Label;
    public float Min;
    public float Max;
    public float Value;
    public float Step;
    public bool IsInt;
    public Func<float, string> Formatter;
    public Action<float> OnChanged;

    // Diff vs default — drives dot indicator + optional badge.
    public float Default = 1f;
    public bool ShowVanillaBadge = false; // "100%" style next to value
    public bool ShowDiffDot = true;

    public int LabelWidth = 90;
    public int ValueWidth = 56;
    public int BadgeWidth = 44;

    private bool _dragging;

    public UICompactSliderRow(string label, float min, float max, float value, float step,
        bool isInt = false, Func<float, string> formatter = null,
        Action<float> onChanged = null, float defaultValue = 1f,
        bool showVanillaBadge = false,
        int labelWidth = 90, int valueWidth = 56, int badgeWidth = 44)
    {
        Label = label;
        Min = min; Max = max; Value = value; Step = step; IsInt = isInt;
        Formatter = formatter ?? (v => isInt ? ((int)v).ToString() : v.ToString("0.00"));
        OnChanged = onChanged;
        Default = defaultValue;
        ShowVanillaBadge = showVanillaBadge;
        LabelWidth = labelWidth; ValueWidth = valueWidth; BadgeWidth = badgeWidth;
        Width = StyleDimension.Fill;
        Height = StyleDimension.FromPixels(22f);
    }

    public override void LeftMouseDown(UIMouseEvent evt) { _dragging = true; UpdateFromMouse(); }
    public override void LeftMouseUp(UIMouseEvent evt)   { _dragging = false; }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (_dragging) UpdateFromMouse();
    }

    private void UpdateFromMouse()
    {
        var bar = GetBarRect();
        if (bar.Width <= 0) return;
        float t = MathHelper.Clamp((Main.MouseScreen.X - bar.X) / (float)bar.Width, 0f, 1f);
        float newVal = Min + t * (Max - Min);
        if (Step > 0) newVal = (float)Math.Round(newVal / Step) * Step;
        if (IsInt) newVal = (float)Math.Round(newVal);
        if (Math.Abs(newVal - Value) > 0.0001f)
        {
            Value = newVal;
            OnChanged?.Invoke(Value);
        }
    }

    private Rectangle GetBarRect()
    {
        var dims = GetDimensions().ToRectangle();
        int leftPad  = LabelWidth + 6;
        int rightPad = ValueWidth + (ShowVanillaBadge ? BadgeWidth : 0) + 8;
        int w = dims.Width - leftPad - rightPad;
        if (w < 24) w = 24;
        return new Rectangle(dims.X + leftPad, dims.Y + dims.Height / 2 - 2, w, 4);
    }

    private bool IsDiff() => Math.Abs(Value - Default) > 0.0005f;

    protected override void DrawSelf(SpriteBatch sb)
    {
        var dims  = GetDimensions().ToRectangle();
        var pixel = TextureAssets.MagicPixel.Value;

        // Diff dot
        if (ShowDiffDot)
        {
            var dotColor = IsDiff() ? new Color(255, 200, 80) : new Color(50, 60, 90);
            sb.Draw(pixel, new Rectangle(dims.X + 2, dims.Y + dims.Height / 2 - 2, 4, 4), dotColor);
        }

        // Label
        Utils.DrawBorderString(sb, Label,
            new Vector2(dims.X + 10, dims.Y + 3), Color.White, 0.78f);

        // Track + fill
        var bar = GetBarRect();
        sb.Draw(pixel, bar, new Color(40, 50, 80));
        float t = MathHelper.Clamp((Value - Min) / (Max - Min), 0f, 1f);
        var fill = new Rectangle(bar.X, bar.Y, Math.Max(1, (int)(bar.Width * t)), bar.Height);
        sb.Draw(pixel, fill, IsDiff() ? new Color(255, 200, 80) : new Color(110, 170, 255));

        // Knob
        var knob = new Rectangle(bar.X + (int)(bar.Width * t) - 3, bar.Y - 4, 6, bar.Height + 8);
        sb.Draw(pixel, knob, Color.White);

        // Value text
        Utils.DrawBorderString(sb, Formatter(Value),
            new Vector2(bar.Right + 6, dims.Y + 3), Color.LightYellow, 0.78f);

        // Vanilla % badge
        if (ShowVanillaBadge)
        {
            float pct = Default > 0f ? (Value / Default) * 100f : 0f;
            string txt = $"{pct:0}%";
            var col = IsDiff()
                ? (pct > 100f ? new Color(140, 220, 140) : new Color(220, 140, 140))
                : new Color(120, 130, 150);
            Utils.DrawBorderString(sb, txt,
                new Vector2(bar.Right + 6 + ValueWidth, dims.Y + 3), col, 0.72f);
        }
    }
}
