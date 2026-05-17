# AGENTS.md

This file provides guidance to Codex (Codex.ai/code) when working with code in this repository.

## Project Overview

**minigame-project** is a Unity game project in early development stage, using low-poly character assets from Polyperfect for prototyping and gameplay mechanics exploration.

## Environment

- **Unity Version**: 6.0.3 (6000.3.15f1)
- **Render Pipeline**: Universal Render Pipeline (URP) 17.3.0
- **Input System**: New Input System 1.19.0
- **C# Version**: .NET Framework compatible with Unity 6

## Project Structure

```
Assets/
├── Scenes/
│   └── SampleScene.unity          # Main gameplay scene
├── Settings/                       # Project-specific configurations
├── Imported/                       # Third-party assets
│   └── polyperfect/              # Polyperfect Low Poly Animated People asset
│       ├── Low Poly Animated People/
│       └── Common/
└── TutorialInfo/                  # README and tutorial assets
```

## Key Dependencies

- **com.unity.ai.navigation** (2.0.12) — AI navigation for character pathfinding
- **com.unity.inputsystem** (1.19.0) — New Input System for input handling
- **com.unity.render-pipelines.universal** (17.3.0) — URP for rendering
- **com.unity.test-framework** (1.6.0) — Unit testing support
- **com.unity.timeline** (1.8.12) — Animation and cinematic support

## Building and Running

### Editor Play Mode
1. Open the project in Unity 6.0.3
2. Load `Assets/Scenes/SampleScene.unity`
3. Press **Play** in the Editor

### Build for Standalone
```bash
# Build is configured in Unity Project Settings > Build
# Use Editor: File > Build Settings > Build
```

## Code Style and Conventions

- **Language**: C# (.NET Framework compatible)
- **Formatter**: CSharpier (auto-applied via PostToolUse hook)
- **Naming**:
  - PascalCase for classes, methods, properties
  - camelCase for local variables and parameters
  - `_field` prefix for private serialized fields (if using Inspector)
- **No file restrictions** on scripts in `Assets/Scripts/` or custom folders
  - **Do not edit** auto-generated IDE project files (`.csproj`, `.sln`)
  - **Do not edit** scene files (`.unity`) directly — use Unity MCP or Editor UI instead
  - **Do not edit** metadata files (`.meta`)

## Unity-Specific Notes

### Scene Management
- Main gameplay happens in `Assets/Scenes/SampleScene.unity`
- Use Unity MCP (when connected) to query/modify scenes programmatically from Codex

### Asset References
- Character models and animations come from **Polyperfect** asset package
- Keep imported assets in `Assets/Imported/` — do not modify them
- Create new gameplay scripts in `Assets/Scripts/` or a dedicated project folder

### Input System
- Project uses the **New Input System** (not the legacy Input Manager)
- Input actions are configured in `Assets/InputSystem_Actions.inputactions`

## Common Tasks

### Run Tests
```bash
# Tests are configured in com.unity.test-framework
# Run via Unity Editor: Window > TextExecution > Test Runner
```

### Code Formatting
After editing `.cs` files, the CSharpier hook automatically formats code on save (via `.Codex/hooks/csharpier-format.sh`).

### Connect Unity MCP
To interact with the running Unity Editor from Codex:
1. Open **Edit > Project Settings > AI > Unity MCP**
2. Ensure **Unity Bridge** is **Running** (green)
3. Click **Configure** next to Codex
4. In a Codex terminal, accept the pending connection
5. Use tools like `Unity_ReadConsole`, `Unity_ManageScene`, `Unity_ManageGameObject`

## Git Workflow

- `.gitignore` excludes Unity auto-generated folders (`Library/`, `Temp/`, `Logs/`, `obj/`, `Build/`)
- `.Codex/settings.json` denies read/write access to meta files, scene files, and generated project files
- Commit changes to C# scripts, scenes (via changes only), and configuration files
- Use `git status` to verify only intended files are staged

## Next Steps

1. Define core gameplay mechanics in new scripts under `Assets/Scripts/`
2. Expand `SampleScene.unity` with interactive elements and game flow
3. Configure character controllers and input handling
4. Integrate AI navigation for NPC behavior (if needed)

