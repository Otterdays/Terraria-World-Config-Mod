<!-- PRESERVATION RULE: Never delete or replace content. Append or annotate only. -->

# STYLE_GUIDE — WorldConfigMod

## [AMENDED 2026-05-20]: Current menu + test guidance

- **Config panel:** `Main.MenuUI.SetState(...)`; do **not** use custom `Main.menuMode` values.
- **Menu HUD:** `DrawMenu` overlay uses framebuffer coords + `Matrix.Identity`; cursor redraw happens after HUD.
- **Tests:** `WorldConfigMod.Tests` links `Core/**` and pure `Common/WorldGenConfig.cs`; do not link Terraria-dependent `Common/Systems/**`.
- **Showcase runner:** `Test.gui.bat` lists discovered tests, runs detailed console output, then pauses for review.

## C# / tModLoader

- **Namespaces:** `WorldConfigMod`, `WorldConfigMod.Core`, `WorldConfigMod.Common`, `WorldConfigMod.Common.Systems`, `WorldConfigMod.Common.UI`, `WorldConfigMod.Common.Ore`, `WorldConfigMod.UI`, `WorldConfigMod.UI.Elements`
- **Core/:** No Terraria references — safe for unit tests; linked by `WorldConfigMod.Tests`
- **Mod systems:** One concern per `ModSystem` file under `Common/Systems/`
- **Config:** Static fields on `WorldGenConfig`; ore keys from `OreCatalog` via `OreConfigHelper`
- **UI:** tML `UIState` + custom elements; overlay via `SpriteBatch`; config via `Main.MenuUI.SetState` (not custom `menuMode`)
- **Menu input:** Config panel tick in `MenuDrawSystem` → `UIInjectSystem.UpdateConfigInterface` — not `UpdateUI` alone
- **Scroll lists:** `UIScrollColumn` + `PlayerInput.LockVanillaMouseScroll` when hovering
- **Text draw:** `Utils.DrawBorderString` — not `SpriteBatch.DrawString` with `DynamicSpriteFont`
- **Toasts:** `ToastManager.Push` / `DrawAndPrune`
- **Hooks:** tML `On_*` detours in `Load`/`Unload` pairs; gen passes by **name** not index
- **Nullable:** disabled project-wide
- **Comments:** WHY only — hook rationale, vanilla assumptions

## Files

- `build.txt` / `description.txt` — workshop metadata
- `AGENTS.md` — AI agent instructions (repo root)
- `Localization/*.hjson` — user-visible strings
- `build.bat`, `test.bat`, `Test.gui.bat` — not copied to ModSources
- `DOCS/` — not copied to ModSources
- `WorldConfigMod.Tests/` — excluded from `.tmod` compile

## [AMENDED 2026-05-19]:

Added Core/, AGENTS.md, UIScrollColumn, UpdateConfigInterface input path. Linked MODDING_GUIDE + EXPANSIONS.

`<type>(<scope>): <description>` — e.g. `fix(build): delayed expansion for x86 paths`
