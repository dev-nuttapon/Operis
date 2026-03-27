import fs from "node:fs";
import path from "node:path";
import process from "node:process";

const distAssetsPath = path.join(process.cwd(), "dist", "assets");

const budgetsKb = new Map([
  ["antd-core-vendor", 1150],
  ["react-vendor", 240],
  ["index", 276],
  ["AdminUsersPage", 26],
  ["ActivityLogsPage", 15],
  ["DocumentDashboardPage", 8],
  ["WorkflowDefinitionsPage", 6],
  ["PublicRegistrationPage", 10],
  ["InvitationAcceptPage", 8],
  ["RegistrationPasswordSetupPage", 7],
]);

if (!fs.existsSync(distAssetsPath)) {
  console.error("dist/assets not found. Run a build first.");
  process.exit(1);
}

const assetFiles = fs.readdirSync(distAssetsPath).filter((file) => file.endsWith(".js"));
const violations = [];

for (const [prefix, budgetKb] of budgetsKb.entries()) {
  const assetFile = assetFiles.find((file) => file.startsWith(`${prefix}-`));
  if (!assetFile) {
    violations.push(`Missing bundle for budget target '${prefix}'.`);
    continue;
  }

  const sizeBytes = fs.statSync(path.join(distAssetsPath, assetFile)).size;
  const sizeKb = sizeBytes / 1024;
  const deltaKb = sizeKb - budgetKb;

  if (sizeKb > budgetKb) {
    violations.push(
      `${assetFile} is ${sizeKb.toFixed(2)} kB, exceeding budget ${budgetKb.toFixed(2)} kB by ${deltaKb.toFixed(2)} kB.`,
    );
  } else {
    console.log(`${assetFile}: ${sizeKb.toFixed(2)} kB / ${budgetKb.toFixed(2)} kB budget`);
  }
}

if (violations.length > 0) {
  console.error("Bundle budget check failed:");
  for (const violation of violations) {
    console.error(`- ${violation}`);
  }

  process.exit(1);
}

console.log("Bundle budget check passed.");
