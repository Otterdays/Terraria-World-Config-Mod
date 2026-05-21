<!-- PRESERVATION RULE: Never delete or replace content. Append or annotate only. -->

# GitHub Pages — World Config public site

**Last updated:** 2026-05-20 (Terraria 1.4.4.9 / tModLoader 2026.3.3.0)

## Purpose

Player-facing documentation at repo root: beginner quick start, full feature list, settings reference, ore catalog, install/troubleshooting, and contributor pointers. Complements `README.md` (GitHub render) with a navigable SPA.

## Files (not in `.tmod`)

| Path | Role |
|------|------|
| `index.html` | Page shell + static copy |
| `styles.css` | Layout + Terraria-night theme |
| `app.js` | Hash router + dynamic tables (`FEATURE_GROUPS`, `SETTINGS`, `ORES`) |
| `assets/banner.svg` | 1200×630 hero + README image |
| `.nojekyll` | Disable Jekyll on Pages |
| `.github/workflows/github-pages.yml` | Deploy `_site/` bundle only |

`build.bat` **excludes** these via robocopy `/XD assets .github` and `/XF index.html styles.css app.js .nojekyll`.

## Enable Pages

1. Push to `main` or `master`.
2. GitHub → **Settings → Pages → Build and deployment → Source:** GitHub Actions.
3. Workflow **Deploy GitHub Pages** uploads `index.html`, CSS, JS, `assets/`.

Project URL pattern: `https://<user>.github.io/<repo>/` (use relative asset paths — already relative).

## Maintenance (agents & humans)

When shipping **player-visible** features:

1. Update `app.js` data arrays to match `WorldGenConfig` / `OreCatalog`.
2. Adjust `index.html` only if new top-level section needed.
3. Append `DOCS/CHANGELOG.md` + `DOCS/SUMMARY.md`.
4. Refresh `assets/banner.svg` if marketing visuals should reflect major releases.
5. Update `README.md` link to Pages URL once known.

## [AMENDED 2026-05-20]: Initial site

- SPA routes: home, start, features, settings, ores, install, advanced, contrib.
- Banner moved from repo root → `assets/banner.svg` (expanded v3: meteor, moon, dungeon, hive, 11 gems, save badge).
