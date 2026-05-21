using System.Linq;
using WorldConfigMod.Core;
using Xunit;

namespace WorldConfigMod.Tests;

// Ore vein math at Terraria 1.4.4.9 world sizes (area scales like vanilla world creation).
public class TerrariaOreGenMathTests
{
    [Fact]
    public void CopperVeinCount_SmallWorld_MatchesFrequencyFormula()
    {
        int count = TerrariaVanillaSpecs.ExpectedCopperVeinCountAt1x(4200, 1200);
        // 4200×1200 × 9e-5 = 453.6 → floor 453
        Assert.Equal(453, count);
    }

    [Theory]
    [MemberData(nameof(VanillaAndModWorldSizes))]
    public void CopperVeinCount_ScalesLinearlyWithWorldArea(WorldSizePreset preset)
    {
        int count = TerrariaVanillaSpecs.ExpectedCopperVeinCountAt1x(preset.Width, preset.Height);
        double expected = preset.TileArea * TerrariaVanillaSpecs.CopperTinBaseFrequency;
        Assert.InRange(count, (int)expected - 1, (int)expected);
        Assert.True(count > 0);
    }

    [Fact]
    public void CopperVeinCount_LargeIsRoughly4xSmall_ByArea()
    {
        int small = TerrariaVanillaSpecs.ExpectedCopperVeinCountAt1x(4200, 1200);
        int large = TerrariaVanillaSpecs.ExpectedCopperVeinCountAt1x(8400, 2400);
        Assert.InRange(large, small * 4 - 4, small * 4 + 4);
    }

    [Fact]
    public void VeinSize_AtVanillaCopperBase_MatchesOreRunnerDefaults()
    {
        var (strength, steps) = OreGenMath.ComputeVeinSize(
            TerrariaVanillaSpecs.CopperBaseStrength,
            TerrariaVanillaSpecs.CopperBaseSteps,
            1f);

        Assert.Equal(4, strength);
        Assert.Equal(5, steps);
    }

    [Theory]
    [InlineData(20f)]
    [InlineData(25f)]
    public void VeinSize_HighMultiplier_CapsLikeInGame(float mul)
    {
        var (strength, steps) = OreGenMath.ComputeVeinSize(
            TerrariaVanillaSpecs.CopperBaseStrength,
            TerrariaVanillaSpecs.CopperBaseSteps,
            mul);

        Assert.Equal(18, strength);
        Assert.Equal(24, steps);
    }

    [Theory]
    [InlineData(4200, 1200, 2f, 2f)]
    [InlineData(8400, 2400, 20f, 20f)]
    public void DebugPresetScale_VeinCountScalesWithFreqAndOreMul(
        int w, int h, float freqMul, float oreMul)
    {
        long area = TerrariaVanillaSpecs.TileArea(w, h);
        int baseline = OreGenMath.ComputeVeinCount(
            area, TerrariaVanillaSpecs.CopperTinBaseFrequency, 1f, 1f);
        int boosted = OreGenMath.ComputeVeinCount(
            area, TerrariaVanillaSpecs.CopperTinBaseFrequency, freqMul, oreMul);

        int expected = OreGenMath.ComputeVeinCount(
            area, TerrariaVanillaSpecs.CopperTinBaseFrequency, freqMul, oreMul);
        Assert.True(boosted > baseline);
        Assert.Equal(expected, boosted);
    }

    [Theory]
    [InlineData(3, 1f, 0)]
    [InlineData(6, 2f, 6)]
    [InlineData(12, 2.5f, 18)]
    public void SupplementalCount_MatchesVanillaExtraFormula(int vanilla, float mul, int expected)
    {
        Assert.Equal(expected, OreGenMath.ComputeSupplementalCount(vanilla, mul));
    }

    public static TheoryData<WorldSizePreset> VanillaAndModWorldSizes =>
        new(TerrariaVanillaSpecs.WorldPresets.ToArray());
}
