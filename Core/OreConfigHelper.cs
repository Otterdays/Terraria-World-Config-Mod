using System.Collections.Generic;

namespace WorldConfigMod.Core;

public static class OreConfigHelper
{
    public static Dictionary<string, float> CreateDefaultOreMul()
    {
        var dict = new Dictionary<string, float>();
        foreach (var key in OreCatalog.OrderedKeys)
            dict[key] = 1f;
        return dict;
    }

    public static void EnsureAllKeys(Dictionary<string, float> oreMul, float defaultValue = 1f)
    {
        foreach (var key in OreCatalog.OrderedKeys)
        {
            if (!oreMul.ContainsKey(key))
                oreMul[key] = defaultValue;
        }
    }

    public static bool HasAllWikiKeys(Dictionary<string, float> oreMul) =>
        oreMul.Count >= OreCatalog.WikiOreCount &&
        AllKeysPresent(oreMul);

    public static bool AllKeysPresent(Dictionary<string, float> oreMul)
    {
        foreach (var key in OreCatalog.OrderedKeys)
        {
            if (!oreMul.ContainsKey(key))
                return false;
        }

        return true;
    }

    public static void ResetAll(Dictionary<string, float> oreMul, float value = 1f)
    {
        EnsureAllKeys(oreMul, value);
        foreach (var key in OreCatalog.OrderedKeys)
            oreMul[key] = value;
    }
}
