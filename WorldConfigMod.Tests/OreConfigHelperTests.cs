using System.Collections.Generic;
using WorldConfigMod.Core;
using Xunit;

namespace WorldConfigMod.Tests;

public class OreConfigHelperTests
{
    [Fact]
    public void CreateDefaultOreMul_HasAll21Keys()
    {
        var dict = OreConfigHelper.CreateDefaultOreMul();
        Assert.True(OreConfigHelper.HasAllWikiKeys(dict));
        Assert.All(dict.Values, v => Assert.Equal(1f, v));
    }

    [Fact]
    public void EnsureAllKeys_AddsMissingWikiKeys()
    {
        var dict = new Dictionary<string, float> { ["Copper"] = 2f };
        OreConfigHelper.EnsureAllKeys(dict);

        Assert.True(OreConfigHelper.AllKeysPresent(dict));
        Assert.Equal(2f, dict["Copper"]);
        Assert.Equal(1f, dict["Hellstone"]);
    }

    [Fact]
    public void ResetAll_SetsEveryKey()
    {
        var dict = OreConfigHelper.CreateDefaultOreMul();
        dict["Gold"] = 5f;
        OreConfigHelper.ResetAll(dict, 0.5f);

        foreach (var key in OreCatalog.OrderedKeys)
            Assert.Equal(0.5f, dict[key]);
    }
}
