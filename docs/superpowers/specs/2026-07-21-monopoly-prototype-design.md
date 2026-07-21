# Monopoly Prototype Design

## Goal

Build the first playable 2D top-down Monopoly-like prototype in the default Unity scene.

## Scope

The prototype has one player token, a loop of board tiles, a roll button, and an on-screen log. Clicking the roll button moves the token step by step. Tiles may emit one message when the token passes over them and another message when the token stops on them.

## Gameplay

- The board is a small 2D loop of square tiles.
- The player starts on the start tile.
- A roll produces a value from 1 to 6.
- The token moves one tile at a time along the loop.
- Pass events are logged for intermediate tiles only.
- Stop events are logged for the final tile only.
- No money, purchasing, rent, turns, multiplayer, popups, or branching choices are included.

## Architecture

- `BoardMoveResolver` is pure C# rule logic and returns ordered pass and stop events for a move.
- `BoardTile` stores each scene tile's display name and optional pass/stop log text.
- `BoardController` owns the board route, roll button flow, token movement, and log output.
- `PlayerToken` moves the visible token between tile positions.
- `GameLogView` renders recent messages in a Unity UI text area.

## Testing

EditMode tests cover core movement rules:

- Passing over a tile triggers its pass event.
- Stopping on a tile triggers its stop event.
- The final tile's pass event does not fire when it is the stopping tile.
- Board movement wraps around the loop.

## Acceptance

Opening `Assets/Scenes/SampleScene.unity` shows a 2D board with a token, roll button, and log. Pressing roll moves the token and writes clear messages for pass and stop interactions.
