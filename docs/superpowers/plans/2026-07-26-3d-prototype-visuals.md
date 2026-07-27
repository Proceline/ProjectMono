# 3D Prototype Visuals Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the runtime-generated 2D placeholder board with a simple playable desktop 3D presentation made from Unity built-in primitive meshes while preserving the existing map, rule, effect, and controller contracts.

**Architecture:** Keep `PrototypeMapData`, `BuildingConfig`, pure rule resolution, `BuildingEventBridge`, and `BoardController` unchanged at the gameplay boundary. Add a presentation-only style mapping, a replaceable `PrototypeBoardTile` prefab, and a `Prototype3DBoardView` that instantiates the prefab in map order beneath each logical `BoardTile`; update only camera setup and token motion in the bootstrapper/presentation layer.

**Tech Stack:** Unity 6000.3.9f1, URP, built-in `PrimitiveType` meshes, `Renderer.material.color`, legacy `TextMesh`/uGUI already present in the project, NUnit EditMode tests.

## Global Constraints

- Work in the existing checkout on a normal `codex/` branch; do not create a worktree.
- Desktop/landscape is the only supported presentation target for this unit.
- Do not add `BuildingVisualProfile`, mobile layouts, Money state, or Teleport state semantics in this unit.
- Do not add SOEvent dependencies to pure rule or building-command types.
- `PrototypeMapData` remains the source of truth for grid coordinates and route order.
- Visual objects must remain replaceable by keeping them below the `BoardTile` logical root, using a prefab asset, and isolating construction in a presentation view.
- Do not edit Unity-generated `Library`, `Temp`, `Logs`, `UserSettings`, `obj`, `.csproj`, or `.sln` files.

---

### Task 1: Define and test the 3D presentation style contract

**Files:**
- Create: `Assets/Scripts/MonopolyPrototype/Prototype3DVisualStyle.cs`
- Create: `Assets/Scripts/MonopolyPrototype/Prototype3DVisualStyle.cs.meta`
- Create: `Assets/Tests/EditMode/Prototype3DVisualStyleTests.cs`
- Create: `Assets/Tests/EditMode/Prototype3DVisualStyleTests.cs.meta`

**Interfaces:**
- Produces a presentation-only `Prototype3DVisualStyle.For(BuildingDefinition building)` value containing tile color, marker color, primitive type, marker scale, and whether a marker exists.
- Blank/null definitions produce no marker and a neutral tile style.
- Known prototype building names receive deterministic primitive/color styles; unknown buildings receive a safe cube fallback.

- [x] Write EditMode tests for blank fallback, Start cylinder styling, Park sphere styling, and unknown-building cube fallback.
- [x] Run the focused EditMode test command and verify the new tests fail because the production style type does not exist.
- [x] Implement only the style value and mapping needed by the failing tests.
- [x] Run the focused tests and verify they pass before moving to board construction.
- [x] Commit the style contract and tests with `test: define 3d prototype visual styles`.

### Task 2: Add the replaceable primitive-based board view

**Files:**
- Create: `Assets/Scripts/MonopolyPrototype/Prototype3DBoardView.cs`
- Create: `Assets/Scripts/MonopolyPrototype/Prototype3DBoardView.cs.meta`
- Create: `Assets/Scripts/MonopolyPrototype/PrototypeBoardTileView.cs`
- Create: `Assets/Scripts/MonopolyPrototype/PrototypeBoardTileView.cs.meta`
- Create: `Assets/Prefabs/PrototypeBoardTile.prefab`
- Create: `Assets/Prefabs/PrototypeBoardTile.prefab.meta`

**Interfaces:**
- `Prototype3DBoardView.Build(PrototypeMapData mapData, Vector2 boardCenter, Vector2 tileSpacing, float tileScale, PrototypeBoardTileView tilePrefab)` returns the ordered `IReadOnlyList<BoardTile>` consumed by the existing controller.
- Each returned `BoardTile` is the root of one instantiated `PrototypeBoardTile` prefab at the map position; the platform remains a separate presentation object.
- The view validates the map before creating route objects and logs validation errors at the presentation boundary.

- [x] Create the board platform as a cube using map bounds from `PrototypeMapLayout`.
- [x] Create `PrototypeBoardTile.prefab` from a root `BoardTile` plus `PrototypeBoardTileView`, a built-in Cube tile surface, marker anchor, and world-space label.
- [x] Instantiate one tile prefab per map route item, position it from the existing grid layout, and preserve map path order and `BoardTile.Configure(...)` data.
- [x] Create deterministic built-in primitive markers from `Prototype3DVisualStyle`, with no ScriptableObject visual references.
- [x] Add readable world-space `TextMesh` labels without changing the pure definition or controller route.
- [x] Keep prefab configuration and all runtime marker construction inside presentation components so a future art prefab can replace it without changing rule code.
- [x] Run the focused style tests and the existing core EditMode tests after the new view compiles.
- [x] Commit with `feat: add replaceable primitive 3d board view`.

### Task 3: Switch runtime composition and camera to the 3D presentation

**Files:**
- Modify: `Assets/Scripts/MonopolyPrototype/PrototypeBootstrapper.cs`

**Interfaces:**
- Existing `BoardController.Configure(...)` arguments remain unchanged.
- `PrototypeBootstrapper` delegates board creation to an optional serialized `Prototype3DBoardView`, creating a runtime fallback component when the scene does not provide one.
- Camera setup uses the existing map dimensions/layout values and frames the 3D board with a desktop-friendly perspective camera.

- [x] Replace the 2D sprite tile construction call path with `Prototype3DBoardView.Build(...)`.
- [x] Preserve the existing runtime UI creation and roll/confirmation wiring.
- [x] Configure the camera to use a perspective view, a near-top-down direction with a small tilt, `cameraFieldOfView`, and a board-size-based distance with padding.
- [x] Assign the tile prefab in `SampleScene` while keeping the existing scene camera and Directional Light compatible.
- [x] Run a script compile check and the focused EditMode tests.
- [x] Commit with `feat: wire prototype bootstrapper to 3d presentation`.

### Task 4: Make the token read as a simple 3D pawn

**Files:**
- Modify: `Assets/Scripts/MonopolyPrototype/PlayerToken.cs`
- Modify: `Assets/Scripts/MonopolyPrototype/PrototypeBootstrapper.cs`

**Interfaces:**
- `PlayerToken.SnapTo(...)` and `PlayerToken.MoveTo(...)` remain the only controller-facing movement methods.
- Token construction uses a built-in Capsule or equivalent primitive and keeps its visual state outside `BoardTile`.

- [x] Change the default token offset to a 3D vertical offset above the tile.
- [x] Add a small visual hop to `MoveTo(...)` without changing movement duration, route order, or confirmation timing.
- [x] Build the token as a colored built-in Capsule primitive; keep the MVP free of an extra shadow object.
- [x] Run compile and EditMode tests; verify no gameplay rule tests changed.
- [x] Commit with `feat: present player token as 3d pawn`.

### Task 5: Verify the playable 3D unit and hand off

**Files:**
- Inspect: `Assets/Scenes/SampleScene.unity`
- Inspect: `Assets/Scripts/MonopolyPrototype/BoardController.cs`
- Inspect: `Assets/Scripts/MonopolyPrototype/PrototypeMapData.cs`
- Inspect: `docs/game-logic/logic-overview.md`

- [x] Run the complete available EditMode/core test suite.
- [x] Run a Unity batchmode compile or project script compile check; if Unity is blocked by an open Editor, record the exact limitation and use the available fallback.
- [x] Verify the final diff contains no changes to pure resolver, building effect, SOEvent, or map data behavior.
- [x] Inspect branch status and the generated asset/script metadata for all new files.
- [x] Commit the verified implementation with `feat: instantiate 3d board tiles from prefabs`.
- [x] Report the exact verification evidence, changed files, branch name, and any Unity Editor limitation.
