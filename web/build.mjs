import { build } from "esbuild";
import { cp, mkdir, rm } from "node:fs/promises";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const webDir = dirname(fileURLToPath(import.meta.url));
const repoRoot = resolve(webDir, "..");
const sharedDir = join(repoRoot, "shared");
const distDir = join(webDir, "dist");

await rm(distDir, { recursive: true, force: true });
await mkdir(distDir, { recursive: true });

await build({
    entryPoints: [
        join(webDir, "script.ts"),
        join(webDir, "harness.ts"),
    ],
    outdir: distDir,
    bundle: true,
    format: "esm",
    target: "es2022",
    sourcemap: "linked",
    logLevel: "info",
});

for (const file of ["index.html", "harness.html", "style.css", "rdview.json"]) {
    await cp(join(webDir, file), join(distDir, file));
}

await cp(join(sharedDir, "Assets"), join(distDir, "Assets"), {
    recursive: true,
});
await cp(join(webDir, "fonts"), join(distDir, "fonts"), {
    recursive: true,
});

console.log(`Built ${distDir}`);
