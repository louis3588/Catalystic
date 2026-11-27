const { spawn, spawnSync } = require("node:child_process");
const fs = require("node:fs");
const os = require("node:os");
const path = require("node:path");

const executable = process.platform === "win32" ? "dotnet.exe" : "dotnet";
const candidates = [
  "dotnet",
  process.env.DOTNET_ROOT && path.join(process.env.DOTNET_ROOT, executable),
  path.join(os.homedir(), ".dotnet", executable),
].filter(Boolean);

const dotnet = candidates.find((candidate) => {
  if (candidate !== "dotnet" && !fs.existsSync(candidate)) return false;
  const result = spawnSync(candidate, ["--list-sdks"], { encoding: "utf8" });
  return result.status === 0 && /^10\./m.test(result.stdout);
});

if (!dotnet) {
  console.error("A .NET 10 SDK was not found. Install it or set DOTNET_ROOT to its installation directory.");
  process.exit(1);
}

const backend = spawn(dotnet, ["run", "--launch-profile", "https"], {
  cwd: path.resolve(__dirname, "../../CatalysticBackend"),
  stdio: "inherit",
  env: path.isAbsolute(dotnet)
    ? { ...process.env, DOTNET_ROOT: path.dirname(dotnet) }
    : process.env,
});

backend.on("error", (error) => {
  console.error(`Unable to start the backend: ${error.message}`);
  process.exitCode = 1;
});
backend.on("exit", (code, signal) => {
  process.exitCode = signal ? 1 : (code ?? 1);
});

for (const signal of ["SIGINT", "SIGTERM"]) {
  process.on(signal, () => {
    if (!backend.killed) backend.kill(signal);
  });
}
