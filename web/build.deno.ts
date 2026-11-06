import { build } from "npm:esbuild@0.25.0";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const webDir = dirname(fileURLToPath(import.meta.url));
const repoRoot = resolve(webDir, "..");
const sharedDir = join(repoRoot, "shared");
const distDir = join(webDir, "dist");

await Deno.remove(distDir, { recursive: true }).catch(() => {});
await Deno.mkdir(distDir, { recursive: true });

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
    await Deno.copyFile(join(webDir, file), join(distDir, file));
}

async function copyDirectory(source: string, destination: string) {
    await Deno.mkdir(destination, { recursive: true });
    for await (const entry of Deno.readDir(source)) {
        const sourcePath = join(source, entry.name);
        const destinationPath = join(destination, entry.name);
        if (entry.isDirectory) {
            await copyDirectory(sourcePath, destinationPath);
        } else {
            await Deno.copyFile(sourcePath, destinationPath);
        }
    }
}

await copyDirectory(join(sharedDir, "Assets"), join(distDir, "Assets"));
await copyDirectory(join(webDir, "fonts"), join(distDir, "fonts"));

console.log(`Built ${distDir}`);
