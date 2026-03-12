import fs from "node:fs";
import path from "node:path";
import process from "node:process";

const distAssetsPath = path.join(process.cwd(), "dist", "assets");
const warningThresholdKb = 500;

if (!fs.existsSync(distAssetsPath)) {
  console.error("dist/assets not found. Run a build first.");
  process.exit(1);
}

const files = fs.readdirSync(distAssetsPath)
  .filter((file) => file.endsWith(".js"))
  .map((file) => {
    const fullPath = path.join(distAssetsPath, file);
    const sizeBytes = fs.statSync(fullPath).size;
    return {
      file,
      sizeBytes,
      sizeKb: sizeBytes / 1024,
    };
  })
  .sort((left, right) => right.sizeBytes - left.sizeBytes);

const topFiles = files.slice(0, 10);
const oversizedFiles = files.filter((file) => file.sizeKb >= warningThresholdKb);

console.log("Top JS bundles:");
for (const file of topFiles) {
  console.log(`- ${file.file}: ${file.sizeKb.toFixed(2)} kB`);
}

if (oversizedFiles.length === 0) {
  console.log(`No JS bundles over ${warningThresholdKb} kB.`);
  process.exit(0);
}

console.log(`\nBundles over ${warningThresholdKb} kB:`);
for (const file of oversizedFiles) {
  console.log(`- ${file.file}: ${file.sizeKb.toFixed(2)} kB`);
}
