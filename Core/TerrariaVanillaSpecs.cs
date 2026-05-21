using System;

namespace WorldConfigMod.Core;

// Terraria 1.4.4.9 + tModLoader 2026.3.3.0 constants for Core tests and WorldGenConfig alignment.
// Wiki: https://terraria.wiki.gg/wiki/World_size · Ore scatter mirrors Common/Ore/OreScatterSpecs.cs
public static class TerrariaVanillaSpecs
{
    public const string TerrariaGameVersion = "1.4.4.9";
    public const string TModLoaderRelease = "2026.3.3.0";

    // Lowest size vanilla AddGenPasses tolerate (Small).
    public const int MinSafeWidth = 4200;
    public const int MinSafeHeight = 1200;

    // Mod UI ceiling (XXL preset).
    public const int ModMaxWidth = 16800;
    public const int ModMaxHeight = 4800;

    // Pre-HM scatter frequencies (per tile area) — keep in sync with OreScatterSpecs.PreHardmode.
    public const float CopperTinBaseFrequency = 9e-5f;
    public const int CopperBaseStrength = 4;
    public const int CopperBaseSteps = 5;

    // Vanilla floating-island style scaling used in FeatureGenSystem (2 + width / 4200).
    public const int FeatureCountDivisorWidth = 4200;
    public const int FeatureCountBase = 2;

    public static readonly WorldSizePreset[] WorldPresets =
    {
        new(1, "Small",  4200,  1200, true),
        new(2, "Medium", 6400,  1800, true),
        new(3, "Large",  8400,  2400, true),
        new(4, "XL",     12000, 3600, false),
        new(5, "XXL",    16800, 4800, false),
    };

    public static bool TryGetPreset(int id, out WorldSizePreset preset)
    {
        foreach (var p in WorldPresets)
        {
            if (p.Id == id)
            {
                preset = p;
                return true;
            }
        }

        preset = default;
        return false;
    }

    public static long TileArea(int width, int height) => (long)width * height;

    // Mirrors FeatureGenSystem: int baseCount = 2 + Main.maxTilesX / 4200;
    public static int VanillaScaledFeatureBaseCount(int worldWidth) =>
        FeatureCountBase + worldWidth / FeatureCountDivisorWidth;

    public static int ExpectedCopperVeinCountAt1x(int width, int height) =>
        OreGenMath.ComputeVeinCount(TileArea(width, height), CopperTinBaseFrequency, 1f, 1f);
}

public readonly struct WorldSizePreset
{
    public int Id { get; }
    public string Label { get; }
    public int Width { get; }
    public int Height { get; }
    public bool IsVanillaUiSize { get; }

    public long TileArea => (long)Width * Height;

    public WorldSizePreset(int id, string label, int width, int height, bool isVanillaUiSize)
    {
        Id = id;
        Label = label;
        Width = width;
        Height = height;
        IsVanillaUiSize = isVanillaUiSize;
    }
}
