<p align="center">
	<a href="https://www.npmjs.com/package/rhythmbase-view"><img src="https://img.shields.io/npm/v/rhythmbase-view" alt="npm version"></a>
	<img src="https://img.shields.io/npm/dm/rhythmbase-view" alt="npm downloads"/>
</p>

# rhythmbase-view

Render Rhythm Doctor event icons and interactive event elements in the browser.

## Installation

```sh
npm install rhythmbase-view
```

## Requirements

- Browser environment only.
- Serve the package's `Assets/` directory at the site root. `assetManager` fetches
  `./Assets/assets` and uses `Assets/assets.png` as the atlas.
- The package does not ship fonts. Fonts used by the demo for hint/tooltip typography
  are demo-only and must be provided by the host if desired.

## Entry Points

- `rhythmbase-view` - barrel export
- `rhythmbase-view/view` - `createElementEvent`, `colorOf`, `IconStyle`
- `rhythmbase-view/definition` - `EventType`, `Tab`, `EventInfos`, geometry types
- `rhythmbase-view/assetManager` - atlas loading (`slices`, `assetSize`, `assetUrl`)

## Usage

```ts
import { createElementEvent, IconStyle } from "rhythmbase-view/view";
import type { HTMLEventElement } from "rhythmbase-view/view";

const rdview = document.createElement("div");
const elems: HTMLEventElement[] = [];
const eventStyle: IconStyle = new IconStyle();

// Parsed entries from rdview.json (or your own event data).
const eventObjects: Record<string, unknown>[] = [];

for (const obj of eventObjects) {
    const style = {
        ...eventStyle,
        enabled: obj.active ?? true,
    };

    const elem = createElementEvent(obj, style);
    if (!elem) continue;

    elem.eventStyle = style;
    elem.style.position = "absolute";

    const x = ((((obj.bar ?? 1) - 1) * 8) + (obj.beat ?? 1) - 1) * 14 * style.scale;
    const y = (obj.y ?? obj.row ?? 0) * 14 * style.scale;
    elem.moveTo(x, y);

    elem.addEventListener("mousedown", () => {
        elem.eventStyle.active = true;
        elem.onStateChange(elem.eventStyle);
        elem.style.zIndex = "10";
    });
    elem.addEventListener("mouseup", () => {
        elem.eventStyle.active = false;
        elem.onStateChange(elem.eventStyle);
    });
    elem.addEventListener("mouseover", () => {
        elem.eventStyle.hover = true;
        elem.onStateChange(elem.eventStyle);
    });
    elem.addEventListener("mouseout", () => {
        elem.eventStyle.hover = false;
        elem.eventStyle.active = false;
        elem.onStateChange(elem.eventStyle);
    });

    elems.push(elem);
}

for (const elem of elems) rdview.appendChild(elem);
```

## Event Style Options

- `scale` - scaling factor for the icon size
- `active` - whether the event is active/selected
- `enabled` - whether the event is enabled or disabled
- `hover` - whether the mouse is hovering over the event

## License

This project is licensed under the GNU Lesser General Public License v3.0.
See [LICENSE.txt](https://github.com/OLDRedstone/RhythmBase.RhythmDoctor.View/blob/main/LICENSE.txt) for the full text.
