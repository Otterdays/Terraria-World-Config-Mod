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
        return n;
    }

    public static bool IsOreDefault(string key) =>
        OreMul.TryGetValue(key, out var v) && NearDefault(v, DefaultOreMul);
}
