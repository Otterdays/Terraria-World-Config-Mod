using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Terraria;
using Terraria.ModLoader;

namespace WorldConfigMod.Common;

// Plain-text key=value persistence for WorldGenConfig.
// Survives between sessions. Saved on config-menu close + mod unload.
public static class ConfigPersistence
{
    private static string FilePath =>
        Path.Combine(Main.SavePath ?? Path.GetTempPath(), "WorldConfigMod_settings.txt");

    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public static void Save()
    {
        try
        {
            var lines = new List<string>
            {
                $"UseCustom={WorldGenConfig.UseCustom}",
                $"UseV2Panel={WorldGenConfig.UseV2Panel}",
                $"WorldWidth={WorldGenConfig.WorldWidth}",
                $"WorldHeight={WorldGenConfig.WorldHeight}",
                $"OreVeinSizeMul={F(WorldGenConfig.OreVeinSizeMul)}",
                $"OreFrequencyMul={F(WorldGenConfig.OreFrequencyMul)}",
                $"CaveDepthMul={F(WorldGenConfig.CaveDepthMul)}",
                $"DungeonSide={WorldGenConfig.DungeonSide}",
                $"JungleSide={WorldGenConfig.JungleSide}",
                $"NoEvilSpread={WorldGenConfig.NoEvilSpread}",
                $"NoHallowSpread={WorldGenConfig.NoHallowSpread}",
                $"GemsMul={F(WorldGenConfig.GemsMul)}",
                $"LifeCrystalsMul={F(WorldGenConfig.LifeCrystalsMul)}",
                $"ChestsMul={F(WorldGenConfig.ChestsMul)}",
                $"FloatingIslandsMul={F(WorldGenConfig.FloatingIslandsMul)}",
                $"MarbleGraniteMul={F(WorldGenConfig.MarbleGraniteMul)}",
                $"PotsMul={F(WorldGenConfig.PotsMul)}",
                $"HellforgesMul={F(WorldGenConfig.HellforgesMul)}",
                $"ShadowOrbsMul={F(WorldGenConfig.ShadowOrbsMul)}",
                $"LivingTreesMul={F(WorldGenConfig.LivingTreesMul)}",
                $"SpiderCavesMul={F(WorldGenConfig.SpiderCavesMul)}",
                $"HivesMul={F(WorldGenConfig.HivesMul)}",
                $"MushroomMul={F(WorldGenConfig.MushroomMul)}",
                $"TrapsMul={F(WorldGenConfig.TrapsMul)}",
                $"HerbsMul={F(WorldGenConfig.HerbsMul)}",
                $"LakesMul={F(WorldGenConfig.LakesMul)}",
                $"ShrinesMul={F(WorldGenConfig.ShrinesMul)}",
                $"AltarPatchMul={F(WorldGenConfig.AltarPatchMul)}",
                $"MeteorChanceMul={F(WorldGenConfig.MeteorChanceMul)}",
                $"PyramidsMode={WorldGenConfig.PyramidsMode}",
            };
            foreach (var kv in WorldGenConfig.OreMul)
                lines.Add($"Ore.{kv.Key}={F(kv.Value)}");

            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            File.WriteAllLines(FilePath, lines);
        }
        catch (Exception e)
        {
            ModContent.GetInstance<WorldConfigMod>()?.Logger?.Warn($"ConfigPersistence.Save failed: {e.Message}");
        }
    }

    public static void Load()
    {
        try
        {
            if (!File.Exists(FilePath))
                return;
            foreach (var raw in File.ReadAllLines(FilePath))
            {
                var line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;
                int eq = line.IndexOf('=');
                if (eq <= 0) continue;
                string key = line.Substring(0, eq).Trim();
                string val = line.Substring(eq + 1).Trim();
                Apply(key, val);
            }
        }
        catch (Exception e)
        {
            ModContent.GetInstance<WorldConfigMod>()?.Logger?.Warn($"ConfigPersistence.Load failed: {e.Message}");
        }
    }

    private static void Apply(string key, string val)
    {
        if (key.StartsWith("Ore.") && TryFloat(val, out var f))
        {
            string oreKey = key.Substring(4);
            if (WorldGenConfig.OreMul.ContainsKey(oreKey))
                WorldGenConfig.OreMul[oreKey] = f;
            return;
        }
        switch (key)
        {
            case "UseCustom":          if (bool.TryParse(val, out var b1)) WorldGenConfig.UseCustom = b1; break;
            case "UseV2Panel":         if (bool.TryParse(val, out var b2)) WorldGenConfig.UseV2Panel = b2; break;
            case "WorldWidth":         if (int.TryParse(val, NumberStyles.Integer, Inv, out var iw))  WorldGenConfig.WorldWidth = iw; break;
            case "WorldHeight":        if (int.TryParse(val, NumberStyles.Integer, Inv, out var ih))  WorldGenConfig.WorldHeight = ih; break;
            case "OreVeinSizeMul":     if (TryFloat(val, out var v1)) WorldGenConfig.OreVeinSizeMul = v1; break;
            case "OreFrequencyMul":    if (TryFloat(val, out var v2)) WorldGenConfig.OreFrequencyMul = v2; break;
            case "CaveDepthMul":       if (TryFloat(val, out var v3)) WorldGenConfig.CaveDepthMul = v3; break;
            case "DungeonSide":        if (int.TryParse(val, NumberStyles.Integer, Inv, out var ds))  WorldGenConfig.DungeonSide = ds; break;
            case "JungleSide":         if (int.TryParse(val, NumberStyles.Integer, Inv, out var js))  WorldGenConfig.JungleSide = js; break;
            case "NoEvilSpread":       if (bool.TryParse(val, out var b3)) WorldGenConfig.NoEvilSpread = b3; break;
            case "NoHallowSpread":     if (bool.TryParse(val, out var b4)) WorldGenConfig.NoHallowSpread = b4; break;
            case "GemsMul":            if (TryFloat(val, out var v4)) WorldGenConfig.GemsMul = v4; break;
            case "LifeCrystalsMul":    if (TryFloat(val, out var v5)) WorldGenConfig.LifeCrystalsMul = v5; break;
            case "ChestsMul":          if (TryFloat(val, out var v6)) WorldGenConfig.ChestsMul = v6; break;
            case "FloatingIslandsMul": if (TryFloat(val, out var v7)) WorldGenConfig.FloatingIslandsMul = v7; break;
            case "MarbleGraniteMul":   if (TryFloat(val, out var v8)) WorldGenConfig.MarbleGraniteMul = v8; break;
            case "PotsMul":            if (TryFloat(val, out var v9)) WorldGenConfig.PotsMul = v9; break;
            case "HellforgesMul":      if (TryFloat(val, out var v10)) WorldGenConfig.HellforgesMul = v10; break;
            case "ShadowOrbsMul":      if (TryFloat(val, out var v11)) WorldGenConfig.ShadowOrbsMul = v11; break;
            case "LivingTreesMul":     if (TryFloat(val, out var v12)) WorldGenConfig.LivingTreesMul = v12; break;
            case "SpiderCavesMul":     if (TryFloat(val, out var v13)) WorldGenConfig.SpiderCavesMul = v13; break;
            case "HivesMul":           if (TryFloat(val, out var v14)) WorldGenConfig.HivesMul = v14; break;
            case "MushroomMul":        if (TryFloat(val, out var v15)) WorldGenConfig.MushroomMul = v15; break;
            case "TrapsMul":           if (TryFloat(val, out var v16)) WorldGenConfig.TrapsMul = v16; break;
            case "HerbsMul":           if (TryFloat(val, out var v17)) WorldGenConfig.HerbsMul = v17; break;
            case "LakesMul":           if (TryFloat(val, out var v18)) WorldGenConfig.LakesMul = v18; break;
            case "ShrinesMul":         if (TryFloat(val, out var v19)) WorldGenConfig.ShrinesMul = v19; break;
            case "AltarPatchMul":      if (TryFloat(val, out var v20)) WorldGenConfig.AltarPatchMul = v20; break;
            case "MeteorChanceMul":    if (TryFloat(val, out var v21)) WorldGenConfig.MeteorChanceMul = v21; break;
            case "PyramidsMode":       if (int.TryParse(val, NumberStyles.Integer, Inv, out var pm)) WorldGenConfig.PyramidsMode = pm; break;
        }
    }

    private static string F(float v) => v.ToString("0.###", Inv);
    private static bool TryFloat(string s, out float v) =>
        float.TryParse(s, NumberStyles.Float, Inv, out v);
}
