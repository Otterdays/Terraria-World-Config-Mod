# AGENTS.md — World Config Mod

Instructions for AI coding agents (Cursor, Claude Code, etc.) working in this repo.

**Last agent run:** 2026-05-20 — Terraria **1.4.4.9** / tML **2026.3.3.0** pin, `TerrariaVanillaSpecs` + expanded tests. **Update this file every session** when layout, workflows, or agent rules change.

## Project

**WorldConfigMod** — tModLoader 1.4.4+ / net8.0 mod. Custom world size + ore multipliers on the New World screen.

| Item | Value |
|------|--------|
| Target | tModLoader **2026.3.3.0** (Terraria **1.4.4.9**), `net8.0` |
| Build | `build.bat` (close tModLoader first) |
| Tests | `test.bat` or `dotnet test WorldConfigMod.Tests` |
| Mod output | `%USERPROFILE%\Documents\My Games\Terraria\tModLoader\Mods\WorldConfigMod.tmod` |
| Public site | `index.html` + `styles.css` + `app.js` → GitHub Pages (see below) |
| Banner | `assets/banner.svg` (1200×630; not shipped in `.tmod`) |

## Read first (in order)

1. [`DOCS/SUMMARY.md`](DOCS/SUMMARY.md) — status + links  
2. [`DOCS/MODDING_GUIDE.md`](DOCS/MODDING_GUIDE.md) — edit/build recipes  
3. [`DOCS/ARCHITECTURE.md`](DOCS/ARCHITECTURE.md) — hooks and data flow  
4. [`DOCS/EXPANSIONS.md`](DOCS/EXPANSIONS.md) — planned world-gen features  
5. [`DOCS/STYLE_GUIDE.md`](DOCS/STYLE_GUIDE.md) — conventions  
6. [`DOCS/WEBSITE.md`](DOCS/WEBSITE.md) — GitHub Pages + user-facing site maintenance  
7. [`DOCS/VERSIONS.md`](DOCS/VERSIONS.md) — pinned Terraria / tML versions  

`DOCS/` is **not** mirrored to ModSources by `build.bat` — dev-only.

## Repo layout (mental map)

```
assets/                        — banner.svg, future static images (excluded from .tmod mirror)
index.html, styles.css, app.js — GitHub Pages SPA (excluded from .tmod mirror)
.github/workflows/             — github-pages.yml deploy bundle

Core/                          — testable logic (no Terraria refs)
  OreCatalog.cs                  — 21 wiki ores, phases, UI keys
  OreGenMath.cs, OreConfigHelper.cs
  TerrariaVanillaSpecs.cs        — Terraria 1.4.4.9 sizes, ore freqs, feature scaling (tests + WorldGenConfig)

Common/
  WorldGenConfig.cs              — session settings (static fields); UseV2Panel default true
  ConfigPersistence.cs           — WorldConfigMod_settings.txt load/save
  Ore/OreScatterSpecs.cs         — tile IDs + scatter params
  Ore/OreScatterRunner.cs        — OreRunner helpers
  Systems/
    WorldSizeSystem.cs           — On_WorldGen.clearWorld
    OreGenSystem.cs              — ore gen hooks
    FeatureGenSystem.cs          — caves, dungeon side, gems, chests, islands, marble/granite, …
    UIInjectSystem.cs            — overlay button, Main.MenuUI V1+V2 panel, input
    MenuDrawSystem.cs            — DrawMenu: framebuffer pixels + Matrix.Identity
    ToastSystem.cs

UI/
  WorldConfigUIStateV2.cs        — sidebar panel (default)
  WorldConfigUIState.cs          — legacy two-column panel
  Elements/                      — UISliderRow, UICompactSliderRow, UITextInput, UIScrollColumn, …

WorldConfigMod.Tests/            — xUnit; links Core/ sources; excluded from .tmod build
DOCS/                            — dev docs only
build.txt, description.txt       — tModLoader workshop metadata (keep at repo root)
```

## GitHub Pages (public site)

| File | Role |
|------|------|
| `index.html` | Shell + static sections; hash routes `#home`, `#start`, … |
| `styles.css` | Terraria-night palette (matches banner) |
| `app.js` | Router, feature/settings/ore tables — **sync when shipping UI/gen features** |
| `assets/banner.svg` | Hero image + README embed |
| `.nojekyll` | Allow static deploy without Jekyll |
| `.github/workflows/github-pages.yml` | Copies only site files to `_site/` |

**Enable:** Repo → Settings → Pages → Source: **GitHub Actions**.  
**After feature work:** Update `app.js` data (`FEATURE_GROUPS`, `SETTINGS`, `ORES`) + append `DOCS/CHANGELOG.md` + `DOCS/SUMMARY.md`.

## Agent doc checklist (every meaningful change)

1. **AGENTS.md** — this file if structure, exclusions, or workflows changed.  
2. **DOCS/SUMMARY.md** + **DOCS/CHANGELOG.md** — append only (preservation rule).  
3. **DOCS/SCRATCHPAD.md** — last actions + active tasks.  
4. **README.md** — user-facing install/usage if behavior changed.  
5. **Website** — `app.js` / `index.html` when player-visible features change.  
6. **DOCS/WEBSITE.md** — if Pages deploy process changes.  

Never delete content in `DOCS/*.md` — append or `[AMENDED date]:` only.

## Critical architecture rules

### Custom menu input (do not break)

The config panel uses **`Main.MenuUI.SetState(WorldConfigUIState)`** — do **not** change `Main.menuMode` or skip `DrawMenu` (causes black screen).

- **HUD draw space:** `UiDrawSpace.Framebuffer*` = `GraphicsDevice.Viewport` (real window); `Layout*` = `Main.screenWidth/Height` (Terraria UI layout, scaled by `UIScaleMatrix`).  
- **Menu overlay/toasts:** `DrawMenu` + `Matrix.Identity` + framebuffer coords; hits `Main.mouseX`/`mouseY`.  
- **Menu cursor:** `RedrawMenuCursor()` after HUD — `EnsureSpriteBatchClosed`, then `Begin(UIScaleMatrix)` + `DrawCursor` + `End` (vanilla cursor is drawn before our HUD).  
- **In-world toasts:** `ModifyInterfaceLayers` + layout coords (`useFramebuffer: false`).  
- **Config panel:** `Main.MenuUI` + `UIState` (wiki); do not mix layout `UserInterface.Draw` inside menu `SpriteBatch.Begin`.  
- Scroll wheel: `PlayerInput.ScrollWheelDelta` forwarded via `WorldConfigUIState.ApplyScrollWheel` + `UIScrollColumn`.

### World generation

Custom gen runs only when `WorldGenConfig.UseCustom == true` at world create time.

| Hook | File |
|------|------|
| Dimensions | `WorldSizeSystem` → `On_WorldGen.clearWorld` |
| Cave depth, dungeon, features | `FeatureGenSystem` → `PreWorldGen` + `ModifyWorldGenTasks` inserts |
| Pre-HM ores | `OreGenSystem` → replace **Shinies** |
| Hellstone | insert pass after **Underworld** |
| Chlorophyte | `ModifyHardmodeTasks` append |
| HM altar ores | `On_WorldGen.SmashAltar` supplement |
| Meteorite events | `On_WorldGen.dropMeteor` supplement |

Ore keys come from `Core/OreCatalog.cs`. UI lists `OreCatalog.WithMultipliers` (19 ores). Obsidian + Luminite are info-only.

### Settings persistence

`Common/ConfigPersistence.cs` → `<SavePath>/WorldConfigMod_settings.txt`. Load in `PostSetupContent`, save on `CloseConfigMenu` + `Unload`.

## Common tasks

| Task | Where to edit |
|------|----------------|
| Default sizes / presets | `Common/WorldGenConfig.cs` |
| New ore type | `Core/OreCatalog.cs` + `Common/Ore/OreScatterSpecs.cs` + `OreGenSystem.cs` + tests + `app.js` ORES |
| New slider | `WorldGenConfig` field + `UI/WorldConfigUIStateV2.cs` + consuming system + `app.js` |
| New world-gen feature | See `DOCS/EXPANSIONS.md`; prefer supplement passes over full pass replacement |
| UI layout / scroll | `UI/WorldConfigUIState.cs`, `UI/Elements/UIScrollColumn.cs`, `UIInjectSystem` |
| Menu HUD / cursor | `MenuDrawSystem.cs`, `UiDrawSpace.cs`, `ToastManager.cs` |
| Banner / site branding | `assets/banner.svg`, `styles.css` |
| GitHub Pages copy | `app.js` feature tables, `DOCS/WEBSITE.md` |
| Fix compile on tML | `Utils.DrawBorderString`, `(float)` casts, `GenPass` not `PassLegacy` |

## Testing

```bat
test.bat          # unit tests — Core catalog, Terraria vanilla specs, ore math, config keys
build.bat         # full .tmod — requires tModLoader closed
```

Add tests in `WorldConfigMod.Tests/` for any new **Core/** logic. Use `TerrariaVanillaSpecs` for world sizes / vanilla formulas — keep in sync with `OreScatterSpecs` and `FeatureGenSystem`. Gameplay is manual in tModLoader.

## Code standards (short)

- Minimal diffs; match existing naming and file placement.  
- One concern per `ModSystem` in `Common/Systems/`.  
- WHY comments only (hook rationale, vanilla assumptions).  
- `Nullable` disabled — do not enable without project-wide decision.  
- Do not edit `DOCS/` content docs unless the user asks; **do** update status docs (SCRATCHPAD, SUMMARY, CHANGELOG) when shipping meaningful changes.  
- Preserve rule on all `DOCS/*.md`: never delete — append or annotate.

## Git / commits

- Only commit when the user explicitly asks.  
- Message format: `feat(scope): …`, `fix(scope): …`, etc.

## Known gaps (do not “fix” unless asked)

- Diagnostic toast on New World detect — remove before polish release.  
- Multiplayer sync of config not implemented.  
- Some items in `EXPANSIONS.md` — roadmap only.

## Out of scope for agents unless requested

- Renaming the mod / assembly  
- Force push, git config changes  
- Adding dependencies without updating `DOCS/SBOM.md`  
- Linux `build.sh` (listed as future work)  
- Committing `.tmod` binaries
