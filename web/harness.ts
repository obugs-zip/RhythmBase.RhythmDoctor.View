import { createElementEvent, IconStyle } from "../ts/view.ts";

type Bounds = {
    left: number;
    top: number;
    right: number;
    bottom: number;
    width: number;
    height: number;
};

type RenderRequest = {
    event: Record<string, unknown>;
    style?: Partial<IconStyle>;
    originX?: number;
    originY?: number;
};

type RenderResult = {
    ok: boolean;
    error?: string;
    bounds?: Bounds;
    childCount?: number;
};

type HarnessGlobal = typeof globalThis & {
    __harnessErrors: string[];
    __harnessReady: boolean;
    renderCase: (request: RenderRequest) => Promise<RenderResult>;
};

const harnessGlobal = globalThis as HarnessGlobal;
const harnessErrors: string[] = [];
harnessGlobal.__harnessErrors = harnessErrors;

globalThis.addEventListener("error", (event) => {
    harnessErrors.push(event.message);
});
globalThis.addEventListener("unhandledrejection", (event) => {
    harnessErrors.push(String(event.reason));
});
const originalConsoleError = console.error.bind(console);
console.error = (...args: unknown[]) => {
    harnessErrors.push(args.map((arg) => String(arg)).join(" "));
    originalConsoleError(...args);
};

function isHidden(element: Element): boolean {
    const style = getComputedStyle(element);
    if (style.display === "none" || style.visibility === "hidden") {
        return true;
    }
    return Number.parseFloat(style.opacity || "1") === 0;
}

function unionBounds(element: HTMLElement): Bounds | null {
    let left = Number.POSITIVE_INFINITY;
    let top = Number.POSITIVE_INFINITY;
    let right = Number.NEGATIVE_INFINITY;
    let bottom = Number.NEGATIVE_INFINITY;

    for (const child of Array.from(element.children)) {
        if (isHidden(child)) {
            continue;
        }
        const rect = child.getBoundingClientRect();
        left = Math.min(left, rect.left);
        top = Math.min(top, rect.top);
        right = Math.max(right, rect.right);
        bottom = Math.max(bottom, rect.bottom);
    }

    if (!Number.isFinite(left)) {
        return null;
    }
    return { left, top, right, bottom, width: right - left, height: bottom - top };
}

function nextFrames(): Promise<void> {
    return new Promise((resolve) => {
        requestAnimationFrame(() => {
            requestAnimationFrame(() => resolve());
        });
    });
}

harnessGlobal.renderCase = async (
    request: RenderRequest,
): Promise<RenderResult> => {
    try {
        const root = document.getElementById("harness-root");
        if (!root) {
            return { ok: false, error: "harness-root element is missing" };
        }
        root.innerHTML = "";

        const style = Object.assign(new IconStyle(), request.style ?? {});
        const element = createElementEvent(request.event, style);
        if (!element) {
            return { ok: false, error: "createElementEvent returned null" };
        }

        element.style.position = "absolute";
        element.style.left = `${request.originX ?? 0}px`;
        element.style.top = `${request.originY ?? 0}px`;
        root.appendChild(element);
        element.onStateChange?.(style);

        await document.fonts.ready;
        await nextFrames();

        const originX = request.originX ?? 0;
        const originY = request.originY ?? 0;
        const bounds = unionBounds(element) ?? {
            left: originX,
            top: originY,
            right: originX,
            bottom: originY,
            width: 0,
            height: 0,
        };

        return { ok: true, bounds, childCount: element.children.length };
    } catch (error) {
        return {
            ok: false,
            error: error instanceof Error
                ? `${error.name}: ${error.message}\n${error.stack ?? ""}`
                : String(error),
        };
    }
};

harnessGlobal.__harnessReady = true;
