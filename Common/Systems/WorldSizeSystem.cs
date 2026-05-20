using Terraria;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace WorldConfigMod.Common.Systems;

// Override Main.maxTilesX / maxTilesY before vanilla clearWorld allocates arrays.
public class WorldSizeSystem : ModSystem
{
    public override void Load()
    {
        On_WorldGen.clearWorld += HookClearWorld;
    }

    public override void Unload()
    {
        On_WorldGen.clearWorld -= HookClearWorld;
    }

    private void HookClearWorld(On_WorldGen.orig_clearWorld orig)
    {
        if (WorldGenConfig.UseCustom)
        {
            Main.maxTilesX = WorldGenConfig.Clamp(WorldGenConfig.WorldWidth,
                WorldGenConfig.MinWidth, WorldGenConfig.MaxWidth);
            Main.maxTilesY = WorldGenConfig.Clamp(WorldGenConfig.WorldHeight,
                WorldGenConfig.MinHeight, WorldGenConfig.MaxHeight);

            // Recompute derived spawn / surface heights so vanilla code does not
            // explode when the player picked a non-standard ratio.
            Main.bottomWorld = Main.maxTilesY * 16f;
            Main.rightWorld = Main.maxTilesX * 16f;
        }
        orig();
    }
}
