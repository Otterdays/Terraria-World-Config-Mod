using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using WorldConfigMod.Common.Ore;
using WorldConfigMod.Core;

namespace WorldConfigMod.Common.Systems;

// Replaces/supplements vanilla ore generation when WorldGenConfig.UseCustom is true.
public class OreGenSystem : ModSystem
{
    public override void Load()
    {
        On_WorldGen.SmashAltar += OnSmashAltar;
        On_WorldGen.dropMeteor += OnDropMeteor;
    }

    public override void Unload()
    {
        On_WorldGen.SmashAltar -= OnSmashAltar;
        On_WorldGen.dropMeteor -= OnDropMeteor;
    }

    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    {
        if (!WorldGenConfig.UseCustom)
            return;

        ReplacePass(tasks, "Shinies", PrehardmodeOres);

        int underIdx = tasks.FindIndex(p => p.Name == "Underworld");
        if (underIdx != -1)
            tasks.Insert(underIdx + 1, new ScaledGenPass("WorldConfig Hellstone", 0.5f, SupplementHellstone));
    }

    public override void ModifyHardmodeTasks(List<GenPass> tasks)
    {
        if (!WorldGenConfig.UseCustom)
            return;

        tasks.Add(new ScaledGenPass("WorldConfig Chlorophyte", 0.5f, SupplementChlorophyte));
    }

    private static void ReplacePass(
        List<GenPass> tasks,
        string name,
        Action<GenerationProgress, GameConfiguration> action)
    {
        int idx = tasks.FindIndex(p => p.Name == name);
        if (idx != -1)
            tasks[idx] = new ScaledGenPass(name, (float)tasks[idx].Weight, action);
    }

    private void PrehardmodeOres(GenerationProgress progress, GameConfiguration config)
    {
        progress.Message =
            $"World Config: ores (×{WorldGenConfig.OreFrequencyMul:0.##} freq, ×{WorldGenConfig.OreVeinSizeMul:0.##} veins)";

        long area = (long)Main.maxTilesX * Main.maxTilesY;

        foreach (var spec in OreScatterSpecs.PreHardmode)
        {
            if (spec.Key == "Demonite" && WorldGen.crimson)
                continue;
            if (spec.Key == "Crimtane" && !WorldGen.crimson)
                continue;

            OreScatterRunner.Scatter(spec, area);
        }
    }

    private void SupplementHellstone(GenerationProgress progress, GameConfiguration config)
    {
        progress.Message = "World Config: hellstone supplement";
        long area = (long)Main.maxTilesX * Main.maxTilesY;
        OreScatterRunner.Scatter(OreScatterSpecs.Hellstone, area);
    }

    private void SupplementChlorophyte(GenerationProgress progress, GameConfiguration config)
    {
        progress.Message = "World Config: chlorophyte supplement";
        long area = (long)Main.maxTilesX * Main.maxTilesY;
        OreScatterRunner.Scatter(OreScatterSpecs.Chlorophyte, area);
    }

    private void OnSmashAltar(On_WorldGen.orig_SmashAltar orig, int i, int j)
    {
        orig(i, j);

        if (!WorldGenConfig.UseCustom)
            return;

        float freq = WorldGenConfig.OreFrequencyMul;
        float vein = WorldGenConfig.OreVeinSizeMul;
        float combined = freq * vein;
        if (combined <= 1f)
            return;

        // Vanilla places one HM tier per altar; add supplemental veins for active world ores.
        int extraVeins = OreGenMath.ComputeSupplementalCount(12, combined);
        if (extraVeins <= 0)
            return;

        foreach (var spec in OreScatterSpecs.HardmodeAltar)
        {
            if (!WasHardmodeOrePlacedNear(i, j, spec.TileId))
                continue;

            float oreMul = OreScatterRunner.GetOreMul(spec.Key);
            if (oreMul <= 0f)
                continue;

            int count = Math.Max(1, (int)(extraVeins * oreMul));
            OreScatterRunner.ScatterAround(i, j, spec, count);
        }
    }

    private static bool WasHardmodeOrePlacedNear(int centerX, int centerY, ushort tileId)
    {
        int radius = 250;
        int xMin = Math.Max(0, centerX - radius);
        int xMax = Math.Min(Main.maxTilesX - 1, centerX + radius);
        int yMin = Math.Max(0, centerY - radius);
        int yMax = Math.Min(Main.maxTilesY - 1, centerY + radius);

        for (int x = xMin; x <= xMax; x += 8)
        {
            for (int y = yMin; y <= yMax; y += 8)
            {
                if (Main.tile[x, y].HasTile && Main.tile[x, y].TileType == tileId)
                    return true;
            }
        }

        return false;
    }

    private void OnDropMeteor(On_WorldGen.orig_dropMeteor orig)
    {
        orig();

        if (!WorldGenConfig.UseCustom)
            return;

        float mul = OreScatterRunner.GetOreMul("Meteorite") * WorldGenConfig.OreFrequencyMul;
        int extra = OreGenMath.ComputeSupplementalCount(1, mul);
        if (extra <= 0)
            return;

        var spec = Array.Find(OreScatterSpecs.PreHardmode, s => s.Key == "Meteorite");
        if (spec == null)
            return;

        for (int n = 0; n < extra; n++)
        {
            int x = WorldGen.genRand.Next(100, Main.maxTilesX - 100);
            int y = WorldGen.genRand.Next(50, (int)Main.worldSurface);
            WorldGen.OreRunner(x, y, 8, 10, TileID.Meteorite);
        }
    }

    private sealed class ScaledGenPass : GenPass
    {
        private readonly Action<GenerationProgress, GameConfiguration> _action;

        public ScaledGenPass(string name, float loadWeight,
            Action<GenerationProgress, GameConfiguration> action)
            : base(name, loadWeight)
        {
            _action = action;
        }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            _action(progress, configuration);
        }
    }
}
