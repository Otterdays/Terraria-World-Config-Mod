# World Config — Terraria tModLoader Mod

MVP mod giving you full control over **world size** and **ore generation** from a
custom UI on the New World screen. Targets **tModLoader 1.4.4+ (Terraria 1.4.4)**.

Developer docs: [`AGENTS.md`](AGENTS.md) · [`DOCS/MODDING_GUIDE.md`](DOCS/MODDING_GUIDE.md) · [`DOCS/SUMMARY.md`](DOCS/SUMMARY.md) · [`DOCS/ARCHITECTURE.md`](DOCS/ARCHITECTURE.md) · [`DOCS/EXPANSIONS.md`](DOCS/EXPANSIONS.md)

---

## Table of contents
- [What it does](#what-it-does)
- [Build & install](#build--install)
- [Use](#use)
- [Settings reference](#settings-reference)
- [File layout](#file-layout)
- [How it works (internals)](#how-it-works-internals)
- [Troubleshooting](#troubleshooting)
- [Caveats (MVP scope)](#caveats-mvp-scope)
- [Extending](#extending)

---

## What it does

- Adds a **World Config** overlay button at the bottom of the vanilla "Create New World" screen.
- Shows a **toast notification** when the mod loads and when the New World screen is detected.
- Opens a **near-fullscreen two-column** config panel:
  - **Left:** world width/height, size presets, quick info
  - **Right:** global ore multipliers + **scrollable** list of all 19 world-gen ores
- **21 [wiki ores](https://terraria.wiki.gg/wiki/Ores)** catalogued; **Obsidian** and **Luminite** shown as not world-gen controlled
- When **Use Custom Generation = ON**, the next world you create:
  - Allocates with your chosen width/height (`clearWorld` intercept).
  - Replaces **Shinies** and supplements hellstone, chlorophyte, hardmode altar ores, and meteor events (see [ore coverage table](#ore-coverage-vs-terraria-wiki)).

---

## Build & install

### Option A — `build.bat` (recommended)

1. Install **tModLoader 1.4.4+** via Steam.
2. Launch tModLoader **once** so it generates the `ModSources/` folder and
   `tModLoader.targets` file under
   `Documents\My Games\Terraria\tModLoader\`.
3. **Close** tModLoader.
4. Double-click **`build.bat`** in this folder (or run `cmd /c build.bat` from PowerShell).
   It will:
   - Mirror the source into `...\ModSources\WorldConfigMod\` via `robocopy /MIR`
     (skipping `bin`, `obj`, `.git`, `.claude`, `DOCS`, `build.bat` itself).
   - Refuse to run if tModLoader is still running (avoids file locks).
   - Auto-detect tModLoader via Steam registry (`tModLoader.dll`, not a separate `.exe`).
   - Prefer `dotnet build -c Release` when an SDK + `tModLoader.targets` exist (~5–15s).
   - Fall back to bundled `dotnet tModLoader.dll -build` (no separate `.exe`; first run ~30–90s).
   - Verify and report the produced `...\tModLoader\Mods\WorldConfigMod.tmod`.
5. Launch tModLoader → **Workshop → Mods** → enable **WorldConfigMod** → Reload.

**Environment overrides** (set before running):

| Var            | Default                                                                | Purpose |
|----------------|------------------------------------------------------------------------|---------|
| `TML_DIR`      | `C:\Program Files (x86)\Steam\steamapps\common\tModLoader`             | tModLoader install dir |
| `TML_DATA_DIR` | `%USERPROFILE%\Documents\My Games\Terraria\tModLoader`                 | Save / ModSources root |
| `MOD_NAME`     | `WorldConfigMod`                                                       | Folder + assembly name |
| `BUILD_CONFIG` | `Release`                                                              | `Release` or `Debug`   |

Example (Steam on D:):
```bat
set TML_DIR=D:\SteamLibrary\steamapps\common\tModLoader
build.bat
```

The script auto-probes common Steam library drives (C/D/E/F) if `TML_DIR` is unset.

**Exit codes:** `0` ok · `1` tModLoader install not found · `2` tModLoader is running ·
`3` robocopy failed · `4` build failed · `5` build OK but `.tmod` missing.

### Option B — manual (in-game)

1. Copy this folder to
   `Documents\My Games\Terraria\tModLoader\ModSources\WorldConfigMod\`
   (folder name MUST match `AssemblyName` in the `.csproj`).
2. Launch tModLoader → **Workshop → Develop Mods → WorldConfigMod → Build + Reload**.
3. Enable it in **Mods**, restart.

### Option C — pure dotnet (after Option A first run)

After `build.bat` (or a manual copy) has populated ModSources at least once:
```bat
dotnet build "%USERPROFILE%\Documents\My Games\Terraria\tModLoader\ModSources\WorldConfigMod\WorldConfigMod.csproj" -c Release
```

---

## Use

1. Main menu → **Single Player → New** → fill in difficulty / evil / seed as usual.
2. Click **World Config** at the bottom of the screen — opens a **near-fullscreen two-column** panel:
   - **Left:** world width/height, size presets, status
   - **Right:** global ore multipliers + scrollable per-ore list
3. Toggle **Use Custom Generation = ON** (header), tweak sliders, or click **DEBUG: Tiny + 20× Ore** (footer), then **Apply & Back**.
4. Click vanilla **Create** to generate the world with your settings.

> Settings are kept in memory until the game closes. Re-open the panel before each
> new world to confirm them.

---

## Settings reference

| Setting | Range | Default | Effect |
|---|---|---|---|
| Use Custom Generation | toggle | OFF | Master switch. OFF = vanilla behavior, nothing is intercepted. |
| World Width  | 1750 – 16800 tiles | 4200 | Sets `Main.maxTilesX` before `clearWorld`. |
| World Height | 600 – 4800 tiles   | 1200 | Sets `Main.maxTilesY` before `clearWorld`. |
| Presets | Small / Med / Large / XL / XXL | — | 4200×1200 / 6400×1800 / 8400×2400 / 12000×3600 / 16800×4800 |
| Debug preset | **DEBUG: Tiny + 20x Ore** (footer button, always visible) | — | 1750×600, vein ×20, frequency ×20, custom gen ON |
| Vein Size | ×0.25 – ×25 | ×1 | Scales `OreRunner` strength + step count. |
| Global Frequency | ×0.1 – ×25 | ×1 | Scales the number of ore-vein attempts world-wide. |
| Per-Ore Multiplier | ×0 – ×5 | ×1 | Per-ore frequency on top of the global multiplier. ×0 disables a single ore. |

---

## File layout

```
build.txt                              — mod metadata
description.txt                        — workshop description
build.bat / test.bat                   — build .tmod / run unit tests (repo-only)
AGENTS.md                              — AI agent instructions
WorldConfigMod.csproj                  — tML build project (imports ..\tModLoader.targets)
WorldConfigMod.cs                      — Mod entry
README.md                              — user guide
Core/                                  — testable ore catalog + math (no Terraria refs)
WorldConfigMod.Tests/                  — xUnit tests (excluded from .tmod)
DOCS/                                  — developer docs (not mirrored to ModSources)
  MODDING_GUIDE.md, EXPANSIONS.md, ARCHITECTURE.md, …
Common/
  WorldGenConfig.cs                    — static settings + presets
  Ore/OreScatterSpecs.cs, OreScatterRunner.cs
  UI/ToastManager.cs
  Systems/
    WorldSizeSystem.cs, OreGenSystem.cs
    UIInjectSystem.cs, MenuDrawSystem.cs, ToastSystem.cs
UI/
  WorldConfigUIState.cs                — two-column config screen
  Elements/UISliderRow.cs, UITextButton.cs, UIScrollColumn.cs
Localization/en-US_Mods.WorldConfigMod.hjson
```

---

## How it works (internals)

| Concern | Hook / API | File |
|---|---|---|
| Detect New World screen | `ShouldDrawOverlay()` when `Main.MenuUI.CurrentState is UIWorldCreation` | [`UIInjectSystem.cs`](Common/Systems/UIInjectSystem.cs) |
| Draw overlay button | `On_Main.DrawMenu` → `DrawOverlayButton` (framebuffer coords, `Matrix.Identity`) | [`MenuDrawSystem.cs`](Common/Systems/MenuDrawSystem.cs), [`UiDrawSpace.cs`](Common/UI/UiDrawSpace.cs) |
| Open custom panel | `Main.MenuUI.SetState(WorldConfigUIState)`; restores prior state on close | [`UIInjectSystem.cs`](Common/Systems/UIInjectSystem.cs) |
| Config panel input | `UpdateUI` → `Main.MenuUI.Update`; scroll via `PostUpdateInput` + `ApplyScrollWheel` | [`UIInjectSystem.cs`](Common/Systems/UIInjectSystem.cs), [`WorldConfigUIState.cs`](UI/WorldConfigUIState.cs) |
| Toast notifications | Menu: `DrawMenu` + viewport pixels; in-world: `ModifyInterfaceLayers` + layout space | [`ToastSystem.cs`](Common/Systems/ToastSystem.cs), [`ToastManager.cs`](Common/UI/ToastManager.cs) |
| Override world dimensions | `On_WorldGen.clearWorld` detour: sets `Main.maxTilesX`, `maxTilesY`, `bottomWorld`, `rightWorld` *before* `orig()` | [`WorldSizeSystem.cs`](Common/Systems/WorldSizeSystem.cs) |
| Custom ore generation | `ModifyWorldGenTasks` (Shinies, Hellstone supplement), `ModifyHardmodeTasks` (Chlorophyte), `On_WorldGen.SmashAltar` + `dropMeteor` hooks | [`OreGenSystem.cs`](Common/Systems/OreGenSystem.cs), [`OreScatterSpecs.cs`](Common/Ore/OreScatterSpecs.cs) |
| Ore catalog (21 wiki types) | `Core/OreCatalog.cs` — 19 with multipliers, 2 N/A (Obsidian, Luminite) | [`Core/OreCatalog.cs`](Core/OreCatalog.cs) |
| Settings storage | Plain static fields + per-ore dictionary from `OreCatalog`; presets in `ApplyPresetSize` / `ApplyDebugWorldGenPreset` | [`WorldGenConfig.cs`](Common/WorldGenConfig.cs) |

---

## Troubleshooting

| Symptom | Likely cause / fix |
|---|---|
| `... was unexpected at this time` when running `build.bat` | Fixed in current script (paths with `(x86)`). Update `build.bat` from repo. |
| `build.bat` says **tModLoader install not found** | Modern tML uses `tModLoader.dll`, not `.exe`. Update `build.bat` from repo — it auto-detects via Steam registry. If still failing, set `TML_DIR` to your Steam `...\common\tModLoader` folder. |
| Build errors in `ToastManager` / UI (`DynamicSpriteFont`, `double`→`float`) | Pull latest source — use `Utils.DrawBorderString` and explicit `(float)` casts for tML net8.0. |
| `build.bat` says **tModLoader is running** | Close tModLoader fully (taskbar tray too) and retry. |
| Build falls back to `tModLoader -build` and takes 30–90s | Normal first time. tModLoader has to JIT the game to compile. Subsequent dotnet incremental builds will be ~5s. |
| `.tmod` not found after success | Inspect `%TML_DATA_DIR%\Logs\Logs.log` and `client.log`. |
| **World Config** button doesn't appear in the New World screen | Mod isn't enabled, or another mod replaced `UIWorldCreation`. Reload mods, check `Mods → Enabled`. |
| World generation crashes with custom width/height | Stay within recommended safe range (width 4200–12000, height 1200–3600). Some vanilla passes hardcode assumptions about the standard 3 sizes. |
| Crash opening World Config (`SpriteBatch.Begin` already called) | Update to latest `MenuDrawSystem.cs` — calls `EnsureSpriteBatchClosed` before drawing. Rebuild + reload. |
| All ores look the same as vanilla | Confirm **Use Custom Generation = ON** *before* clicking Create. The toggle is per-session in memory. |
| Scroll wheel / scrollbar doesn't work on ore list | Update to latest build — `UIInjectSystem.UpdateUI` + `PostUpdateInput` drive `Main.MenuUI`. Reload mod after rebuild. |
| Toast / button stuck in top-left corner on big monitor | Old builds used `Main.screenWidth` (layout) with `Matrix.Identity`. Update — menu HUD uses `GraphicsDevice.Viewport` via `UiDrawSpace`. |
| Cursor draws **behind** World Config button | Update `MenuDrawSystem` — calls `RedrawMenuCursor()` after overlay draw. Reload mod. |

---

## Caveats

- **Custom dimensions** outside the vanilla 3 presets are *experimental*; very small
  or very tall worlds can break vanilla generation steps (chasms, dungeon placement,
  jungle temple). Recommended safe range: width 4200–12000, height 1200–3600.
- **Settings are per-session in memory** — not persisted to disk yet. Toggle them
  every time before creating a world.
- **All 21 [wiki ores](https://terraria.wiki.gg/wiki/Ores)** are listed in the UI.
  **19** have world-gen multipliers; **Obsidian** (water+lava) and **Luminite**
  (Moon Lord drop) are documented as not world-gen controlled.
- Custom gen **supplements** vanilla passes (Shinies replacement + extra scatter).
  Hardmode altar ores get **bonus veins** after each smash when multipliers > 1×.
- **Evil-biome ores** (Crimtane / Demonite) multiplier applies to supplemental
  scatter — chasm-placed evil ore from vanilla is untouched.
- Single-player tested only; **multiplayer/server sync** of these settings is not
  implemented (config is per-client, applied during local world generation).
- **Windows-only build script** (`build.bat`). Linux/Mac: `dotnet build` on the
  `.csproj` under `ModSources/`, or run unit tests via `test.bat` / `dotnet test`.

### Ore coverage vs [Terraria wiki](https://terraria.wiki.gg/wiki/Ores)

| Phase | Ores | Hook |
|---|---|---|
| Pre-Hardmode scatter | Copper, Tin, Iron, Lead, Silver, Tungsten, Gold, Platinum, Demonite, Crimtane, Meteorite (rare) | Replaces **Shinies** pass |
| Underworld | Hellstone | Pass after **Underworld** |
| Hardmode altar | Cobalt, Palladium, Mythril, Orichalcum, Adamantite, Titanium | `On_WorldGen.SmashAltar` supplement |
| Hardmode natural | Chlorophyte | `ModifyHardmodeTasks` pass |
| Meteor events | Meteorite | `On_WorldGen.dropMeteor` supplement |
| Not world-gen | Obsidian, Luminite | Info rows only (no slider) |

---

## Tests

Pure logic (catalog, vein math, config keys) lives in `Core/` and is tested without tModLoader:

```bat
test.bat
```

Or: `dotnet test WorldConfigMod.Tests\WorldConfigMod.Tests.csproj`

23 unit tests — catalog completeness (21 wiki ores), multiplier math, config defaults.

---

## Extending

See **[`DOCS/MODDING_GUIDE.md`](DOCS/MODDING_GUIDE.md)** for step-by-step recipes (new sliders, presets, ores, debug tweaks).

See **[`DOCS/EXPANSIONS.md`](DOCS/EXPANSIONS.md)** for the full world-gen expansion roadmap.

See **[`AGENTS.md`](AGENTS.md)** if you use AI coding agents in this repo.

Ideas not yet implemented:
- **Biome size controls**: hook `WorldGen.jungleOriginX`, dungeon side, evil-biome
  width, etc., in a new system that sets values before `WorldGen.GenerateWorld`.
- **Persist settings**: wrap `WorldGenConfig` fields with tML `Preferences`
  save/load triggered on `Refresh()` / `Apply`.
- **Per-world settings**: store a JSON blob via `ModSystem.SaveWorldData` /
  `LoadWorldData` so each world remembers the multipliers used to create it.
- **Linux/Mac build script**: port `build.bat` to `build.sh` (rsync + dotnet build).
