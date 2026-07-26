# Building Asset Migration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the four existing prototype buildings real `BuildingConfig` assets referenced by the scene, then remove their duplicate programmatic definitions from the route.

**Architecture:** `BuildingConfig` remains the Unity authoring layer. A `PrototypeBuildingCatalog` asset owns the set of building asset references and is serialized on `PrototypeBootstrapper`; each generated `BoardTile` receives only the matched `BuildingConfig`. `BoardMoveResolver` continues to consume pure `BuildingDefinition` values produced by `BoardTile.ToDefinition()`.

**Tech Stack:** Unity 6000.3.9f1, C#, ScriptableObject YAML assets, NUnit EditMode tests.

## Global Constraints

- Core gameplay rules stay independent of UI and MonoBehaviour listeners.
- `docs/game-logic/logic-overview.md` must be updated in the same branch and commit series.
- Existing facility interaction behavior must remain unchanged.
- Runtime and test source stays under `Assets/Scripts/MonopolyPrototype` and `Assets/Tests/EditMode`.
- Unity `.meta` files must remain synchronized with new Assets files.

### Task 1: Define the asset catalog contract

**Files:**
- Create: `Assets/Scripts/MonopolyPrototype/PrototypeBuildingCatalog.cs`
- Create: `Assets/Scripts/MonopolyPrototype/PrototypeBuildingCatalog.cs.meta`
- Modify: `Assets/Scripts/MonopolyPrototype/BuildingConfig.cs`
- Modify: `Assets/Scripts/MonopolyPrototype/BoardTile.cs`
- Test: `Assets/Tests/EditMode/PrototypeBuildingCatalogTests.cs`
- Test: `Assets/Tests/EditMode/BoardTileBuildingTests.cs`

- [x] Write a failing test that configures two `BuildingConfig` objects, registers them in a catalog, and verifies `Find("Bank")` returns the Bank asset while an unknown name returns null.
- [x] Run the targeted Roslyn test compile and confirm it fails because `PrototypeBuildingCatalog` is missing.
- [x] Write the minimal catalog with serialized `List<BuildingConfig>`, `Configure(IReadOnlyList<BuildingConfig>)` for tests, and `Find(string)` for runtime lookup.
- [x] Expose `BuildingConfig.BuildingName` for catalog matching.
- [x] Remove the runtime `BuildingDefinition` parameter from `BoardTile.Configure`; keep `BoardTile.ToDefinition()` converting only its serialized `BuildingConfig`.
- [x] Update the existing BoardTile test to verify the SO conversion path only.
- [x] Re-run the targeted tests and confirm green at compile level; Unity execution remains blocked by the open Editor.

### Task 2: Wire the scene bootstrapper to the catalog

**Files:**
- Modify: `Assets/Scripts/MonopolyPrototype/PrototypeBootstrapper.cs`
- Test: `Assets/Tests/EditMode/PrototypeBuildingCatalogTests.cs`

- [x] Add a serialized `PrototypeBuildingCatalog buildingCatalog` field to `PrototypeBootstrapper`.
- [x] During board creation, look up the tile name in the catalog and pass the resulting config to `BoardTile.Configure`.
- [x] Keep route geometry, labels, and legacy `FacilityInteractionType` data in `PrototypeBoardRoute`; building effects come only from the catalog lookup.
- [x] Add a test that a configured tile converts the catalog-provided asset into the expected pure definition.
- [x] Compile the runtime and EditMode assemblies with Unity's Roslyn response files.

### Task 3: Create and reference actual Unity assets

**Files:**
- Create: `Assets/Data/Buildings/Bank.asset` and `.meta`
- Create: `Assets/Data/Buildings/Gate.asset` and `.meta`
- Create: `Assets/Data/Buildings/Shop.asset` and `.meta`
- Create: `Assets/Data/Buildings/Harbor.asset` and `.meta`
- Create: `Assets/Data/Buildings/PrototypeBuildingCatalog.asset` and `.meta`
- Modify: `Assets/Scenes/SampleScene.unity`

- [x] Serialize the current Bank, Gate, Shop, and Harbor trigger/effect values into four `BuildingConfig` assets.
- [x] Serialize the four asset references into `PrototypeBuildingCatalog.asset`.
- [x] Add the catalog reference to the `Prototype Bootstrapper` component in `SampleScene.unity`.
- [x] Verify the GUID in every YAML reference matches its `.meta` file.

### Task 4: Delete duplicate route definitions and document the new source of truth

**Files:**
- Modify: `Assets/Scripts/MonopolyPrototype/PrototypeBoardRoute.cs`
- Modify: `Assets/Tests/EditMode/PrototypeBoardRouteTests.cs`
- Modify: `docs/game-logic/logic-overview.md`

- [x] Remove `TileSpec.Building`, the optional building constructor argument, and `CreateBank`, `CreateGate`, `CreateShop`, and `CreateHarbor`.
- [x] Change `TileSpec.ToDefinition()` to create tiles without a building; runtime tiles obtain building data from `PrototypeBuildingCatalog`.
- [x] Replace route tests that asserted embedded definitions with tests asserting route geometry/facility behavior remains intact.
- [x] Document the catalog asset and the scene data flow as the new source of truth.

### Task 5: Verify and commit

- [x] Run `git diff --check`.
- [x] Run the full available script compile check; Unity EditMode execution is blocked by the open Editor and is reported explicitly.
- [x] Inspect the final diff for stale `CreateBank`/`TileSpec.Building` references and confirm the catalog asset is referenced by the scene.
- [x] Commit the focused migration and push the feature branch.
