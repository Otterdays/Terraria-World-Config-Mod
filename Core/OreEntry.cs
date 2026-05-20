namespace WorldConfigMod.Core;

public sealed class OreEntry
{
    public OreEntry(
        string key,
        string displayName,
        OreGenPhase phase,
        bool hasWorldGenMultiplier,
        string notes = null)
    {
        Key = key;
        DisplayName = displayName;
        Phase = phase;
        HasWorldGenMultiplier = hasWorldGenMultiplier;
        Notes = notes ?? string.Empty;
    }

    public string Key { get; }
    public string DisplayName { get; }
    public OreGenPhase Phase { get; }
    public bool HasWorldGenMultiplier { get; }
    public string Notes { get; }
}
