using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using WorldConfigMod.Common.UI;

namespace WorldConfigMod.Common.Systems;

// DrawMenu: use Viewport pixels + Matrix.Identity (Main.screen* is layout space, not window size).
[Autoload(Side = ModSide.Client)]
public class MenuDrawSystem : ModSystem
{
    public override void Load()
    {
        On_Main.DrawMenu += DrawMenuExtras;
    }

    public override void Unload()
    {
        On_Main.DrawMenu -= DrawMenuExtras;
    }

    private void DrawMenuExtras(On_Main.orig_DrawMenu orig, Main self, GameTime gameTime)
    {
        orig(self, gameTime);
        if (!Main.gameMenu)
            return;

        var sb = Main.spriteBatch;
        EnsureSpriteBatchClosed(sb);
        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
            SamplerState.LinearClamp, DepthStencilState.None,
            RasterizerState.CullNone, null, Matrix.Identity);

        ToastManager.DrawAndPrune(sb, useFramebuffer: true);
        if (UIInjectSystem.ShouldDrawOverlay())
            UIInjectSystem.DrawOverlayButton(sb);

        sb.End();

        // orig_DrawMenu already drew the cursor; our HUD runs after — redraw so cursor stays on top.
        RedrawMenuCursor();
    }

    private static void RedrawMenuCursor()
    {
        var sb = Main.spriteBatch;
        EnsureSpriteBatchClosed(sb);
        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
            SamplerState.LinearClamp, DepthStencilState.None,
            RasterizerState.CullNone, null, Main.UIScaleMatrix);
        try { Main.DrawCursor(Main.DrawThickCursor()); }
        catch (System.Exception) { /* tML API drift */ }
        finally { EnsureSpriteBatchClosed(sb); }
    }

    // Calling sb.End() speculatively throws InvalidOperationException when the batch
    // is already closed — and tML's IL hook weaver observes the exception before our
    // catch block runs, spamming "Silently Caught Exception" warnings in the log.
    // Read the private beginCalled field instead so we never throw.
    private static readonly System.Reflection.FieldInfo _beginCalledField =
        typeof(SpriteBatch).GetField("beginCalled",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

    private static void EnsureSpriteBatchClosed(SpriteBatch sb)
    {
        if (_beginCalledField?.GetValue(sb) is true)
            sb.End();
    }
}
