using System;
using Terraria;
using Terraria.ID;
using WorldConfigMod.Core;

namespace WorldConfigMod.Common.Ore;

public static class OreScatterRunner
{
    public static float GetOreMul(string key)
    {
        if (WorldGenConfig.OreMul.TryGetValue(key, out float mul))
            return mul;
        return 1f;
    }

    // Upper bound on veins per ore per world-gen pass.
    // Prevents multi-minute hangs at extreme frequency multipliers (e.g. ×25).
    private const int MaxVeinsPerOre = 6000;

    public static void Scatter(OreScatterSpecs.Spec spec, long area, string progressLabel = null)
    {
        float oreMul = GetOreMul(spec.Key);
        int count = OreGenMath.ComputeVeinCount(
            area, spec.BaseFrequency, WorldGenConfig.OreFrequencyMul, oreMul);
        if (count <= 0)
            return;

        count = Math.Min(count, MaxVeinsPerOre);

        var (strength, steps) = OreGenMath.ComputeVeinSize(
            spec.BaseStrength, spec.BaseSteps, WorldGenConfig.OreVeinSizeMul);

        ScatterVeins(spec, count, strength, steps);
    }

    public static void ScatterVeins(OreScatterSpecs.Spec spec, int count, int strength, int steps)
    {
        if (count <= 0)
            return;

        GetYRange(spec, out int yMin, out int yMax);

        for (int i = 0; i < count; i++)
        {
            int x = WorldGen.genRand.Next(0, Main.maxTilesX);
            int y = WorldGen.genRand.Next(yMin, yMax);

            if (spec.JungleOnly && !IsJungle(x, y))
            {
                i--;
                continue;
            }

            WorldGen.OreRunner(x, y, strength, steps, spec.TileId);
        }
    }

    public static void ScatterAround(int centerX, int centerY, OreScatterSpecs.Spec spec, int veinCount)
    {
        if (veinCount <= 0)
            return;

        var (strength, steps) = OreGenMath.ComputeVeinSize(
            spec.BaseStrength, spec.BaseSteps, WorldGenConfig.OreVeinSizeMul);

        int radius = 400;
        for (int i = 0; i < veinCount; i++)
        {
            int x = centerX + WorldGen.genRand.Next(-radius, radius);
            int y = centerY + WorldGen.genRand.Next(-radius, radius);
            x = Math.Clamp(x, 50, Main.maxTilesX - 50);
            y = Math.Clamp(y, (int)Main.rockLayer, Main.maxTilesY - 50);
            WorldGen.OreRunner(x, y, strength, steps, spec.TileId);
        }
    }

    private static void GetYRange(OreScatterSpecs.Spec spec, out int yMin, out int yMax)
    {
        if (spec.UnderworldOnly)
        {
            yMin = Main.UnderworldLayer;
            yMax = Main.maxTilesY - 20;
        }
        else if (spec.DeepBias)
        {
            yMin = (int)(Main.rockLayer + (Main.maxTilesY - Main.rockLayer) * 0.3);
            yMax = Main.maxTilesY - 50;
        }
        else
        {
            yMin = (int)Main.worldSurface;
            yMax = Main.maxTilesY - 50;
        }

        if (yMax <= yMin)
            yMax = yMin + 1;
    }

    private static bool IsJungle(int x, int y)
    {
        if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
            return false;

        return Main.tile[x, y].TileType == TileID.JungleGrass
            || Main.tile[x, y].TileType == TileID.Mud
            || Main.tile[x, y].TileType == TileID.JunglePlants
            || Main.tile[x, y].TileType == TileID.JunglePlants2;
    }
}
