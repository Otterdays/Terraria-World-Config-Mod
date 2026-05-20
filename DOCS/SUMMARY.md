<!-- PRESERVATION RULE: Never delete or replace content. Append or annotate only. -->

# World Config Mod — Project Summary

**Status:** Active · tModLoader 1.4.4+ / net8.0 · **23/23 tests pass** · menu HUD + cursor verified in-game · custom world indicator live  
**Last updated:** 2026-05-20

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

Custom **World Config** on the New World screen: two-column UI (world size left, scrollable ores right). When **Use Custom Generation** is ON, controls:

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

## [AMENDED 2026-05-20]: Menu HUD + cursor (shipped)

| Piece | Draw path |
|-------|-----------|
| Load toast + overlay button | `MenuDrawSystem` → `DrawMenu` after `orig`; `UiDrawSpace` framebuffer + `Matrix.Identity` |
| Cursor on top of button | `RedrawMenuCursor()` after HUD — `DrawCursor` inside `Begin(UIScaleMatrix)` |
| In-world toasts | `ToastSystem` / `ModifyInterfaceLayers` (layout space; skipped when `gameMenu`) |
| Config panel | `Main.MenuUI.SetState(WorldConfigUIState)`; input in `UpdateUI` + `PostUpdateInput` |

Do **not** use `Main.screenWidth` for raw menu `SpriteBatch` rects (layout space). User-verified on 2560×1494.

## [AMENDED 2026-05-19]: Docs + UI + scroll refresh

Added `AGENTS.md`. Updated modding guide, architecture, UI layout fix, scroll/input fix. 23 unit tests. Full ore catalog.
