const fs = require("node:fs");
const archiver = require("archiver");
const path = require("node:path");

const PACKAGE_JSON = path.join(__dirname, "../", "package.json");
const packageJson = JSON.parse(fs.readFileSync(PACKAGE_JSON, "utf8"));

const PACKAGE_NAME = packageJson.name;
const PACKAGE_VERSION = packageJson.version;

const BUILD_DIR = path.join(__dirname, "build");
if (!fs.existsSync(BUILD_DIR)) {
    fs.mkdirSync(BUILD_DIR);
}

const ZIP_FILE_NAME = path.join(BUILD_DIR, `${PACKAGE_NAME}-v${PACKAGE_VERSION}.zip`);
if (fs.existsSync(ZIP_FILE_NAME)) {
    fs.unlinkSync(ZIP_FILE_NAME);
}

const output = fs.createWriteStream(ZIP_FILE_NAME);
const archive = archiver("zip", { zlib: { level: 9 } });

archive.glob("**/*", {
    cwd: path.join(__dirname, "../"),
    ignore: ["Build~/**"]
});

output.on("close", () => {
    console.log(`ZIP作成完了: ${archive.pointer()} total bytes`);
});

archive.on("error", err => { throw err; });

archive.pipe(output);

archive.finalize();
