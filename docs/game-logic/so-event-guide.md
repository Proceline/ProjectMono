# SOEvent Guide

This guide is the working contract for the ScriptableObject event extension layer. Read it before adding, using, or integrating an SOEvent in a later gameplay session.

## Purpose

SOEvent provides a reusable asset-based event surface for cross-system extension callbacks:

- Inspector-authored persistent callbacks through serialized `UnityEvent` fields.
- Runtime callbacks through typed `Register` and `Unregister` APIs.
- Stable runtime callback order through an integer `order` and registration sequence.
- Array payloads that can be observed or mutated by later listeners in the same raise.

SOEvent is an extension and integration layer. It is not the core movement/building rule model, and it does not replace pure definitions or commands.

## Canonical Files

The reusable event primitives live in `Assets/Scripts/MonopolyPrototype/SOEvents/` and are compiled by the `MonopolyPrototype.SOEvents` assembly definition. The building-specific integration types live in `Assets/Scripts/MonopolyPrototype/BuildingEvents/` and are compiled with the application runtime assembly, which references the reusable SOEvent assembly.

| Type | Purpose |
| --- | --- |
| `SOEvent` | Abstract ScriptableObject base with runtime listener cleanup. |
| `VoidSOEvent` | Event with no payload. |
| `IntSOEvent` | Event with one `int` payload. |
| `IntArraySOEvent` | Event with one `int[]` payload. |
| `IntArrayIntSOEvent` | Event with an `int[]` payload and one additional `int` context value. |
| `OrderedEventListeners<TDelegate>` | Pure C# ordered runtime listener registry used by the concrete events. |
| `BuildingEventSOEvent` | Building integration event with a typed `BuildingEventContext` payload. |
| `BuildingEventProfile` | Optional building-side grouping of trigger, command, and confirmation event references. |
| `BuildingEventBridge` | Application-boundary adapter that translates resolved building events into SOEvent raises. |
| `MoneyChangeRequestedSOEvent` | Mutable money-change request event raised before a future money state applies a delta. |
| `MoneyChangedSOEvent` | Money-change result event raised after a state adapter applies a request. |
| `MoneyChangeRequest` | Reference payload with source metadata, `BaseDelta`, and one shared mutable `int[] CurrentDeltaPayload` (amount at index `0`). |
| `MoneyChangeResult` | Reference payload with requested/applied deltas, balances, success state, and failure reason. |
| `AdjustMoneyEffectAsset` | Effect asset with a signed money delta and direct references to the two money events. |
| `MoneyChangedDebugProbeSO` | Debug/extension ScriptableObject with Inspector-bindable callbacks for mutating or logging a money request payload. |
| `MoneyChangedCoinFeedback` | Scene-level 3D presentation MonoBehaviour that listens only to `MoneyChangedSOEvent` and displays transient applied-delta feedback. |

EditMode coverage is in `Assets/Tests/EditMode/SOEvents/SOEventTests.cs` and uses the `MonopolyPrototype.SOEvents.EditModeTests` assembly.

## When To Use SOEvent

Use an SOEvent when a system needs to announce an occurrence to one or more independently configured consumers, for example:

- A scene or presentation adapter needs to react to a resolved gameplay result.
- A feature needs Inspector-configured callbacks without hard-coding every receiver.
- Multiple runtime systems need a typed notification with a defined callback order.
- A mutable array payload needs to pass through an ordered processing chain.

Do not use an SOEvent to hide a rule decision, replace a value object, or make core rules discover listeners. If the result can be represented as a pure value or a `BuildingEffectCommand`, keep it there.

## Creating An Event Asset

Concrete event assets are available from Unity's Create Asset menu:

- `Monopoly Prototype/SO Events/Void Event`
- `Monopoly Prototype/SO Events/Int Event`
- `Monopoly Prototype/SO Events/Int Array Event`
- `Monopoly Prototype/SO Events/Int Array + Int Event`
- `Monopoly Prototype/SO Events/Building Event`
- `Monopoly Prototype/SO Events/Money Change Requested`
- `Monopoly Prototype/SO Events/Money Changed`

The concrete event classes contain private fields marked with `[SerializeField]`, such as `onRaised`. Unity displays these fields in the Inspector even though the field is not public. Add persistent Inspector callbacks there when the callback is intentionally part of the asset configuration.

Each `Raise(...)` call invokes:

1. Persistent callbacks stored in the serialized `UnityEvent`, in Unity's serialized listener order.
2. Runtime callbacks registered through the typed `Register` API, in ascending `order` and then registration sequence.

Persistent Inspector callback order and runtime registration order are separate concerns. Do not assume that a runtime `order` value can reorder persistent Inspector callbacks.

## Runtime Registration

Use the concrete event's typed `UnityAction` API:

```csharp
using UnityEngine;
using UnityEngine.Events;

namespace MonopolyPrototype
{
    public sealed class IntEventConsumer : MonoBehaviour
    {
        [SerializeField] private IntSOEvent scoreChanged;

        private void OnEnable()
        {
            scoreChanged.Register(OnScoreChanged, order: 100);
        }

        private void OnDisable()
        {
            scoreChanged.Unregister(OnScoreChanged);
        }

        private void OnScoreChanged(int value)
        {
            Debug.Log($"Score changed: {value}");
        }
    }
}
```

The registration contract is:

- `Register(listener, order)` adds a runtime callback.
- Registering an equivalent delegate again updates its order instead of adding a duplicate entry.
- Lower order values run first.
- Equal order values run in the order the callbacks were first registered.
- `Unregister(listener)` returns `true` only when a callback was removed.
- Registering `null` throws `ArgumentNullException`; unregistering `null` returns `false`.
- `Raise(...)` takes a snapshot of the runtime listeners. Registering or unregistering during a raise affects the next raise, not the current iteration.

Register and unregister from the lifetime owner of the callback. The common pattern for a `MonoBehaviour` adapter is `OnEnable`/`OnDisable`; a pure C# adapter should use its own explicit attach/detach lifecycle.

`SOEvent.OnDisable()` clears runtime listeners. Runtime registrations are not serialized into the asset, so they must be restored by the owner after a domain reload or a new Play session.

## Payload And Array Semantics

UnityEvent does not provide an Inspector-friendly C# `ref` or `out` event signature. For a pass-by-reference-like processing chain, use an array payload:

```csharp
private void ApplyModifier(int[] values)
{
    values[0] += 10;
}

private void ReadModifiedValue(int[] values)
{
    Debug.Log(values[0]);
}

// The same array instance is passed to both listeners.
modifierEvent.Raise(new[] { 5 });
```

`IntArraySOEvent` and `IntArrayIntSOEvent` do not clone the array. If an earlier listener changes an element, later listeners and the caller observe that change. Treat this as an explicit mutable pipeline, document which listener owns each index, and avoid sharing a payload outside the raise unless that lifetime is intentional.

Money request payloads follow the same convention without discarding request metadata. `MoneyChangeRequest` remains the typed reference passed by `MoneyChangeRequestedSOEvent`, but its adjustable amount is stored in one request-owned `int[] CurrentDeltaPayload`. Element `0` is the only amount slot. `CurrentDelta` is a convenience property over that slot, and `SetCurrentDelta(...)`/`AddToCurrentDelta(...)` update the same array. A later listener therefore observes the exact value written by an earlier listener or probe callback, and the caller's request observes it after `Raise(...)` returns.

For a different parameter list, add a new concrete event type rather than weakening an existing event's signature. Define a serializable `UnityEvent<...>` subclass, keep its serialized event field private, and give the SOEvent typed `Register`, `Unregister`, and `Raise` methods that use the matching `UnityAction<...>` delegate.

## Extending With A New Concrete Event

Reusable generic concrete events belong under `Assets/Scripts/MonopolyPrototype/SOEvents/` and stay in the `MonopolyPrototype.SOEvents` assembly. Application-specific concrete events, such as the money events, belong under `Assets/Scripts/MonopolyPrototype/BuildingEvents/` and use the application runtime assembly while reusing the same `SOEvent` base and ordered listener helper. Follow the existing concrete event pattern:

1. Add a `[Serializable]` subclass of the required `UnityEvent<...>` type.
2. Add a `[CreateAssetMenu]` concrete `ScriptableObject` that derives from `SOEvent`.
3. Add a private `[SerializeField]` field for the serializable UnityEvent subclass.
4. Keep a private `OrderedEventListeners<UnityAction<...>>` registry for runtime callbacks.
5. Implement typed `Register`, `Unregister`, and `Raise` methods.
6. Implement `RuntimeListenerCount` and `ClearRuntimeListeners`.
7. Add EditMode tests for raise payloads, unregister behavior, order, and any mutation semantics.
8. Update this guide if the new type introduces a usage rule or a new supported payload pattern.

Do not add UI-specific behavior, player-state mutation, or scene lookups to the SOEvent asset itself. Those belong in the callback receiver or an integration adapter.

## Boundary With Building Effects

The current building pipeline is intentionally separate:

```text
BuildingConfig asset
        |
        v
pure BuildingDefinition
        |
        v
BuildingRuleResolver
        |
        v
presentation-agnostic BuildingEffectCommand values
```

`BoardMoveResolver`, `BuildingRuleResolver`, `BuildingDefinition`, and `BuildingEffectCommand` must not depend on SOEvent, UnityEvent, UI, or MonoBehaviour listeners.

Building integration uses the following application-boundary flow:

```text
pure rule result or BuildingEffectCommand
        |
        v
BuildingEventBridge
        |
        v
BuildingEventProfile -> optional BuildingEventSOEvent.Raise(...)
```

`BuildingConfig` may reference a `BuildingEventProfile` as application metadata. `BuildingConfig.ToDefinition()` ignores this reference, and the bridge retains the original asset-side mapping by tile index. The adapter translates a resolved move event or command into a `BuildingEventContext` payload, but the core resolver remains usable without any event asset. Do not make `BuildingEffectAsset.ToCommand()` raise an event, and do not make a building effect asset discover or invoke UI listeners.

`BuildingEventProfile` has three optional channels:

- `OnBuildingTriggered`: raised once when the building event has been resolved.
- `OnEffectCommandProduced`: raised once per resolved command, preserving the building effect list order.
- `OnConfirmationCompleted`: raised only after the presentation flow completes the confirmation wait.

All current building assets reference the shared `Assets/Data/BuildingEvents/General.asset`, whose three channels point to the global building event assets. A future building-specific profile can replace that reference without changing the bridge. Persistent Inspector callbacks and runtime ordered listeners keep the ordering rules defined above. The profile and bridge do not own listener lifetimes; callback owners still register in `OnEnable` and unregister in `OnDisable`.

Effect events are currently limited to `AdjustMoneyEffectAsset`. It stores optional `MoneyChangeRequestedSOEvent` and `MoneyChangedSOEvent` references directly in the Effect asset, so positive and negative adjustments use the same configuration shape. Other Effect assets do not carry an event profile until they have a concrete event requirement.

For money effects, the bridge raises `MoneyChangeRequestedSOEvent` after an `AdjustMoney` command is produced. The request wrapper preserves `BaseDelta`, exposes `CurrentDelta` and `CurrentDeltaPayload`, and includes the originating building, Effect name/index, tile, and timing. A future money state adapter owns the balance mutation and should raise `MoneyChangedSOEvent` with `MoneyChangeResult` after applying the request. UI and scene presentation should listen to the result event because it contains the actual applied delta and final balance.

### Money Request Debug Probe

`MoneyChangedDebugProbeSO` is an optional Debug/Extension asset for demonstrating the mutable request chain. The checked-in default is `Assets/Data/BuildingEvents/MoneyChangedDebugProbe.asset`; additional instances can be created from `Monopoly Prototype/Debug/Money Changed Debug Probe`. The default asset is intentionally unbound, so it does not alter prototype money requests until its request overloads are added to the `onRaised` Inspector UnityEvent on a `MoneyChangeRequestedSOEvent` asset:

- `AddMoney(MoneyChangeRequest)` adds the configured `addAmount` to payload element `0`.
- `SubtractMoney(MoneyChangeRequest)` subtracts the configured `subtractAmount` from payload element `0`.
- `OverrideMoney(MoneyChangeRequest)` replaces payload element `0` with the configured `overrideAmount`.
- `LogMoney(MoneyChangeRequest)` writes the current amount to the Unity log without changing it.

The same asset also exposes `int[]` overloads for use with `IntArraySOEvent` or another array-shaped Inspector UnityEvent. These overloads mutate element `0` in place. The probe is a passive callback target: it does not register itself, discover events, own a balance, depend on UI or a MonoBehaviour listener, or play coin effects. Persistent callbacks execute before runtime listeners, so a later request observer sees the probe's mutation during the same raise.

### Money Changed Coin Feedback

`SampleScene` contains a `Money Changed Coin Feedback` child under `Prototype Bootstrapper`. Its `moneyChangedEvent` reference points to `Assets/Data/BuildingEvents/MoneyChanged.asset`, and its optional camera reference points to the scene's perspective `Main Camera`. On enable it registers `OnMoneyChanged` with that result event; on disable it unregisters the same callback. A received `MoneyChangeResult` creates a transient world-space primitive cylinder and `TextMesh` showing `+N` or `-N` from `AppliedDelta`, then floats and destroys the feedback object.

This component must remain bound to `MoneyChangedSOEvent`, not `MoneyChangeRequestedSOEvent`. It does not modify request payloads, apply balances, or make the money state adapter's job part of the presentation layer. The current prototype still does not raise `MoneyChangedSOEvent` during normal building flow; the PlayMode test raises a result directly only to verify the scene binding and 3D feedback object.

UI confirmation follows the same rule. Core logic reports that confirmation is required; a presentation-layer flow may raise an SOEvent after confirmation if a later design needs that notification.

## Testing Checklist

For a new SOEvent or a behavior change, add or update EditMode tests that verify:

- The typed payload reaches runtime listeners unchanged.
- Runtime listeners execute in ascending order.
- Equal order values remain stable by registration sequence.
- `Unregister` prevents future raises.
- A mutable array is the same reference for the entire listener chain when that behavior is required.
- Money request probe callbacks mutate the shared `CurrentDeltaPayload`, and later listeners observe the mutation.
- `MoneyChangedCoinFeedback` registers and unregisters against `MoneyChangedSOEvent`, formats the applied delta, and does not subscribe to the request event.
- Runtime listeners are cleared by `ClearRuntimeListeners`.

Run the SOEvent EditMode tests together with the existing core rule tests. The current SOEvent test assembly is `MonopolyPrototype.SOEvents.EditModeTests`; probe and feedback coverage lives beside `SOEventTests.cs` in that assembly.

## Checklist For Future Codex Sessions

Before changing or integrating SOEvent, a later session should:

1. Read `AGENTS.md`, `docs/game-logic/logic-overview.md`, and this guide.
2. Inspect the existing concrete event whose parameter shape is closest to the requested feature.
3. Decide whether the requirement is an event extension or a core rule/value/command requirement.
4. Keep core rules independent from SOEvent and MonoBehaviour listeners.
5. Add a new concrete event for a genuinely new parameter list instead of overloading an unrelated event.
6. Register and unregister runtime callbacks from the owning lifecycle.
7. Add focused EditMode tests and update this guide plus `logic-overview.md` for important behavior changes.
8. When integrating an Effect with SOEvents, add direct event references only to the concrete Effect that needs them; never raise from `BuildingEffectAsset.ToCommand()` or the pure rule resolver.

Useful prompt prefix for a new session:

```text
Read AGENTS.md, docs/game-logic/logic-overview.md, and
docs/game-logic/so-event-guide.md before editing. Keep SOEvent as an
independent extension layer. Do not connect it to BuildingEffectAsset or
BuildingEffectCommand unless this task explicitly asks for that integration.
```
