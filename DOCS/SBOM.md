<!-- PRESERVATION RULE: Never delete or replace content. Append or annotate only. -->

# SBOM — Software Bill of Materials

**Last updated:** 2026-05-19

## Runtime (provided by tModLoader / Terraria)

| Component | Version | Source |
|-----------|---------|--------|
| tModLoader | 1.4.4+ | Steam |
| Terraria | 1.4.4 | Bundled with tML |
| .NET | net8.0 | tML SDK (2025+ builds) |

## Build tooling

| Tool | Purpose | Notes |
|------|---------|-------|
| `dotnet` SDK | Primary mod build via `tModLoader.targets` | Fallback: bundled `dotnet tModLoader.dll -build` |
| `robocopy` | Mirror sources to ModSources | Windows built-in |
| `build.bat` | One-click build script | Windows only |

## Project dependencies

No NuGet packages beyond tModLoader SDK import. Zero third-party libraries in mod source.

## [AMENDED 2026-05-19]:

Initial SBOM for WorldConfigMod MVP.

## [AMENDED 2026-05-19]:

Target framework updated to net8.0. Build fallback documents `tModLoader.dll` launcher (no standalone `.exe`).
