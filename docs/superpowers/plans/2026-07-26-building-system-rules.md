# Building System Rules Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the first pass of a ScriptableObject-authored building system whose core rules stay pure C# and output presentation-agnostic effect commands.

**Architecture:** Building assets are Unity authoring data only. Runtime and tests use pure definitions: building trigger modes, effect definitions, and resolved effect commands. Board movement remains responsible for path timing; a building resolver receives pass/stop timing and returns add-money, subtract-money, teleport, confirmation, and feedback commands.

**Tech Stack:** Unity 6000.3.9f1, C#, ScriptableObject authoring, EditMode tests, existing `MonopolyPrototype` asmdefs.

## Global Constraints

- Runtime prototype code stays under `Assets/Scripts/MonopolyPrototype`.
- EditMode tests stay under `Assets/Tests/EditMode`.
- Core gameplay rules must be pure C# where practical and testable without Play Mode.
- ScriptableObjects may describe building data, but core rules must not depend on UI or MonoBehaviour listeners.
- Every important gameplay logic change updates `docs/game-logic/logic-overview.md`.

---

### Task 1: Pure Building Rule Model

**Files:**
- Create: `Assets/Scripts/MonopolyPrototype/BuildingRules.cs`
- Create: `Assets/Tests/EditMode/BuildingRuleResolverTests.cs`

**Interfaces:**
- Produces: `BuildingTriggerMode`, `BuildingEffectType`, `BuildingEffectDefinition`, `BuildingDefinition`, `BuildingEffectCommand`, `BuildingRuleResolver.Resolve(...)`.

- [ ] **Step 1: Write failing tests**

Test that pass/stop trigger modes select the right buildings and that multiple effects preserve order.

- [ ] **Step 2: Verify the tests fail because the rule types do not exist**

Run EditMode tests or Unity script compile. Expected failure: missing `BuildingRuleResolver` and related types.

- [ ] **Step 3: Implement minimal pure rule types**

Add pure C# definitions and a resolver that converts matching building effects into commands.

- [ ] **Step 4: Verify tests pass or script compilation succeeds when Unity tests are blocked**

Run EditMode tests when Unity can open batchmode; otherwise use Unity editor compile output and state the limitation.

### Task 2: ScriptableObject Authoring Layer

**Files:**
- Create: `Assets/Scripts/MonopolyPrototype/BuildingConfig.cs`
- Create: `Assets/Tests/EditMode/BuildingConfigTests.cs`

**Interfaces:**
- Consumes: `BuildingDefinition` and `BuildingEffectDefinition`.
- Produces: `BuildingConfig.ToDefinition()`.

- [ ] **Step 1: Write failing conversion tests**

Test that a configured building asset converts to a pure definition with trigger mode and effect list intact.

- [ ] **Step 2: Verify the tests fail because `BuildingConfig` does not exist**

Run EditMode tests or Unity script compile.

- [ ] **Step 3: Implement `BuildingConfig` as authoring-only data**

Use serialized fields for name, trigger mode, and effect configs. Convert to pure definitions without returning Unity UI objects or listeners.

- [ ] **Step 4: Verify conversion tests pass or script compilation succeeds when Unity tests are blocked**

Use the same verification path as Task 1.

### Task 3: Route Integration And Documentation

**Files:**
- Modify: `Assets/Scripts/MonopolyPrototype/BoardTile.cs`
- Modify: `Assets/Scripts/MonopolyPrototype/PrototypeBoardRoute.cs`
- Modify: `Assets/Scripts/MonopolyPrototype/BoardMoveResolver.cs`
- Modify: `docs/game-logic/logic-overview.md`
- Modify: existing tests as needed.

**Interfaces:**
- Consumes: `BuildingDefinition`.
- Produces: board tile definitions that can carry optional building definitions while keeping movement and building effects testable.

- [ ] **Step 1: Write failing integration tests**

Test that prototype route tiles expose building definitions and that movement timing can be used to resolve building effects.

- [ ] **Step 2: Implement minimal integration**

Extend tile specs/definitions to carry optional building data. Keep existing facility interaction behavior until it can be replaced intentionally.

- [ ] **Step 3: Update `logic-overview.md`**

Document the new building authoring layer, pure rule layer, command output, and UI boundary.

- [ ] **Step 4: Run verification and commit**

Run compile/tests, then commit the code, tests, meta files, plan, and docs together.
