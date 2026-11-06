import { EventInfos, EventType, hiddenEventType } from "../ts/definition.ts";
import {
    colorOf,
    createElementEvent,
    IconStyle,
} from "../ts/view.ts";
import type { HTMLEventElement } from "../ts/view.ts";
import { randomizeSample } from "./randomizeSample.ts";
import { eventSamples } from "./samples.ts";
type EventObject = {
    type: EventType;
    bar?: number;
    beat?: number;
    row?: number;
    y?: number;
    active?: boolean;
    _startPos: number;
    _endPos: number;
};

const body = document.body;
const defaultData = (await fetch("rdview.json").then((res) => res.text()))
    .slice(1, -1);
const rdview = document.createElement("div");
const rdviewError = document.createElement("div");
const rdviewContent = document.createElement("textarea");
const rdviewContentReloadButton = document.createElement("button");
const rdviewContentResetButton = document.createElement("button");
const rdviewClearButton = document.createElement("button");
const rdviewEventTypeLabel = document.createElement("label");
const rdviewEventType = document.createElement("select");
const rdviewAddEventButton = document.createElement("button");
const rdviewDownloadButton = document.createElement("button");
const footer = document.createElement("div");
rdview.style.position = "relative";
rdview.style.height = "200px";
rdviewContent.value = defaultData;
rdviewError.style.color = "red";
rdviewError.style.fontFamily = "d9, monospace";
rdviewError.style.whiteSpace = "pre-wrap";
rdviewError.style.margin = "4px 0";
rdviewError.style.minHeight = "1.2em";
rdviewContentReloadButton.type = "button";
rdviewContentReloadButton.textContent = "Reload Content";
rdviewContentResetButton.type = "button";
rdviewContentResetButton.textContent = "Reset Content";
rdviewClearButton.type = "button";
rdviewClearButton.textContent = "Clear Content";
rdviewAddEventButton.type = "button";
rdviewAddEventButton.textContent = "Add Event";
rdviewDownloadButton.type = "button";
rdviewDownloadButton.textContent = "Download";
rdviewEventType.id = "rdviewEventType";
rdviewEventTypeLabel.textContent = "Event Type: ";
rdviewEventTypeLabel.htmlFor = "rdviewEventType";
rdviewContent.id = "rdviewContent";
rdviewContent.placeholder =
    '{ "bar": 1, "beat": 1, "y": 0, "type": "SetTheme", "rooms": [0], "preset": "OrientalTechno", "skipPaintEffects": false },\n{ "bar": 1, "y": 0, "type": "PlaySong", "song": {"filename": "sndOrientalTechno"}, "bpm": 100, "loop": false },\n{ "bar": 1, "beat": 1, "y": 0, "type": "AddClassicBeat", "row": 0, "tick": 1, "swing": 0, "hold": 0 }';
footer.style.marginTop = "16px";
footer.style.fontSize = "16px";
footer.style.color = "#666";
footer.style.textAlign = "center";
footer.style.fontFamily = "d7, monospace";
const line1 = document.createElement("p"),line2 = document.createElement("p"),line3 = document.createElement("p");
line1.innerHTML =
    'RhythmBase.RhythmDoctor.View - A simple event viewer for Rhythm Doctor Level data.';
line2.innerHTML = 'Developed with â™?by obugs.';
line3.innerHTML =
    'Sponsored by RhythmBase Project. Visit <a href="https://github.com/RDCN-Community-Developers/RhythmToolkit">RhythmBase</a> for more info.';
footer.appendChild(line1);
footer.appendChild(line2);
footer.appendChild(line3);
footer.querySelector("a")!.style.color = "#666";
for (let eventTypeKey in EventType) {
    if (hiddenEventType.indexOf(eventTypeKey as EventType) > -1) continue;
    const option = document.createElement("option");
    const typeInfo = EventInfos[eventTypeKey as keyof typeof EventType];
    const color = colorOf(typeInfo.defaultTab);
    option.style.color = color.toHexString(false);
    option.style.backgroundColor = color.withAlpha(192).toHexString(true);
    option.value = EventType[eventTypeKey as keyof typeof EventType];
    option.textContent = EventType[eventTypeKey as keyof typeof EventType];
    rdviewEventType.appendChild(option);
}
rdviewContentReloadButton.addEventListener("click", () => {
    rdviewContent.dispatchEvent(new Event("input"));
});
rdviewContentResetButton.addEventListener("click", () => {
    rdviewContent.value = defaultData;
    rdviewContent.dispatchEvent(new Event("input"));
});
rdviewDownloadButton.addEventListener("click", () => {
    alert("Can't you just copy the content straight out of the textarea?");
});
function loadContent(): EventObject[] | string {
    const contentText = rdviewContent.value;
    let objs: EventObject[] = [];
    try {
        objs = JSON.parse(`[${contentText}]`);
        let depth = 0;
        let objIndex = 0;
        for (let i = 0; i < contentText.length; i++) {
            const ch = contentText[i];
            if (ch === "{") depth++;
            else if (ch === "}") depth--;
            if (depth === 1 && ch === "{") objs[objIndex]._startPos = i;
            if (depth === 0 && ch === "}") {
                objs[objIndex]._endPos = i + 1;
                objIndex++;
            }
        }
    } catch (e) {
        return `Error parsing content: ${e}`;
    }
    return objs;
}
let tempX = 0;
let tempY = 0;
let focusingEventElem: HTMLEventElement | null = null;
document.addEventListener("mousemove", (event: MouseEvent) => {
    if (!focusingEventElem) return;
    focusingEventElem.moveTo(
        event.clientX - tempX,
        event.clientY - tempY,
    );
});

async function rendContent(objs: EventObject[]): Promise<void> {
    const eventStyle: IconStyle = new IconStyle();
    eventStyle.scale = 2.0;
    eventStyle.showDuration = false;
    const elems: HTMLEventElement[] = [];
    for (let obj of objs) {
        const style = {
            ...eventStyle,
            enabled: obj.active ?? true,
        };
        const elem = createElementEvent(obj, style);

        if (!elem) continue;
        elem.eventStyle = style;
        elem.style.position = "absolute";
        let x = ((((obj.bar ?? 1) - 1) * 8) + (obj.beat ?? 1) - 1) *
            14 * eventStyle.scale;
        let y = (obj.y ?? obj.row ?? 0) * 14 * eventStyle.scale;
        elem.moveTo(x, y);
        elem.addEventListener("mousedown", (event) => {
            const eStyle = elem.eventStyle;
            eStyle.active = true;
            elem.onStateChange(eStyle);
            elem.style.zIndex = "10";
            focusingEventElem = elem;
            x = elem.style.left ? parseFloat(elem.style.left) : 0;
            y = elem.style.top ? parseFloat(elem.style.top) : 0;
            tempX = event.clientX - x;
            tempY = event.clientY - y;
        });
        elem.addEventListener("mouseup", (event) => {
            const eStyle = elem.eventStyle;
            eStyle.active = false;
            elem.onStateChange(eStyle);
            if (focusingEventElem) {
                focusingEventElem.style.zIndex = "0";
            }
            focusingEventElem = null;
        });
        elem.addEventListener("dblclick", (event) => {
            rdviewContent.focus();
            rdviewContent.setSelectionRange(obj._startPos, obj._endPos);
        });
        elem.addEventListener("mouseover", (event) => {
            const eStyle = elem.eventStyle;
            eStyle.hover = true;
            elem.onStateChange(eStyle);
        });
        elem.addEventListener("mouseout", (event) => {
            const eStyle = elem.eventStyle;
            eStyle.hover = false;
            eStyle.active = false;
            elem.onStateChange(eStyle);
        });
        elems.push(elem);
    }
    for (const elem of elems) {
        if (elem && rdview) {
            rdview.appendChild(elem);
        }
    }
}
rdviewContent.addEventListener("input", async (event) => {
    rdview.innerHTML = "";
    rdviewError.textContent = "";
    const content = loadContent();
    if (typeof content === "string") {
        rdviewError.textContent = content;
    } else {
        await rendContent(content);
    }
});
rdviewClearButton.addEventListener("click", (event) => {
    rdviewContent.value = "";
    rdviewContent.dispatchEvent(new Event("input"));
});
rdviewAddEventButton.addEventListener("click", async (event) => {
    const type = rdviewEventType.value;
    const sample = eventSamples[type]
        ? randomizeSample(eventSamples[type])
        : randomizeSample({ bar: 1, beat: 1, y: 0, type });
    sample.type = type;
    sample.bar ??= 1;
    sample.beat ??= 1;
    sample.y ??= 0;

    let str = "";
    if (
        rdviewContent.value.trim().length > 0 &&
        !rdviewContent.value.trim().endsWith(",")
    ) {
        str += ",\n";
    }
    str += JSON.stringify(sample, null, 2);
    rdviewContent.value += str;
    rdviewContent.dispatchEvent(new Event("input"));
});
body.appendChild(rdview);
body.appendChild(rdviewError);
body.appendChild(rdviewContentReloadButton);
body.appendChild(rdviewContentResetButton);
body.appendChild(rdviewClearButton);
body.appendChild(document.createElement("br"));
body.appendChild(rdviewEventTypeLabel);
body.appendChild(rdviewEventType);
body.appendChild(rdviewAddEventButton);
body.appendChild(document.createElement("br"));
body.appendChild(rdviewDownloadButton);
body.appendChild(document.createElement("br"));
body.appendChild(footer);
body.appendChild(rdviewContent);
rdviewContent.dispatchEvent(new Event("input"));
