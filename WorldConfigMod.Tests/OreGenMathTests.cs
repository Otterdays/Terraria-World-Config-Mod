using WorldConfigMod.Core;
using Xunit;

namespace WorldConfigMod.Tests;

public class OreGenMathTests
{
    [Fact]
    public void ComputeVeinCount_ScalesWithMultipliers()
    {
        long area = 4200L * 1200L;
        int baseline = OreGenMath.ComputeVeinCount(area, 9e-5f, 1f, 1f);
        int doubled = OreGenMath.ComputeVeinCount(area, 9e-5f, 2f, 1f);
        int oreDoubled = OreGenMath.ComputeVeinCount(area, 9e-5f, 1f, 2f);

        Assert.True(baseline > 0);
        Assert.InRange(doubled, baseline * 2 - 1, baseline * 2 + 1);
        Assert.InRange(oreDoubled, baseline * 2 - 1, baseline * 2 + 1);
    }

    [Theory]
    [InlineData(0, 1f, 1f, 0)]
    [InlineData(1000, 0f, 1f, 0)]
    [InlineData(1000, 1f, 0f, 0)]
    [InlineData(1000, 1f, -1f, 0)]
    public void ComputeVeinCount_InvalidInputsReturnZero(long area, float freq, float oreMul, int expected)
    {
        Assert.Equal(expected, OreGenMath.ComputeVeinCount(area, freq, 1f, oreMul));
    }

    [Fact]
    public void ComputeVeinSize_EnforcesMinimum()
    {
        var (strength, steps) = OreGenMath.ComputeVeinSize(4, 5, 0.1f, minSize: 2);
        Assert.Equal(2, strength);
        Assert.Equal(2, steps);
    }

    [Fact]
    public void ComputeVeinSize_ScalesUp()
    {
        var (strength, steps) = OreGenMath.ComputeVeinSize(4, 5, 3f);
        Assert.Equal(12, strength);
        Assert.Equal(15, steps);
    }

    [Theory]
    [InlineData(12, 1f, 0)]
    [InlineData(12, 2f, 12)]
    [InlineData(12, 3f, 24)]
    [InlineData(0, 5f, 0)]
    public void ComputeSupplementalCount_OnlyAddsAboveVanilla(int vanilla, float totalMul, int expected)
    {
        Assert.Equal(expected, OreGenMath.ComputeSupplementalCount(vanilla, totalMul));
    }
}
