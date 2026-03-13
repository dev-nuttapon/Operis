import { readdir, readFile } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

const scriptDirectory = path.dirname(fileURLToPath(import.meta.url));
const repoRoot = path.resolve(scriptDirectory, "..");
const contractsDocPath = path.join(repoRoot, "docs", "MODULE_CONTRACTS.md");
const frontendModulesRoot = path.join(repoRoot, "apps", "frontend", "src", "modules");
const backendModulesRoot = path.join(repoRoot, "apps", "backend", "Operis_API", "Modules");

const contractsDoc = await readFile(contractsDocPath, "utf8");
const violations = [];

async function listDirectories(rootPath) {
  const entries = await readdir(rootPath, { withFileTypes: true });
  return entries
    .filter((entry) => entry.isDirectory() && !entry.name.startsWith("."))
    .map((entry) => entry.name)
    .sort((left, right) => left.localeCompare(right));
}

function requireDocIncludes(description, expectedText) {
  if (!contractsDoc.includes(expectedText)) {
    violations.push(`${description}: missing '${expectedText}' in docs/MODULE_CONTRACTS.md.`);
  }
}

const frontendModules = await listDirectories(frontendModulesRoot);
for (const moduleName of frontendModules) {
  requireDocIncludes(`frontend module ${moduleName} section`, `## ${moduleName}`);
  requireDocIncludes(
    `frontend module ${moduleName} manifest`,
    `/Users/nuttapon/Github-dev/Operis/apps/frontend/src/modules/${moduleName}/README.md`,
  );
}

const backendModules = await listDirectories(backendModulesRoot);
for (const moduleName of backendModules) {
  const sectionName = moduleName.toLowerCase();
  requireDocIncludes(`backend module ${moduleName} section`, `## ${sectionName}`);
  requireDocIncludes(
    `backend module ${moduleName} manifest`,
    `/Users/nuttapon/Github-dev/Operis/apps/backend/Operis_API/Modules/${moduleName}/README.md`,
  );
}

if (violations.length > 0) {
  console.error("Module contracts violations found:");
  for (const violation of violations) {
    console.error(`- ${violation}`);
  }
  process.exit(1);
}

console.log(`Module contracts check passed for ${frontendModules.length} frontend modules and ${backendModules.length} backend modules.`);
