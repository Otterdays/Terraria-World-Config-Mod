/**
 * World Config — single-page site router + dynamic tables
 * Keep feature/ore data in sync with WorldGenConfig + OreCatalog when shipping changes.
 */
(function () {
  "use strict";

  const PAGES = ["home", "start", "features", "settings", "ores", "install", "advanced", "contrib"];

  const FEATURE_GROUPS = [
    {
      title: "UI & session",
      rows: [
        ["V2 sidebar panel (default)", "Tabs: World, Shape, Features, Ores, Presets, Info", "WorldConfigUIStateV2"],
        ["V1 legacy panel", "Two-column layout; swap anytime", "WorldConfigUIState"],
        ["Live summary strip", "Size, preset label, change count", "V2 header"],
        ["Diff dots & vanilla %", "Per-category and per-ore badges", "WorldGenConfig helpers"],
        ["Settings persistence", "Plain-text key=value file", "ConfigPersistence.cs"],
        ["Overlay button HUD", "Green when custom ON; cog icon", "MenuDrawSystem"],
      ],
    },
    {
      title: "World size & shape",
      rows: [
        ["Width / height sliders", "4200–16800 × 1200–4800", "WorldSizeSystem"],
        ["5 size presets", "S → XXL", "ApplyPresetSize"],
        ["Cave depth ×", "0.5–2.0 surface/rock layer", "FeatureGenSystem"],
        ["Dungeon side", "Left / random / right", "PreWorldGen"],
        ["Jungle side", "25% / vanilla / 75%", "PreWorldGen"],
        ["Pyramids", "Disable / vanilla / boost", "Pass insert"],
        ["No evil / hallow spread", "HM spread toggles", "ModifyHardmodeTasks"],
      ],
    },
    {
      title: "Ore generation",
      rows: [
        ["Global vein size ×", "0.25–25", "OreScatterRunner"],
        ["Global frequency ×", "0.1–25", "Shinies replacement"],
        ["19 per-ore multipliers", "0–5 per type", "OreCatalog.WithMultipliers"],
        ["Pre-HM scatter", "Copper → platinum, evil, meteorite", "Replace Shinies"],
        ["Hellstone", "Underworld supplement", "After Underworld pass"],
        ["HM altar ores", "Cobalt → titanium", "On SmashAltar"],
        ["Chlorophyte", "HM natural", "ModifyHardmodeTasks"],
        ["Meteor events", "Biome drops", "On dropMeteor"],
        ["Altar patch × / meteor chance ×", "HM meta sliders", "Ore meta"],
      ],
    },
    {
      title: "World features (supplement passes)",
      rows: [
        ["Gems, life crystals, chests", "×0–5", "FeatureGenSystem"],
        ["Floating islands, marble/granite", "×0–5", "FeatureGenSystem"],
        ["Pots, hellforges, shadow orbs", "×0–5", "FeatureGenSystem"],
        ["Living trees, spider caves", "×0–5", "FeatureGenSystem"],
        ["Hives, mushroom patches", "×0–5", "FeatureGenSystem"],
        ["Traps, herbs, lakes, shrines", "×0–5", "FeatureGenSystem"],
      ],
    },
    {
      title: "Preset bundles",
      rows: [
        ["Resource Rich", "Ore + loot density boost", "ApplyResourceRichPreset"],
        ["Cave Labyrinth", "Caves, marble, spiders, traps", "ApplyCaveLabyrinthPreset"],
        ["Minimal Evil", "No spread + fewer orbs/patches", "ApplyMinimalEvilPreset"],
        ["DEBUG tiny + 20× ore", "Fast test world", "ApplyDebugWorldGenPreset"],
      ],
    },
  ];

  const SETTINGS = [
    {
      title: "Master & dimensions",
      rows: [
        ["Use Custom Generation", "toggle", "OFF", "Must be ON for any custom effect"],
        ["World width", "4200 – 16800", "4200", "Main.maxTilesX at clearWorld"],
        ["World height", "1200 – 4800", "1200", "Main.maxTilesY at clearWorld"],
      ],
    },
    {
      title: "Ore",
      rows: [
        ["Vein size ×", "0.25 – 25", "1", "Runner strength + steps"],
        ["Global frequency ×", "0.1 – 25", "1", "Vein attempt count"],
        ["Per-ore ×", "0 – 5", "1", "×0 disables that ore type"],
      ],
    },
    {
      title: "Shape & spread",
      rows: [
        ["Cave depth ×", "0.5 – 2", "1", "After Terrain"],
        ["Dungeon side", "−1 / 0 / +1", "0", "GenVars.dungeonSide"],
        ["Jungle side", "−1 / 0 / +1", "0", "GenVars.jungleOriginX"],
        ["Pyramids mode", "−1 / 0 / +1", "0", "Disable / vanilla / boost"],
        ["No evil spread", "bool", "false", "HM corruption/crimson"],
        ["No hallow spread", "bool", "false", "HM hallow"],
      ],
    },
    {
      title: "Features (all ×0–5 unless noted)",
      rows: [
        ["Gems, life crystals, chests", "×0–5", "1", ""],
        ["Floating islands, marble/granite", "×0–5", "1", ""],
        ["Pots, hellforges, shadow orbs", "×0–5", "1", ""],
        ["Living trees, spider caves, hives", "×0–5", "1", ""],
        ["Mushroom, traps, herbs, lakes, shrines", "×0–5", "1", ""],
        ["Altar patch ×, meteor chance ×", "multiplier", "1", "Ore meta tab"],
      ],
    },
  ];

  const ORES = [
    { name: "Copper", phase: "Pre-HM scatter", slider: true },
    { name: "Tin", phase: "Pre-HM scatter", slider: true },
    { name: "Iron", phase: "Pre-HM scatter", slider: true },
    { name: "Lead", phase: "Pre-HM scatter", slider: true },
    { name: "Silver", phase: "Pre-HM scatter", slider: true },
    { name: "Tungsten", phase: "Pre-HM scatter", slider: true },
    { name: "Gold", phase: "Pre-HM scatter", slider: true },
    { name: "Platinum", phase: "Pre-HM scatter", slider: true },
    { name: "Meteorite", phase: "Meteor event + supplement", slider: true, note: "Also scales meteor drops." },
    { name: "Demonite", phase: "Pre-HM scatter", slider: true },
    { name: "Crimtane", phase: "Pre-HM scatter", slider: true },
    { name: "Obsidian", phase: "Not world gen", slider: false, note: "Lava + water; info only." },
    { name: "Hellstone", phase: "Underworld", slider: true },
    { name: "Cobalt", phase: "HM altar", slider: true },
    { name: "Palladium", phase: "HM altar", slider: true },
    { name: "Mythril", phase: "HM altar", slider: true },
    { name: "Orichalcum", phase: "HM altar", slider: true },
    { name: "Adamantite", phase: "HM altar", slider: true },
    { name: "Titanium", phase: "HM altar", slider: true },
    { name: "Chlorophyte", phase: "HM natural", slider: true },
    { name: "Luminite", phase: "Not world gen", slider: false, note: "Moon Lord drop; info only." },
  ];

  const TROUBLESHOOT = [
    ["build.bat — tModLoader not found", "Set TML_DIR to Steam …\\common\\tModLoader (needs tModLoader.dll)."],
    ["build.bat — game still running", "Close tModLoader completely (including tray)."],
    ["No World Config button", "Enable mod + Reload; check UI conflicts on New World."],
    ["World gen crash on tiny sizes", "Minimum 4200×1200; smaller breaks vanilla passes."],
    ["Ores look vanilla", "Turn Use Custom Generation ON before Create."],
    ["HUD wrong on 4K", "Update mod — menu HUD uses framebuffer viewport."],
    ["Cursor behind button", "MenuDrawSystem redraws cursor after overlay."],
    ["Scroll dead on ore list", "UIInjectSystem + Main.MenuUI scroll forwarding."],
  ];

  function tableHtml(headers, rows) {
    const th = headers.map((h) => `<th>${h}</th>`).join("");
    const body = rows
      .map((r) => `<tr>${r.map((c) => `<td>${c}</td>`).join("")}</tr>`)
      .join("");
    return `<table class="data-table"><thead><tr>${th}</tr></thead><tbody>${body}</tbody></table>`;
  }

  function renderFeatureTables() {
    const el = document.getElementById("featureTables");
    if (!el) return;
    el.innerHTML = FEATURE_GROUPS.map(
      (g) =>
        `<div class="feature-group"><h2>${g.title}</h2>${tableHtml(
          ["Feature", "Detail", "Code"],
          g.rows
        )}</div>`
    ).join("");
  }

  function renderSettingsTables() {
    const el = document.getElementById("settingsTables");
    if (!el) return;
    el.innerHTML = SETTINGS.map(
      (g) =>
        `<div class="feature-group"><h2>${g.title}</h2>${tableHtml(
          ["Setting", "Range", "Default", "Notes"],
          g.rows
        )}</div>`
    ).join("");
  }

  function renderOreCatalog(filter) {
    const el = document.getElementById("oreCatalog");
    if (!el) return;
    const q = (filter || "").toLowerCase();
    const list = ORES.filter(
      (o) =>
        !q ||
        o.name.toLowerCase().includes(q) ||
        o.phase.toLowerCase().includes(q) ||
        (o.note && o.note.toLowerCase().includes(q))
    );
    el.innerHTML =
      `<div class="ore-grid">` +
      list
        .map(
          (o) => `<article class="ore-card${o.slider ? "" : " dim"}">
          <h3>${o.name}${o.slider ? "" : " *"}</h3>
          <div class="phase">${o.phase}</div>
          ${o.note ? `<p class="note">${o.note}</p>` : ""}
          <p class="note">${o.slider ? "Slider ×0–5" : "No slider — reference only"}</p>
        </article>`
        )
        .join("") +
      `</div>`;
  }

  function renderTroubleshoot() {
    const el = document.getElementById("troubleshootTable");
    if (!el) return;
    el.innerHTML = tableHtml(["Symptom", "Fix"], TROUBLESHOOT);
  }

  function setActivePage(route) {
    const id = PAGES.includes(route) ? route : "home";
    document.querySelectorAll(".page").forEach((p) => {
      p.classList.toggle("active", p.dataset.page === id);
    });
    document.querySelectorAll("[data-route]").forEach((a) => {
      a.classList.toggle("active", a.dataset.route === id);
    });
    document.getElementById("siteNav")?.classList.remove("open");
    document.getElementById("navToggle")?.setAttribute("aria-expanded", "false");
    if (id !== "home") window.scrollTo({ top: 0, behavior: "smooth" });
  }

  function routeFromHash() {
    const h = (location.hash || "#home").replace("#", "");
    return PAGES.includes(h) ? h : "home";
  }

  function onNavClick(e) {
    const a = e.target.closest("[data-route]");
    if (!a) return;
    e.preventDefault();
    const r = a.dataset.route;
    history.pushState(null, "", `#${r}`);
    setActivePage(r);
  }

  function initRepoLink() {
    const meta = document.querySelector('meta[name="github-repo"]');
    const url = meta?.content;
    if (url) {
      const link = document.getElementById("repoLink");
      if (link) link.href = url;
    }
  }

  document.addEventListener("click", onNavClick);
  window.addEventListener("popstate", () => setActivePage(routeFromHash()));
  window.addEventListener("hashchange", () => setActivePage(routeFromHash()));

  document.getElementById("navToggle")?.addEventListener("click", () => {
    const nav = document.getElementById("siteNav");
    const btn = document.getElementById("navToggle");
    const open = nav.classList.toggle("open");
    btn.setAttribute("aria-expanded", open ? "true" : "false");
  });

  document.getElementById("oreFilter")?.addEventListener("input", (e) => {
    renderOreCatalog(e.target.value);
  });

  renderFeatureTables();
  renderSettingsTables();
  renderOreCatalog();
  renderTroubleshoot();
  initRepoLink();
  setActivePage(routeFromHash());
})();
