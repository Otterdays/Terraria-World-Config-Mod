# AGENTS.md — World Config Mod

Instructions for AI coding agents (Cursor, Claude Code, etc.) working in this repo.

## Project

**WorldConfigMod** — tModLoader 1.4.4+ / net8.0 mod. Custom world size + ore multipliers on the New World screen.

| Item | Value |
|------|--------|
| Target | tModLoader 1.4.4+ (Terraria 1.4.4), `net8.0` |
| Build | `build.bat` (close tModLoader first) |
| Tests | `test.bat` or `dotnet test WorldConfigMod.Tests` |
| Mod output | `%USERPROFILE%\Documents\My Games\Terraria\tModLoader\Mods\WorldConfigMod.tmod` |

## Read first (in order)

1. [`DOCS/SUMMARY.md`](DOCS/SUMMARY.md) — status + links  
2. [`DOCS/MODDING_GUIDE.md`](DOCS/MODDING_GUIDE.md) — edit/build recipes  
3. [`DOCS/ARCHITECTURE.md`](DOCS/ARCHITECTURE.md) — hooks and data flow  
4. [`DOCS/EXPANSIONS.md`](DOCS/EXPANSIONS.md) — planned world-gen features  
5. [`DOCS/STYLE_GUIDE.md`](DOCS/STYLE_GUIDE.md) — conventions  

`DOCS/` is **not** mirrored to ModSources by `build.bat` — dev-only.

## Repo layout (mental map)

```
Core/                          — testable logic (no Terraria refs)
  OreCatalog.cs                  — 21 wiki ores, phases, UI keys
  OreGenMath.cs, OreConfigHelper.cs

Common/
  WorldGenConfig.cs              — session settings (static fields)
  Ore/OreScatterSpecs.cs         — tile IDs + scatter params
  Ore/OreScatterRunner.cs        — OreRunner helpers
  Systems/
    WorldSizeSystem.cs           — On_WorldGen.clearWorld
    OreGenSystem.cs              — ore gen hooks
    UIInjectSystem.cs            — overlay button, Main.MenuUI config panel, input
    MenuDrawSystem.cs            — DrawMenu: framebuffer pixels + Matrix.Identity
    ToastSystem.cs

UI/
  WorldConfigUIState.cs          — two-column config panel
  Elements/                      — UISliderRow, UITextButton, UIScrollColumn

WorldConfigMod.Tests/            — xUnit; links Core/ sources; excluded from .tmod build
```

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
| Pre-HM ores | `OreGenSystem` → replace **Shinies** |
| Hellstone | insert pass after **Underworld** |
| Chlorophyte | `ModifyHardmodeTasks` append |
| HM altar ores | `On_WorldGen.SmashAltar` supplement |
| Meteorite events | `On_WorldGen.dropMeteor` supplement |

Ore keys come from `Core/OreCatalog.cs`. UI lists `OreCatalog.WithMultipliers` (19 ores). Obsidian + Luminite are info-only.

### Build constraints

- `WorldConfigMod.csproj` excludes `WorldConfigMod.Tests/**` from compile.  
- `build.bat` robocopy excludes `DOCS/`, `WorldConfigMod.Tests/`, `.git`, etc.  
- Never commit `.tmod` to repo; it lands in tModLoader `Mods/`.  
- Close tModLoader before `build.bat` (exit code 2 if running).

## Common tasks

| Task | Where to edit |
|------|----------------|
| Default sizes / presets | `Common/WorldGenConfig.cs` |
| New ore type | `Core/OreCatalog.cs` + `Common/Ore/OreScatterSpecs.cs` + `OreGenSystem.cs` + tests |
| New slider | `WorldGenConfig` field + `UI/WorldConfigUIState.cs` + system that consumes it |
| New world-gen feature | See `DOCS/EXPANSIONS.md`; prefer supplement passes over full pass replacement |
| UI layout / scroll | `UI/WorldConfigUIState.cs`, `UI/Elements/UIScrollColumn.cs`, `UIInjectSystem` (`UpdateUI`, `PostUpdateInput`) |
| Menu HUD / cursor | `MenuDrawSystem.cs`, `UiDrawSpace.cs`, `ToastManager.cs` |
| Fix compile on tML | `Utils.DrawBorderString`, `(float)` casts, `GenPass` not `PassLegacy` |

## Testing

```bat
test.bat          # 23 unit tests — Core catalog, math, config keys
build.bat         # full .tmod — requires tModLoader closed
```

Add tests in `WorldConfigMod.Tests/` for any new **Core/** logic. Gameplay/world-gen behavior is manual in tModLoader.

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

- Settings not persisted (`Preferences` / `SaveWorldData` not implemented).  
- Diagnostic toast on New World detect — remove before polish release.  
- Multiplayer sync of config not implemented.  
- Gems/caves/biomes — roadmap in `EXPANSIONS.md` only.

## Out of scope for agents unless requested

- Renaming the mod / assembly  
- Force push, git config changes  
- Adding dependencies without updating `DOCS/SBOM.md`  
- Linux `build.sh` (listed as future work)
