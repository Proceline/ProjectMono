# Game Logic Overview

This document is the source-of-truth summary for the current gameplay logic layer. Every major gameplay logic change must update this document in the same branch and commit series.

## Current Prototype

The project currently implements a simple 2D top-down Monopoly-like prototype. The player clicks a roll button, receives a dice result from the configured `IDiceRoller` implementation, and the token moves step by step around a fixed loop of board tiles.

The prototype is intentionally narrow:

- One player token.
- One board route.
- Default random 1-6 movement through `UnityRandomDiceRoller`.
- Facility feedback through logs and confirmation UI.
- No money model, health model, inventory, ownership, rent, turns, multiplayer, save data, or AI yet.

## Logic Files

- `Assets/Scripts/MonopolyPrototype/BoardMoveResolver.cs`
  - Pure C# movement and facility event resolver.
  - Should stay independent from Unity scene objects.
  - Covered by EditMode-style rule tests.
- `Assets/Scripts/MonopolyPrototype/PrototypeBoardRoute.cs`
  - Explicit prototype route data: tile names, positions, facility interaction types, and feedback logs.
  - Converts route specs into `BoardMoveResolver.TileDefinition` values for rule resolution.
- `Assets/Scripts/MonopolyPrototype/DiceRollers.cs`
  - Defines the `IDiceRoller` contract used by runtime flow.
  - `UnityRandomDiceRoller` is the default 1-6 Unity random implementation.
- `Assets/Scripts/MonopolyPrototype/BoardTile.cs`
  - Scene-side tile data holder.
  - Converts scene data into `BoardMoveResolver.TileDefinition`.
- `Assets/Scripts/MonopolyPrototype/BoardController.cs`
  - Runtime flow controller for rolling, moving, emitting logs, and waiting for confirmations.
  - Receives an `IDiceRoller` so tests or later scene wiring can drive deterministic movement without changing movement rules.
- `Assets/Scripts/MonopolyPrototype/PlayerToken.cs`
  - Visual token positioning and movement interpolation.
- `Assets/Scripts/MonopolyPrototype/GameLogView.cs`
  - Displays recent gameplay feedback messages.
- `Assets/Scripts/MonopolyPrototype/ConfirmationView.cs`
  - Displays blocking confirmation UI for interactions that require player acknowledgement.
- `Assets/Scripts/MonopolyPrototype/PrototypeBootstrapper.cs`
  - Creates the current prototype board, UI, event system, and controller at Play time.
  - Builds scene tiles from `PrototypeBoardRoute.Default` instead of owning route data directly.

## Facility Interaction Types

Facility behavior is represented by `FacilityInteractionType`.

### `None`

Blank tile. It has no pass feedback, no stop feedback, and never pauses movement.

### `StopAutoFeedback`

Triggers only when the token stops on the tile. It logs feedback and does not require confirmation.

### `StopConfirmFeedback`

Triggers only when the token stops on the tile. It logs feedback and pauses until the player confirms.

### `PassAutoFeedback`

Triggers when the token passes over the tile. It also triggers if the token stops on this tile. It logs feedback and does not require confirmation.

### `PassConfirmFeedback`

Triggers when the token passes over the tile. It also triggers if the token stops on this tile. It logs feedback and pauses until the player confirms.

## Movement Resolution Rules

`BoardMoveResolver.ResolveMove(...)` takes:

- A loop of `TileDefinition` values.
- The current tile index.
- A non-negative step count.

It returns:

- The final tile index.
- An ordered list of `MoveEvent` values.

Intermediate steps produce `MoveEventTiming.Pass` events only for pass-capable facilities:

- `PassAutoFeedback`
- `PassConfirmFeedback`

The final step produces a `MoveEventTiming.Stop` event for any feedback-capable facility:

- `StopAutoFeedback`
- `StopConfirmFeedback`
- `PassAutoFeedback`
- `PassConfirmFeedback`

`None` never produces an event.

## Confirmation Rules

Movement pauses only when a resolved move event has `RequiresConfirmation == true`.

Confirmation is currently required by:

- `StopConfirmFeedback`
- `PassConfirmFeedback`

Confirmation happens inside `BoardController.MoveRoutine(...)` by yielding on `ConfirmationView.WaitForConfirmation(...)`. After the player confirms, movement continues if there are remaining steps.

## Testing Expectations

Gameplay rule changes should update `Assets/Tests/EditMode/BoardMoveResolverTests.cs`. Prototype route data changes should update `Assets/Tests/EditMode/PrototypeBoardRouteTests.cs`.

The current rule tests cover:

- Pass events for intermediate pass facilities.
- Stop event for a final `PassConfirmFeedback` tile.
- Stop event for `StopAutoFeedback`.
- Confirming stop event for `StopConfirmFeedback`.
- No events for blank tiles.
- Default prototype route tile count, ordering, positions, facility interactions, and conversion into resolver tile definitions.

When Unity batchmode is unavailable because the project is open in the Editor, run a script compile check and the reflected core rule tests, then state the limitation clearly.

## Future Logic Architecture Notes

The next logic architecture pass should separate prototype responsibilities more clearly:

- Board route data is now explicit in `PrototypeBoardRoute`; future passes can move it into authorable assets if needed.
- Dice rolling is now injectable through `IDiceRoller`; a later controller-level test harness can drive deterministic movement without depending on Unity random.
- Facility effects should eventually become commands or handlers instead of only log strings.
- UI confirmation should remain a presentation concern; core logic should only mark events as requiring confirmation.
- Long-term gameplay systems such as money, health, ownership, turns, and player state should be introduced as separate pure logic units before being wired into scene UI.

## Maintenance Rule

Every important gameplay logic change must update this document before the change is considered complete. The dedicated logic task should treat this file as required reading and required maintenance.
