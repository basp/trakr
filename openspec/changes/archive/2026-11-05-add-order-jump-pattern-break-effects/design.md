# Design

## Context

`PlaybackEngine` already scans row-start channel effects on tick zero to apply supported transport changes and to expose all row-start effects through `PlaybackStep` and `IPlaybackEventSink`. The engine still advances only sequentially through order and row positions, though, so the existing `Bxx` and `Dxx` effect payloads can be observed but cannot change playback flow. See `proposal.md` for motivation; this design only defines how the existing playback flow should apply tracker-style order jump and pattern break behavior without changing deterministic callback ordering.

## Goals / Non-Goals

**Goals:**
- Apply tracker-style `Bxx` order jump and `Dxx` pattern break semantics inside the existing playback flow.
- Reject impossible jump or break destinations before the first playback step, matching the existing song-validation boundary.
- Preserve the current rule that transport changes affect the current row immediately while flow-control changes affect only the next playback position after the current row completes.
- Keep `PlaybackStep`, `PlaybackStepEventSinkAdapter`, and `IPlaybackEventSink` aligned so structured playback and callback-style playback observe the same row-start effect ordering.

**Non-Goals:**
- Adding behavior for effects beyond `Bxx` and `Dxx`.
- Introducing tracker file parsing or changing how songs are constructed in memory.
- Changing the public shape of `PlaybackStep`, `EffectCommand`, or `IPlaybackEventSink` unless strictly required by the existing behavior model.

## Decisions

### Resolve supported flow-control effects during the existing tick-zero row scan
`PlaybackEngine` should detect `Bxx` and `Dxx` while it already scans row-start channel effects for transport commands and row-start payload collection. This keeps supported flow commands in the same deterministic channel-order pipeline as existing effect handling and avoids introducing a second pass that could drift from the observable event order.

**Alternatives considered:**
- Scan flow-control effects only while advancing position: rejected because it would duplicate row inspection logic and make precedence rules harder to keep aligned with row-start payload ordering.
- Add separate preprocessing of all row effects during song construction: rejected because playback still needs per-row deterministic resolution and validation can stay narrower than full execution.

### Apply flow-control redirects as pending next-position overrides
Supported `Bxx` and `Dxx` commands should be resolved on tick zero of the current row, but they should not move playback immediately. Instead, the engine should store pending order and row overrides and apply them only when the row boundary is reached, after the current row’s ticks have finished.

**Alternatives considered:**
- Jump immediately on tick zero: rejected because the spec requires the current row to remain observable and complete normally before redirecting playback.
- Encode redirect state directly in `PlaybackStep`: rejected because the public playback result does not need new flow metadata to support the behavior.

### Share flow-target resolution between validation and playback
The rules that identify supported flow commands, default the destination order for `Dxx`, and apply last-wins channel-order precedence should live in one internal helper that both `Song` validation and `PlaybackEngine` can reuse. This keeps validation and runtime resolution aligned and reduces the chance of accepting a destination that playback would interpret differently.

**Alternatives considered:**
- Duplicate the resolution logic in `Song` and `PlaybackEngine`: rejected because precedence and destination defaults are easy to let drift apart.
- Move all validation into `PlaybackEngine`: rejected because invalid playback setup should still fail before playback begins, matching the current model.

### Reuse existing position normalization for redirected destinations
After applying a pending redirect at a row boundary, playback should continue through the existing `NormalizePosition()` path so valid destinations that land on empty ordered patterns keep the current skip behavior. This minimizes new traversal rules and keeps end-of-song handling in one place.

**Alternatives considered:**
- Special-case empty-pattern skipping only for redirected positions: rejected because it would duplicate normalization behavior and create a second traversal path.

### Cover the behavior with focused playback-engine tests
`PlaybackEngineTests` should validate invalid targets, redirected positions, combined `Bxx` + `Dxx` rows, precedence, empty-pattern destinations, and event observability through `TryStep`, `Step`, and event-sink dispatch. `Program.cs` can remain a lightweight manual example once the core behavior is covered in tests.

**Alternatives considered:**
- Rely on manual console verification alone: rejected because redirect timing and precedence rules are better locked down with deterministic assertions.

## Risks / Trade-offs

- [Redirect timing could accidentally skip the triggering row] → Keep redirects pending until the row boundary and assert both the tick-zero step and the resulting next position in tests.
- [Validation and runtime resolution could diverge] → Reuse a single helper for detecting and resolving supported flow commands.
- [Empty-pattern destinations could regress existing traversal behavior] → Route redirected positions through `NormalizePosition()` and test a jump into an empty ordered pattern.

## Migration Plan

1. Extend `PlaybackEngineTests.cs` with invalid-target coverage and valid edge destinations for `Bxx` and `Dxx`.
2. Update `Song.cs` and a shared internal helper so invalid supported flow-control targets are rejected before playback begins.
3. Update `PlaybackEngine.cs` to resolve supported flow-control effects during tick-zero processing, store pending redirects, and apply them after the current row finishes.
4. Add tests for combined commands, precedence, empty-pattern normalization, and preserved event ordering, then verify the targeted test suite and demo song behavior.