const randomInt = (min: number, max: number): number =>
    Math.floor(Math.random() * (max - min + 1)) + min;

const pick = <T>(values: readonly T[]): T =>
    values[randomInt(0, values.length - 1)];

const randomBool = (): boolean => Math.random() < 0.5;

const randomColor = (): string =>
    Array.from(
        { length: 3 },
        () => randomInt(0, 255).toString(16).padStart(2, "0"),
    ).join("")
        .toUpperCase() + "FF";

const randomRooms = (): number[] => {
    const pool = [0, 1, 2, 3, 4];
    for (let i = pool.length - 1; i > 0; i--) {
        const j = randomInt(0, i);
        [pool[i], pool[j]] = [pool[j], pool[i]];
    }
    return pool.slice(0, randomInt(1, 3)).sort((a, b) => a - b);
};

const randomOrder = (length: number): number[] => {
    const values = Array.from({ length }, (_, index) => index);
    for (let i = values.length - 1; i > 0; i--) {
        const j = randomInt(0, i);
        [values[i], values[j]] = [values[j], values[i]];
    }
    return values;
};

const randomPair = (min: number, max: number): number[] => [
    randomInt(min, max),
    randomInt(min, max),
];

const pickNumber = (
    values: readonly number[],
): number => pick(values);

export function randomizeSample(
    sample: Record<string, unknown>,
): Record<string, unknown> {
    const result = JSON.parse(JSON.stringify(sample)) as Record<string, unknown>;

    result.bar = randomInt(1, 3);
    result.beat = randomInt(1, 8);
    result.y = randomInt(0, 6);

    for (const [key, value] of Object.entries(result)) {
        if (key === "type" || key === "if" || key === "$style") {
            continue;
        }
        if (Array.isArray(value)) {
            switch (key) {
                case "rooms":
                    result[key] = randomRooms();
                    break;
                case "order":
                    result[key] = randomOrder(value.length);
                    break;
                case "position":
                case "roomPosition":
                case "cameraPosition":
                case "textPosition":
                case "pivot":
                    result[key] = randomPair(0, 100);
                    break;
                case "scale":
                    result[key] = randomPair(10, 100);
                    break;
                case "heights":
                    result[key] = Array.from(
                        { length: value.length },
                        () => randomInt(0, 100),
                    );
                    break;
                case "players":
                case "cpuMarkers":
                case "cornerPositions":
                    break;
                default:
                    break;
            }
            continue;
        }
        if (typeof value === "boolean") {
            result[key] = randomBool();
            continue;
        }
        if (typeof value !== "number" && typeof value !== "string") {
            continue;
        }

        switch (key) {
            case "tag":
                result[key] = `tag-${randomInt(1000, 9999)}`;
                break;
            case "text":
                result[key] = `Sample text ${randomInt(100, 999)}`;
                break;
            case "methodName":
                result[key] = `customMethod${randomInt(1, 99)}`;
                break;
            case "pattern":
                result[key] = Array.from(
                    { length: 6 },
                    () => randomBool() ? "x" : "-",
                ).join("");
                break;
            case "color":
            case "outlineColor":
            case "tintColor":
            case "borderColor":
            case "startColor":
            case "endColor":
                result[key] = randomColor();
                break;
            case "row":
                result[key] = randomInt(0, 7);
                break;
            case "tick":
                result[key] = pickNumber([0.5, 1, 1.5, 2]);
                break;
            case "swing":
                result[key] = pickNumber([0, 0.5, 1]);
                break;
            case "syncoBeat":
                result[key] = randomInt(0, 6);
                break;
            case "syncoSwing":
                result[key] = pickNumber([0, 0.5, 1]);
                break;
            case "syncoVolume":
                result[key] = randomInt(0, 100);
                break;
            case "syncoPitch":
                result[key] = randomInt(50, 200);
                break;
            case "interval":
                result[key] = pickNumber([1, 1.5, 2, 3]);
                break;
            case "hold":
                result[key] = pickNumber([0, 0.5, 1, 2]);
                break;
            case "length":
                result[key] = randomInt(3, 9);
                break;
            case "loops":
                result[key] = randomInt(0, 3);
                break;
            case "subdivisions":
                result[key] = randomInt(1, 4);
                break;
            case "pulse":
                result[key] = randomInt(-1, 6);
                break;
            case "customPulse":
                result[key] = randomInt(0, 6);
                break;
            case "duration":
            case "easingDuration":
                result[key] = pickNumber([0.5, 1, 1.5, 2, 3, 4]);
                break;
            case "transitionTime":
                result[key] = pickNumber([0.5, 1, 2]);
                break;
            case "delay":
                result[key] = pickNumber([0, 0.5, 1, 1.5]);
                break;
            case "speed":
                result[key] = pickNumber([0.5, 0.75, 1, 1.25, 1.5, 2]);
                break;
            case "opacity":
            case "volume":
                result[key] = pickNumber([0, 25, 50, 75, 100]);
                break;
            case "angle":
                result[key] = pickNumber([-180, -90, -45, 0, 45, 90, 180]);
                break;
            case "strength":
                if (typeof value === "number") {
                    result[key] = randomInt(1, 100);
                }
                break;
            case "count":
                result[key] = randomInt(1, 5);
                break;
            case "frequency":
                result[key] = randomInt(1, 4);
                break;
            case "bpm":
            case "beatsPerMinute":
                result[key] = randomInt(60, 200);
                break;
            case "crotchetsPerBar":
                result[key] = randomInt(3, 12);
                break;
            case "visualBeatMultiplier":
                result[key] = pickNumber([0.5, 1, 1.5, 2]);
                break;
            case "size":
                result[key] = randomInt(4, 16);
                break;
            case "fadeOutRate":
                result[key] = randomInt(1, 5);
                break;
            case "subdivOffset":
                result[key] = pickNumber([0, 0.5, 1]);
                break;
            case "shakeX":
            case "shakeY":
                result[key] = randomInt(-20, 20);
                break;
            default:
                break;
        }
    }

    if (result.preXs && typeof result.preXs === "object") {
        result.preXs = {
            syncoBeat: randomInt(0, 6),
            syncoSwing: pickNumber([0, 0.5, 1]),
        };
    }

    return result;
}
