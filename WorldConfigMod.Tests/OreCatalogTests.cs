using System.Linq;
using WorldConfigMod.Core;
using Xunit;

namespace WorldConfigMod.Tests;

public class OreCatalogTests
{
    [Fact]
    public void Catalog_HasAll21WikiOres()
    {
        Assert.Equal(21, OreCatalog.WikiOreCount);
        Assert.Equal(21, OreCatalog.All.Count);
    }

    [Fact]
    public void Catalog_KeysAreUnique()
    {
        var keys = OreCatalog.All.Select(e => e.Key).ToList();
        Assert.Equal(keys.Count, keys.Distinct().Count());
    }

    [Fact]
    public void Catalog_HasExpectedWorldGenAndNonGenSplit()
    {
        Assert.Equal(19, OreCatalog.WithMultipliers.Count());
        Assert.Equal(2, OreCatalog.WithoutMultipliers.Count());
        Assert.Contains(OreCatalog.All, e => e.Key == "Obsidian" && !e.HasWorldGenMultiplier);
        Assert.Contains(OreCatalog.All, e => e.Key == "Luminite" && !e.HasWorldGenMultiplier);
    }

    [Theory]
    [InlineData("Copper", OreGenPhase.PreHardmodeScatter)]
    [InlineData("Hellstone", OreGenPhase.Underworld)]
    [InlineData("Cobalt", OreGenPhase.HardmodeAltar)]
    [InlineData("Chlorophyte", OreGenPhase.HardmodeNatural)]
    [InlineData("Meteorite", OreGenPhase.MeteorEvent)]
    public void Catalog_PhasesMatchWiki(string key, OreGenPhase expected)
    {
        Assert.True(OreCatalog.TryGet(key, out var entry));
        Assert.Equal(expected, entry.Phase);
    }

    [Fact]
    public void Catalog_ContainsAllTierAlternates()
    {
        string[] alternates = { "Copper", "Tin", "Iron", "Lead", "Silver", "Tungsten", "Gold", "Platinum",
            "Cobalt", "Palladium", "Mythril", "Orichalcum", "Adamantite", "Titanium" };
        foreach (var key in alternates)
            Assert.True(OreCatalog.TryGet(key, out _), $"Missing {key}");
    }
}
