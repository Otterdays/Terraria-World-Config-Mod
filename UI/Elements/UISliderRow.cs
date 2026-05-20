using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.UI;

namespace WorldConfigMod.UI.Elements;

public class UISliderRow : UIElement
{
    public string Label;
    public float Min;
    public float Max;
    public float Value;
    public float Step;
    public bool IsInt;
    public int LabelWidth = 140;
    public int ValuePad = 72;
    public Func<float, string> Formatter;
    public Action<float> OnChanged;

    private bool _dragging;

    public UISliderRow(string label, float min, float max, float value, float step,
        bool isInt = false, Func<float, string> formatter = null,
        Action<float> onChanged = null, int labelWidth = 140, int valuePad = 72)
    {
        Label = label;
        Min = min;
        Max = max;
        Value = value;
        Step = step;
        IsInt = isInt;
        LabelWidth = labelWidth;
        ValuePad = valuePad;
        Formatter = formatter ?? (v => isInt ? ((int)v).ToString() : v.ToString("0.00"));
        OnChanged = onChanged;
        Width = StyleDimension.Fill;
        Height = StyleDimension.FromPixels(36f);
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
        if (Step > 0)
            newVal = (float)Math.Round(newVal / Step) * Step;
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
        int barH = 8;
        int leftPad = LabelWidth + 8;
        int rightPad = ValuePad + 8;
        int w = dims.Width - leftPad - rightPad;
        if (w < 24) w = 24;
        return new Rectangle(dims.X + leftPad, dims.Y + dims.Height / 2 - barH / 2, w, barH);
    }

    protected override void DrawSelf(SpriteBatch sb)
    {
        var dims = GetDimensions().ToRectangle();
        var pixel = TextureAssets.MagicPixel.Value;

        Utils.DrawBorderString(sb, Label,
            new Vector2(dims.X + 8, dims.Y + dims.Height / 2 - 10), Color.White);

        var bar = GetBarRect();
        sb.Draw(pixel, bar, new Color(40, 50, 80));

        float t = (Value - Min) / (Max - Min);
        var fill = new Rectangle(bar.X, bar.Y, Math.Max(1, (int)(bar.Width * t)), bar.Height);
        sb.Draw(pixel, fill, new Color(110, 170, 255));

        var knob = new Rectangle(bar.X + (int)(bar.Width * t) - 5, bar.Y - 6, 10, bar.Height + 12);
        sb.Draw(pixel, knob, Color.White);

        Utils.DrawBorderString(sb, Formatter(Value),
            new Vector2(bar.Right + 8, dims.Y + dims.Height / 2 - 10), Color.LightYellow);
    }
}
