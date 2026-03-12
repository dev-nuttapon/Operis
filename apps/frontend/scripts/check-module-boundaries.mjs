import fs from "node:fs";
import path from "node:path";
import process from "node:process";

const projectRoot = process.cwd();
const srcRoot = path.join(projectRoot, "src");
const modulesRoot = path.join(srcRoot, "modules");
const allowedPublicEntries = new Set(["index.ts", "index.tsx", "public.ts", "public.tsx"]);
const sourceExtensions = [".ts", ".tsx", ".mts", ".cts", ".js", ".jsx", ".mjs", ".cjs"];
const ignoredDirectories = new Set(["dist", "coverage", "node_modules"]);

function walk(directory) {
  const entries = fs.readdirSync(directory, { withFileTypes: true });
  const files = [];

  for (const entry of entries) {
    if (entry.name.startsWith(".")) {
      continue;
    }

    const fullPath = path.join(directory, entry.name);
    if (entry.isDirectory()) {
      if (ignoredDirectories.has(entry.name)) {
        continue;
      }

      files.push(...walk(fullPath));
      continue;
    }

    if (!sourceExtensions.includes(path.extname(entry.name))) {
      continue;
    }

    if (entry.name.endsWith(".test.ts") || entry.name.endsWith(".test.tsx") || entry.name.endsWith(".spec.ts") || entry.name.endsWith(".spec.tsx")) {
      continue;
    }

    files.push(fullPath);
  }

  return files;
}

function extractImports(content) {
  const imports = [];
  const pattern = /\bimport\s+(?:type\s+)?(?:[^"'`]+\s+from\s+)?["'`]([^"'`]+)["'`]/g;

  for (const match of content.matchAll(pattern)) {
    imports.push(match[1]);
  }

  return imports;
}

function fileExists(candidate) {
  try {
    return fs.statSync(candidate).isFile();
  } catch {
    return false;
  }
}

function directoryExists(candidate) {
  try {
    return fs.statSync(candidate).isDirectory();
  } catch {
    return false;
  }
}

function resolveImport(fromFile, specifier) {
  if (!specifier.startsWith(".")) {
    return null;
  }

  const basePath = path.resolve(path.dirname(fromFile), specifier);
  const candidates = [
    basePath,
    ...sourceExtensions.map((extension) => `${basePath}${extension}`),
    ...sourceExtensions.map((extension) => path.join(basePath, `index${extension}`)),
    ...sourceExtensions.map((extension) => path.join(basePath, `public${extension}`)),
  ];

  for (const candidate of candidates) {
    if (fileExists(candidate)) {
      return candidate;
    }
  }

  if (directoryExists(basePath)) {
    for (const entryName of allowedPublicEntries) {
      const candidate = path.join(basePath, entryName);
      if (fileExists(candidate)) {
        return candidate;
      }
    }
  }

  return null;
}

function getModuleInfo(filePath) {
  const relativePath = path.relative(modulesRoot, filePath);
  if (relativePath.startsWith("..")) {
    return null;
  }

  const [moduleName] = relativePath.split(path.sep);
  if (!moduleName) {
    return null;
  }

  return {
    moduleName,
    relativePath,
  };
}

function isAllowedPublicTarget(targetFile) {
  return allowedPublicEntries.has(path.basename(targetFile));
}

const sourceFiles = walk(srcRoot);
const violations = [];

for (const sourceFile of sourceFiles) {
  const sourceContent = fs.readFileSync(sourceFile, "utf8");
  const sourceModule = getModuleInfo(sourceFile);

  for (const specifier of extractImports(sourceContent)) {
    const targetFile = resolveImport(sourceFile, specifier);
    if (!targetFile) {
      continue;
    }

    const targetModule = getModuleInfo(targetFile);
    if (!targetModule) {
      continue;
    }

    if (sourceModule && sourceModule.moduleName === targetModule.moduleName) {
      continue;
    }

    if (isAllowedPublicTarget(targetFile)) {
      continue;
    }

    const sourceLabel = path.relative(projectRoot, sourceFile);
    const targetLabel = path.relative(projectRoot, targetFile);
    violations.push(`${sourceLabel} imports ${targetLabel}. Cross-module imports must use a module public API.`);
  }
}

if (violations.length > 0) {
  console.error("Module boundary violations found:\n");
  for (const violation of violations) {
    console.error(`- ${violation}`);
  }
  process.exit(1);
}

console.log(`Module boundary check passed for ${sourceFiles.length} files.`);
