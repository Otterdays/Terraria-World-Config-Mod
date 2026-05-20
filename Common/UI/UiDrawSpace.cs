using Microsoft.Xna.Framework;
using Terraria;

namespace WorldConfigMod.Common.UI;

// Terraria keeps two sizes: layout (Main.screen*) used with UIScaleMatrix, and the real back buffer (Viewport).
internal static class UiDrawSpace
{
    public static int FramebufferWidth =>
        Main.graphics?.GraphicsDevice?.Viewport.Width ?? Main.screenWidth;

    public static int FramebufferHeight =>
        Main.graphics?.GraphicsDevice?.Viewport.Height ?? Main.screenHeight;

    public static int LayoutWidth => Main.screenWidth;

    public static int LayoutHeight => Main.screenHeight;

    public static int Width(bool framebuffer) => framebuffer ? FramebufferWidth : LayoutWidth;

    public static int Height(bool framebuffer) => framebuffer ? FramebufferHeight : LayoutHeight;

    public static Rectangle TopRight(int boxW, int boxH, bool framebuffer, int marginRight = 20, int marginTop = 20) =>
        new Rectangle(Width(framebuffer) - boxW - marginRight, marginTop, boxW, boxH);

    public static Rectangle BottomCenter(int boxW, int boxH, int fromBottom, bool framebuffer) =>
        new Rectangle(
            (Width(framebuffer) - boxW) / 2,
            Height(framebuffer) - fromBottom - boxH,
            boxW,
            boxH);

    public static bool ContainsMouse(Rectangle rect) =>
        rect.Contains(Main.mouseX, Main.mouseY);
}
