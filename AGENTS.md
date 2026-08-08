# Project Agent Rules

## Project Context

- This is a Unity 6000.3.9f1 project.
- The current prototype is a simple 2D top-down Monopoly-like board game.
- Runtime prototype code lives under `Assets/Scripts/MonopolyPrototype`.
- EditMode tests live under `Assets/Tests/EditMode`.

## Unity Rules

- Do not edit `Library`, `Temp`, `Logs`, `UserSettings`, `obj`, generated `.csproj`, or generated `.sln` files as source changes.
- Keep Unity `.meta` files in sync whenever adding, moving, or deleting files under `Assets`.
- Prefer small MonoBehaviours with clear responsibilities.
- Keep core gameplay rules in pure C# where practical so they can be tested without entering Play Mode.
- Use assembly definition files for new runtime and test assemblies when adding new feature areas.

## Gameplay Prototype Rules

- Keep the prototype focused on playable mechanics before adding polish.
- Maintain the current logic summary in [logic-overview.md](docs/game-logic/logic-overview.md).
- Every important gameplay logic change must update [logic-overview.md](docs/game-logic/logic-overview.md) in the same branch and commit series.
- Building behavior should remain explicit and testable through `BuildingConfig` assets and pure `BuildingDefinition` values.
- Non-blank tiles use one catalog-backed `BuildingConfig` with a `Pass`, `Stop`, or `PassOrStop` trigger.
- Building effects are emitted as presentation-agnostic commands; UI confirmation remains outside the core rules.
- For SOEvent work, read [so-event-guide.md](docs/game-logic/so-event-guide.md) before editing. SOEvent is an independent extension layer and must not be added as a dependency of the core rule or building-command pipeline without an explicit integration task.
- Movement should pause only for interactions that require confirmation.

## Verification

- Add or update EditMode tests for gameplay rule changes.
- Prefer the Unity CLI (`Unity.exe -batchmode ...`) for compilation, test runs, asset refresh, and other verification whenever it can run safely; if an open Editor locks the project or the CLI is unavailable, use the open Editor and document that limitation.
- Before reporting a gameplay rule change as complete, run the core rule tests and a script compile check when Unity batchmode is unavailable.
- If Unity is already open and blocks batchmode tests, state that limitation clearly.

## Git

- Keep commits focused and descriptive.
- For important feature changes, create a new branch before making the change.
- Commit progress promptly when a meaningful unit of work is complete.
- Unless the user says otherwise, pushing committed work to the remote branch is allowed.
- Notify the user before merging any branch.
- Do not revert user changes unless explicitly asked.
