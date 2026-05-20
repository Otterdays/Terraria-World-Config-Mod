using Terraria.ModLoader;

namespace WorldConfigMod;

public class WorldConfigMod : Mod
{
    public override void Load()
    {
        Logger.Info($"WorldConfigMod v{Version} Load() running");
    }

    public override void PostSetupContent()
    {
        Logger.Info($"WorldConfigMod v{Version} PostSetupContent done — toast should fire next frame");
    }
}
