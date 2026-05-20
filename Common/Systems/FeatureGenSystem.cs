using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using WorldConfigMod.Common;

namespace WorldConfigMod.Common.Systems;

/// <summary>
/// Tier-1 world-feature supplements: gems, life crystals, chests, floating islands,
/// marble/granite, cave depth scaling, and dungeon-side forcing.
/// All hooks are no-ops unless WorldGenConfig.UseCustom is true.
/// </summary>
public class FeatureGenSystem : ModSystem
{
    public override void PreWorldGen()
    {
        if (!WorldGenConfig.UseCustom)
            return;

        // Force dungeon to one side before the Dungeon pass reads GenVars.dungeonSide.
        if (WorldGenConfig.DungeonSide != 0)
            GenVars.dungeonSide = WorldGenConfig.DungeonSide;
    }

    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    {
        if (!WorldGenConfig.UseCustom)
            return;

        // Cave depth — insert right after Terrain so every subsequent pass uses scaled values.
        InsertAfter(tasks, "Terrain", 0.1f,
            "WorldConfig CaveDepth", ApplyCaveDepth);

        // Gems — insert after the last gem pass (Ice Biome gems run last).
        InsertAfterAny(tasks, new[] { "Gems In Ice Biome", "Random Gems", "Gems" }, 0.5f,
            "WorldConfig Gems", SupplementGems);

        // Life crystals — insert after vanilla "Life Crystals" pass.
        InsertAfter(tasks, "Life Crystals", 0.5f,
            "WorldConfig LifeCrystals", SupplementLifeCrystals);

        // Chests — insert after the last chest pass.
        InsertAfterAny(tasks, new[] { "Water Chests", "Surface Chests", "Buried Chests" }, 0.5f,
            "WorldConfig Chests", SupplementChests);

        // Floating islands — insert after vanilla "Floating Islands" pass.
        InsertAfter(tasks, "Floating Islands", 0.5f,
            "WorldConfig FloatingIslands", SupplementFloatingIslands);

        // Marble & granite — insert after whichever runs last.
        InsertAfterAny(tasks, new[] { "Marble", "Granite" }, 0.5f,
            "WorldConfig MarbleGranite", SupplementMarbleGranite);
    }

    // ── Cave depth ────────────────────────────────────────────────────────────

    private static void ApplyCaveDepth(GenerationProgress progress, GameConfiguration config)
    {
        float mul = WorldGenConfig.CaveDepthMul;
        if (Math.Abs(mul - 1f) < 0.01f)
            return;

        // Clamp so the world doesn't flip inside-out.
        double newSurface = Math.Clamp(Main.worldSurface * mul, 50, Main.maxTilesY * 0.35);
        double newRock    = Math.Clamp(Main.rockLayer    * mul, newSurface + 50, Main.maxTilesY * 0.65);

        Main.worldSurface   = newSurface;
        Main.rockLayer      = newRock;
        GenVars.worldSurface = (int)newSurface;
        GenVars.rockLayer    = (int)newRock;
    }

    // ── Gems ──────────────────────────────────────────────────────────────────

    // Amber is a fossil material, not a gem-cave tile; omit to avoid missing-ID errors.
    private static readonly ushort[] GemTileIds =
    {
        TileID.Amethyst, TileID.Topaz, TileID.Sapphire,
        TileID.Emerald,  TileID.Ruby,  TileID.Diamond,
    };

    private static void SupplementGems(GenerationProgress progress, GameConfiguration config)
    {
        float mul = WorldGenConfig.GemsMul;
        if (mul <= 1f)
            return;

        long area  = (long)Main.maxTilesX * Main.maxTilesY;
        int extras = (int)(area / 4000f * (mul - 1f));
        if (extras <= 0)
            return;

        int yMin = (int)Main.worldSurface;
        int yMax = Math.Max(yMin + 1, Main.maxTilesY - 100);

        for (int i = 0; i < extras; i++)
        {
            int type     = GemTileIds[WorldGen.genRand.Next(GemTileIds.Length)];
            int x        = WorldGen.genRand.Next(50, Main.maxTilesX - 50);
            int y        = WorldGen.genRand.Next(yMin, yMax);
            int strength = WorldGen.genRand.Next(2, 5);
            int steps    = WorldGen.genRand.Next(3, 8);
            WorldGen.OreRunner(x, y, strength, steps, (ushort)type);
        }
    }

    // ── Life Crystals ─────────────────────────────────────────────────────────

    private static void SupplementLifeCrystals(GenerationProgress progress, GameConfiguration config)
    {
        float mul = WorldGenConfig.LifeCrystalsMul;
        if (mul <= 1f)
            return;

        long area  = (long)Main.maxTilesX * Main.maxTilesY;
        int extras = (int)(area / 20000f * (mul - 1f));
        if (extras <= 0)
            return;

        int yMin = (int)Main.worldSurface + 20;
        int yMax = Math.Max(yMin + 1, Main.maxTilesY - 200);
        int tries = extras * 12;

        for (int n = 0; n < tries && extras > 0; n++)
        {
            int x = WorldGen.genRand.Next(100, Main.maxTilesX - 100);
            int y = WorldGen.genRand.Next(yMin, yMax);
            if (WorldGen.AddLifeCrystal(x, y))
                extras--;
        }
    }

    // ── Chests ────────────────────────────────────────────────────────────────

    private static void SupplementChests(GenerationProgress progress, GameConfiguration config)
    {
        float mul = WorldGenConfig.ChestsMul;
        if (mul <= 1f)
            return;

        long area  = (long)Main.maxTilesX * Main.maxTilesY;
        int extras = (int)(area / 30000f * (mul - 1f));
        if (extras <= 0)
            return;

        int yMin = (int)Main.worldSurface + 10;
        int yMax = Math.Max(yMin + 1, Main.maxTilesY - 200);
        int tries = extras * 12;

        for (int n = 0; n < tries && extras > 0; n++)
        {
            int x = WorldGen.genRand.Next(100, Main.maxTilesX - 100);
            int y = WorldGen.genRand.Next(yMin, yMax);
            if (WorldGen.AddBuriedChest(x, y))
                extras--;
        }
    }

    // ── Floating Islands ──────────────────────────────────────────────────────

    // FloatingIsland generates real terrain — cap tightly to prevent hangs at high multipliers.
    private const int MaxExtraIslands = 20;

    private static void SupplementFloatingIslands(GenerationProgress progress, GameConfiguration config)
    {
        float mul = WorldGenConfig.FloatingIslandsMul;
        if (mul <= 1f)
            return;

        // Vanilla places roughly (maxTilesX / 1200) + 3 islands on a Small world.
        int baseCount = 3 + Main.maxTilesX / 1200;
        int extras    = Math.Min((int)(baseCount * (mul - 1f)), MaxExtraIslands);
        if (extras <= 0)
            return;

        int yMax = Math.Max(51, (int)(Main.worldSurface * 0.4));

        for (int i = 0; i < extras; i++)
        {
            int x = WorldGen.genRand.Next(250, Main.maxTilesX - 250);
            int y = WorldGen.genRand.Next(50, yMax);
            WorldGen.FloatingIsland(x, y);
        }
    }

    // ── Marble & Granite ──────────────────────────────────────────────────────

    private static void SupplementMarbleGranite(GenerationProgress progress, GameConfiguration config)
    {
        float mul = WorldGenConfig.MarbleGraniteMul;
        if (mul <= 1f)
            return;

        long area  = (long)Main.maxTilesX * Main.maxTilesY;
        int extras = (int)(area / 8000f * (mul - 1f));
        if (extras <= 0)
            return;

        int yMin = Math.Max(1, (int)Main.rockLayer);
        int yMax = Math.Max(yMin + 1, Main.UnderworldLayer - 50);

        for (int i = 0; i < extras; i++)
        {
            int tileType = WorldGen.genRand.Next(2) == 0 ? TileID.Marble : TileID.Granite;
            int x        = WorldGen.genRand.Next(50, Main.maxTilesX - 50);
            int y        = WorldGen.genRand.Next(yMin, yMax);
            WorldGen.TileRunner(x, y, WorldGen.genRand.Next(6, 18), WorldGen.genRand.Next(10, 28),
                tileType, addTile: true);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void InsertAfter(List<GenPass> tasks, string name, float weight,
        string newName, Action<GenerationProgress, GameConfiguration> action)
    {
        int idx = tasks.FindLastIndex(p => p.Name == name);
        if (idx != -1)
            tasks.Insert(idx + 1, new SimpleGenPass(newName, weight, action));
    }

    private static void InsertAfterAny(List<GenPass> tasks, string[] names, float weight,
        string newName, Action<GenerationProgress, GameConfiguration> action)
    {
        int idx = -1;
        foreach (var name in names)
        {
            int found = tasks.FindLastIndex(p => p.Name == name);
            if (found > idx) idx = found;
        }
        if (idx != -1)
            tasks.Insert(idx + 1, new SimpleGenPass(newName, weight, action));
    }

    private sealed class SimpleGenPass : GenPass
    {
        private readonly Action<GenerationProgress, GameConfiguration> _action;

        public SimpleGenPass(string name, float weight,
            Action<GenerationProgress, GameConfiguration> action)
            : base(name, weight) => _action = action;

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
            => _action(progress, configuration);
    }
}
