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
  - ScriptableObject effect translator layer for money adjustment, teleport, confirmation, and feedback.
  - Each concrete effect type lives in its own same-named script so Unity can bind its asset to the correct `MonoScript`.
  - Each effect asset can produce a pure definition or a `BuildingEffectCommand`; it does not apply player state or UI side effects.
  - `AdjustMoneyEffectAsset` owns the two optional money event references used for both positive and negative adjustments; other effect assets have no event references yet.
- `Assets/Scripts/MonopolyPrototype/BuildingEvents/`
  - Application-boundary integration layer between building assets and SOEvent assets.
  - `BuildingEventProfile` is optional metadata referenced by a `BuildingConfig`; it does not participate in `BuildingConfig.ToDefinition()`.
  - `BuildingEventBridge` translates resolved move events and commands into typed `BuildingEventContext` payloads and raises the configured SOEvents.
  - `BuildingEventSOEvent` exposes a serialized UnityEvent plus ordered runtime `Register`/`Unregister` callbacks for building-specific notifications.
  - `MoneyChangeRequestedSOEvent` carries a mutable `MoneyChangeRequest` for money-effect modifiers; `MoneyChangedSOEvent` carries a post-application `MoneyChangeResult` for UI and other observers.
- `Assets/Scripts/MonopolyPrototype/SOEvents/`
  - Reusable ScriptableObject event extension layer. Core movement and building rule types do not depend on it; application-boundary building integration references it explicitly.
  - The abstract `SOEvent` base provides runtime listener lifecycle and cleanup, while concrete events define their own typed `Raise(...)` signature.
  - Each concrete event exposes a serialized `UnityEvent` for Inspector callbacks. `Raise(...)` invokes those persistent callbacks in Unity's serialized order, then invokes runtime callbacks through `Register`/`Unregister` using fixed integer order and registration sequence as a stable tie-breaker.
  - Array payloads are passed through without cloning, so listeners can intentionally observe or mutate the same array instance during one raise.
  - See [so-event-guide.md](so-event-guide.md) for asset creation, runtime registration, extension, testing, and integration boundaries.
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
  - Consumes building effect commands as presentation-layer feedback/confirmation logs for now and raises money-change requests through the application bridge.
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

- `AdjustMoney`
- `Teleport`
- `RequestConfirmation`
- `ShowFeedback`

`BuildingRuleResolver.Resolve(...)` takes a pure `BuildingDefinition` and a `MoveEventTiming`, then returns ordered `BuildingEffectCommand` values. These commands describe what should happen; they do not apply UI, animation, player state, or MonoBehaviour listener side effects by themselves. Each pure command carries an `EffectIndex` ordinal so the application boundary can locate the original Effect asset without storing a ScriptableObject reference in the rule model.

`BuildingEffectAsset.ToCommand()` is an authoring-layer convenience for translating one effect asset into the same pure command used by the resolver. Building effects do not raise SOEvents from `ToCommand()`. After `BoardMoveResolver` has produced a `MoveEvent`, `BoardController` passes it to `BuildingEventBridge`, which looks up the tile's optional `BuildingEventProfile` and raises building notifications. For `AdjustMoney` commands, the bridge finds the original `AdjustMoneyEffectAsset` by `EffectIndex`, creates a `MoneyChangeRequest`, and raises the two event references stored directly on that Effect asset. Other Effect assets currently have no event integration.

The bridge raises events in this order:

1. `BuildingTriggered` once for the resolved building event.
2. `EffectCommandProduced` once per command, in the serialized effect order.
3. `ConfirmationCompleted` after `ConfirmationView.WaitForConfirmation(...)` returns for a confirmation command.

The profile is application metadata only. `BoardMoveResolver`, `BuildingRuleResolver`, `BuildingDefinition`, and `BuildingEffectCommand` remain usable without SOEvent assets.

Money events use two stages:

1. `MoneyChangeRequestedSOEvent` is raised for `AdjustMoney` commands. Its `MoneyChangeRequest` keeps the original `BaseDelta` and exposes `CurrentDelta` for ordered modifiers to adjust before a future money state system applies it.
2. `MoneyChangedSOEvent` is reserved for the result after money state is applied. Its `MoneyChangeResult` contains the requested and applied deltas, balances before and after, success state, and failure reason so UI can display the actual result rather than a predicted command amount.

The current prototype has no money state model yet. Therefore the request event is wired and the prototype log uses its possibly modified `CurrentDelta`, while `MoneyChangedSOEvent` is available for the later state-application adapter and is not raised by the command resolver.

At Play time, `PrototypeBootstrapper` reads each ordered tile from `PrototypeMapData` and assigns its serialized `BuildingConfig` to `BoardTile`. `BoardTile.ToDefinition()` converts the asset to a pure `BuildingDefinition`, which is carried by `BoardMoveResolver.TileDefinition`. When movement reaches a tile, `BoardMoveResolver` resolves the building for the pass or stop timing and includes any resulting commands on the emitted `MoveEvent`. The original `BuildingConfig` remains available to the application layer so `BuildingEventBridge` can look up its optional `BuildingEventProfile` without adding event references to the pure definition.

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

SOEvent EditMode tests in `Assets/Tests/EditMode/SOEvents/SOEventTests.cs` cover stable ordered registration, unregister behavior, runtime listener cleanup, and array payload mutation/reference identity. They exercise the extension layer without UI objects or MonoBehaviour listeners.

When Unity batchmode is unavailable because the project is open in the Editor, run a script compile check and the reflected core rule tests, then state the limitation clearly.

## Future Logic Architecture Notes

The next logic architecture pass should separate prototype responsibilities more clearly:

- Map geometry and path order are authored in `PrototypeMapData`; all building effects remain authorable assets in `Assets/Data/Buildings`.
- `PrototypeMapData.asset` is the prototype scene's source of truth for which `BuildingConfig` belongs to each ordered tile.
- `PrototypeMapPainterWindow` is intentionally data-only; runtime visuals remain generated by `PrototypeBootstrapper`.
- SOEvent concrete assets are integrated at the application boundary through `BuildingEventProfile` and `BuildingEventBridge`; the core movement/building rule pipeline remains independent.
- Dice rolling is now injectable through `IDiceRoller`; a later controller-level test harness can drive deterministic movement without depending on Unity random.
- The old `FacilityInteractionType`, route feedback fields, and controller facility branch have been removed; building commands are the only interaction path.
- Money and teleport commands are currently surfaced as feedback logs by `BoardController`; future passes should connect them to dedicated player state and movement handlers.
- UI confirmation should remain a presentation concern; core logic should only mark events as requiring confirmation.
- Long-term gameplay systems such as money, health, ownership, turns, and player state should be introduced as separate pure logic units before being wired into scene UI.

## Maintenance Rule

Every important gameplay logic change must update this document before the change is considered complete. The dedicated logic task should treat this file as required reading and required maintenance.
