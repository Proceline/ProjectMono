# Full Building ScriptableObject Migration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace every legacy facility configuration in the prototype route with real `BuildingConfig` assets and remove the legacy runtime path.

**Architecture:** Every non-blank route tile gets a `BuildingConfig` asset listed in `PrototypeBuildingCatalog.asset`. The route keeps only names and positions; the catalog supplies building behavior. Core movement resolves only pure building definitions and emits presentation-agnostic commands, while the controller consumes those commands for logging and confirmation UI.

**Tech Stack:** Unity 6000.3.9f1, C#, ScriptableObject YAML assets, NUnit EditMode tests, Unity CLI.

## Global Constraints

- Core gameplay rules stay independent of UI and MonoBehaviour listeners.
- `docs/game-logic/logic-overview.md` must be updated in the same branch and commit series.
- Existing trigger timing, feedback text, confirmation pauses, and effect ordering must be preserved.
- `Blank` remains the only prototype tile without a building asset.
- Unity `.meta` files must remain synchronized with new Assets files.

### Task 1: Red tests for legacy removal and complete asset coverage

**Files:**
- Modify: `Assets/Tests/EditMode/BoardMoveResolverTests.cs`
- Modify: `Assets/Tests/EditMode/BoardTileBuildingTests.cs`
- Modify: `Assets/Tests/EditMode/PrototypeBoardRouteTests.cs`
- Modify: `Assets/Tests/EditMode/PrototypeBuildingAssetTests.cs`

- [x] Rewrite resolver fixtures to use `BuildingDefinition` and `BuildingEffectDefinition` only; assert pass, stop, pass-or-stop, feedback, and confirmation behavior through commands.
- [x] Update tile tests to configure only `BuildingConfig` and expect `BoardMoveResolver.TileDefinition` without legacy facility fields.
- [x] Update route tests to assert names and positions only, and assert no legacy interaction or feedback properties exist in the route contract.
- [x] Extend asset tests with all thirteen expected catalog entries and their migrated trigger/effect data.
- [x] Compile the changed tests before implementing the removal and confirm failures identify the missing new API or stale legacy references.

### Task 2: Make the pure resolver building-only

**Files:**
- Modify: `Assets/Scripts/MonopolyPrototype/BoardMoveResolver.cs`
- Modify: `Assets/Scripts/MonopolyPrototype/BuildingRules.cs` only if command semantics need coverage

- [x] Remove `FacilityInteractionType`, `FeedbackLog`, `InteractionType`, and legacy `MoveEvent.Message` data from the pure resolver.
- [x] Change `TileDefinition` to carry a tile name and optional pure `BuildingDefinition` only.
- [x] Resolve pass events for intermediate steps and stop events for the final step exclusively through `BuildingRuleResolver.Resolve`.
- [x] Emit no event when the building is absent or has no matching commands.
- [x] Make `MoveEvent.RequiresConfirmation` depend only on `RequestConfirmation` commands.
- [x] Compile the pure resolver test set through the Unity Roslyn path; runtime test execution is blocked by the already-open Unity Editor.

### Task 3: Remove legacy scene/runtime handling

**Files:**
- Modify: `Assets/Scripts/MonopolyPrototype/BoardTile.cs`
- Modify: `Assets/Scripts/MonopolyPrototype/PrototypeBoardRoute.cs`
- Modify: `Assets/Scripts/MonopolyPrototype/PrototypeBootstrapper.cs`
- Modify: `Assets/Scripts/MonopolyPrototype/BoardController.cs`

- [x] Remove legacy fields and parameters from `BoardTile`, leaving only tile name and `BuildingConfig`.
- [x] Remove legacy interaction and feedback fields from `PrototypeBoardRoute.TileSpec` and all route entries.
- [x] Look up each named asset through `PrototypeBuildingCatalog` and derive tile color from `BuildingDefinition.TriggerMode` and whether it contains `RequestConfirmation`.
- [x] Delete the old facility message/confirmation branch from `BoardController`; handle all feedback and confirmation through building commands.
- [x] Compile the runtime assembly and scan for zero remaining references to `FacilityInteractionType`, `FeedbackLog`, and the old controller branch.

### Task 4: Create the complete asset set

**Files:**
- Create: `Assets/Data/Buildings/Start.asset` and `.meta`
- Create: `Assets/Data/Buildings/Station.asset` and `.meta`
- Create: `Assets/Data/Buildings/Park.asset` and `.meta`
- Create: `Assets/Data/Buildings/Library.asset` and `.meta`
- Create: `Assets/Data/Buildings/Museum.asset` and `.meta`
- Create: `Assets/Data/Buildings/Hotel.asset` and `.meta`
- Create: `Assets/Data/Buildings/Market.asset` and `.meta`
- Create: `Assets/Data/Buildings/Clinic.asset` and `.meta`
- Create: `Assets/Data/Buildings/Theater.asset` and `.meta`
- Modify: `Assets/Data/Buildings/Bank.asset`
- Modify: `Assets/Data/Buildings/Gate.asset`
- Modify: `Assets/Data/Buildings/Shop.asset`
- Modify: `Assets/Data/Buildings/Harbor.asset`
- Modify: `Assets/Data/Buildings/PrototypeBuildingCatalog.asset`

- [x] Serialize the legacy behavior mapping into the nine new assets.
- [x] Merge the former legacy message and current building effects into the four existing assets in their previous runtime order.
- [x] Add all thirteen asset references to the catalog and keep `Blank` absent.
- [x] Verify every catalog and scene GUID resolves to a matching `.meta` file.

### Task 5: Document and verify

**Files:**
- Modify: `docs/game-logic/logic-overview.md`

- [x] Replace the legacy facility section with building trigger/effect rules and the full asset catalog source of truth.
- [x] Document that all non-blank prototype tiles are SO-authored and that `Blank` is the only empty tile.
- [x] Run `git diff --check`, runtime compile, and EditMode test compile. Unity CLI EditMode execution is blocked because the project is already open in another Unity instance.
- [ ] Commit the focused migration and push the feature branch without merging.
