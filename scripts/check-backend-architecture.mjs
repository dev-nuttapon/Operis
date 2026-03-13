import { readdir, readFile, stat } from "node:fs/promises";
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

async function fileExists(targetPath) {
  try {
    const info = await stat(targetPath);
    return info.isFile();
  } catch {
    return false;
  }
}

function hasManifestSections(manifestContent, sectionTitles) {
  return sectionTitles.every((sectionTitle) => manifestContent.includes(`${sectionTitle}:`));
}

const files = await walk(modulesRoot);
const violations = [];
const forbiddenHandlerDependencies = [
  "OperisDbContext",
  "DbContext",
  "IAuditLogWriter",
  "IKeycloakAdminClient",
  "IReferenceDataCache",
];

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
  const hasContractsFolder = files.some((candidate) => candidate.startsWith(path.join(path.dirname(file), "Contracts")));
  const mapsEndpoints = /Map(Get|Post|Put|Delete)\(/.test(source);
  if (mapsEndpoints && !hasApplicationFolder) {
    violations.push(`${relPath}: module exposes endpoints but has no Application layer folder.`);
  }
  if (mapsEndpoints && !hasContractsFolder) {
    violations.push(`${relPath}: module exposes endpoints but has no Contracts layer folder.`);
  }
  const manifestPath = path.join(path.dirname(file), "README.md");
  if (mapsEndpoints && !await fileExists(manifestPath)) {
    violations.push(`${relPath}: module exposes endpoints but has no README.md module manifest.`);
  } else if (mapsEndpoints) {
    const manifestContent = await readFile(manifestPath, "utf8");
    const requiredSections = ["Purpose", "Public surface", "Owned data", "Notes"];
    if (!hasManifestSections(manifestContent, requiredSections)) {
      violations.push(`${relPath}: README.md module manifest must include sections: ${requiredSections.join(", ")}.`);
    }
  }

  if (/OperisDbContext\s+\w+/.test(source) || /DbContext\s+\w+/.test(source)) {
    violations.push(`${relPath}: module composition layer must not depend on DbContext directly. Move persistence orchestration into Application or Infrastructure services.`);
  }

  if (source.includes("SaveChangesAsync(")) {
    violations.push(`${relPath}: module composition layer must not call SaveChangesAsync directly.`);
  }

  const mapEndpointsIndex = source.indexOf("public IEndpointRouteBuilder MapEndpoints");
  if (mapEndpointsIndex >= 0) {
    const endpointSection = source.slice(mapEndpointsIndex);
    for (const dependency of forbiddenHandlerDependencies) {
      const pattern = new RegExp(`\\b${dependency}\\s+\\w+`, "g");
      if (pattern.test(endpointSection)) {
        violations.push(`${relPath}: endpoint composition must not depend on ${dependency} directly. Delegate through Application services.`);
      }
    }
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
