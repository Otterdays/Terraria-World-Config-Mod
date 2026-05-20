using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;

namespace WorldConfigMod.Common.UI;

public static class ToastManager
{
    private sealed class Toast
    {
        public string Text;
        public Color Accent;
        public DateTime SpawnedAt;
        public TimeSpan Lifetime;
    }

    private static readonly List<Toast> _active = new();

    public static void Push(string text, Color? accent = null, double lifetimeSec = 6.0)
    {
        _active.Add(new Toast
        {
            Text = text,
            Accent = accent ?? new Color(110, 170, 255),
            SpawnedAt = DateTime.UtcNow,
            Lifetime = TimeSpan.FromSeconds(lifetimeSec),
        });
    }

    public static void Clear() => _active.Clear();

    public static int ActiveCount => _active.Count;

    // useFramebuffer=true for DrawMenu + Matrix.Identity; false for InterfaceScaleType.UI (layout + UIScaleMatrix).
    public static void DrawAndPrune(SpriteBatch sb, bool useFramebuffer = false)
    {
        if (_active.Count == 0) return;
        var now = DateTime.UtcNow;
        _active.RemoveAll(t => (now - t.SpawnedAt) >= t.Lifetime);
        if (_active.Count == 0) return;

        var font = FontAssets.MouseText.Value;
        var pixel = TextureAssets.MagicPixel.Value;

        const int padX = 16;
        const int padY = 10;
        const int marginRight = 20;
        const int marginTop = 20;
        const int gap = 8;

        int y = marginTop;
        int spaceW = UiDrawSpace.Width(useFramebuffer);

        foreach (var t in _active)
        {
            var elapsed = (now - t.SpawnedAt).TotalSeconds;
            var total = t.Lifetime.TotalSeconds;
            float fadeIn  = MathHelper.Clamp((float)(elapsed / 0.25), 0f, 1f);
            float fadeOut = MathHelper.Clamp((float)((total - elapsed) / 0.4), 0f, 1f);
            float alpha   = Math.Min(fadeIn, fadeOut);

            var size = font.MeasureString(t.Text);
            int w = (int)size.X + padX * 2;
            int h = (int)size.Y + padY * 2;

            float slide = (1f - fadeIn) * 18f;
            int x = spaceW - w - marginRight + (int)slide;

            var bg     = new Color(15, 18, 32) * (0.92f * alpha);
            var accent = t.Accent * alpha;
            var border = new Color(80, 100, 160) * alpha;

            sb.Draw(pixel, new Rectangle(x, y, w, h), bg);
            sb.Draw(pixel, new Rectangle(x, y, 4, h), accent);
            sb.Draw(pixel, new Rectangle(x, y, w, 1), border);
            sb.Draw(pixel, new Rectangle(x, y + h - 1, w, 1), border);
            sb.Draw(pixel, new Rectangle(x + w - 1, y, 1, h), border);

            Utils.DrawBorderString(sb, t.Text,
                new Vector2(x + padX, y + padY), Color.White * alpha);

            y += h + gap;
        }
    }
}
