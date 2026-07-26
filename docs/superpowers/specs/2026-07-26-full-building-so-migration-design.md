# Full Building ScriptableObject Migration Design

## Goal

Replace every legacy facility configuration in the prototype route with a `BuildingConfig` ScriptableObject asset, while preserving current pass/stop timing, feedback text, confirmation pauses, and the four existing building effects.

## Scope

- Create assets for every non-blank prototype tile: Start, Bank, Gate, Shop, Station, Park, Library, Museum, Hotel, Market, Clinic, Theater, and Harbor.
- Keep `Blank` without an asset and without a move event.
- Preserve existing behavior and feedback order.
- Remove `FacilityInteractionType`, route-level `FeedbackLog`, and the controller's legacy facility handling.
- Keep `BoardMoveResolver` pure C# and let it resolve only `BuildingDefinition` values into `BuildingEffectCommand` values.

## Data Mapping

Legacy facility modes map to building data as follows:

- `StopAutoFeedback` becomes `BuildingTriggerMode.Stop` plus `ShowFeedback`.
- `StopConfirmFeedback` becomes `BuildingTriggerMode.Stop` plus `RequestConfirmation`.
- `PassAutoFeedback` becomes `BuildingTriggerMode.PassOrStop` plus `ShowFeedback`.
- `PassConfirmFeedback` becomes `BuildingTriggerMode.PassOrStop` plus `RequestConfirmation`.
- `None` remains an absent building configuration.

Existing building effects remain in their assets. Where a tile had both a legacy facility message and building effects, the messages are ordered in the asset to preserve the current runtime sequence. For example, Shop requests confirmation first, then subtracts money, then shows the shop fee.

## Runtime Flow

`PrototypeBootstrapper` obtains a `BuildingConfig` from `PrototypeBuildingCatalog` by tile name. It derives visual color from the pure building definition, then gives the config to `BoardTile`. `BoardTile.ToDefinition()` converts the config to a pure `BuildingDefinition`. `BoardMoveResolver` evaluates the building for pass or stop timing and emits only building commands. `BoardController` consumes those commands for logs and confirmation UI.

## Compatibility and Cleanup

The migration intentionally removes the old facility enum and feedback fields from runtime and tests. No compatibility adapter remains. UI confirmation is still handled by `BoardController`; the core resolver only exposes `RequiresConfirmation` through a command.

## Verification

- EditMode tests cover pass, stop, pass-or-stop, feedback, and confirmation command output without `FacilityInteractionType`.
- Asset tests load the catalog and verify all thirteen assets and their migrated data.
- Route tests verify geometry and names only, with no embedded feedback or facility interaction data.
- A script compile check and Unity CLI EditMode run are performed; if the open Unity Editor locks the project, that limitation is reported.
