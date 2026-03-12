import { readdir, readFile } from "node:fs/promises";
import path from "node:path";

const repoRoot = process.cwd();
const modulesRoot = path.join(repoRoot, "apps", "backend", "Operis_API", "Modules");

async function walk(dir) {
  const entries = await readdir(dir, { withFileTypes: true });
  const files = await Promise.all(
    entries.map(async (entry) => {
      const fullPath = path.join(dir, entry.name);
      if (entry.isDirectory()) {
        return walk(fullPath);
      }
      return entry.isFile() && fullPath.endsWith(".cs") ? [fullPath] : [];
    }),
  );

  return files.flat();
}

const files = await walk(modulesRoot);
const violations = [];

for (const file of files) {
  if (!file.endsWith("Module.cs")) {
    continue;
  }

  const source = await readFile(file, "utf8");
  const relPath = path.relative(repoRoot, file);

  if (source.includes("MapGet(\"/\", async (") || source.includes("MapPost(\"/\", async (") || source.includes("MapPut(\"/\", async (") || source.includes("MapDelete(\"/\", async (")) {
    violations.push(`${relPath}: inline endpoint lambda found; route handlers should delegate to named methods or application services.`);
  }

  const hasApplicationFolder = files.some((candidate) => candidate.startsWith(path.join(path.dirname(file), "Application")));
  const mapsEndpoints = /Map(Get|Post|Put|Delete)\(/.test(source);
  if (mapsEndpoints && !hasApplicationFolder) {
    violations.push(`${relPath}: module exposes endpoints but has no Application layer folder.`);
  }
}

if (violations.length > 0) {
  console.error("Backend architecture violations found:");
  for (const violation of violations) {
    console.error(`- ${violation}`);
  }
  process.exit(1);
}

console.log(`Backend architecture check passed for ${files.filter((file) => file.endsWith("Module.cs")).length} modules.`);
