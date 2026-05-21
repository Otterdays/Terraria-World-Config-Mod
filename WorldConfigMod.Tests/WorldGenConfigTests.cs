using System.Linq;
using WorldConfigMod.Common;
using WorldConfigMod.Core;
using Xunit;

namespace WorldConfigMod.Tests;

public class WorldGenConfigTests
{
    [Fact]
    public void Reset_RestoresAllDefaultsAndOreKeys()
    {
        WorldGenConfig.ApplyResourceRichPreset();
        WorldGenConfig.ApplyPresetSize(5);
        WorldGenConfig.OreMul["Copper"] = 3f;

        WorldGenConfig.Reset();

        Assert.False(WorldGenConfig.UseCustom);
        Assert.Equal(TerrariaVanillaSpecs.MinSafeWidth, WorldGenConfig.WorldWidth);
        Assert.Equal(TerrariaVanillaSpecs.MinSafeHeight, WorldGenConfig.WorldHeight);
        Assert.Equal(0, WorldGenConfig.CountChanges());
        Assert.Equal(0, WorldGenConfig.CountOreChanges());
        Assert.Equal(0, WorldGenConfig.CountWorldChanges());
        Assert.Equal(0, WorldGenConfig.CountFeatureChanges());
        Assert.True(OreConfigHelper.HasAllWikiKeys(WorldGenConfig.OreMul));
        Assert.All(OreCatalog.OrderedKeys, key => Assert.Equal(1f, WorldGenConfig.OreMul[key]));
    }

    [Theory]
    [InlineData(1, 4200, 1200)]
    [InlineData(3, 8400, 2400)]
    [InlineData(5, 16800, 4800)]
    public void ApplyPresetSize_UsesTerrariaVanillaSpecs(int preset, int width, int height)
    {
        WorldGenConfig.Reset();

        WorldGenConfig.ApplyPresetSize(preset);

        Assert.Equal(width, WorldGenConfig.WorldWidth);
        Assert.Equal(height, WorldGenConfig.WorldHeight);
    }

    [Fact]
    public void ApplyPresetSize_IgnoresUnknownPreset()
    {
        WorldGenConfig.Reset();

        WorldGenConfig.ApplyPresetSize(99);

        Assert.Equal(WorldGenConfig.DefaultWorldWidth, WorldGenConfig.WorldWidth);
        Assert.Equal(WorldGenConfig.DefaultWorldHeight, WorldGenConfig.WorldHeight);
        Assert.Equal(0, WorldGenConfig.CountWorldChanges());
    }

    [Fact]
    public void CountHelpers_GroupChangesByUiTab()
    {
        WorldGenConfig.Reset();

        WorldGenConfig.WorldWidth = 6400;
        WorldGenConfig.CaveDepthMul = 1.25f;
        WorldGenConfig.GemsMul = 2f;
        WorldGenConfig.OreFrequencyMul = 1.5f;
        WorldGenConfig.OreMul["Gold"] = 4f;

        Assert.Equal(5, WorldGenConfig.CountChanges());
        Assert.Equal(2, WorldGenConfig.CountWorldChanges());
        Assert.Equal(1, WorldGenConfig.CountFeatureChanges());
        Assert.Equal(2, WorldGenConfig.CountOreChanges());
        Assert.False(WorldGenConfig.IsOreDefault("Gold"));
        Assert.True(WorldGenConfig.IsOreDefault("Copper"));
    }

    [Fact]
    public void PresetBundles_EnableCustomAndTouchExpectedGroups()
    {
        WorldGenConfig.Reset();
        WorldGenConfig.ApplyResourceRichPreset();

        Assert.True(WorldGenConfig.UseCustom);
        Assert.Equal(11, WorldGenConfig.CountChanges());
        Assert.Equal(2, WorldGenConfig.CountOreChanges());
        Assert.Equal(7, WorldGenConfig.CountFeatureChanges());

        WorldGenConfig.Reset();
        WorldGenConfig.ApplyCaveLabyrinthPreset();

        Assert.True(WorldGenConfig.UseCustom);
        Assert.Equal(7, WorldGenConfig.CountChanges());
        Assert.Equal(1, WorldGenConfig.CountWorldChanges());
        Assert.Equal(6, WorldGenConfig.CountFeatureChanges());

        WorldGenConfig.Reset();
        WorldGenConfig.ApplyMinimalEvilPreset();

        Assert.True(WorldGenConfig.UseCustom);
        Assert.Equal(4, WorldGenConfig.CountChanges());
        Assert.Equal(2, WorldGenConfig.CountWorldChanges());
        Assert.Equal(0, WorldGenConfig.CountOreChanges());
    }

    [Fact]
    public void DebugPreset_UsesSafeMinSizeAndBoostsAllOres()
    {
        WorldGenConfig.Reset();

        WorldGenConfig.ApplyDebugWorldGenPreset();

        Assert.True(WorldGenConfig.UseCustom);
        Assert.Equal(WorldGenConfig.MinWidth, WorldGenConfig.WorldWidth);
        Assert.Equal(WorldGenConfig.MinHeight, WorldGenConfig.WorldHeight);
        Assert.Equal(20f, WorldGenConfig.OreVeinSizeMul);
        Assert.Equal(20f, WorldGenConfig.OreFrequencyMul);
        Assert.Equal(2, WorldGenConfig.CountChanges());
        Assert.Equal(2, WorldGenConfig.CountOreChanges());
        Assert.All(WorldGenConfig.OreMul.Values, value => Assert.Equal(1f, value));
        Assert.Equal(OreCatalog.WikiOreCount, WorldGenConfig.OreMul.Keys.Intersect(OreCatalog.OrderedKeys).Count());
    }
}
