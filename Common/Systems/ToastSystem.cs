using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using WorldConfigMod.Common.UI;

namespace WorldConfigMod.Common.Systems;

// Mod-load toast + queue drawn via ModifyInterfaceLayers / InterfaceScaleType.UI (wiki).
[Autoload(Side = ModSide.Client)]
public class ToastSystem : ModSystem
{
    public override void OnModLoad()
    {
        if (Main.dedServ) return;

        string version = Mod?.Version?.ToString() ?? "0.1";
        ToastManager.Push(
            $"World Config v{version} loaded — open New World to configure!",
            new Color(110, 200, 110),
            lifetimeSec: 7.0);
    }

    public override void OnModUnload()
    {
        if (Main.dedServ) return;
        ToastManager.Clear();
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        if (Main.dedServ) return;

        int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
        if (mouseTextIndex == -1)
            mouseTextIndex = layers.Count;

        layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
            "WorldConfigMod: Toasts",
            () =>
            {
                // Interface layers do not run on the title / New World menu (Main.gameMenu).
                if (!Main.gameMenu)
                    ToastManager.DrawAndPrune(Main.spriteBatch, useFramebuffer: false);
                return true;
            },
            InterfaceScaleType.UI));
    }
}
