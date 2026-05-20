<!-- PRESERVATION RULE: Never delete or replace content. Append or annotate only. -->

# World Config Mod — Project Summary

**Status:** Active · tModLoader 1.4.4+ / net8.0 · **23/23 tests pass** · menu HUD + cursor verified in-game · custom world indicator live · V2 sidebar panel built  
**Last updated:** 2026-05-20 (docs audit + GitHub README refresh)

## Quick links

| Doc | Purpose |
|-----|---------|
| [README.md](../README.md) | User-facing install, usage, troubleshooting |
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
test.bat    # unit tests (Core/, no game)
build.bat   # .tmod (close tModLoader first)
```

Requires tModLoader on Steam, launched once for `tModLoader.targets`. Detects install via registry + `tModLoader.dll`.

## Known gaps

- Settings in-memory only (no persistence).
- Gems, chests, caves, biomes — see [EXPANSIONS.md](EXPANSIONS.md).
- Custom dimensions below vanilla Small (4200×1200) cause world-gen crashes in vanilla `AddGenPasses` (hardcoded range inversions).
- Multiplayer config sync not implemented.

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
