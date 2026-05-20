using System.Collections.Generic;
using System.Linq;

namespace WorldConfigMod.Core;

// Canonical list of all 21 ore types from https://terraria.wiki.gg/wiki/Ores
public static class OreCatalog
{
    public const int WikiOreCount = 21;

    private static readonly OreEntry[] AllEntries =
    {
        new("Copper", "Copper", OreGenPhase.PreHardmodeScatter, true),
        new("Tin", "Tin", OreGenPhase.PreHardmodeScatter, true),
        new("Iron", "Iron", OreGenPhase.PreHardmodeScatter, true),
        new("Lead", "Lead", OreGenPhase.PreHardmodeScatter, true),
        new("Silver", "Silver", OreGenPhase.PreHardmodeScatter, true),
        new("Tungsten", "Tungsten", OreGenPhase.PreHardmodeScatter, true),
        new("Gold", "Gold", OreGenPhase.PreHardmodeScatter, true),
        new("Platinum", "Platinum", OreGenPhase.PreHardmodeScatter, true),
        new("Meteorite", "Meteorite", OreGenPhase.MeteorEvent, true,
            "Meteor biomes (post-gen events); mod adds supplemental scatter + scales meteor drops."),
        new("Demonite", "Demonite", OreGenPhase.PreHardmodeScatter, true),
        new("Crimtane", "Crimtane", OreGenPhase.PreHardmodeScatter, true),
        new("Obsidian", "Obsidian", OreGenPhase.NotWorldGen, false,
            "Forms from water + lava contact; not vein ore generation."),
        new("Hellstone", "Hellstone", OreGenPhase.Underworld, true),
        new("Cobalt", "Cobalt", OreGenPhase.HardmodeAltar, true),
        new("Palladium", "Palladium", OreGenPhase.HardmodeAltar, true),
        new("Mythril", "Mythril", OreGenPhase.HardmodeAltar, true),
        new("Orichalcum", "Orichalcum", OreGenPhase.HardmodeAltar, true),
        new("Adamantite", "Adamantite", OreGenPhase.HardmodeAltar, true),
        new("Titanium", "Titanium", OreGenPhase.HardmodeAltar, true),
        new("Chlorophyte", "Chlorophyte", OreGenPhase.HardmodeNatural, true),
        new("Luminite", "Luminite", OreGenPhase.NotWorldGen, false,
            "Moon Lord drop only; not placed during world generation."),
    };

    public static IReadOnlyList<OreEntry> All => AllEntries;

    public static IEnumerable<OreEntry> WithMultipliers =>
        AllEntries.Where(e => e.HasWorldGenMultiplier);

    public static IEnumerable<OreEntry> WithoutMultipliers =>
        AllEntries.Where(e => !e.HasWorldGenMultiplier);

    public static IEnumerable<string> OrderedKeys =>
        AllEntries.Select(e => e.Key);

    public static bool TryGet(string key, out OreEntry entry)
    {
        foreach (var e in AllEntries)
        {
            if (e.Key == key)
            {
                entry = e;
                return true;
            }
        }

        entry = null;
        return false;
    }

    public static IEnumerable<OreEntry> ByPhase(OreGenPhase phase) =>
        AllEntries.Where(e => e.Phase == phase);
}
