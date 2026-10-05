// Launch adapter: the built-in node debugger starts this file, which runs a shell script.
// Stopping the session terminates that script and the dotnet build it started.
import { spawn } from "node:child_process";

const [command, ...args] = process.argv.slice(2);
if (!command) {
  console.error("Usage: run-script.mjs <script> [args...]");
  process.exit(1);
}

const child = spawn(command, args, { stdio: "inherit" });

function stop() {
  if (child.exitCode === null && child.signalCode === null) {
    child.kill("SIGTERM");
  }
}

process.on("SIGTERM", stop);
process.on("SIGINT", stop);

child.on("error", (error) => {
  console.error(error.message);
  process.exit(1);
});

child.on("exit", (code) => {
  process.exit(code ?? 1);
});
