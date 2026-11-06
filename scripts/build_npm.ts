import { build } from "jsr:@deno/dnt@0.44.0";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const scriptDir = dirname(fileURLToPath(import.meta.url));
const repoRoot = resolve(scriptDir, "..");
const outputDirectory = join(scriptDir, "npm");

await Deno.remove(outputDirectory, { recursive: true }).catch(() => {});
await Deno.mkdir(outputDirectory, { recursive: true });

await build({
    entryPoints: [
        { name: ".", path: join(repoRoot, "ts/mod.ts") },
        { name: "./definition", path: join(repoRoot, "ts/definition.ts") },
        { name: "./assetManager", path: join(repoRoot, "ts/assetManager.ts") },
        { name: "./view", path: join(repoRoot, "ts/view.ts") },
    ],
    outDir: outputDirectory,
    scriptModule: false,
    declaration: "inline",
    test: false,
    skipNpmInstall: true,
    shims: { deno: false },
    compilerOptions: {
        target: "ES2022",
        lib: ["ES2022", "DOM", "DOM.Iterable"],
        strict: false,
    },
    package: {
        name: "rhythmbase-view",
        version: "1.0.0",
        description: "Render Rhythm Doctor event icons in the browser.",
        license: "LGPL-3.0-only",
        repository: {
            type: "git",
            url: "git+https://github.com/OLDRedstone/RhythmBase.RhythmDoctor.View.git",
        },
    },
});

const packagePath = join(outputDirectory, "package.json");
const packageJson = JSON.parse(await Deno.readTextFile(packagePath));
packageJson.files = ["esm", "Assets", "README.md", "LICENSE.txt"];
await Deno.writeTextFile(packagePath, `${JSON.stringify(packageJson, null, 2)}\n`);

await Deno.mkdir(join(outputDirectory, "Assets"), { recursive: true });
for (const file of ["assets", "assets.png"]) {
    await Deno.copyFile(
        join(repoRoot, "shared/Assets", file),
        join(outputDirectory, "Assets", file),
    );
}
await Deno.copyFile(join(repoRoot, "ts/README.md"), join(outputDirectory, "README.md"));
await Deno.copyFile(join(repoRoot, "LICENSE.txt"), join(outputDirectory, "LICENSE.txt"));

console.log(`npm package written to ${outputDirectory}`);
console.log("Inspect it with: npm publish --dry-run ./scripts/npm");
console.log("Publish it with: npm publish ./scripts/npm");
