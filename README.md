<p align="center">
	<a href="https://www.nuget.org/packages/RhythmBase.RhythmDoctor.View/"><img src="https://img.shields.io/nuget/v/RhythmBase.RhythmDoctor.View?logo=nuget" alt="NuGet Version"></a>
	<img src="https://img.shields.io/nuget/dt/RhythmBase.RhythmDoctor.View" alt="NuGet Downloads"/>
	<a href="https://www.npmjs.com/package/rhythmbase-view"><img src="https://img.shields.io/npm/v/rhythmbase-view" alt="npm Version"/></a>
</p>

# RhythmBase.RhythmDoctor.View

A powerful rendering library for Rhythm Doctor events, built with [SkiaSharp](https://github.com/mono/SkiaSharp).

Sponsored by RhythmBase Project. Visit [RhythmBase](https://github.com/RDCN-Community-Developers/RhythmToolkit) for more info.

## Overview

RhythmBase.RhythmDoctor.View provides a comprehensive solution for visualizing Rhythm Doctor level events. It offers both C# and TypeScript implementations, enabling developers to render event icons, timelines, and interactive visualizations with ease.

**[Core Library →](https://github.com/RDCN-Community-Developers/RhythmToolkit)**  
**[Live Demo (TypeScript) →](https://view.obugs.cn)**

## Repository Layout

The repository is split into six parts:

| Path | Purpose |
|---|---|
| `shared/` | Atlas (`assets`, `assets.png`, `assets.ase`, `assets.json`) used by both implementations |
| `RhythmBase.RhythmDoctor.View/` | C# / SkiaSharp implementation (NuGet package) |
| `RhythmBase.RhythmDoctor.View.Test1/` | C# experiment console used while developing the renderer |
| `ts/` | TypeScript implementation (`definition.ts`, `assetManager.ts`, `view.ts`) |
| `web/` | TypeScript demo, demo-only WOFF2 fonts, parity harness, npm build, generated `dist/` |
| `RhythmBase.RhythmDoctor.View.Compare/` | Console tool that renders single events with both implementations and diffs them |

The embedded resources keep the manifest names `RhythmBase.RhythmDoctor.View.Assets.assets` and
`RhythmBase.RhythmDoctor.View.Assets.assets.png`, so moving files into `shared/` does not change
resource loading.

## Building the TypeScript Side

```sh
cd web
npm install
npm run check   # tsc typecheck + esbuild bundle
```

From the repository root, `npm --prefix web run build` is also supported by the
parity tool and by the scripts.

### Deno

Deno 2.5+ can run the same build without npm:

```sh
deno task web:check   # deno check (DOM libs configured in deno.json)
deno task web:build   # web/build.deno.ts, uses npm:esbuild
deno task web:serve   # http://127.0.0.1:8000, serves web/dist
```

`web/build.deno.ts` uses `npm:esbuild@0.25.0`; the first run downloads and caches it.
The TypeScript sources import with explicit `.ts` extensions, so Deno resolves them
without an import map. When building with Deno, run the parity tool with `--no-build`
so it reuses `web/dist` instead of invoking npm:

```sh
deno task web:build
dotnet run --project RhythmBase.RhythmDoctor.View.Compare -- --no-build
```

### Demo-only fonts

The demo uses the DinkieBitmap WOFF2 files in `web/fonts` for its hint/tooltip
typography. These files are git-ignored and are not included in the npm package or any
release. If you build the demo locally, provide the font files in `web/fonts`; the
renderer itself does not ship or require them.

The build copies `web/index.html`, `web/harness.html`, `web/style.css`, `web/rdview.json`,
`web/fonts`, and `shared/Assets` into `web/dist`. The WOFF2 fonts are demo-only assets
and are not part of the npm package. The demo is served from `web/dist` (not from the
repository root), so any static hosting configuration must point there.

## Publishing the TypeScript Package to npm

The browser library in `ts/` is transformed into an npm package with
[`dnt`](https://jsr.io/@deno/dnt) (Deno to Node Transform). No separate
`package.json` for the library is required.

```sh
deno task pkg:build      # writes scripts/npm/
npm publish --dry-run ./scripts/npm
npm publish ./scripts/npm
# or: deno task pkg:publish
```

`scripts/build_npm.ts` emits ESM plus `.d.ts` files (no CommonJS, because
`assetManager.ts` uses top-level await) and publishes these entry points:

- `rhythmbase-view` (barrel: `ts/mod.ts`)
- `rhythmbase-view/definition`
- `rhythmbase-view/assetManager`
- `rhythmbase-view/view`

Change the package name and version in `scripts/build_npm.ts` before publishing.
The package includes `Assets/assets` and `Assets/assets.png`. The WOFF2 fonts in
`web/fonts` are only used by the demo and are intentionally not published.
`assetManager.ts` fetches `./Assets/assets` relative to the page, so consumers must
serve the package's `Assets/` directory at their site root (or configure their bundler
to copy it). For JSR instead of npm, `deno publish` can publish the `ts/` sources
directly.

## Single-Event Parity Tool

`RhythmBase.RhythmDoctor.View.Compare` builds the TS bundle, renders every event case with both
implementations, and writes `expected.png`, `actual.png`, `diff.png`, plus
`report.json` / `report.md`.

```sh
# Validate the diff algorithm (does not start a browser)
dotnet run --project RhythmBase.RhythmDoctor.View.Compare -- --self-test

# First run: download the Playwright Chromium build
dotnet run --project RhythmBase.RhythmDoctor.View.Compare -- --install-browser --filter AddClassicBeat

# List cases, run one family, or run everything
dotnet run --project RhythmBase.RhythmDoctor.View.Compare -- --list
dotnet run --project RhythmBase.RhythmDoctor.View.Compare -- --filter AddClassicBeat
dotnet run --project RhythmBase.RhythmDoctor.View.Compare -- --out artifacts/parity
```

Useful options: `--scale`, `--padding`, `--headful`, `--no-build`, `--config`.
Thresholds and per-case overrides live in `RhythmBase.RhythmDoctor.View.Compare/parity.config.json`:
`channelDelta` (default 24), `significantPixelRatio` (0.005), and
`meanAbsoluteError` (2.0). `bad%` counts pixels whose maximum channel delta exceeds
`channelDelta`; `meanAbsoluteError` is the average over all pixels of
`max(|ΔR|, |ΔG|, |ΔB|, |ΔA|)` in 0-255 units, so it also reacts to many sub-threshold
differences. `diff.png` only paints pixels above `channelDelta`, which is why it can be
fully black while `meanAbsoluteError` is still nonzero. Exit codes: `0` pass, `1` case
failures, `2` environment or build errors.

### C# visual test modes

The visual tests from `RhythmBase.RhythmDoctor.View.Test1` are now part of the compare tool and do
not require npm or Playwright:

```sh
# All baseline event types in one image (formerly Tests.Run3)
dotnet run --project RhythmBase.RhythmDoctor.View.Compare -- --montage --out artifacts/tests

# One event with a red bounding box (formerly Tests.Run2)
dotnet run --project RhythmBase.RhythmDoctor.View.Compare -- --single AddClassicBeat --out artifacts/tests
dotnet run --project RhythmBase.RhythmDoctor.View.Compare -- --single AddClassicBeat --active --hover

# Level timeline with tabs, BPM marks, bookmarks, and event lanes
# (formerly Tests.Run1/Run4; supports .rdlevel, .Level, and .rdzip)
dotnet run --project RhythmBase.RhythmDoctor.View.Compare -- --timeline path/to/level.rdlevel --out artifacts/tests
```

`--montage` writes `montage.png`; `--single` writes `single-<type>.png`; `--timeline`
writes `timeline/<name>_timeline.png`. `RhythmBase.RhythmDoctor.View.Test1` keeps only the
experimental GPU/video runs (`Run4_2`, `Run5`).

### Known exclusions in the first version

- Hover tooltips (`createHintLayer`) and `$style.hsb` filters are not comparable yet.
- Events that read external image files (`SetBackgroundColor` / `SetForeground` image mode)
  are skipped.
- Palette-indexed colors, multi-scale rendering, non-1 DPR, and full timeline
  (`LevelViewer`) comparisons are out of scope.
- `report.md` records skipped cases and their reasons.
- There is no CI workflow yet; the commands above are intended to run locally.

## Package Documentation

- C# / NuGet: [`RhythmBase.RhythmDoctor.View`](RhythmBase.RhythmDoctor.View/README.md)
  - installation, quick start, and event style options.
- TypeScript / npm: [`rhythmbase-view`](ts/README.md)
  - installation, entry points, browser usage, and atlas requirements.

This root README covers the repository layout, build steps, demo, and parity tooling.
Package-specific documentation lives in the two READMEs above.
## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests to improve the library.

## License

This project is licensed under the GNU Lesser General Public License v3.0.
See [LICENSE.txt](LICENSE.txt) for the full text.

## Related Projects

- [RhythmBase](https://github.com/RDCN-Community-Developers/RhythmToolkit) - Core library for Rhythm Doctor level manipulation

---

<p align="center">Made with ❤️ for the Rhythm Doctor community</p>