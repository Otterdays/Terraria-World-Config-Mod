<p align="center">
  <img src="assets/banner.svg" alt="World Config — Terraria tModLoader mod banner" width="100%" />
</p>

<h1 align="center">World Config</h1>

<p align="center">
  <strong>Resize worlds. Tune every ore vein. Shape caves, dungeons, and loot — before you hit Create.</strong>
</p>

<p align="center">
  <a href="#install">Install</a> ·
  <a href="#features">Features</a> ·
  <a href="#usage">Usage</a> ·
  <a href="#settings">Settings</a> ·
  <a href="#tests">Tests</a> ·
  <a href="index.html">Full docs site</a> ·
  <a href="DOCS/SUMMARY.md">Developer docs</a>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Terraria-1.4.4-5a73b4?style=flat-square" alt="Terraria 1.4.4" />
  <img src="https://img.shields.io/badge/tModLoader-1.4.4%2B-f0a23a?style=flat-square" alt="tModLoader 1.4.4+" />
  <img src="https://img.shields.io/badge/.NET-net8.0-512bd4?style=flat-square" alt=".NET 8" />
  <img src="https://img.shields.io/badge/tests-23%20passing-7ad36a?style=flat-square" alt="23 tests passing" />
  <img src="https://img.shields.io/badge/version-0.1-9cb4ff?style=flat-square" alt="v0.1" />
</p>

---

## What is this?

**World Config** is a [tModLoader](https://tmodloader.net/) mod that adds a **World Config** button on Terraria’s **Create New World** screen. Open it, dial in size and generation rules, toggle **Use Custom Generation**, then create the world — vanilla passes stay in place; the mod **replaces or supplements** the hooks that matter (ores, dimensions, caves, gems, chests, and more).

No world edit tools. No post-gen cheats. Everything applies at generation time.

---

## Features

| Area | What you get |
|------|----------------|
| **World size** | Width / height sliders + **5 presets** (Small → XXL, up to 16800×4800) |
| **Ore control** | Global vein size × and frequency ×, plus **19 per-ore** multipliers ([wiki catalog](https://terraria.wiki.gg/wiki/Ores)) |
| **World shape** | Cave depth ×, dungeon side (left / random / right) |
| **World features** | Gems, life crystals, chests, floating islands, marble/granite density × |
| **V2 UI** *(default)* | Sidebar tabs, live summary strip, diff dots, vanilla % badges, ore name filter |
| **V1 UI** | Classic two-column panel — swap anytime via **Try New UI** / **Legacy Panel** |
| **HUD** | Green **● ON** button + status line when custom gen is active; load toast on New World |
| **Persistence** | Settings saved to `WorldConfigMod_settings.txt` in your tModLoader save folder |
| **30+ controls** | Features tab: hives, traps, lakes, pyramids, spread toggles, preset bundles |

When **Use Custom Generation** is **OFF**, Terraria behaves exactly as stock.

**Full documentation:** open [`index.html`](index.html) locally or enable [GitHub Pages](DOCS/WEBSITE.md) on this repo for the hosted site (Quick Start, every slider, ore catalog, troubleshooting).

---

## Install

### Quick path — `build.bat` (Windows)

1. Install **tModLoader 1.4.4+** on Steam and launch it **once** (creates `ModSources` + `tModLoader.targets`).
2. **Close** tModLoader completely.
3. Run **`build.bat`** in this repo (or `cmd /c build.bat` from PowerShell).
4. In tModLoader: **Workshop → Mods** → enable **WorldConfigMod** → **Reload**.

Output: `%USERPROFILE%\Documents\My Games\Terraria\tModLoader\Mods\WorldConfigMod.tmod`

| Variable | Default | Purpose |
|----------|---------|---------|
| `TML_DIR` | Steam `...\common\tModLoader` | Install folder (`tModLoader.dll`) |
| `TML_DATA_DIR` | `Documents\My Games\Terraria\tModLoader` | ModSources / Mods root |
| `MOD_NAME` | `WorldConfigMod` | Assembly + folder name |
| `BUILD_CONFIG` | `Release` | `Release` or `Debug` |

**Exit codes:** `0` ok · `1` install not found · `2` game still running · `3` robocopy failed · `4` build failed · `5` `.tmod` missing

### Manual / cross-platform

Copy the project to `ModSources\WorldConfigMod\` (name must match `AssemblyName`), then **Develop Mods → Build + Reload**, or:

```bat
dotnet build "%USERPROFILE%\Documents\My Games\Terraria\tModLoader\ModSources\WorldConfigMod\WorldConfigMod.csproj" -c Release
```

Unit tests (no game required): `test.bat` or `dotnet test WorldConfigMod.Tests`

---

## Usage

1. Main menu → **Single Player → New** — set difficulty, evil, seed as usual.
2. Click **World Config** at the bottom of the screen.
3. Turn **Use Custom Generation** **ON**, tweak tabs (World / Shape / Features / Ores / Presets), or hit **DEBUG: Tiny + 20× Ore** for a fast test world.
4. **Apply & Back**, then vanilla **Create**.

> Settings persist between game sessions (`WorldConfigMod_settings.txt`). Re-open the panel before each **new world** to confirm values for that run.

**UI tips (V2):** sidebar shows `●N` when a category differs from vanilla; the summary strip updates live; use the ore filter on the Ores tab; switch to V1 anytime from the header.

---

## Settings

| Setting | Range | Default | Effect |
|---------|-------|---------|--------|
| Use Custom Generation | toggle | OFF | Master switch — OFF = vanilla only |
| World width | 4200 – 16800 | 4200 | `Main.maxTilesX` at `clearWorld` |
| World height | 1200 – 4800 | 1200 | `Main.maxTilesY` at `clearWorld` |
| Size presets | S / M / L / XL / XXL | — | 4200×1200 … 16800×4800 |
| Vein size | ×0.25 – ×25 | ×1 | OreRunner strength + steps |
| Global frequency | ×0.1 – ×25 | ×1 | World-wide vein attempt count |
| Per-ore multiplier | ×0 – ×5 | ×1 | Per-type frequency; ×0 disables one ore |
| Cave depth | ×0.5 – ×2 | ×1 | Scales surface / rock layer after Terrain |
| Dungeon side | Left / Random / Right | Random | `GenVars.dungeonSide` |
| Gems / hearts / chests / islands / marble-granite | ×0 – ×5 | ×1 | Supplemental scatter passes |
| Debug preset | footer button | — | Min safe size (4200×1200), vein ×20, freq ×20, custom ON |

**Not slider-controlled:** [Obsidian](https://terraria.wiki.gg/wiki/Obsidian) (lava+water), [Luminite](https://terraria.wiki.gg/wiki/Luminite) (boss drop) — listed for reference only.

### Ore generation coverage

| Phase | Ores | Mechanism |
|-------|------|-----------|
| Pre-Hardmode | Copper → Platinum, evil, rare meteorite | Replaces **Shinies** pass |
| Underworld | Hellstone | Pass after **Underworld** |
| Hardmode altar | Cobalt → Titanium | `SmashAltar` supplement |
| Hardmode natural | Chlorophyte | `ModifyHardmodeTasks` |
| Meteor events | Meteorite | `dropMeteor` supplement |

---

## Project layout

```
Core/                    Ore catalog + math (unit-tested, no Terraria refs)
Common/
  WorldGenConfig.cs      Session settings + presets + diff helpers
  Systems/               WorldSize, OreGen, FeatureGen, UI inject, menu draw, toasts
  Ore/                   Scatter specs + runner
UI/
  WorldConfigUIStateV2   Sidebar panel (default)
  WorldConfigUIState     Legacy two-column panel
  Elements/              Sliders, scroll, compact rows, text input
WorldConfigMod.Tests/    23 xUnit tests
DOCS/                    Architecture, modding guide, roadmap (dev-only)
build.bat / test.bat     Build .tmod / run tests (not shipped in .tmod)
index.html + app.js      GitHub Pages docs site (not shipped in .tmod)
assets/banner.svg        Social preview + site hero (1200×630)
```

**For contributors & agents:** [`AGENTS.md`](AGENTS.md) · [`DOCS/MODDING_GUIDE.md`](DOCS/MODDING_GUIDE.md) · [`DOCS/ARCHITECTURE.md`](DOCS/ARCHITECTURE.md) · [`DOCS/EXPANSIONS.md`](DOCS/EXPANSIONS.md)

---

## Troubleshooting

| Symptom | Fix |
|---------|-----|
| `build.bat` — tModLoader not found | Point `TML_DIR` at Steam `...\common\tModLoader` (uses `tModLoader.dll`, not `.exe`) |
| `build.bat` — game still running | Close tModLoader (tray too) |
| No **World Config** button | Enable mod + Reload; check for UI conflicts on New World |
| World gen crash on tiny sizes | Minimum safe size is **4200×1200** (vanilla Small); smaller worlds break vanilla passes |
| Ores look vanilla | Turn **Use Custom Generation ON** *before* Create |
| Button / toast in top-left on 4K | Update mod — menu HUD uses framebuffer viewport, not layout size |
| Cursor behind button | Update `MenuDrawSystem` — cursor redraw after overlay |
| Scroll wheel dead on ore list | Rebuild; input via `UIInjectSystem` + `Main.MenuUI` |

---

## Caveats

- **Experimental sizes** above vanilla Large can stress dungeon/jungle/temple placement — sweet spot often 4200–12000 wide.
- **Single-player tested** — multiplayer config sync not implemented.
- **Windows-first build script** — other OS: copy to ModSources + `dotnet build` / `dotnet test`.

---

## Tests

```bat
test.bat
```

23 tests — ore catalog (21 wiki types), vein math, config keys/defaults. Gameplay is manual in tModLoader.

---

## License & credits

Terraria and tModLoader are respective trademarks of Re-Logic and the tModLoader team. This mod is community tooling — see `build.txt` for mod metadata.

**Roadmap:** [`DOCS/EXPANSIONS.md`](DOCS/EXPANSIONS.md) — biomes, persistence, multiplayer sync, and more.
