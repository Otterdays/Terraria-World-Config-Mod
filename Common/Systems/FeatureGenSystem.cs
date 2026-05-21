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

        // Force jungle to one side. Vanilla picks jungleOriginX in the Jungle pass; we override.
        if (WorldGenConfig.JungleSide != 0)
        {
            int w = Main.maxTilesX;
            // Left = ~25% across, Right = ~75% across.
            GenVars.jungleOriginX = WorldGenConfig.JungleSide < 0
                ? (int)(w * 0.25f)
                : (int)(w * 0.75f);
        }
    }

    public override void ModifyHardmodeTasks(List<GenPass> tasks)
    {
        if (!WorldGenConfig.UseCustom)
            return;

        if (WorldGenConfig.NoEvilSpread)
        {
            DisablePass(tasks, "Hardmode Good");      // hallow + evil block-spread setup
            DisablePass(tasks, "Hardmode Evil");
        }
        if (WorldGenConfig.NoHallowSpread)
        {
            DisablePass(tasks, "Hardmode Good Remix");
        }
    }

    private static void DisablePass(List<GenPass> tasks, string name)
    {
        var p = tasks.Find(x => x.Name == name);
        p?.Disable();
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

        // Pots — supplement underground pots.
        InsertAfterAny(tasks, new[] { "Pots", "Pyramids" }, 0.3f,
            "WorldConfig Pots", SupplementPots);

        // Hellforges — extra forges in underworld.
        InsertAfterAny(tasks, new[] { "Hellforge", "Underworld" }, 0.3f,
            "WorldConfig Hellforges", SupplementHellforges);

        // Shadow Orbs / Crimson Hearts.
        InsertAfterAny(tasks, new[] { "Altars", "Shadow Orbs" }, 0.3f,
            "WorldConfig ShadowOrbs", SupplementShadowOrbs);

        // Living Trees.
        InsertAfterAny(tasks, new[] { "Living Trees", "Trees" }, 0.3f,
            "WorldConfig LivingTrees", SupplementLivingTrees);

        // Spider Caves.
        InsertAfterAny(tasks, new[] { "Spider Caves", "Spider Cave" }, 0.3f,
            "WorldConfig SpiderCaves", SupplementSpiderCaves);

        // Hives — supplement bee hives in jungle.
        InsertAfterAny(tasks, new[] { "Hives", "Jungle" }, 0.3f,
            "WorldConfig Hives", SupplementHives);

        // Mushroom Patches — surface glowing mushroom.
        InsertAfterAny(tasks, new[] { "Mushroom Patches", "Jungle Trees" }, 0.3f,
            "WorldConfig Mushroom", SupplementMushroom);

        // Pyramids — disable or supplement.
        if (WorldGenConfig.PyramidsMode < 0)
            DisablePass(tasks, "Pyramids");
        else if (WorldGenConfig.PyramidsMode > 0)
            InsertAfter(tasks, "Pyramids", 0.3f, "WorldConfig PyramidsBoost", SupplementPyramids);

        // Traps.
        InsertAfterAny(tasks, new[] { "Traps", "Dungeon" }, 0.3f,
            "WorldConfig Traps", SupplementTraps);

        // Herbs.
        InsertAfterAny(tasks, new[] { "Planting Trees", "Trees" }, 0.3f,
            "WorldConfig Herbs", SupplementHerbs);

        // Lakes — extra surface water pools.
        InsertAfterAny(tasks, new[] { "Lakes", "Surface" }, 0.3f,
            "WorldConfig Lakes", SupplementLakes);

        // Enchanted Sword shrines.
        InsertAfterAny(tasks, new[] { "Micro Biomes", "Hives" }, 0.3f,
            "WorldConfig Shrines", SupplementShrines);
    }

    // ── Pots ──────────────────────────────────────────────────────────────────
    private static void SupplementPots(GenerationProgress progress, GameConfiguration config)
    {
        float mul = WorldGenConfig.PotsMul;
        if (mul <= 1f) return;
        long area = (long)Main.maxTilesX * Main.maxTilesY;
        int extras = (int)(area / 1500f * (mul - 1f));
        if (extras <= 0) return;
        int yMin = (int)Main.worldSurface + 5;
        int yMax = Math.Max(yMin + 1, Main.maxTilesY - 220);
        int tries = extras * 6;
        for (int n = 0; n < tries && extras > 0; n++)
        {
            int x = WorldGen.genRand.Next(50, Main.maxTilesX - 50);
            int y = WorldGen.genRand.Next(yMin, yMax);
            try { if (WorldGen.PlacePot(x, y, TileID.Pots)) extras--; }
            catch { /* placement constraints failed — try again */ }
        }
    }

    // ── Hellforges ────────────────────────────────────────────────────────────
    private static void SupplementHellforges(GenerationProgress progress, GameConfiguration config)
    {
        float mul = WorldGenConfig.HellforgesMul;
        if (mul <= 1f) return;
        int baseCount = Main.maxTilesX / 200;
        int extras = (int)(baseCount * (mul - 1f));
        if (extras <= 0) return;
        int yMin = Math.Max(Main.UnderworldLayer, 50);
        int yMax = Math.Max(yMin + 5, Main.maxTilesY - 40);
        int tries = extras * 20;
        for (int n = 0; n < tries && extras > 0; n++)
        {
            int x = WorldGen.genRand.Next(80, Main.maxTilesX - 80);
            int y = WorldGen.genRand.Next(yMin, yMax);
            try
            {
                if (WorldGen.PlaceTile(x, y, TileID.Hellforge, mute: true))
                    extras--;
            }
            catch { }
        }
    }

    // ── Shadow Orbs / Crimson Hearts ──────────────────────────────────────────
    private static void SupplementShadowOrbs(GenerationProgress progress, GameConfiguration config)
    {
        float mul = WorldGenConfig.ShadowOrbsMul;
        if (mul <= 1f) return;
        // Vanilla places ~3-6 per world; small bonus to avoid breaking HM trigger pacing.
        int baseCount = 4 + Main.maxTilesX / 2100;
        int extras = (int)(baseCount * (mul - 1f));
        if (extras <= 0) return;
        int yMin = (int)Main.rockLayer;
        int yMax = Math.Max(yMin + 5, Main.maxTilesY - 220);
        int tries = extras * 30;
        for (int n = 0; n < tries && extras > 0; n++)
        {
            int x = WorldGen.genRand.Next(120, Main.maxTilesX - 120);
            int y = WorldGen.genRand.Next(yMin, yMax);
            // Place a Shadow Orb / Crimson Heart tile directly (style picks per world evil).
            try
            {
                int style = WorldGen.crimson ? 1 : 0;
                if (WorldGen.PlaceTile(x, y, TileID.ShadowOrbs, mute: true, forced: false, plr: -1, style: style))
                    extras--;
            }
            catch { }
        }
    }

    // ── Living Trees ──────────────────────────────────────────────────────────
    private static void SupplementLivingTrees(GenerationProgress progress, GameConfiguration config)
    {
        float mul = WorldGenConfig.LivingTreesMul;
        if (mul <= 1f) return;
        int baseCount = 2 + Main.maxTilesX / 4200;
        int extras = Math.Min((int)(baseCount * (mul - 1f)), 10);
        if (extras <= 0) return;
        int yMin = 80;
        int yMax = Math.Max(yMin + 5, (int)Main.worldSurface - 10);
        int tries = extras * 30;
        for (int n = 0; n < tries && extras > 0; n++)
        {
            int x = WorldGen.genRand.Next(150, Main.maxTilesX - 150);
            int y = WorldGen.genRand.Next(yMin, yMax);
            try
            {
                if (WorldGen.GrowLivingTree(x, y))
                    extras--;
            }
            catch { }
        }
    }

    // ── Spider Caves ──────────────────────────────────────────────────────────
    private static void SupplementSpiderCaves(GenerationProgress progress, GameConfiguration config)
    {
        float mul = WorldGenConfig.SpiderCavesMul;
        if (mul <= 1f) return;
        long area = (long)Main.maxTilesX * Main.maxTilesY;
        int extras = (int)(area / 60000f * (mul - 1f));
        if (extras <= 0) return;
        int yMin = (int)Main.rockLayer + 30;
        int yMax = Math.Max(yMin + 5, Main.maxTilesY - 200);
        for (int i = 0; i < extras; i++)
        {
            int x = WorldGen.genRand.Next(80, Main.maxTilesX - 80);
            int y = WorldGen.genRand.Next(yMin, yMax);
            try
            {
                // Scatter cobweb clusters around cave space — closest stable approximation
                // of spider-cave density that doesn't depend on the private vanilla nest gen.
                int radius = WorldGen.genRand.Next(8, 14);
                for (int k = 0; k < 40; k++)
                {
                    int cx = x + WorldGen.genRand.Next(-radius, radius + 1);
                    int cy = y + WorldGen.genRand.Next(-radius, radius + 1);
                    if (cx < 5 || cx >= Main.maxTilesX - 5) continue;
                    if (cy < 5 || cy >= Main.maxTilesY - 5) continue;
                    var t = Main.tile[cx, cy];
                    if (t != null && !t.HasTile)
                        WorldGen.PlaceTile(cx, cy, TileID.Cobweb, mute: true);
                }
            }
            catch { }
        }
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

    // ── Hives ─────────────────────────────────────────────────────────────────
    private static void SupplementHives(GenerationProgress progress, GameConfiguration config)
    {
        float mul = WorldGenConfig.HivesMul;
        if (mul <= 1f) return;
        int baseCount = 2 + Main.maxTilesX / 2100;
        int extras = Math.Min((int)(baseCount * (mul - 1f)), 12);
        if (extras <= 0) return;
        // Jungle band roughly: jungleOriginX ± width/8, below worldSurface.
        int jx = GenVars.jungleOriginX == 0 ? Main.maxTilesX / 2 : GenVars.jungleOriginX;
        int band = Math.Max(200, Main.maxTilesX / 8);
        int yMin = (int)Main.worldSurface + 40;
        int yMax = Math.Max(yMin + 5, Main.maxTilesY - 250);
        int tries = extras * 20;
        for (int n = 0; n < tries && extras > 0; n++)
        {
            int x = Math.Clamp(jx + WorldGen.genRand.Next(-band, band + 1), 100, Main.maxTilesX - 100);
            int y = WorldGen.genRand.Next(yMin, yMax);
            try
            {
                // Hive tile scatter — vanilla hive carve is private; approximate via TileRunner.
                WorldGen.TileRunner(x, y, WorldGen.genRand.Next(8, 14),
                    WorldGen.genRand.Next(15, 25), TileID.Hive, addTile: true);
                extras--;
            }
            catch { }
        }
    }

    // ── Mushroom Patches ──────────────────────────────────────────────────────
    private static void SupplementMushroom(GenerationProgress progress, GameConfiguration config)
    {
        float mul = WorldGenConfig.MushroomMul;
        if (mul <= 1f) return;
        int baseCount = 1 + Main.maxTilesX / 2100;
        int extras = Math.Min((int)(baseCount * (mul - 1f)), 8);
        if (extras <= 0) return;
        int yMin = (int)Main.rockLayer;
        int yMax = Math.Max(yMin + 5, Main.maxTilesY - 220);
        int tries = extras * 20;
        for (int n = 0; n < tries && extras > 0; n++)
        {
            int x = WorldGen.genRand.Next(150, Main.maxTilesX - 150);
            int y = WorldGen.genRand.Next(yMin, yMax);
            try
            {
                WorldGen.TileRunner(x, y, WorldGen.genRand.Next(10, 18),
                    WorldGen.genRand.Next(20, 40), TileID.MushroomGrass, addTile: true);
                extras--;
            }
            catch { }
        }
    }

    // ── Pyramids supplement (when PyramidsMode > 0) ───────────────────────────
    private static void SupplementPyramids(GenerationProgress progress, GameConfiguration config)
    {
        // Vanilla places at most 1 pyramid per desert; we attempt a couple of extra placements.
        // Pyramid() is private — best-effort via TileRunner sand mound; cosmetic only.
        for (int i = 0; i < 2; i++)
        {
            int x = WorldGen.genRand.Next(200, Main.maxTilesX - 200);
            int y = Math.Max(50, (int)Main.worldSurface - 20);
            try
            {
                WorldGen.TileRunner(x, y, 30, 8, TileID.SandstoneBrick, addTile: true);
            }
            catch { }
        }
    }

    // ── Traps ─────────────────────────────────────────────────────────────────
    private static void SupplementTraps(GenerationProgress progress, GameConfiguration config)
    {
        float mul = WorldGenConfig.TrapsMul;
        if (mul <= 1f) return;
        long area = (long)Main.maxTilesX * Main.maxTilesY;
        int extras = (int)(area / 50000f * (mul - 1f));
        if (extras <= 0) return;
        int yMin = (int)Main.rockLayer;
        int yMax = Math.Max(yMin + 5, Main.maxTilesY - 200);
        int tries = extras * 8;
        for (int n = 0; n < tries && extras > 0; n++)
        {
            int x = WorldGen.genRand.Next(80, Main.maxTilesX - 80);
            int y = WorldGen.genRand.Next(yMin, yMax);
            try
            {
                int style = WorldGen.genRand.Next(6);
                if (WorldGen.PlaceTile(x, y, TileID.Traps, mute: true, forced: false, plr: -1, style: style))
                    extras--;
            }
            catch { }
        }
    }

    // ── Herbs ─────────────────────────────────────────────────────────────────
    private static void SupplementHerbs(GenerationProgress progress, GameConfiguration config)
    {
        float mul = WorldGenConfig.HerbsMul;
        if (mul <= 1f) return;
        long area = (long)Main.maxTilesX * Main.maxTilesY;
        int extras = (int)(area / 3000f * (mul - 1f));
        if (extras <= 0) return;
        int yMin = 50;
        int yMax = Math.Max(yMin + 5, Main.maxTilesY - 200);
        int tries = extras * 4;
        for (int n = 0; n < tries && extras > 0; n++)
        {
            int x = WorldGen.genRand.Next(50, Main.maxTilesX - 50);
            int y = WorldGen.genRand.Next(yMin, yMax);
            try
            {
                if (WorldGen.PlaceTile(x, y, TileID.MatureHerbs, mute: true, forced: false, plr: -1,
                    style: WorldGen.genRand.Next(7)))
                    extras--;
            }
            catch { }
        }
    }

    // ── Lakes (surface water pools) ───────────────────────────────────────────
    private static void SupplementLakes(GenerationProgress progress, GameConfiguration config)
    {
        float mul = WorldGenConfig.LakesMul;
        if (mul <= 1f) return;
        int baseCount = 2 + Main.maxTilesX / 4200;
        int extras = Math.Min((int)(baseCount * (mul - 1f)), 8);
        if (extras <= 0) return;
        for (int i = 0; i < extras; i++)
        {
            int x = WorldGen.genRand.Next(200, Main.maxTilesX - 200);
            int y = Math.Max(50, (int)Main.worldSurface - 5);
            try
            {
                // Carve a small bowl, then flood with water.
                int radius = WorldGen.genRand.Next(8, 16);
                for (int dx = -radius; dx <= radius; dx++)
                for (int dy = -radius / 2; dy <= radius / 2; dy++)
                {
                    int tx = x + dx, ty = y + dy;
                    if (tx < 5 || tx >= Main.maxTilesX - 5 || ty < 5 || ty >= Main.maxTilesY - 5) continue;
                    if (dx * dx + dy * dy * 4 > radius * radius) continue;
                    var t = Main.tile[tx, ty];
                    if (t == null) continue;
                    t.ClearTile();
                    t.LiquidAmount = 255;
                    t.LiquidType = LiquidID.Water;
                }
            }
            catch { }
        }
    }

    // ── Enchanted Sword shrines ───────────────────────────────────────────────
    private static void SupplementShrines(GenerationProgress progress, GameConfiguration config)
    {
        float mul = WorldGenConfig.ShrinesMul;
        if (mul <= 1f) return;
        int extras = Math.Min((int)(2 * (mul - 1f) + 0.5f), 6);
        if (extras <= 0) return;
        int yMin = (int)Main.worldSurface + 30;
        int yMax = Math.Max(yMin + 5, (int)Main.rockLayer + 80);
        int tries = extras * 30;
        for (int n = 0; n < tries && extras > 0; n++)
        {
            int x = WorldGen.genRand.Next(200, Main.maxTilesX - 200);
            int y = WorldGen.genRand.Next(yMin, yMax);
            try
            {
                // Stone shrine block + breakable sword art via TileID.BreakableIce as placeholder;
                // vanilla shrine carve is private. Approximate with a small stone alcove.
                WorldGen.TileRunner(x, y, 6, 8, TileID.Stone, addTile: true);
                extras--;
            }
            catch { }
        }
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
