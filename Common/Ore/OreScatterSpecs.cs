using Terraria.ID;

namespace WorldConfigMod.Common.Ore;

// Scatter parameters per config key. Tile IDs from Terraria.ID.TileID.
public static class OreScatterSpecs
{
    public sealed class Spec
    {
        public string Key;
        public ushort TileId;
        public float BaseFrequency;
        public int BaseStrength;
        public int BaseSteps;
        public bool DeepBias;
        public bool UnderworldOnly;
        public bool JungleOnly;
    }

    public static readonly Spec[] PreHardmode =
    {
        new() { Key = "Copper",    TileId = TileID.Copper,    BaseFrequency = 9e-5f, BaseStrength = 4, BaseSteps = 5 },
        new() { Key = "Tin",       TileId = TileID.Tin,       BaseFrequency = 9e-5f, BaseStrength = 4, BaseSteps = 5 },
        new() { Key = "Iron",      TileId = TileID.Iron,      BaseFrequency = 8e-5f, BaseStrength = 4, BaseSteps = 5 },
        new() { Key = "Lead",      TileId = TileID.Lead,      BaseFrequency = 8e-5f, BaseStrength = 4, BaseSteps = 5 },
        new() { Key = "Silver",    TileId = TileID.Silver,    BaseFrequency = 6e-5f, BaseStrength = 4, BaseSteps = 6, DeepBias = true },
        new() { Key = "Tungsten",  TileId = TileID.Tungsten,  BaseFrequency = 6e-5f, BaseStrength = 4, BaseSteps = 6, DeepBias = true },
        new() { Key = "Gold",      TileId = TileID.Gold,      BaseFrequency = 5e-5f, BaseStrength = 5, BaseSteps = 6, DeepBias = true },
        new() { Key = "Platinum",  TileId = TileID.Platinum,  BaseFrequency = 5e-5f, BaseStrength = 5, BaseSteps = 6, DeepBias = true },
        new() { Key = "Demonite",  TileId = TileID.Demonite,  BaseFrequency = 2e-5f, BaseStrength = 4, BaseSteps = 5, DeepBias = true },
        new() { Key = "Crimtane",  TileId = TileID.Crimtane,  BaseFrequency = 2e-5f, BaseStrength = 4, BaseSteps = 5, DeepBias = true },
        // Rare surface/cavern meteorite pockets (true meteor biomes are event-driven).
        new() { Key = "Meteorite", TileId = TileID.Meteorite, BaseFrequency = 3e-6f, BaseStrength = 6, BaseSteps = 8, DeepBias = false },
    };

    public static readonly Spec Hellstone = new()
    {
        Key = "Hellstone",
        TileId = TileID.Hellstone,
        BaseFrequency = 4e-5f,
        BaseStrength = 5,
        BaseSteps = 6,
        UnderworldOnly = true,
    };

    public static readonly Spec Chlorophyte = new()
    {
        Key = "Chlorophyte",
        TileId = TileID.Chlorophyte,
        BaseFrequency = 2e-5f,
        BaseStrength = 5,
        BaseSteps = 7,
        JungleOnly = true,
        DeepBias = true,
    };

    public static readonly Spec[] HardmodeAltar =
    {
        new() { Key = "Cobalt",     TileId = TileID.Cobalt,     BaseFrequency = 1f, BaseStrength = 5, BaseSteps = 6 },
        new() { Key = "Palladium",  TileId = TileID.Palladium,  BaseFrequency = 1f, BaseStrength = 5, BaseSteps = 6 },
        new() { Key = "Mythril",    TileId = TileID.Mythril,    BaseFrequency = 1f, BaseStrength = 5, BaseSteps = 7 },
        new() { Key = "Orichalcum", TileId = TileID.Orichalcum, BaseFrequency = 1f, BaseStrength = 5, BaseSteps = 7 },
        new() { Key = "Adamantite", TileId = TileID.Adamantite, BaseFrequency = 1f, BaseStrength = 6, BaseSteps = 8 },
        new() { Key = "Titanium",   TileId = TileID.Titanium,   BaseFrequency = 1f, BaseStrength = 6, BaseSteps = 8 },
    };
}
