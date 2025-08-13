# Coding Agent Scaffold

This repo is scaffolded for a patch-only, containerized agent workflow.

## Quick start
- `dotnet build tools/agent/AgentRunner.csproj`
- `powershell -ExecutionPolicy Bypass -File tools/scripts/agent.ps1 run build`
- `powershell -ExecutionPolicy Bypass -File tools/scripts/agent.ps1 run test`
- Copy a unified diff to clipboard and run **Agent: Apply Diff From Clipboard** VS Code task.

## Notes
- Writes are restricted by `.agent/config.json` (allow/deny globs).
- The agent only edits via **unified diffs**.
- All runs are executed inside the `mcr.microsoft.com/dotnet/sdk:8.0` container for consistency.