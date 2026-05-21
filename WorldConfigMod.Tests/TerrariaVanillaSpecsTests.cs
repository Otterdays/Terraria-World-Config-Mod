using WorldConfigMod.Core;
using Xunit;

namespace WorldConfigMod.Tests;

public class TerrariaVanillaSpecsTests
{
    [Fact]
    public void Targets_Terraria1449_And_TModLoader2026330()
    {
        Assert.Equal("1.4.4.9", TerrariaVanillaSpecs.TerrariaGameVersion);
        Assert.Equal("2026.3.3.0", TerrariaVanillaSpecs.TModLoaderRelease);
    }

    [Fact]
    public void WorldPresets_MatchTerrariaSmallMediumLarge()
    {
        Assert.Equal(5, TerrariaVanillaSpecs.WorldPresets.Length);
        Assert.Equal(3, System.Linq.Enumerable.Count(TerrariaVanillaSpecs.WorldPresets, p => p.IsVanillaUiSize));

        var small = TerrariaVanillaSpecs.WorldPresets[0];
        Assert.Equal("Small", small.Label);
        Assert.Equal(4200, small.Width);
        Assert.Equal(1200, small.Height);
        Assert.Equal(5_040_000L, small.TileArea);

        var medium = TerrariaVanillaSpecs.WorldPresets[1];
        Assert.Equal(6400, medium.Width);
        Assert.Equal(1800, medium.Height);

        var large = TerrariaVanillaSpecs.WorldPresets[2];
        Assert.Equal(8400, large.Width);
        Assert.Equal(2400, large.Height);
    }

    [Theory]
    [InlineData(1, 4200, 1200)]
    [InlineData(2, 6400, 1800)]
    [InlineData(3, 8400, 2400)]
    [InlineData(4, 12000, 3600)]
    [InlineData(5, 16800, 4800)]
    public void TryGetPreset_ReturnsModUiSizes(int id, int w, int h)
    {
        Assert.True(TerrariaVanillaSpecs.TryGetPreset(id, out var p));
        Assert.Equal(w, p.Width);
        Assert.Equal(h, p.Height);
    }

    [Theory]
    [InlineData(4200, 3)]
    [InlineData(6400, 3)]
    [InlineData(8400, 4)]
    [InlineData(16800, 6)]
    public void FeatureBaseCount_ScalesLikeVanillaPerWidth(int width, int expected)
    {
        Assert.Equal(expected, TerrariaVanillaSpecs.VanillaScaledFeatureBaseCount(width));
    }

    [Fact]
    public void MinSafeSize_IsVanillaSmall()
    {
        Assert.Equal(4200, TerrariaVanillaSpecs.MinSafeWidth);
        Assert.Equal(1200, TerrariaVanillaSpecs.MinSafeHeight);
    }

    [Fact]
    public void ModMax_MatchesXxlPreset()
    {
        Assert.True(TerrariaVanillaSpecs.TryGetPreset(5, out var xxl));
        Assert.Equal(TerrariaVanillaSpecs.ModMaxWidth, xxl.Width);
        Assert.Equal(TerrariaVanillaSpecs.ModMaxHeight, xxl.Height);
    }
}
