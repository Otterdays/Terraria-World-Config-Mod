using Terraria.ModLoader;
using WorldConfigMod.Common;

namespace WorldConfigMod;

public class WorldConfigMod : Mod
{
    public override void Load()
    {
        Logger.Info($"WorldConfigMod v{Version} Load() running");
    }

    public override void PostSetupContent()
    {
        ConfigPersistence.Load();
        Logger.Info($"WorldConfigMod v{Version} PostSetupContent done — settings loaded, toast should fire next frame");
    }

    public override void Unload()
    {
        ConfigPersistence.Save();
    }
}
