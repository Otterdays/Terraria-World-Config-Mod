using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;

namespace WorldConfigMod.UI.Elements;

public class UITextButton : UIElement
{
    public string Text;
    public Color BaseColor = new(33, 43, 79);
    public Color HoverColor = new(73, 94, 171);
    public Action OnClick;
    private bool _hover;

    public UITextButton(string text, float width = 160f, float height = 36f, Action onClick = null)
    {
        Text = text;
        OnClick = onClick;
        Width = StyleDimension.FromPixels(width);
        Height = StyleDimension.FromPixels(height);
    }

    public override void MouseOver(UIMouseEvent evt) { _hover = true; }
    public override void MouseOut(UIMouseEvent evt)  { _hover = false; }
    public override void LeftClick(UIMouseEvent evt) { OnClick?.Invoke(); }

    protected override void DrawSelf(SpriteBatch sb)
    {
        var dims = GetDimensions().ToRectangle();
        var pixel = TextureAssets.MagicPixel.Value;
        sb.Draw(pixel, dims, _hover ? HoverColor : BaseColor);
        sb.Draw(pixel, new Rectangle(dims.X, dims.Y, dims.Width, 1), Color.Black);
        sb.Draw(pixel, new Rectangle(dims.X, dims.Bottom - 1, dims.Width, 1), Color.Black);
        sb.Draw(pixel, new Rectangle(dims.X, dims.Y, 1, dims.Height), Color.Black);
        sb.Draw(pixel, new Rectangle(dims.Right - 1, dims.Y, 1, dims.Height), Color.Black);

        var center = new Vector2(dims.Center.X, dims.Center.Y);
        Utils.DrawBorderString(sb, Text, center, Color.White, 0.9f, 0.5f, 0.5f);
    }
}
