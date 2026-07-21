# Monopoly Prototype Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a playable 2D top-down Monopoly-like prototype with pass and stop building interactions.

**Architecture:** Keep gameplay rules in a pure C# resolver covered by EditMode tests, and keep Unity scene behavior in small MonoBehaviours. Add one scene bootstrapper object that builds the prototype board and UI at play time.

**Tech Stack:** Unity 6000.3.9f1, URP, Unity Test Framework, UnityEngine.UI.

## Global Constraints

- Use the existing `Assets/Scenes/SampleScene.unity`.
- First version only logs interactions; it does not implement money, purchasing, rent, turns, multiplayer, or popups.
- Use 2D top-down presentation.
- Keep code small and prototype-focused.

---

### Task 1: Core Movement Rules

**Files:**
- Create: `Assets/Scripts/MonopolyPrototype/BoardMoveResolver.cs`
- Test: `Assets/Tests/EditMode/BoardMoveResolverTests.cs`

**Interfaces:**
- Produces: `BoardMoveResolver.ResolveMove(IReadOnlyList<BoardMoveResolver.TileDefinition> tiles, int startIndex, int steps)` returning `BoardMoveResolver.MoveResult`.
- Produces: `TileDefinition(string name, string passLog, string stopLog)`.

- [x] **Step 1: Write failing EditMode tests**
- [x] **Step 2: Run tests and verify they fail because the resolver is missing**
- [x] **Step 3: Implement the minimal resolver**
- [x] **Step 4: Run tests and verify they pass**

### Task 2: Scene Runtime Components

**Files:**
- Create: `Assets/Scripts/MonopolyPrototype/BoardTile.cs`
- Create: `Assets/Scripts/MonopolyPrototype/PlayerToken.cs`
- Create: `Assets/Scripts/MonopolyPrototype/GameLogView.cs`
- Create: `Assets/Scripts/MonopolyPrototype/BoardController.cs`

**Interfaces:**
- Consumes: `BoardMoveResolver.ResolveMove(...)`.
- Produces: scene components that can be wired to tile transforms, token transform, a roll button, and a UI text log.

- [x] **Step 1: Add MonoBehaviours for tiles, token movement, log view, and board flow**
- [x] **Step 2: Keep board flow deterministic except for dice value generation**
- [x] **Step 3: Guard against empty boards and double-click movement**

### Task 3: Prototype Scene

**Files:**
- Create: `Assets/Scripts/MonopolyPrototype/PrototypeBootstrapper.cs`
- Modify: `Assets/Scenes/SampleScene.unity`

**Interfaces:**
- Consumes: runtime components from Task 2.
- Produces: a saved scene containing a bootstrapper that creates a 2D board, token, roll button, and log when Play starts.

- [x] **Step 1: Add a runtime bootstrapper that creates the prototype scene objects**
- [x] **Step 2: Attach the bootstrapper to `SampleScene.unity`**
- [x] **Step 3: Verify scene file references the bootstrapper script GUID**

### Task 4: Verification

**Files:**
- Verify: Unity EditMode tests
- Verify: Unity script compilation through test run

- [x] **Step 1: Run EditMode tests**
- [x] **Step 2: Inspect git diff for intended files only**
- [x] **Step 3: Report result and any verification limitations**
