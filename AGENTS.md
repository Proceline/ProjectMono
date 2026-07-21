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
- Facility behavior should remain explicit and testable.
- Current facility interaction types are:
  - `None`: blank tile, no feedback.
  - `StopAutoFeedback`: feedback only when stopped on, no confirmation.
  - `StopConfirmFeedback`: feedback only when stopped on, requires confirmation.
  - `PassAutoFeedback`: feedback when passed or stopped on, no confirmation.
  - `PassConfirmFeedback`: feedback when passed or stopped on, requires confirmation.
- Movement should pause only for interactions that require confirmation.

## Verification

- Add or update EditMode tests for gameplay rule changes.
- Before reporting a gameplay rule change as complete, run the core rule tests and a script compile check when Unity batchmode is unavailable.
- If Unity is already open and blocks batchmode tests, state that limitation clearly.

## Git

- Keep commits focused and descriptive.
- For important feature changes, create a new branch before making the change.
- Commit progress promptly when a meaningful unit of work is complete.
- Unless the user says otherwise, pushing committed work to the remote branch is allowed.
- Notify the user before merging any branch.
- Do not revert user changes unless explicitly asked.
