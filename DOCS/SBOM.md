<!-- PRESERVATION RULE: Never delete or replace content. Append or annotate only. -->

# SBOM — Software Bill of Materials

**Last updated:** 2026-05-20 (test-suite audit; no dependency changes)

## Runtime (provided by tModLoader / Terraria)

| Component | Version | Source |
|-----------|---------|--------|
| **tModLoader** | **2026.3.3.0** | Steam |
| **Terraria** | **1.4.4.9** | Bundled with tML |
| .NET | net8.0 | tML SDK (2026.3.x builds) |

See [`VERSIONS.md`](VERSIONS.md) for the full pin list.

## Build tooling

| Tool | Purpose | Notes |
|------|---------|-------|
| `dotnet` SDK | Primary mod build via `tModLoader.targets` | Fallback: bundled `dotnet tModLoader.dll -build` |
| `robocopy` | Mirror sources to ModSources | Windows built-in |
| `build.bat` | One-click build script | Windows only |

## Test dependencies (dev only)

| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.NET.Test.Sdk | 17.11.1 | xUnit runner |
| xunit | 2.9.2 | Unit tests |
| xunit.runner.visualstudio | 2.8.2 | IDE / `dotnet test` |

## [AMENDED 2026-05-20]: Test-suite audit

No package changes. `WorldGenConfigTests` reuse existing xUnit dependencies. `Test.gui.bat` is a Windows batch wrapper around `dotnet test`; no new tooling dependency.

## Project dependencies (mod)

No NuGet packages beyond tModLoader SDK import. Zero third-party libraries in mod source.

## [AMENDED 2026-05-19]:

Initial SBOM for WorldConfigMod MVP.

## [AMENDED 2026-05-19]:

Target framework updated to net8.0. Build fallback documents `tModLoader.dll` launcher (no standalone `.exe`).

## [AMENDED 2026-05-20]:

Pinned **Terraria 1.4.4.9** and **tModLoader 2026.3.3.0**. Documented xUnit test packages. `Core/TerrariaVanillaSpecs.cs` holds world-size and ore-frequency constants for Terraria-mimicking tests.
