<!-- PRESERVATION RULE: Never delete or replace content. Append or annotate only. -->

# SCRATCHPAD

## Active tasks

- [x] Menu HUD coords — framebuffer `Viewport` + `Identity` in `DrawMenu` (verified 2026-05-20)
- [x] Cursor behind World Config button — `RedrawMenuCursor()` after overlay (verified 2026-05-20)
- [x] GitHub Pages site — `index.html` / `styles.css` / `app.js` + workflow + `DOCS/WEBSITE.md`
- [x] `assets/banner.svg` v3 — expanded hero; root `banner.svg` removed
- [x] `AGENTS.md` — Pages, layout, doc checklist, persistence
- [x] Terraria **1.4.4.9** / tML **2026.3.3.0** — `TerrariaVanillaSpecs`, **51/51** tests
- [ ] Enable GitHub Pages in repo Settings (manual, user)
- [ ] Verify config panel clicks / scroll on full config panel (manual)
- [ ] `build.bat` when tML closed (last run: tests OK, build blocked — game open)

## Last 5 actions (2026-05-20)

1. **Terraria 1.4.4.9 / tML 2026.3.3.0** — `TerrariaVanillaSpecs`, expanded tests, VERSIONS.md, SBOM, WorldGenConfig presets.
2. **Project audit + web** — SPA docs site, banner → `assets/`, build exclusions, README persistence fix.
2. **`banner.svg` v2** — V2 panel mock, world layers, nebula/starfield, 8 faceted gems, feature pills, vignette.
2. **Docs audit** — GitHub README + `banner.svg`; fixed stale 1750×600 / V1-only README; ARCHITECTURE + AGENTS + MODDING_GUIDE amendments; 23/23 tests.
2. Fixed cursor Z-order — `RedrawMenuCursor()` with closed batch + `UIScaleMatrix` for `DrawCursor`.
2. Menu HUD uses `GraphicsDevice.Viewport` (not `Main.screenWidth`) for toasts + overlay button.
3. Removed temp `DrawMenuDebug` line after user verification.
4. Docs sync: SUMMARY, CHANGELOG, AGENTS, README, ARCHITECTURE.
5. `test.bat` — 23/23 pass.

## [AMENDED 2026-05-20]: Cursor Z-order on New World overlay

- `On_Main.DrawMenu` runs mod HUD after `orig` (cursor already drawn) → cursor appeared behind button.
- Fix: `Main.DrawCursor` once after `spriteBatch.End()` (batch must be closed).

## [AMENDED 2026-05-20]: Docs refresh + debug removed

- User verified menu toast + `WCM fb=… lay=…` debug on 2560×1494 / ui≈1.66.
- Removed temporary `DrawMenuDebug` yellow line (verification done).
- Updated SUMMARY, CHANGELOG, AGENTS, README internals table.

## [AMENDED 2026-05-20]: Layout vs framebuffer — real window size

- `Main.screenWidth/Height` = Terraria **layout** space (UIScaleMatrix); `GraphicsDevice.Viewport` = real window pixels.
- Drawing layout coords with `Matrix.Identity` pins UI to top-left “small box” on large monitors.
- Menu toasts/button/debug: `Viewport` + `Identity` + `mouseX`/`mouseY`; debug shows `fb=` vs `lay=`.
- Removed overlay `UserInterface` on menu (layout draw path); entry button back to SpriteBatch framebuffer rects.

## [AMENDED 2026-05-20]: Top-left “minimized box” — double UIScaleMatrix

- `Begin(UIScaleMatrix)` then `UserInterface.Draw` scaled twice → UI crammed in top-left; toasts used logical coords on scaled batch → wrong placement.
- Menu: toasts/debug `Matrix.Identity` + `screenSpace: true`; overlay `UserInterface.Draw` alone (close batch first).

## [AMENDED 2026-05-20]: Menu invisible — restore DrawMenu draw path

- `ModifyInterfaceLayers` does **not** run when `Main.gameMenu` — toasts + overlay were never drawn.
- Restored `MenuDrawSystem` (`DrawMenu` + `UIScaleMatrix`); `ToastSystem` layer only when `!gameMenu`.
- Overlay still wiki `UIState`/`UserInterface`; draw via `UIInjectSystem.DrawMenuOverlay`; `MousePosition` set in `UpdateUI`.
- Small debug: yellow line bottom-left + `DBG: New World overlay active` toast on first New World visit.

## [AMENDED 2026-05-20]: Toast/button placement — wiki UI path

- Root cause: `MenuDrawSystem` drew with `SpriteBatch.Begin(UIScaleMatrix)` while `UiDrawSpace` used `M11` for size — coords collapsed (toast center, button mid-left).
- Fix per [Advanced guide to custom UI](https://github.com/tModLoader/tModLoader/wiki/Advanced-guide-to-custom-UI): `WorldConfigOverlayUIState` + dedicated `UserInterface`, `HAlign`/`VAlign`/`Main.MouseScreen`.
- Removed mistaken “layers-only” draw (see amendment above).
- [x] Full ore coverage audit vs wiki (21 ores, tests, gen hooks)
- [x] EXPANSIONS.md roadmap
- [x] UI layout overlap fix (column title bars)
- [x] Scroll / scrollbar input fix (UpdateConfigInterface)
- [x] Scroll regression fix — raw mouse wheel + MenuDrawSystem update path (2026-05-19)
- [x] AGENTS.md + doc refresh (2026-05-19)

## [AMENDED 2026-05-20]: Entry button missing — restore overlay

- `UIWorldCreation.Append` did not show (vanilla rebuild clears foreign children).
- Restored SpriteBatch overlay + `MenuOverlayCoords`; removed `WorldConfigEntryButton` inject.

## [AMENDED 2026-05-20]: Grok UI guide refinement

- Kept `[Autoload(Client)]`, `Main.MenuUI` config panel, centered `UITextButton` labels.
- Overlay stays SpriteBatch (injection failed); config panel uses real `UIState` per Grok.

## [AMENDED 2026-05-20]: Dual cursor + overlay shifted left

- Removed extra `Main.DrawCursor` in `MenuDrawSystem` (vanilla already draws cursor → ghost second cursor).
- Overlay/toasts use `MenuOverlayCoords` (UI logical space + `UIScaleMatrix` + `MouseScreen` hits).

## [AMENDED 2026-05-20]: Overlay button click only on bottom half

- Draw used `UIScaleMatrix`, hits used screen rect + `MouseScreen` — misaligned.
- Overlay/toasts now `Matrix.Identity`; hits use `mouseX`/`mouseY`; text centered via DrawBorderString anchors.

## [AMENDED 2026-05-20]: Scroll freeze + click offset

- Scroll: removed double `UIList.ScrollWheel` + `LockVanillaMouseScroll`; direct `ViewPosition` on scrollbar.
- Clicks: zero `UIPanel` padding on shell/header/footer; footer/preset buttons use `Top` not `VAlign`; `UpdateUI` sets `MenuUI.MousePosition`.

## [AMENDED 2026-05-20]: Black screen after opening config

- `menuMode 31337` + skipping `orig_DrawMenu` left game on blank menu — removed entirely.
- Config opens via `Main.MenuUI.SetState(WorldConfigUIState)` only; vanilla draws/updates UI.
- `MenuDrawSystem` always calls `orig`; mod adds overlay + toasts on top.

## [AMENDED 2026-05-20]: Overlay "World Config" button dead click

- Regression: `PostUpdateInput` cleared `_prevMouseLeft` every frame before `DrawMenu` → release never detected.
- Fix: `HandleOverlayInput()` only in `PostUpdateInput`; draw-only in `MenuDrawSystem`.

## [AMENDED 2026-05-20]: Dead menu clicks fix

- Root cause: config used a **standalone** `UserInterface` + `Update` in `DrawMenu` — tML #1930; clicks need `Main.MenuUI` + update-phase tick.
- `OpenConfigMenu` now `Main.MenuUI.SetState(_configState)`; input in `PostUpdateInput` + `UpdateUI`; draw-only in `MenuDrawSystem`.
- Removed diagnostic toast on New World screen detect.
- 3-frame `Recalculate` warmup after open.

## [AMENDED 2026-05-19]: Doc refresh + AGENTS.md

Updated MODDING_GUIDE, ARCHITECTURE, SUMMARY, README, CHANGELOG, STYLE_GUIDE. Added root `AGENTS.md` for AI agents.

## [AMENDED 2026-05-19]: Scroll regression after freeze fix

- Restored `UpdateConfigInterface` in `MenuDrawSystem` (without `gd.Clear` — that caused freeze).
- Scroll delta now from `Mouse.GetState()` + `PostUpdateInput`; `PlayerInput.ScrollWheelDelta` is 0 on main menu.
- `UIList.ManualSortMethod = _ => { }` — fixes scrambled ore section order in screenshot.

## [AMENDED 2026-05-19]: UI scroll + layout

- Fixed column header overlap (`ColumnTitleHeight`, dedicated title bars).
- Fixed dead scroll: `MenuDrawSystem` → `UpdateConfigInterface` → `UserInterface.Update`.
- Added `UIScrollColumn`, `ApplyScrollWheel`, `PlayerInput.LockVanillaMouseScroll`.

## [AMENDED 2026-05-19]: EXPANSIONS.md

Added `DOCS/EXPANSIONS.md` — hook tiers, vanilla passes, implementation order. Linked from README + SUMMARY.

## [AMENDED 2026-05-19]: Ore coverage + tests

- `Core/OreCatalog` — 21 wiki ores, 19 gen multipliers.
- `OreGenSystem` — Shinies, Hellstone, Chlorophyte, SmashAltar, dropMeteor hooks.
- `WorldConfigMod.Tests` — 23 passing unit tests.
- Build OK (~49 KB `.tmod`).

## [AMENDED 2026-05-19]:

Two-column near-fullscreen World Config UI (96%×92% screen). Left: world size. Right: scrollable ore sliders.

## Last 5 actions (2026-05-19, pm3)

1. Redesigned `WorldConfigUIState` — side-by-side columns, near-fullscreen panel.
2. Compact `UISliderRow` with configurable label width for narrow columns.
3. Debug button remains in footer; toggle moved to header bar.
4. Build OK (~38 KB `.tmod`).
5. README Use section updated for two-column layout.

## Last 5 actions (2026-05-19, pm2)

1. Added `DOCS/MODDING_GUIDE.md` — dummy-friendly modding walkthrough.
2. Updated README settings table (debug preset, ×25 slider maxes).
3. Linked modding guide from README Extending section + SUMMARY.
4. CHANGELOG + SUMMARY amended for debug preset and guide.
5. Debug preset vein size raised from ×4 to ×20.

## Last 5 actions (2026-05-19, pm)

1. Full doc audit — README, ARCHITECTURE, SUMMARY, STYLE_GUIDE, CHANGELOG updated.
2. Documented `ToastSystem` / `ToastManager` and overlay-button UI refactor.
3. Fixed `ToastManager` compile errors (`float` casts, `Utils.DrawBorderString`).
4. Corrected internals table (`GenPass` not `PassLegacy`; overlay not UITextPanel inject).
5. Build pipeline docs: `tModLoader.dll` + Steam registry detection.

## Last 5 actions (2026-05-19, am)

1. Ran `build.bat` — hit `\Steam\...\tModLoader.exe was unexpected at this time` (parens in `Program Files (x86)`).
2. Rewrote `build.bat` to use delayed expansion throughout parenthesized blocks.
3. Created `DOCS/SUMMARY`, `SCRATCHPAD`, `SBOM`, `STYLE_GUIDE`, `CHANGELOG`, `ARCHITECTURE`.
4. Aligned README caveats with code (Meteorite UI-only, Shinies-only hook).
5. Excluded `DOCS/` from robocopy mirror (dev docs stay in repo only).

## Blockers

- None — build + tests verified (`WorldConfigMod.tmod` ~51 KB, 23 tests pass).

## [AMENDED 2026-05-19]:

Modern tML (2025+) ships `tModLoader.dll` + bundled dotnet runtime, not `tModLoader.exe`. `build.bat` updated to detect via registry/Steam path. Code updated for net8.0 + current tML APIs (`GenPass`, `Utils.DrawBorderString`, UIMouseEvent).

## Out-of-Scope Observations

- Diagnostic toast `"World Config: detected New World screen"` in `UIInjectSystem` — remove before release polish.
- **Presets:** document new presets in `MODDING_GUIDE.md` when adding to `WorldGenConfig`.
- `build.sh` for Linux/Mac listed in README extending section — not implemented.
