using System;

namespace WorldConfigMod.Core;

public static class OreGenMath
{
    public static int ComputeVeinCount(long area, float baseFrequency, float frequencyMul, float oreMul)
    {
        if (area <= 0 || baseFrequency <= 0f || frequencyMul <= 0f || oreMul <= 0f)
            return 0;

        double raw = area * baseFrequency * frequencyMul * oreMul;
        if (raw <= 0d)
            return 0;

        return (int)Math.Floor(raw);
    }

    // WorldGen.OreRunner cost is O(strength² · steps).
    // At ×25 mul: strength=100, steps=125 → ~3.9 M tile ops per vein × 11 k veins = hang.
    // Caps keep each vein fast while still producing visibly large veins at high multipliers.
    private const int MaxStrength = 18;  // ~3× vanilla max (6)
    private const int MaxSteps    = 24;  // ~3× vanilla max (8)

    public static (int strength, int steps) ComputeVeinSize(
        int baseStrength,
        int baseSteps,
        float veinSizeMul,
        int minSize = 2)
    {
        int strength = Math.Clamp((int)(baseStrength * veinSizeMul), minSize, MaxStrength);
        int steps    = Math.Clamp((int)(baseSteps    * veinSizeMul), minSize, MaxSteps);
        return (strength, steps);
    }

    public static int ComputeSupplementalCount(int vanillaCount, float totalMul)
    {
        if (vanillaCount <= 0 || totalMul <= 1f)
            return 0;

        return (int)Math.Floor(vanillaCount * (totalMul - 1f));
    }
}
