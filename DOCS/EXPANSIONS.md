# World Gen Expansions Roadmap

Compact roadmap for World Config Mod world-gen features — what's shipped, what's next, and difficulty.

**Related:** [ARCHITECTURE.md](ARCHITECTURE.md) · [MODDING_GUIDE.md](MODDING_GUIDE.md) · [CHANGELOG.md](CHANGELOG.md) · [Vanilla World Gen Steps](https://github.com/tModLoader/tModLoader/wiki/Vanilla-World-Generation-Steps)

## Legend

**Difficulty:** 🟢 easy (slider + supplement pass) · 🟡 moderate (new system / wrap) · 🔴 hard (fragile, high crash risk)
**Status:** ✅ shipped · 🔜 next up · 💤 backlog

---

## Phase 1 — Foundation ✅ SHIPPED

- 🟢 ✅ Custom world width / height + 5 presets (`WorldSizeSystem`, `On_WorldGen.clearWorld`)
- 🟢 ✅ Use Custom Generation master toggle — mod fully inert when OFF
- 🟡 ✅ Full ore catalog — all 21 wiki ores in `Core/OreCatalog.cs`, 19 with multipliers
- 🟢 ✅ Visual indicator — overlay button turns green + status pill when custom active

## Phase 2 — Tier 1 Features ✅ SHIPPED

All in `FeatureGenSystem.cs` / `OreGenSystem.cs`, gated on `UseCustom`.

- 🟢 ✅ Ore generation — global vein size × + frequency ×, 19 per-ore sliders
- 🟢 ✅ Gems × (6 types), Life Crystals ×, Heart Crystal supplement
- 🟢 ✅ Buried chests ×, Floating Islands ×, Marble + Granite patches ×
- 🟡 ✅ Cave depth scaling (`worldSurface`, `rockLayer`)
- 🟡 ✅ Dungeon side — force left / random / right (`GenVars.dungeonSide`)
- 🟡 ✅ Hellstone supplement, Chlorophyte (HM) supplement
- 🟡 ✅ Bonus altar ore veins, bonus meteorite on meteor events

## Phase 3 — Feature Waves + Persistence ✅ SHIPPED

**Wave 1 — 8 features:**
- 🟢 ✅ Pots ×, Hellforges ×, Shadow Orbs ×, Spider Caves ×
- 🟡 ✅ Living Trees × (cap +10), Jungle Side preset (`jungleOriginX`)
- 🟡 ✅ Disable Evil Spread, Disable Hallow Spread (`ModifyHardmodeTasks`)

**Wave 2 — 10 features:**
- 🟢 ✅ Hives ×, Mushroom Patches ×, Traps ×, Herbs ×, Lakes ×, Shrines ×
- 🟡 ✅ Pyramids tri-state (disable / vanilla / supplement)
- 🟡 ✅ Altar Patch ×, Meteor Chance ×
- 🟡 ✅ Preset bundles — Resource-rich / Cave-labyrinth / Minimal-evil

**Infrastructure:**
- 🟡 ✅ Settings persistence — `Common/ConfigPersistence.cs` → `<SavePath>/WorldConfigMod_settings.txt`

---

## Phase 4 — Larger Systems 🔜 NEXT UP

New UI columns / multi-pass wraps.

- 🟡 🔜 **Global cave density ×** — one multiplier wrapping Tunnels, Dirt/Rock Layer Caves, Surface/Wavy/Mountain Caves
- 🟡 🔜 **Flora pass** — tree density ×, herb/plant abundance × (Planting Trees, Herbs, Vines, Flowers, Cactus, Coral) → "lush world" preset
- 🟡 💤 **Liquids** — lake count, ocean cave depth, beach width (Lakes, Ocean Caves, Beaches)
- 🔴 💤 **Temple / altars** — Jungle Temple size, Lihzahrd altar count (hard-coded size assumptions)
- 🔴 💤 **Dungeon size / complexity** — highest coupling, fragile on custom dimensions
- 🟡 💤 **Biome spread rate** — post-Plantera "world evolution" tuning (`WorldGen.Spread`)

## Phase 5 — Infrastructure 💤 BACKLOG

Do before the slider count passes ~30.

- 🟡 💤 `GenPassCatalog` — mirror `OreCatalog`: pass name, phase, risk tier, default mul
- 🟡 💤 `PassWrapperHelper` — shared replace / insert / supplement helper
- 🟡 💤 `PreWorldGenSystem` / `PostWorldGenSystem` — central `GenVars` presets + late sweeps
- 🟡 💤 Per-world "created with" metadata (`SaveWorldData` / `LoadWorldData`)
- 🔴 💤 Multiplayer config sync — network packets + server authority
- 🟢 💤 Linux / Mac `build.sh` mirroring `build.bat`

---

## Hook reference

**ModSystem pipeline:** `PreWorldGen` (set `GenVars` flags) · `ModifyWorldGenTasks` (replace/insert/`Disable` passes) · `PostWorldGen` (mass sweeps) · `ModifyHardmodeTasks` (HM spread).
**`On_WorldGen.*` detours:** `clearWorld` (dimensions ✅) · `SmashAltar` (HM ore ✅) · `dropMeteor`/`spawnMeteor` (meteor events ✅).

**Rules of thumb:**
1. Find passes by name, never index — other mods reorder the list.
2. Prefer supplement passes over full replacement when vanilla logic is complex.
3. Respect `GenVars.structures` — `CanPlace` before placing; `AddProtectedStructure` for landmarks.
4. Custom dimensions + aggressive terrain edits = highest crash risk.

**🔴 Treat with care:** Terrain / Reset (foundation) · Dungeon / Jungle Temple (hard-coded sizes) · Settle Liquids (order-sensitive) · Tile/Final Cleanup (floating tiles, broken wires).

## Adding a new expansion (checklist)

1. Add field(s) + default constant to `WorldGenConfig.cs`; update `CountChanges` if relevant.
2. Add `UICompactSliderRow` slider in `WorldConfigUIStateV2.cs` under the right tab (`defaultValue` + `showVanillaBadge: true`).
3. Implement hook in a `ModSystem` — pick tier from this roadmap.
4. If testable math, add `Core/` helpers + `WorldConfigMod.Tests/` tests.
5. Document here (under the phase) and in [CHANGELOG.md](CHANGELOG.md).

## References

- [tModLoader World Generation wiki](https://github.com/tModLoader/tModLoader/wiki/World-Generation)
- [WorldGen API (tML docs)](https://docs.tmodloader.net/docs/stable/class_world_gen.html)
- [ModSystem hooks](https://docs.tmodloader.net/docs/stable/class_mod_system.html)
