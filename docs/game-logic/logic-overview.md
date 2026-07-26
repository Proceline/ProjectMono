# Game Logic Overview

This document is the source-of-truth summary for the current gameplay logic layer. Every major gameplay logic change must update this document in the same branch and commit series.

## Current Prototype

The project currently implements a simple 2D top-down Monopoly-like prototype. The player clicks a roll button, receives a dice result from the configured `IDiceRoller` implementation, and the token moves step by step around a fixed loop of board tiles.

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
  - Converts Unity-authored data into pure `BuildingDefinition` values before rule resolution.
- `Assets/Scripts/MonopolyPrototype/PrototypeBuildingCatalog.cs`
  - ScriptableObject catalog of the building assets used by the prototype scene.
  - Matches generated route tiles to `BuildingConfig` assets by building name.
- `Assets/Scripts/MonopolyPrototype/PrototypeBoardRoute.cs`
  - Explicit prototype route data: tile names and positions only.
  - Does not own building effects; those are referenced through `PrototypeBuildingCatalog`.
  - Converts route specs into geometry-only `BoardMoveResolver.TileDefinition` values.
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
  - Builds scene tiles from `PrototypeBoardRoute.Default` and looks up building configs through the serialized catalog.

## Building-backed Tile Behavior

Every non-blank route tile is backed by one `BuildingConfig` asset in `Assets/Data/Buildings`.
The route stores only identity and geometry; the catalog supplies the building definition at bootstrap time.

The current prototype uses these pure rule combinations:

- `Stop` with `ShowFeedback`: Start, Park, Market.
- `Stop` with `RequestConfirmation`: Shop, Museum, Theater.
- `PassOrStop` with `ShowFeedback`: Bank, Library, Hotel, Clinic.
- `PassOrStop` with `RequestConfirmation`: Gate, Station, Harbor.
- Blank: no `BuildingConfig`, no event, and no feedback.

An asset may contain multiple effects. Effects are resolved and emitted in serialized order, so confirmation, state-change requests, and feedback can be composed without introducing UI dependencies into the rule layer.

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

Buildings are authored as `BuildingConfig` ScriptableObject assets under `Assets/Data/Buildings`. `PrototypeBuildingCatalog.asset` references all 13 non-blank prototype building assets and is assigned to the `Prototype Bootstrapper` object in `Assets/Scenes/SampleScene.unity`. Core rules do not consume ScriptableObjects directly. `BuildingConfig.ToDefinition()` produces a pure `BuildingDefinition` with:

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

At Play time, `PrototypeBootstrapper` finds a `BuildingConfig` by the generated tile name and assigns it to `BoardTile`. `BoardTile.ToDefinition()` converts the asset to a pure `BuildingDefinition`, which is carried by `BoardMoveResolver.TileDefinition`. When movement reaches a tile, `BoardMoveResolver` resolves the building for the pass or stop timing and includes any resulting commands on the emitted `MoveEvent`.

## Confirmation Rules

Movement pauses only when a resolved move event has `RequiresConfirmation == true`.

Confirmation is currently required only by a building command with `BuildingEffectType.RequestConfirmation`.

Confirmation happens inside `BoardController.MoveRoutine(...)` by yielding on `ConfirmationView.WaitForConfirmation(...)`. Core rules only mark building events as requiring confirmation; the UI wait remains a presentation-layer concern. After the player confirms, movement continues if there are remaining steps.

## Testing Expectations

Gameplay rule changes should update `Assets/Tests/EditMode/BoardMoveResolverTests.cs`. Prototype route data changes should update `Assets/Tests/EditMode/PrototypeBoardRouteTests.cs`. Building rule and authoring changes should update `Assets/Tests/EditMode/BuildingRuleResolverTests.cs`, `Assets/Tests/EditMode/BuildingConfigTests.cs`, or `Assets/Tests/EditMode/BoardTileBuildingTests.cs`.

The current rule tests cover:

- Pass events for intermediate buildings with `Pass` or `PassOrStop` triggers.
- Stop events for final buildings with `Stop` or `PassOrStop` triggers.
- Confirmation requirements emitted by building commands.
- No events for blank tiles.
- Default prototype route tile count, ordering, positions, and conversion into geometry-only resolver tile definitions.
- Building trigger matching for pass, stop, and pass-or-stop timing.
- Ordered building effect command output for money, teleport, confirmation, and feedback effects.
- ScriptableObject building configs converting into pure building definitions.
- Building catalogs resolving named `BuildingConfig` assets.
- Prototype building asset tests covering all 13 catalog entries, effect ordering, money payloads, and teleport targets.
- Board tiles converting catalog-provided building configs into pure definitions.

When Unity batchmode is unavailable because the project is open in the Editor, run a script compile check and the reflected core rule tests, then state the limitation clearly.

## Future Logic Architecture Notes

The next logic architecture pass should separate prototype responsibilities more clearly:

- Route geometry is explicit in `PrototypeBoardRoute`; all building effects are authorable assets in `Assets/Data/Buildings`.
- `PrototypeBuildingCatalog.asset` is the prototype scene's source of truth for which `BuildingConfig` asset belongs to each named non-blank tile.
- Dice rolling is now injectable through `IDiceRoller`; a later controller-level test harness can drive deterministic movement without depending on Unity random.
- The old `FacilityInteractionType`, route feedback fields, and controller facility branch have been removed; building commands are the only interaction path.
- Money and teleport commands are currently surfaced as feedback logs by `BoardController`; future passes should connect them to dedicated player state and movement handlers.
- UI confirmation should remain a presentation concern; core logic should only mark events as requiring confirmation.
- Long-term gameplay systems such as money, health, ownership, turns, and player state should be introduced as separate pure logic units before being wired into scene UI.

## Maintenance Rule

Every important gameplay logic change must update this document before the change is considered complete. The dedicated logic task should treat this file as required reading and required maintenance.
