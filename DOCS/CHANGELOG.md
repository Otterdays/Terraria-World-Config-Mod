<!-- PRESERVATION RULE: Never delete or replace content. Append or annotate only. -->

# Changelog

All notable changes to this project are documented here ([Keep a Changelog](https://keepachangelog.com/) format).

## [Unreleased]

### Added (2026-05-20 — test-suite audit + GUI runner)

- **`WorldGenConfigTests`** — covers reset/default restoration, size preset mapping, unknown preset no-op, UI diff counters, preset bundles, and debug preset behavior.
- **`Test.gui.bat`** — Windows pause-on-finish runner that lists discovered tests, runs the full suite with detailed console output, then shows pass/fail state.
- **`WorldConfigMod.Tests.csproj`** — links pure `Common/WorldGenConfig.cs` alongside `Core/**` so config behavior stays under unit test without pulling Terraria-dependent systems.

### Changed (2026-05-20 — test tooling hygiene)

- **`build.bat`** — excludes `test.bat` and `Test.gui.bat` from the ModSources mirror so dev-only scripts do not ship into the `.tmod` source tree.
- Verified `test.bat` and `Test.gui.bat`: **59/59** tests passing.

### Changed (2026-05-20 — documentation sync)

- **`DOCS/ARCHITECTURE.md`**, **`MODDING_GUIDE.md`**, **`VERSIONS.md`**, **`WEBSITE.md`**, **`STYLE_GUIDE.md`** — **59** tests, `Test.gui.bat`, `WorldGenConfig` test link, build mirror exclusions.
- **`index.html`** — hero stats, Install tests card, Advanced repo map, Contribute hygiene.
- **`test.bat`** — comment points to `Test.gui.bat` for full discovery + detailed run.

### Added (2026-05-20 — Terraria 1.4.4.9 / tML 2026.3.3.0 + vanilla-mimic tests)

- **`Core/TerrariaVanillaSpecs.cs`** — pinned game/tML versions, world presets S–XXL, copper frequency, feature `2 + width/4200` scaling.
- **`TerrariaVanillaSpecsTests`**, **`TerrariaOreGenMathTests`** — vein counts at Small/Medium/Large, area linearity, cap behavior, supplemental formula.
- **`DOCS/VERSIONS.md`** — canonical version table.
- **`WorldGenConfig`** — min/max and `ApplyPresetSize` delegate to `TerrariaVanillaSpecs`.

### Changed (2026-05-20 — version pins)

- SBOM, AGENTS, README, site (`index.html`, `app.js`) — **Terraria 1.4.4.9**, **tModLoader 2026.3.3.0**.

### Added (2026-05-20 — GitHub Pages site + repo layout)

- **Public docs site** — `index.html`, `styles.css`, `app.js` at repo root; hash-routed SPA (Home, Quick Start, Features, Settings, Ores, Install, Advanced, Contribute).
- **`assets/banner.svg` v3** — expanded hero (moon, meteor, dungeon, pyramid, hive, spider web, 11 gems, save badge, feature pills); moved from repo root.
- **`DOCS/WEBSITE.md`** — Pages deploy + agent maintenance checklist.
- **`.github/workflows/github-pages.yml`** — deploys site bundle only (not full mod tree).
- **`build.bat`** — robocopy excludes `assets/`, `.github/`, web root files from ModSources mirror.
- **`AGENTS.md`** — GitHub Pages section, doc checklist, persistence note, updated layout map.

### Added (2026-05-20 — 10 more features: features wave 2 + ore meta + preset bundles)

All gated on `WorldGenConfig.UseCustom`. Build: 109,789 bytes.

- **Hives ×** — `SupplementHives`; jungle band scatter via `TileRunner(TileID.Hive)`. Capped at +12 extras.
- **Mushroom Patches ×** — `SupplementMushroom`; `TileRunner(TileID.MushroomGrass)` cavern scatter, capped +8.
- **Pyramids tri-state** (`PyramidsMode`: -1 disable / 0 vanilla / +1 boost) — disables pass at -1, inserts `SupplementPyramids` sand mound at +1.
- **Traps ×** — `SupplementTraps`; `WorldGen.PlaceTile(TileID.Traps)` with random style 0-5 in cavern.
- **Herbs ×** — `SupplementHerbs`; `PlaceTile(TileID.MatureHerbs)` random style 0-6 surface→cavern.
- **Lakes ×** — `SupplementLakes`; carves bowl + sets `Main.tile[x,y].LiquidAmount=255` + `LiquidType=Water`. Capped +8.
- **Shrines ×** — `SupplementShrines`; small stone alcove approximation (vanilla EnchantedSwordEntrance is private). Capped +6.
- **Altar Patch ×** — `WorldGenConfig.AltarPatchMul` factors into existing `OnSmashAltar` formula → scales vanilla HM ore vein per smash.
- **Meteor Chance ×** — `WorldGenConfig.MeteorChanceMul` factors into existing `OnDropMeteor` formula → biases meteor event payoff.
- **Preset bundles** (Presets tab) — `ApplyResourceRichPreset` / `ApplyCaveLabyrinthPreset` / `ApplyMinimalEvilPreset`; one-click multi-field apply with toast confirmation. Force `UseCustom = true`.

### Changed (2026-05-20 — wave 2 wiring)

- `WorldGenConfig` — 9 new fields + matching `Default*` constants; all included in `Reset()`, `CountChanges()`, `CountFeatureChanges()`, `CountWorldChanges()` (PyramidsMode in World tab count).
- `FeatureGenSystem.ModifyWorldGenTasks` — 7 new supplement inserts (Hives, Mushroom, Pyramids boost, Traps, Herbs, Lakes, Shrines); Pyramids disable when `PyramidsMode == -1`.
- `OreGenSystem.OnSmashAltar` — `combined * AltarPatchMul` in supplemental vein formula.
- `OreGenSystem.OnDropMeteor` — `OreFrequencyMul * MeteorChanceMul` in extra meteor formula.
- `UI/WorldConfigUIStateV2` — Features tab gets 7 new sliders + Ore Meta subsection (2 sliders); Shape tab gains Pyramids tri-state row; Presets tab gains 3 bundle buttons with description text.
- `ConfigPersistence` — 9 new keys persisted across sessions.

### Added (2026-05-20 — 8 new gen features + settings persistence)

- **Pots ×** — `SupplementPots` inserts after vanilla `Pots`/`Pyramids`; calls `WorldGen.PlacePot` with `TileID.Pots`. Density scaled per area.
- **Hellforges ×** — `SupplementHellforges` inserts after `Hellforge`/`Underworld`; `WorldGen.PlaceTile(TileID.Hellforge)` in underworld layer.
- **Shadow Orbs / Crimson Hearts ×** — `SupplementShadowOrbs` inserts after `Altars`/`Shadow Orbs`; `WorldGen.PlaceTile(TileID.ShadowOrbs, style: WorldGen.crimson ? 1 : 0)` chooses Heart vs Orb per world.
- **Living Trees ×** — `SupplementLivingTrees`; `WorldGen.GrowLivingTree` wrapped in try/catch, capped at +10 extras to avoid hangs.
- **Spider Caves ×** — `SupplementSpiderCaves`; scatters `TileID.Cobweb` clusters in cavern empty space (vanilla nest gen is private — stable approximation).
- **Jungle Side** — `GenVars.jungleOriginX` overridden in `PreWorldGen` (Left ≈ 25 % width, Right ≈ 75 %); UI mirrors Dungeon Side row.
- **No Evil Spread** — `ModifyHardmodeTasks` disables vanilla "Hardmode Good" + "Hardmode Evil" passes when set; stops HM corruption/crimson V-cuts.
- **No Hallow Spread** — disables "Hardmode Good Remix" pass.
- **Settings persistence** — `Common/ConfigPersistence.cs`. Plain `key=value` file at `<SavePath>/WorldConfigMod_settings.txt`. Loaded in `WorldConfigMod.PostSetupContent`, saved in `Mod.Unload` and on every `CloseConfigMenu()`. Includes all per-ore multipliers (`Ore.<key>=…`).
- **`WorldGenConfig`** — 10 new fields with defaults: `PotsMul`, `HellforgesMul`, `ShadowOrbsMul`, `LivingTreesMul`, `SpiderCavesMul`, `JungleSide`, `NoEvilSpread`, `NoHallowSpread` + matching `Default*` constants; included in `Reset()`, `CountChanges()`, `CountFeatureChanges()`, `CountWorldChanges()`.
- **V2 UI rows** — Features tab gets 5 new sliders (Pots / Hellforges / Shadow Orbs / Living Trees / Spider Caves). Shape tab gains Jungle Side button row + "Disable Evil Spread" / "Disable Hallow Spread" toggle rows.

All hooks remain no-ops when `WorldGenConfig.UseCustom == false`; vanilla world gen is untouched.

### Changed (2026-05-20 — overlay button redesign)

- **World Config overlay button** (`UIInjectSystem.DrawOverlayButton`) — complete visual redesign:
  - Size: 300×54 → **340×62** px
  - Background: solid fill → **3-band vertical gradient** (top/mid/bot) in blue (OFF) or green (ON), brightens on hover
  - **Inner highlight** (1px under top border) + **bottom shadow** (1px above bottom border) for depth
  - **5px left accent strip** — green when custom ON, red-brown when OFF; glows brighter on hover
  - **2px border** all sides — color-coded to match state (green/blue), brightens on hover
  - **Pixel-art cog icon** (`DrawCogIcon`) — 18×18, 8-spoke star from MagicPixel rectangles; drawn left of title
  - **Title** — "World Config" at `0.95f` scale, positioned to right of cog
  - **Subtitle** (state-aware) — when OFF: *"click to customize world generation"*; when ON: *"applies on next world create · N change(s)"*
  - **Status pill** (`DrawStatusPill`) — 70×26 pill on right edge; colored dot + "ON"/"OFF" text; green bg when ON, dark-red when OFF
  - **Custom status badge** below button (when ON) unchanged — still shows `W×H ore ×freq · ×veins`

### Changed

- **README.md** — GitHub layout with centered `banner.svg`, shields, V2 UI + tier-1 features, corrected 4200×1200 minimum and debug preset.
- **`banner.svg`** — full redesign: V2 UI mockup, world cross-section, starfield/nebula, faceted crystals, feature pills, dual slider card, vignette.
- **DOCS** — ARCHITECTURE / MODDING_GUIDE / AGENTS / SUMMARY synced after audit (removed stale `menuMode 31337` guidance in new amendments).
- **Section headings (V2 panel)** — scale 0.85→0.70 (no bold); now rendered in a 20px dark-blue pill panel with 6px spacer above; prevents Terraria font blowout in every tab.
- **`UICompactSliderRow`** — bar capped at 420px max width (was unbounded; stretched 900px+ on wide panel); row height 22→26px for breathing room; label width params exposed per-callsite.
- **Features/Ores label widths** — bumped to 100–115px so "Floating Islands", "Life Crystals", "Marble/Granite" no longer clip.
- **Info tab** — massively expanded; sections: What this mod does, The tabs, Reading the sliders, World size, Cave Depth, Dungeon Side, Ore generation (how it works + stacking), Features (each slider explained), Controls, Performance notes, Known limits, Ore Phases (2-col grid with phase abbreviation key).

### Added

- **V2 sidebar panel** (`WorldConfigUIStateV2.cs`) — compact redesign with sidebar nav (World / Shape / Features / Ores / Presets / Info tabs), 22px dense rows, live summary strip, ore search/filter, diff dot indicators, and vanilla % badges.
- **`UICompactSliderRow`** — 22px inline slider row; yellow diff dot when value ≠ vanilla default; optional vanilla % badge (green = above, red = below vanilla); colored fill when changed.
- **`UITextInput`** — click-to-focus single-line text input; used for ore filter in Ores tab.
- **`WorldGenConfig` diff helpers** — `CountChanges()`, `CountOreChanges()`, `CountWorldChanges()`, `CountFeatureChanges()`, `IsOreDefault(key)` for sidebar diff badges.
- **`WorldGenConfig.UseV2Panel`** — static flag (default `true`) to toggle between legacy two-column panel and new V2 sidebar panel; persists for session.
- **`WorldGenConfig` default constants** — `DefaultWorldWidth`, `DefaultWorldHeight`, `DefaultOreVeinSizeMul`, etc. for use in diff detection and reset logic.
- **V1/V2 swap buttons** — "Try New UI →" in legacy panel header; "← Legacy Panel" in V2 sidebar; both call `UIInjectSystem.ReopenConfigMenu()` for instant mid-session swap.
- **`UIInjectSystem.ReopenConfigMenu()`** — swaps active config panel state without losing return-menu context.
- **Live summary strip** in V2 — shows `W×H · Size · Nk tiles · est gen ~Xs · ore freq ×N · vein ×N · N changes` updating on every slider change.
- **Diff badges on sidebar nav** — `●N` next to World / Features / Ores category when any value in that tab differs from vanilla.
- **Ore filter in Ores tab** — text input to filter per-ore sliders by name; × clear button; live rebuild on type.
- **Compact preset button row** — 5 presets (Small/Med/Large/XL/XXL) in a single 26px row; reused in both World tab and Presets tab of V2 panel.

### Changed

- `UIInjectSystem` — holds both `WorldConfigUIState` (V1) and `WorldConfigUIStateV2`; activates both on `Load()`; picks active state via `WorldGenConfig.UseV2Panel`.
- EXPANSIONS.md checklist updated to reference V2 panel for new slider additions.

## [Previous — Tier-1 World Gen Expansions]

### Added

- **Tier-1 world-gen expansions** (`FeatureGenSystem.cs`):
  - **Cave Depth ×** (0.5–2.0) — scales `worldSurface` + `rockLayer` after the Terrain pass; all subsequent passes use the adjusted depths.
  - **Dungeon Side** — Left / Random / Right preset buttons; sets `GenVars.dungeonSide` in `PreWorldGen`.
  - **Gems ×** (0–5) — supplements gem-cave tile veins (amethyst, topaz, sapphire, emerald, ruby, diamond) in cavern zones.
  - **Life Crystals ×** (0–5) — supplements `WorldGen.AddLifeCrystal` placements.
  - **Chests ×** (0–5) — supplements `WorldGen.AddBuriedChest` placements.
  - **Floating Islands ×** (0–5) — supplements `WorldGen.FloatingIsland` calls above surface.
  - **Marble/Granite ×** (0–5) — supplements marble + granite patch density via `WorldGen.TileRunner`.
  - All hooks are no-ops when `UseCustom = false`.
- **Visual indicator for custom world size** — World Config button turns green with "● ON" label when `UseCustom` is active. Status badge below button shows `Custom: WxH   ore ×freq · ×veins` config.
- **Promotional SVG banner** — `banner.svg` (1200×630) with Terraria-themed dark navy background, gold title, ore gem clusters, and mock slider UI.
- **Full ore catalog** — all 21 [wiki ores](https://terraria.wiki.gg/wiki/Ores); 19 world-gen sliders + Obsidian/Luminite info rows.
- `Core/OreCatalog.cs`, `OreGenMath.cs`, `OreConfigHelper.cs` — testable catalog and vein math.
- `WorldConfigMod.Tests` — 23 xUnit tests (`test.bat` / `dotnet test`).
- Ore gen hooks: Hellstone (post-Underworld pass), Chlorophyte (`ModifyHardmodeTasks`), hardmode altar supplement (`On_WorldGen.SmashAltar`), meteor supplement (`On_WorldGen.dropMeteor`).
- Per-tier alternates in UI: Tin, Lead, Tungsten, Platinum, Palladium, Orichalcum, Titanium.
- `ToastSystem` + `ToastManager` — mod-load toast and in-game toast overlay layer.
- Overlay **World Config** button drawn on New World screen (replaces UI-tree injection).
- `DOCS/` — SUMMARY, SCRATCHPAD, SBOM, STYLE_GUIDE, ARCHITECTURE, CHANGELOG.
- Debug preset **Tiny map + 20× ore** — `ApplyDebugWorldGenPreset()` (4200×1200, vein ×20, freq ×20).
- Slider max increased to ×25 for vein size and global frequency.
- **`DOCS/EXPANSIONS.md`** — world-gen expansion roadmap (hooks, passes, tiers).
- **`AGENTS.md`** — AI agent instructions for this repo.

### Fixed

- **Cursor behind World Config button** — `Main.DrawCursor` redraw was called with no active `SpriteBatch`; now wraps in `Begin(...UIScaleMatrix) / End()` so cursor actually repaints on top.
- **World gen crash on tiny worlds** — vanilla `AddGenPasses` has hardcoded `UnifiedRandom.Next(min, max)` ranges that invert below 4200×1200. Raised mod's `MinWidth`/`MinHeight` to vanilla Small (4200×1200), the lowest safe size. Debug preset now uses this floor.
- **Menu UI placement** — toasts and New World overlay button used `Main.screenWidth` (layout space) with `Matrix.Identity`, cramming HUD into a top-left box on large displays. Now uses `UiDrawSpace` framebuffer (`GraphicsDevice.Viewport`) + `mouseX`/`mouseY` in `DrawMenu`; in-world toasts stay on layout coords in `ModifyInterfaceLayers`.
- Config panel scroll wheel and scrollbar drag — `Main.MenuUI.Update` in `UpdateUI` + `PostUpdateInput` scroll handling.
- Column title text overlapping first slider row — dedicated title bars + list offset.

### Changed

- **Config panel UI sizing** — header 54 → 72 px, title 1.05× → 1.35×; column titles 34 → 44 px, 0.92× → 1.15×; section labels 0.80× → 0.95×; footer 58 → 66 px; spacer/padding bumps for readability.
- Two-column UI layout polish (`ColumnTitleHeight`, header/footer spacing).
- `UIInjectSystem` — config menu input no longer relies on `UpdateUI` alone.

### Fixed

- `ToastManager.cs` — `MathHelper.Clamp` float casts; `Utils.DrawBorderString` for text.
- `build.bat` detects modern tModLoader installs (`tModLoader.dll` + bundled dotnet), reads Steam path from registry, and skips SDK-less `dotnet build` attempts.
- Compile errors on current tML: `PassLegacy` → custom `GenPass`, `Utils.DrawBorderString` for UI text, UIMouseEvent handler fix.
- Target framework bumped to `net8.0` for current tModLoader.
- `MenuDrawSystem` — `EnsureSpriteBatchClosed()` before `SpriteBatch.Begin`; fixes crash opening World Config panel.

### Changed

- `UIInjectSystem` — state-based overlay detection instead of appending into `UIWorldCreation`.
- README / architecture docs synced with current file layout and hooks.
- **`DOCS/MODDING_GUIDE.md`** — beginner modding walkthrough (build loop, recipes, gotchas).

## [0.1] — MVP

### Added

- World Config button on New World screen (menu mode 888).
- Custom UI for world size, presets, ore multipliers.
- `WorldSizeSystem` — `On_WorldGen.clearWorld` dimension override.
- `OreGenSystem` — replaces vanilla **Shinies** pass with scaled `OreRunner` scatter.
