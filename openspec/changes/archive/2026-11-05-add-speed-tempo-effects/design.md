# Design

## Context

`PlaybackEngine` currently copies `Song.InitialSpeed` and `Song.InitialTempo` into immutable engine properties, emits those values through `PlaybackStep` and `IPlaybackEventSink.OnTick`, and never interprets the `EffectCommand` values already present on row-start channel events. See `proposal.md` for motivation; this design only defines how the existing playback flow should apply tracker-style transport effects without changing its deterministic event ordering.

## Goals / Non-Goals

**Goals:**
- Apply tracker-style `Axx` and `Fxx` transport semantics inside the existing playback flow.
- Keep `PlaybackStep`, `PlaybackStepEventSinkAdapter`, and `IPlaybackEventSink` aligned so structured playback and callback-style playback observe the same speed and tempo values.
- Preserve existing row-start event ordering while making transport updates visible on the same tick-zero step that carries the triggering effect payloads.
- Keep the implementation focused on transport effects and testable through `PlaybackEngineTests`.

**Non-Goals:**
- Adding behavior for pattern-control, volume, or any non-transport effects.
- Changing the public shape of `EffectCommand`, `PlaybackStep`, or `IPlaybackEventSink` beyond what is needed to surface the already-existing speed and tempo data.
- Treating console formatting in `ConsolePlaybackEventSink` as part of the stable contract.

## Decisions

### Interpret supported effects during tick-zero step construction
`PlaybackEngine` should evaluate row-start effects while creating the tick-zero `PlaybackStep`, before the step is returned or dispatched. This lets the returned `PlaybackStep.Speed`, `PlaybackStep.Tempo`, and `OnTick` callback reflect the updated transport values immediately, while keeping row, channel, and effect emissions in their existing order.

**Alternatives considered:**
- Apply effects after dispatching the tick-zero step: rejected because `PlaybackStep` and `OnTick` would expose stale transport values for the triggering row.
- Apply effects only when advancing to the next row: rejected because the spec requires the current row start to report the updated transport state.

### Make engine transport state mutable and row-driven
`PlaybackEngine` should store mutable current speed and tempo fields instead of immutable properties backed only by `Song.InitialSpeed` and `Song.InitialTempo`. Tick advancement must use the current speed in effect after row-start interpretation so a speed change can alter the length of the row that introduced it.

**Alternatives considered:**
- Keep immutable properties and compute temporary override values per step: rejected because `AdvancePosition` also needs the updated speed, which would split one piece of transport state across multiple code paths.
- Add a separate transport-state object: rejected as unnecessary complexity for two integers owned by a single engine.

### Interpret transport effects in channel order and keep unsupported effects passive
The engine should scan row-start channel events in ascending channel order, applying supported transport commands as it encounters them. If multiple supported effects update the same field on one row, the later channel wins; unsupported codes stay observable in `PlaybackStep.Effects` and `OnEffect` but do not mutate transport.

**Alternatives considered:**
- First supported effect wins: rejected because it would conflict with the playback system’s established channel-order determinism when multiple row-start effects are present.
- Reject unsupported effect codes during playback: rejected because the current engine already preserves them as metadata and this change is intentionally scoped to transport execution only.

### Cover the behavior with focused playback-engine tests
`PlaybackEngineTests` should grow around `TryStep`, `Step`, and event-sink dispatch so the implementation is validated through observable speed, tempo, position, and event-order outcomes. The console demo in `Program.cs` can remain a lightweight manual example once the engine behavior is covered in tests.

**Alternatives considered:**
- Rely on manual console verification alone: rejected because the transport rules affect per-tick state and are better locked down with deterministic assertions.

## Risks / Trade-offs

- [Same-row speed changes alter row length] → Add tests that assert both the tick-zero step values and the exact row boundary after `Axx` or low-value `Fxx` commands.
- [Immediate transport updates could accidentally reorder event callbacks] → Keep transport interpretation inside step construction and verify that `OnTick`, `OnRow`, `OnChannelEvent`, and `OnEffect` still arrive in the current sequence.
- [Tracker-style `Fxx` semantics are easy to misread] → Encode the `< 0x20` speed and `>= 0x20` tempo split directly in tests and implementation helpers instead of leaving it implicit.

## Migration Plan

1. Extend `PlaybackEngineTests.cs` with failing cases for row-start speed and tempo effects, same-row row-boundary behavior, and unsupported effect pass-through.
2. Update `PlaybackEngine.cs` to maintain mutable transport state, interpret supported tick-zero effects in channel order, and use the updated speed during position advancement.
3. Keep `PlaybackStep` and `PlaybackStepEventSinkAdapter` as the transport-observation boundary, verifying that dispatch still emits events in the documented order.
4. Run the targeted test suite and optionally review `Program.cs` output to confirm the demo now reflects the implemented transport behavior.