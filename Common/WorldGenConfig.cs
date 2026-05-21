using System.Collections.Generic;
using WorldConfigMod.Core;

namespace WorldConfigMod.Common;

public static class WorldGenConfig
{
    // Vanilla Small (4200x1200) is the lowest size vanilla world gen passes survive.
    // Below this, hardcoded UnifiedRandom.Next(min, max) ranges in vanilla AddGenPasses invert.
    public const int MinWidth = 4200;
    public const int MaxWidth = 16800;
    public const int MinHeight = 1200;
    public const int MaxHeight = 4800;

    public static bool UseCustom = false;

    // UI: false = legacy 2-column panel, true = sidebar V2 panel.
    public static bool UseV2Panel = true;

    public static int WorldWidth = 4200;
    public static int WorldHeight = 1200;

    // Ore generation
    public static float OreVeinSizeMul = 1f;
    public static float OreFrequencyMul = 1f;
    public static readonly Dictionary<string, float> OreMul = OreConfigHelper.CreateDefaultOreMul();

    // World Shape
    public static float CaveDepthMul = 1f;   // 0.5–2.0: scales worldSurface + rockLayer after Terrain pass
    public static int   DungeonSide  = 0;    // -1 = force left, 0 = vanilla random, +1 = force right

    // World Features
    public static float GemsMul           = 1f;  // supplement gem veins in cavern zones
    public static float LifeCrystalsMul   = 1f;  // supplement heart crystal placements
    public static float ChestsMul         = 1f;  // supplement buried chest placements
    public static float FloatingIslandsMul = 1f; // supplement floating island count
    public static float MarbleGraniteMul  = 1f;  // supplement marble + granite patch density
    public static float PotsMul           = 1f;  // supplement underground pots
    public static float HellforgesMul     = 1f;  // supplement underworld hellforges
    public static float ShadowOrbsMul     = 1f;  // supplement Shadow Orbs / Crimson Hearts
    public static float LivingTreesMul    = 1f;  // supplement surface living trees
    public static float SpiderCavesMul    = 1f;  // supplement spider cave nests
    public static float HivesMul          = 1f;  // supplement bee hives in jungle
    public static float MushroomMul       = 1f;  // supplement surface glowing mushroom patches
    public static float TrapsMul          = 1f;  // supplement cave/dungeon traps
    public static float HerbsMul          = 1f;  // supplement herb plants on surface/cavern
    public static float LakesMul          = 1f;  // supplement surface lakes (water pools)
    public static float ShrinesMul        = 1f;  // supplement enchanted-sword shrine attempts

    // Ore meta (extends existing altar/meteor hooks)
    public static float AltarPatchMul     = 1f;  // scales vanilla SmashAltar tile patch size
    public static float MeteorChanceMul   = 1f;  // biases meteor spawn roll

    // World Shape extras
    public static int   JungleSide    = 0;       // -1 = left, 0 = vanilla, +1 = right
    public static int   PyramidsMode  = 0;       // -1 = disable, 0 = vanilla, +1 = force/boost
    public static bool  NoEvilSpread = false;    // disable HM corruption/crimson spread
    public static bool  NoHallowSpread = false;  // disable HM hallow spread

    public static void Reset()
    {
        UseCustom = false;
        WorldWidth = 4200;
        WorldHeight = 1200;
        OreVeinSizeMul = 1f;
        OreFrequencyMul = 1f;
        OreConfigHelper.ResetAll(OreMul);
        CaveDepthMul        = 1f;
        DungeonSide         = 0;
        GemsMul             = 1f;
        LifeCrystalsMul     = 1f;
        ChestsMul           = 1f;
        FloatingIslandsMul  = 1f;
        MarbleGraniteMul    = 1f;
        PotsMul             = 1f;
        HellforgesMul       = 1f;
        ShadowOrbsMul       = 1f;
        LivingTreesMul      = 1f;
        SpiderCavesMul      = 1f;
        HivesMul            = 1f;
        MushroomMul         = 1f;
        TrapsMul            = 1f;
        HerbsMul            = 1f;
        LakesMul            = 1f;
        ShrinesMul          = 1f;
        AltarPatchMul       = 1f;
        MeteorChanceMul     = 1f;
        JungleSide          = 0;
        PyramidsMode        = 0;
        NoEvilSpread        = false;
        NoHallowSpread      = false;
    }

    public static void ApplyPresetSize(int preset)
    {
        switch (preset)
        {
            case 1: WorldWidth = 4200; WorldHeight = 1200; break;
            case 2: WorldWidth = 6400; WorldHeight = 1800; break;
            case 3: WorldWidth = 8400; WorldHeight = 2400; break;
            case 4: WorldWidth = 12000; WorldHeight = 3600; break;
            case 5: WorldWidth = 16800; WorldHeight = 4800; break;
        }
    }

    // ---- Preset bundles (one-click multi-field apply) ----

    public static void ApplyResourceRichPreset()
    {
        UseCustom = true;
        OreFrequencyMul = 2.5f;
        OreVeinSizeMul  = 1.5f;
        GemsMul         = 3f;
        LifeCrystalsMul = 2f;
        ChestsMul       = 2f;
        PotsMul         = 2f;
        HellforgesMul   = 2f;
        ShadowOrbsMul   = 1.5f;
        HerbsMul        = 2f;
        MeteorChanceMul = 2f;
        AltarPatchMul   = 1.5f;
    }

    public static void ApplyCaveLabyrinthPreset()
    {
        UseCustom = true;
        CaveDepthMul     = 1.6f;
        MarbleGraniteMul = 2.5f;
        SpiderCavesMul   = 3f;
        TrapsMul         = 2.5f;
        GemsMul          = 1.5f;
        MushroomMul      = 2f;
        LakesMul         = 1.5f;
    }

    public static void ApplyMinimalEvilPreset()
    {
        UseCustom = true;
        NoEvilSpread   = true;
        NoHallowSpread = true;
        ShadowOrbsMul  = 0f;       // none generated past vanilla; vanilla still places base orbs
        AltarPatchMul  = 0.5f;     // smaller HM ore patches
    }

    // Tiny map + heavy ore for fast iteration. Size = mod minimum (~0.4× vanilla small width).
    public static void ApplyDebugWorldGenPreset()
    {
        UseCustom = true;
        WorldWidth = MinWidth;
        WorldHeight = MinHeight;
        OreVeinSizeMul = 20f;
        OreFrequencyMul = 20f;
        OreConfigHelper.ResetAll(OreMul);
    }

    public static int Clamp(int v, int lo, int hi) => v < lo ? lo : v > hi ? hi : v;

    // ---- Defaults / diff detection ----
    public const int     DefaultWorldWidth         = 4200;
    public const int     DefaultWorldHeight        = 1200;
    public const float   DefaultOreVeinSizeMul     = 1f;
    public const float   DefaultOreFrequencyMul    = 1f;
    public const float   DefaultCaveDepthMul       = 1f;
    public const int     DefaultDungeonSide        = 0;
    public const float   DefaultGemsMul            = 1f;
    public const float   DefaultLifeCrystalsMul    = 1f;
    public const float   DefaultChestsMul          = 1f;
    public const float   DefaultFloatingIslandsMul = 1f;
    public const float   DefaultMarbleGraniteMul   = 1f;
    public const float   DefaultPotsMul            = 1f;
    public const float   DefaultHellforgesMul      = 1f;
    public const float   DefaultShadowOrbsMul      = 1f;
    public const float   DefaultLivingTreesMul     = 1f;
    public const float   DefaultSpiderCavesMul     = 1f;
    public const float   DefaultHivesMul           = 1f;
    public const float   DefaultMushroomMul        = 1f;
    public const float   DefaultTrapsMul           = 1f;
    public const float   DefaultHerbsMul           = 1f;
    public const float   DefaultLakesMul           = 1f;
    public const float   DefaultShrinesMul         = 1f;
    public const float   DefaultAltarPatchMul      = 1f;
    public const float   DefaultMeteorChanceMul    = 1f;
    public const int     DefaultPyramidsMode       = 0;
    public const int     DefaultJungleSide         = 0;
    public const bool    DefaultNoEvilSpread       = false;
    public const bool    DefaultNoHallowSpread     = false;
    public const float   DefaultOreMul             = 1f;

    private static bool NearDefault(float v, float d) => System.Math.Abs(v - d) < 0.0005f;

    public static int CountChanges()
    {
        int n = 0;
        if (WorldWidth         != DefaultWorldWidth)              n++;
        if (WorldHeight        != DefaultWorldHeight)             n++;
        if (!NearDefault(OreVeinSizeMul,     DefaultOreVeinSizeMul))     n++;
        if (!NearDefault(OreFrequencyMul,    DefaultOreFrequencyMul))    n++;
        if (!NearDefault(CaveDepthMul,       DefaultCaveDepthMul))       n++;
        if (DungeonSide        != DefaultDungeonSide)             n++;
        if (!NearDefault(GemsMul,            DefaultGemsMul))            n++;
        if (!NearDefault(LifeCrystalsMul,    DefaultLifeCrystalsMul))    n++;
        if (!NearDefault(ChestsMul,          DefaultChestsMul))          n++;
        if (!NearDefault(FloatingIslandsMul, DefaultFloatingIslandsMul)) n++;
        if (!NearDefault(MarbleGraniteMul,   DefaultMarbleGraniteMul))   n++;
        if (!NearDefault(PotsMul,            DefaultPotsMul))            n++;
        if (!NearDefault(HellforgesMul,      DefaultHellforgesMul))      n++;
        if (!NearDefault(ShadowOrbsMul,      DefaultShadowOrbsMul))      n++;
        if (!NearDefault(LivingTreesMul,     DefaultLivingTreesMul))     n++;
        if (!NearDefault(SpiderCavesMul,     DefaultSpiderCavesMul))     n++;
        if (!NearDefault(HivesMul,           DefaultHivesMul))           n++;
        if (!NearDefault(MushroomMul,        DefaultMushroomMul))        n++;
        if (!NearDefault(TrapsMul,           DefaultTrapsMul))           n++;
        if (!NearDefault(HerbsMul,           DefaultHerbsMul))           n++;
        if (!NearDefault(LakesMul,           DefaultLakesMul))           n++;
        if (!NearDefault(ShrinesMul,         DefaultShrinesMul))         n++;
        if (!NearDefault(AltarPatchMul,      DefaultAltarPatchMul))      n++;
        if (!NearDefault(MeteorChanceMul,    DefaultMeteorChanceMul))    n++;
        if (JungleSide != DefaultJungleSide) n++;
        if (PyramidsMode != DefaultPyramidsMode) n++;
        if (NoEvilSpread != DefaultNoEvilSpread) n++;
        if (NoHallowSpread != DefaultNoHallowSpread) n++;
        foreach (var kv in OreMul)
            if (!NearDefault(kv.Value, DefaultOreMul)) n++;
        return n;
    }

    public static int CountOreChanges()
    {
        int n = 0;
        if (!NearDefault(OreVeinSizeMul,  DefaultOreVeinSizeMul))  n++;
        if (!NearDefault(OreFrequencyMul, DefaultOreFrequencyMul)) n++;
        foreach (var kv in OreMul)
            if (!NearDefault(kv.Value, DefaultOreMul)) n++;
        return n;
    }

    public static int CountWorldChanges()
    {
        int n = 0;
        if (WorldWidth  != DefaultWorldWidth)  n++;
        if (WorldHeight != DefaultWorldHeight) n++;
        if (!NearDefault(CaveDepthMul, DefaultCaveDepthMul)) n++;
        if (DungeonSide != DefaultDungeonSide) n++;
        if (JungleSide   != DefaultJungleSide)   n++;
        if (PyramidsMode != DefaultPyramidsMode) n++;
        if (NoEvilSpread   != DefaultNoEvilSpread)   n++;
        if (NoHallowSpread != DefaultNoHallowSpread) n++;
        return n;
    }

    public static int CountFeatureChanges()
    {
        int n = 0;
        if (!NearDefault(GemsMul,            DefaultGemsMul))            n++;
        if (!NearDefault(LifeCrystalsMul,    DefaultLifeCrystalsMul))    n++;
        if (!NearDefault(ChestsMul,          DefaultChestsMul))          n++;
        if (!NearDefault(FloatingIslandsMul, DefaultFloatingIslandsMul)) n++;
        if (!NearDefault(MarbleGraniteMul,   DefaultMarbleGraniteMul))   n++;
        if (!NearDefault(PotsMul,            DefaultPotsMul))            n++;
        if (!NearDefault(HellforgesMul,      DefaultHellforgesMul))      n++;
        if (!NearDefault(ShadowOrbsMul,      DefaultShadowOrbsMul))      n++;
        if (!NearDefault(LivingTreesMul,     DefaultLivingTreesMul))     n++;
        if (!NearDefault(SpiderCavesMul,     DefaultSpiderCavesMul))     n++;
        if (!NearDefault(HivesMul,           DefaultHivesMul))           n++;
        if (!NearDefault(MushroomMul,        DefaultMushroomMul))        n++;
        if (!NearDefault(TrapsMul,           DefaultTrapsMul))           n++;
        if (!NearDefault(HerbsMul,           DefaultHerbsMul))           n++;
        if (!NearDefault(LakesMul,           DefaultLakesMul))           n++;
        if (!NearDefault(ShrinesMul,         DefaultShrinesMul))         n++;
        return n;
    }

    public static bool IsOreDefault(string key) =>
        OreMul.TryGetValue(key, out var v) && NearDefault(v, DefaultOreMul);
}
