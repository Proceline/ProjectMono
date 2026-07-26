# Game Logic Overview

This document is the source-of-truth summary for the current gameplay logic layer. Every major gameplay logic change must update this document in the same branch and commit series.

## Current Prototype

The project currently implements a simple 2D top-down Monopoly-like prototype. The player clicks a roll button, receives a dice result from the configured `IDiceRoller` implementation, and the token moves step by step around the closed loop described by a `PrototypeMapData` asset.

The prototype is intentionally narrow:

- One player token.
- One board route.
- Default random 1-6 movement through `UnityRandomDiceRoller`.
- Building feedback and confirmation commands consumed by the presentation layer.
- ScriptableObject-authored building data that converts into pure C# building definitions.
- Building effects represented as presentation-agnostic commands: money changes, teleport requests, confirmation requests, and feedback requests.
- No money model, health model, inventory, ownership, rent, turns, multiplayer, save data, or AI yet.

## Logic Files

- `Assets/Scripts/MonopolyPrototype/BoardMoveResolver.cs`
  - Pure C# movement and building event resolver.
  - Should stay independent from Unity scene objects.
  - Covered by EditMode-style rule tests.
- `Assets/Scripts/MonopolyPrototype/BuildingRules.cs`
  - Pure C# building trigger/effect model.
  - Resolves pass/stop timing into ordered `BuildingEffectCommand` values.
  - Does not depend on UI, MonoBehaviour listeners, or ScriptableObjects.
- `Assets/Scripts/MonopolyPrototype/BuildingConfig.cs`
  - ScriptableObject authoring layer for buildings.
  - Holds an ordered list of `BuildingEffectAsset` references and converts them into pure `BuildingDefinition` values before rule resolution.
- `Assets/Scripts/MonopolyPrototype/BuildingEffects/`
  - ScriptableObject effect translator layer for add/subtract money, teleport, confirmation, and feedback.
  - Each concrete effect type lives in its own same-named script so Unity can bind its asset to the correct `MonoScript`.
  - Each effect asset can produce a pure definition or a `BuildingEffectCommand`; it does not apply player state or UI side effects.
- `Assets/Scripts/MonopolyPrototype/PrototypeMapData.cs`
  - ScriptableObject map data: square grid size and ordered path tiles.
  - Stores each tile's grid coordinate, name, and optional `BuildingConfig` reference.
  - Validates bounds, unique cells, orthogonal adjacency, and closed-loop connectivity.
- `Assets/Scripts/MonopolyPrototype/PrototypeMapLayout.cs`
  - Pure layout helper shared by runtime board generation and the editor preview.
  - Converts grid coordinates into positions around a configurable center and spacing, and calculates camera-fit bounds.
- `Assets/Scripts/MonopolyPrototype/DiceRollers.cs`
  - Defines the `IDiceRoller` contract used by runtime flow.
  - `UnityRandomDiceRoller` is the default 1-6 Unity random implementation.
- `Assets/Scripts/MonopolyPrototype/BoardTile.cs`
  - Scene-side tile data holder.
  - References a `BuildingConfig` and converts it into a pure definition.
  - Converts scene data into `BoardMoveResolver.TileDefinition`.
- `Assets/Scripts/MonopolyPrototype/BoardController.cs`
  - Runtime flow controller for rolling, moving, emitting logs, and waiting for confirmations.
  - Receives an `IDiceRoller` so tests or later scene wiring can drive deterministic movement without changing movement rules.
  - Consumes building effect commands as presentation-layer feedback/confirmation logs for now.
- `Assets/Scripts/MonopolyPrototype/PlayerToken.cs`
  - Visual token positioning and movement interpolation.
- `Assets/Scripts/MonopolyPrototype/GameLogView.cs`
  - Displays recent gameplay feedback messages.
- `Assets/Scripts/MonopolyPrototype/ConfirmationView.cs`
  - Displays blocking confirmation UI for interactions that require player acknowledgement.
- `Assets/Scripts/MonopolyPrototype/PrototypeBootstrapper.cs`
  - Creates the current prototype board, UI, event system, and controller at Play time.
  - Builds scene tiles from the serialized `PrototypeMapData` asset.
- `Assets/Editor/MonopolyPrototype/PrototypeMapPainterWindow.cs`
  - Editor-only map authoring tool.
  - Draws an N x N placeholder grid in the Scene view and stores only map data; it does not create runtime visual objects.
  - Clicking an existing cell with a selected building replaces that cell while preserving path order. `Erase` removes the selected cell, while `Blank` remains a valid tile type.
  - The Scene preview exposes center, spacing, and visual scale controls.

## Building-backed Tile Behavior

Every non-blank map tile is backed by one `BuildingConfig` asset in `Assets/Data/Buildings`.
The `PrototypeMapData` asset assigned to the scene is the runtime source of truth for grid size, path order, tile identity, and building references.
The Map Painter discovers the individual `BuildingConfig` assets directly for its palette; the palette is not a source of map placement.

The current prototype uses these pure rule combinations:

- `Stop` with `ShowFeedback`: Start, Park, Market.
- `Stop` with `RequestConfirmation`: Shop, Museum, Theater.
- `PassOrStop` with `ShowFeedback`: Bank, Library, Hotel, Clinic.
- `PassOrStop` with `RequestConfirmation`: Gate, Station, Harbor.
- Blank: no `BuildingConfig`, no event, and no feedback.

An asset may contain multiple effect SO references. Effects are resolved and emitted in serialized order, so confirmation, state-change requests, and feedback can be composed without introducing UI dependencies into the rule layer. A building may contain at most one `RequestConfirmation` effect.

## Movement Resolution Rules

`BoardMoveResolver.ResolveMove(...)` takes:

- A loop of `TileDefinition` values.
- The current tile index.
- A non-negative step count.

It returns:

- The final tile index.
- An ordered list of `MoveEvent` values.

Intermediate steps use `MoveEventTiming.Pass`. A building emits commands when its trigger mode is `Pass` or `PassOrStop`.

The final step uses `MoveEventTiming.Stop`. A building emits commands when its trigger mode is `Stop` or `PassOrStop`.

Blank tiles and tiles without a building definition never produce an event.

## Building Rules

Buildings are authored as `BuildingConfig` ScriptableObject assets under `Assets/Data/Buildings`. Each of the 13 prototype building assets directly references its ordered effect assets under `Assets/Data/BuildingEffects` through the `effects` list. Their effect translator scripts are grouped under `Assets/Scripts/MonopolyPrototype/BuildingEffects/`. A `PrototypeMapData` asset is assigned to the `Prototype Bootstrapper` object in `Assets/Scenes/SampleScene.unity`; each map tile directly references its building asset. Core rules do not consume ScriptableObjects directly. `BuildingConfig.ToDefinition()` produces a pure `BuildingDefinition` with:

- A building name.
- A `BuildingTriggerMode`.
- An ordered list of `BuildingEffectDefinition` values.

Current building trigger modes are:

- `Pass`: triggers only when the token passes over the tile before its final step.
- `Stop`: triggers only when the token stops on the tile.
- `PassOrStop`: triggers for either pass or stop timing.

Current building effect types are:

- `AddMoney`
- `SubtractMoney`
- `Teleport`
- `RequestConfirmation`
- `ShowFeedback`

`BuildingRuleResolver.Resolve(...)` takes a pure `BuildingDefinition` and a `MoveEventTiming`, then returns ordered `BuildingEffectCommand` values. These commands describe what should happen; they do not apply UI, animation, player state, or MonoBehaviour listener side effects by themselves.

`BuildingEffectAsset.ToCommand()` is an authoring-layer convenience for translating one effect asset into the same pure command used by the resolver. SO Event extensions are intentionally outside the current scope and will be designed in a separate session.

At Play time, `PrototypeBootstrapper` reads each ordered tile from `PrototypeMapData` and assigns its serialized `BuildingConfig` to `BoardTile`. `BoardTile.ToDefinition()` converts the asset to a pure `BuildingDefinition`, which is carried by `BoardMoveResolver.TileDefinition`. When movement reaches a tile, `BoardMoveResolver` resolves the building for the pass or stop timing and includes any resulting commands on the emitted `MoveEvent`.

The runtime layout uses the map's square dimensions together with `boardCenter`, `tileSpacing`, and `tileScale`. When `fitCameraToBoard` is enabled, the orthographic camera centers on the board and calculates its size from the map bounds plus `cameraPadding`.

## Confirmation Rules

Movement pauses only when a resolved move event has `RequiresConfirmation == true`.

Confirmation is currently required only by a building command with `BuildingEffectType.RequestConfirmation`.

Confirmation happens inside `BoardController.MoveRoutine(...)` by yielding on `ConfirmationView.WaitForConfirmation(...)`. Core rules only mark building events as requiring confirmation; the UI wait remains a presentation-layer concern. After the player confirms, movement continues if there are remaining steps.

## Testing Expectations

Gameplay rule changes should update `Assets/Tests/EditMode/BoardMoveResolverTests.cs`. Map data changes should update `Assets/Tests/EditMode/PrototypeMapDataTests.cs`. Building rule and authoring changes should update `Assets/Tests/EditMode/BuildingRuleResolverTests.cs`, `Assets/Tests/EditMode/BuildingConfigTests.cs`, or `Assets/Tests/EditMode/BoardTileBuildingTests.cs`.

The current rule tests cover:

- Pass events for intermediate buildings with `Pass` or `PassOrStop` triggers.
- Stop events for final buildings with `Stop` or `PassOrStop` triggers.
- Confirmation requirements emitted by building commands.
- No events for blank tiles.
- Map data validation for square bounds, unique cells, adjacent path order, and closed-loop connectivity.
- Default map asset tile count, ordering, dimensions, and conversion into resolver tile definitions.
- Building trigger matching for pass, stop, and pass-or-stop timing.
- Ordered building effect command output for money, teleport, confirmation, and feedback effects.
- ScriptableObject building configs converting into pure building definitions.
- Building effect assets translating into pure definitions and commands.
- Building config validation rejecting more than one confirmation effect.
- Prototype building asset tests covering all 13 individual building assets, effect ordering, money payloads, and teleport targets.
- Board tiles converting map-provided building configs into pure definitions.

When Unity batchmode is unavailable because the project is open in the Editor, run a script compile check and the reflected core rule tests, then state the limitation clearly.

## Future Logic Architecture Notes

The next logic architecture pass should separate prototype responsibilities more clearly:

- Map geometry and path order are authored in `PrototypeMapData`; all building effects remain authorable assets in `Assets/Data/Buildings`.
- `PrototypeMapData.asset` is the prototype scene's source of truth for which `BuildingConfig` belongs to each ordered tile.
- `PrototypeMapPainterWindow` is intentionally data-only; runtime visuals remain generated by `PrototypeBootstrapper`.
- Dice rolling is now injectable through `IDiceRoller`; a later controller-level test harness can drive deterministic movement without depending on Unity random.
- The old `FacilityInteractionType`, route feedback fields, and controller facility branch have been removed; building commands are the only interaction path.
- Money and teleport commands are currently surfaced as feedback logs by `BoardController`; future passes should connect them to dedicated player state and movement handlers.
- UI confirmation should remain a presentation concern; core logic should only mark events as requiring confirmation.
- Long-term gameplay systems such as money, health, ownership, turns, and player state should be introduced as separate pure logic units before being wired into scene UI.

## Maintenance Rule

Every important gameplay logic change must update this document before the change is considered complete. The dedicated logic task should treat this file as required reading and required maintenance.
