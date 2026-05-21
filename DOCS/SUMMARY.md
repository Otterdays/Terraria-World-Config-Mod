<!-- PRESERVATION RULE: Never delete or replace content. Append or annotate only. -->

# World Config Mod — Project Summary

**Status:** Active · **Terraria 1.4.4.9** · **tModLoader 2026.3.3.0** · net8.0 · unit tests (Terraria vanilla specs + config presets) · V2 sidebar · settings persistence  
**Last updated:** 2026-05-20 (doc sync: 59 tests, Test.gui.bat across guides + site)

## Quick links

| Doc | Purpose |
|-----|---------|
| [index.html](../index.html) | **Public docs site** (GitHub Pages) — quick start + full reference |
| [README.md](../README.md) | User-facing install, usage, troubleshooting |
| [DOCS/WEBSITE.md](WEBSITE.md) | Pages deploy + site maintenance |
| [DOCS/VERSIONS.md](VERSIONS.md) | Pinned Terraria / tModLoader versions |
| [AGENTS.md](../AGENTS.md) | **AI agent instructions** — read before coding |
| [MODDING_GUIDE.md](MODDING_GUIDE.md) | **Beginner guide** — edit, build, recipes |
| [EXPANSIONS.md](EXPANSIONS.md) | **Expansion roadmap** — future world-gen hooks |
| [ARCHITECTURE.md](ARCHITECTURE.md) | Systems, hooks, data flow |
| [STYLE_GUIDE.md](STYLE_GUIDE.md) | C# / tML conventions |
| [SCRATCHPAD.md](SCRATCHPAD.md) | Active tasks and recent actions |
| [CHANGELOG.md](CHANGELOG.md) | Version history |

## What this mod does

Custom **World Config** on the New World screen. Two UI panels (swap via in-panel buttons):

- **V2 sidebar panel** *(default)* — compact sidebar nav (World / Shape / Features / Ores / Presets / Info), 22px dense rows, live summary strip, ore search filter, diff dots + vanilla % badges
- **V1 legacy panel** — two-column layout (world size left, scrollable ores right)

When **Use Custom Generation** is ON, controls:

- **World size** — width/height sliders + 5 presets (Small → XXL)
- **World shape** — cave depth ×, dungeon side (left/random/right)
- **World features** — gems ×, life crystals ×, chests ×, floating islands ×, marble/granite ×
- **Ore generation** — global vein size × + frequency ×, plus 19 per-ore sliders
- **Visual indicator** — button turns green + status badge when custom is active

## Build & test

```bat
test.bat      # 59 unit tests (Core/, WorldGenConfig, Terraria 1.4.4.9 specs, no game)
Test.gui.bat  # visible full-suite showcase: discovery + detailed pass output + pause
build.bat     # .tmod (close tModLoader first)
```

Requires tModLoader on Steam, launched once for `tModLoader.targets`. Detects install via registry + `tModLoader.dll`.

## Known gaps

- ~~Settings in-memory only (no persistence).~~ **Resolved 2026-05-20** — persisted to `<SavePath>/WorldConfigMod_settings.txt`.
- Gems, chests, caves, biomes — see [EXPANSIONS.md](EXPANSIONS.md).
- Custom dimensions below vanilla Small (4200×1200) cause world-gen crashes in vanilla `AddGenPasses` (hardcoded range inversions).
- Multiplayer config sync not implemented.

## [AMENDED 2026-05-20]: Test-suite audit + GUI runner

| Finding | Action |
|---------|--------|
| `WorldGenConfig` pure config behavior had no unit coverage | Added `WorldGenConfigTests` for reset, size presets, diff counts, bundle presets, debug preset |
| Test project linked only `Core/**` | Linked pure `Common/WorldGenConfig.cs` only; left Terraria-dependent systems out |
| No visible “show full suite” runner | Added `Test.gui.bat` with test discovery + detailed console run + pause |
| Build mirror docs said test scripts were excluded, but script only excluded `build.bat` | `build.bat` now excludes `test.bat` and `Test.gui.bat` from ModSources |

Verification: `test.bat` and `Test.gui.bat` both pass **59/59** tests.

## [AMENDED 2026-05-20]: Doc drift — test count + runners

Synced stale **23** / **51** test references to **59** in `ARCHITECTURE.md`, `MODDING_GUIDE.md`, `VERSIONS.md`, `WEBSITE.md`, `index.html`, `test.bat` comment. `STYLE_GUIDE.md` lists `Test.gui.bat` as ModSources exclusion.

## [AMENDED 2026-05-20]: 10 more features — wave 2 + bundles (shipped)

All gated on `WorldGenConfig.UseCustom`. Build: 109,789 bytes.

| Feature | Where | Hook / approach |
|---------|-------|-----------------|
| Hives × | Features tab | Insert after `Hives`/`Jungle` → `TileRunner(TileID.Hive)` in jungle band |
| Mushroom Patches × | Features tab | Insert after `Mushroom Patches` → `TileRunner(TileID.MushroomGrass)` |
| Pyramids tri-state | Shape tab | Disable pass at -1, supplement sand mound at +1 |
| Traps × | Features tab | `PlaceTile(TileID.Traps)` style 0-5 in cavern |
| Herbs × | Features tab | `PlaceTile(TileID.MatureHerbs)` style 0-6 |
| Lakes × | Features tab | Carve bowl + `Main.tile.LiquidAmount=255 LiquidType=Water` |
| Shrines × | Features tab | Stone alcove (vanilla shrine carve is private) |
| Altar Patch × | Features tab (Ore Meta) | Factor into existing `OnSmashAltar` formula |
| Meteor Chance × | Features tab (Ore Meta) | Factor into existing `OnDropMeteor` formula |
| **Preset bundles** | Presets tab | `ApplyResourceRichPreset` / `ApplyCaveLabyrinthPreset` / `ApplyMinimalEvilPreset` |

## [AMENDED 2026-05-20]: 8 new gen features + persistence (shipped)

All gated on `WorldGenConfig.UseCustom`. With Custom OFF the mod is fully inert.

| Feature | Where | Hook |
|---------|-------|------|
| Pots × | Features tab | Insert after `Pots`/`Pyramids` → `WorldGen.PlacePot` |
| Hellforges × | Features tab | Insert after `Hellforge`/`Underworld` → `PlaceTile(TileID.Hellforge)` |
| Shadow Orbs × | Features tab | Insert after `Altars` → `PlaceTile(TileID.ShadowOrbs, style=crimson?1:0)` |
| Living Trees × | Features tab | Insert after `Living Trees` → `WorldGen.GrowLivingTree` (cap +10) |
| Spider Caves × | Features tab | Insert after `Spider Caves` → cobweb cluster scatter |
| Jungle Side | Shape tab | `PreWorldGen` → `GenVars.jungleOriginX` 25 %/75 % |
| Disable Evil Spread | Shape tab | `ModifyHardmodeTasks` → disable Hardmode Good/Evil |
| Disable Hallow Spread | Shape tab | `ModifyHardmodeTasks` → disable Hardmode Good Remix |
| **Settings persistence** | All | `Common/ConfigPersistence.cs` → `<SavePath>/WorldConfigMod_settings.txt`; load in `PostSetupContent`, save in `Unload` + `CloseConfigMenu` |

Build: 103,088 bytes (clean, 0 warnings).

## [AMENDED 2026-05-20]: Overlay button redesign (shipped)

| Change | Detail |
|--------|--------|
| Button size | 300×54 → 340×62 px |
| Background | Solid fill → 3-band vertical gradient; blue (OFF) / green (ON); brightens on hover |
| Left accent strip | 5px full-height strip; green ON / red-brown OFF; glow on hover |
| Border | 2px all sides, color-coded (green/blue), hover-brightens |
| Cog icon | Pixel-art 18×18 8-spoke star via `DrawCogIcon` (MagicPixel rects); left of title |
| Title | "World Config" 0.95f, right of cog |
| Subtitle | State-aware: call-to-action (OFF) or change count (ON) |
| Status pill | 70×26 right-edge pill via `DrawStatusPill`: colored dot + ON/OFF text |

## [AMENDED 2026-05-20]: V2 visual polish pass 2 (shipped)

| Change | Detail |
|--------|--------|
| Slider bar max width | Capped at 420px — was unbounded, stretched full panel width on wide screens |
| Row height | 22→26px for breathing room between rows |
| Label widths | 100–115px per-callsite; "Floating Islands" / "Life Crystals" no longer clip |
| Info tab expansion | 12 sections covering mechanics, sizes, ore gen, stacking, limits, controls |

## [AMENDED 2026-05-20]: V2 visual polish (shipped)

| Change | Detail |
|--------|--------|
| Section heading scale | 0.85 bold → 0.70 non-bold in pill panel; prevents Terraria font blowout |
| Info tab rewrite | Beginner-friendly: What it does, tab guide, slider legend, controls, heads-up |
| Ore phases 2-col | 21 ores in 2-column grid; non-world-gen ores dimmed + asterisked |

## [AMENDED 2026-05-20]: V2 sidebar panel (shipped)

| File | Role |
|------|------|
| `UI/WorldConfigUIStateV2.cs` | New panel: sidebar nav + compact content + live summary |
| `UI/Elements/UICompactSliderRow.cs` | 22px dense slider with diff dot + vanilla % badge |
| `UI/Elements/UITextInput.cs` | Click-to-focus text input (ore filter) |
| `Common/WorldGenConfig.cs` | `UseV2Panel` flag, default constants, diff count helpers |
| `Common/Systems/UIInjectSystem.cs` | Holds both V1 + V2 states; `ReopenConfigMenu()` for mid-session swap |

## [AMENDED 2026-05-20]: Menu HUD + cursor (shipped)

| Piece | Draw path |
|-------|-----------|
| Load toast + overlay button | `MenuDrawSystem` → `DrawMenu` after `orig`; `UiDrawSpace` framebuffer + `Matrix.Identity` |
| Cursor on top of button | `RedrawMenuCursor()` after HUD — `DrawCursor` inside `Begin(UIScaleMatrix)` |
| In-world toasts | `ToastSystem` / `ModifyInterfaceLayers` (layout space; skipped when `gameMenu`) |
| Config panel | `Main.MenuUI.SetState(WorldConfigUIState)`; input in `UpdateUI` + `PostUpdateInput` |

Do **not** use `Main.screenWidth` for raw menu `SpriteBatch` rects (layout space). User-verified on 2560×1494.

## [AMENDED 2026-05-20]: Docs audit

| Finding | Action |
|---------|--------|
| README described V1 only; debug preset still said 1750×600 | README rewritten for GitHub (banner, V2, FeatureGen, 4200 min) |
| ARCHITECTURE referenced `menuMode 31337` and 1750 debug size | Appended current MenuUI + V2 + FeatureGen amendment |
| MODDING_GUIDE min width 1750 | Annotated — safe floor is 4200×1200 (`MinWidth`/`MinHeight`) |
| AGENTS layout missing V2 / FeatureGen | Updated repo map + hook table |
| `banner.svg` footer | Added CUSTOM ON badge, V2/feature footer line, v0.1 tag |

## [AMENDED 2026-05-19]: Docs + UI + scroll refresh

Added `AGENTS.md`. Updated modding guide, architecture, UI layout fix, scroll/input fix. 23 unit tests. Full ore catalog.
