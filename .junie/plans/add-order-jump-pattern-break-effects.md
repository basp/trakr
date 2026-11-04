---
sessionId: session-261105-003711-17k5
---

# Requirements

### Overview & Goals
Create a new OpenSpec change that extends the existing `song-playback` capability with tracker-style flow control at row start.

### Scope
#### In Scope
- Add `Bxx` order-jump behavior to playback.
- Add `Dxx` pattern-break behavior to playback.
- Treat `Dxx` as a raw zero-based row index in the destination order.
- Allow `Bxx` and `Dxx` to combine on the same row.
- Reject invalid jump/break destinations before playback begins.
- Preserve the current deterministic tick, row, channel, and effect ordering already defined for `song-playback`.

#### Out of Scope
- New parsing/file-loading formats.
- Terminal UI or audio output work.
- Non-flow effects beyond `Bxx` and `Dxx`.
- Broad changes to public playback result/event contracts unless required by the existing behavior model.

### Functional Requirements
- Row-start flow effects are interpreted on tick zero, alongside the existing transport-effect scan.
- `Bxx` schedules playback to continue at order `xx` after the current row finishes.
- `Dxx` schedules playback to continue at row `xx` in the destination order after the current row finishes.
- If a row has `Dxx` without `Bxx`, the destination order is the next sequential order.
- If a row has both `Bxx` and `Dxx`, playback continues at the jumped order and the broken row.
- If multiple supported flow effects appear on one row, channel-order determinism is preserved and the last command affecting each destination field wins.
- Invalid destination orders or rows are treated as invalid playback setup and must fail before the first playback step.
- Existing transport effects (`Axx`, `Fxx`) and unsupported-effect observability remain intact.

# Technical Design

### Current Implementation
- `openspec/specs/song-playback/spec.md` already defines validation, deterministic stepping, empty-pattern skipping, structured playback steps, transport effects, and passive unsupported effects.
- `src/Trakr.Core/PlaybackEngine.cs` is the only playback state machine. It scans row-start channel effects, applies supported transport effects immediately through `ApplyTransportEffect`, and advances through order/row/tick state with `AdvancePosition()` and `NormalizePosition()`.
- `src/Trakr.Core/Song.cs` currently validates static playback setup such as positive initial transport values and valid order references before playback starts.
- `src/Trakr.Core/PlaybackStep.cs` and `src/Trakr.Core/PlaybackStepEventSinkAdapter.cs` expose the observable per-tick contract and deterministic callback ordering.
- `src/Trakr.Tests/PlaybackEngineTests.cs` follows a focused rule-per-test style for sequencing, transport effects, unsupported effects, empty-pattern skipping, and event ordering.
- `src/Trakr/Program.cs` already includes a `D` effect in the demo song, which makes it a good reference surface for the new behavior.

### Key Decisions
- Modify the existing `song-playback` capability rather than introduce a new capability path; the OpenSpec delta should live under `specs/song-playback/spec.md` in the new change.
- Keep flow-effect handling inside the existing row-start effect scan in `PlaybackEngine`, matching the current transport-effect pattern.
- Apply `Bxx`/`Dxx` as pending next-position overrides: they are discovered on tick zero of the current row, but they take effect only when the row boundary is reached.
- Interpret `Dxx` as a raw zero-based row index, not tracker-decimal decoding.
- Reject invalid jump/break destinations before playback starts, following the current validation style seen in `Song.cs` rather than silently ignoring bad control flow.
- Preserve the current public observation boundary (`PlaybackStep`, `IPlaybackEventSink`, `PlaybackStepEventSinkAdapter`) unless implementation proves extra metadata is strictly necessary.

### Proposed Changes
- Extend the OpenSpec change artifacts to describe `Bxx` order jump, `Dxx` pattern break, their combined behavior, and their interaction with the existing deterministic stepping rules.
- Add playback-engine logic to resolve pending destination overrides from row-start effects while keeping the current same-row transport behavior unchanged.
- Add upfront validation over song rows/effects so impossible order or row destinations are rejected before playback begins.
- Reuse existing normalization behavior so destinations that land on valid empty ordered patterns continue through the existing `NormalizePosition()` skip logic.
- Update the console demo in `src/Trakr/Program.cs` only as a lightweight example surface after the core behavior is specified and tested.

### Data Models / Contracts
- No new public domain type is required; the likely implementation is internal pending destination state in `PlaybackEngine` such as:
  - `pendingOrderIndex`
  - `pendingRowIndex`
- Validation will need to inspect `Song.Patterns`, `Song.Order`, and row/channel `EffectCommand` values before stepping starts.
- Observable contracts stay centered on existing outputs:
  - `PlaybackStep.Position`
  - `PlaybackStep.Speed`
  - `PlaybackStep.Tempo`
  - `IPlaybackEventSink.OnTick/OnRow/OnChannelEvent/OnEffect`

### Risks
- `Bxx`/`Dxx` interact with both `AdvancePosition()` and `NormalizePosition()`, so empty-pattern skipping and end-of-song completion need explicit tests.
- Early validation may fit awkwardly between `Song` and `PlaybackEngine`; the change should keep responsibility clear instead of scattering rule checks.
- Combined transport and flow effects on one row can be easy to misread, so spec scenarios should separate “affects current row transport” from “affects next row destination.”

# Testing

### Validation Approach
- Extend `src/Trakr.Tests/PlaybackEngineTests.cs` with deterministic unit tests, following the current one-rule-per-scenario pattern.
- Validate behavior through `TryStep()`/`Step()` outputs and existing event-sink dispatch, not through console-only observation.

### Key Scenarios
- `Bxx` jumps to a different order after the current row completes.
- `Dxx` breaks to a specific row in the next sequential order.
- `Bxx` + `Dxx` on the same row combine into a jumped order plus explicit row target.
- Later channel events override earlier flow targets for the same field.
- Transport updates on the triggering row still apply immediately while flow redirects apply only after the row boundary.

### Edge Cases
- Jumping to the final order and completing after its final row.
- Jumping or breaking into an empty ordered pattern and preserving the existing skip behavior.
- Rejecting invalid order indices or invalid row indices before playback starts.
- Preserving unsupported-effect visibility and current event ordering when supported flow effects are present.

### Test Changes
- Add focused tests in `PlaybackEngineTests.cs` for valid `Bxx`, valid `Dxx`, combined commands, precedence, invalid-target rejection, and empty-pattern destination handling.
- Keep `Program.cs` as optional manual confirmation only; no dedicated console-surface test suite is needed for this change.

# Delivery Steps

### ✓ Step 1: Add upfront validation for jump and break destinations
Playback rejects invalid `Bxx` and `Dxx` destinations before the first step begins.
- Extend the current validation boundary in `src/Trakr.Core/Song.cs` or `src/Trakr.Core/PlaybackEngine.cs` so row-start flow commands are checked against `Song.Order` and the target pattern row counts.
- Define the exact validity rules for `Bxx` destination orders and `Dxx` raw zero-based row indices.
- Add failing and passing coverage in `src/Trakr.Tests/PlaybackEngineTests.cs` for invalid order targets, invalid row targets, and valid edge destinations.

### ✓ Step 2: Implement pending flow-control resolution in the playback engine
`PlaybackEngine` applies `Bxx` and `Dxx` as row-start commands that redirect playback only after the current row completes.
- Update `src/Trakr.Core/PlaybackEngine.cs` to scan supported flow effects during tick-zero step construction alongside existing transport effects.
- Store pending destination overrides so `AdvancePosition()` can switch to the chosen order and row at the next row boundary.
- Preserve current semantics where transport changes affect the current row immediately, while flow-control changes affect the next playback position.
- Keep `src/Trakr.Core/PlaybackStep.cs` and `src/Trakr.Core/PlaybackStepEventSinkAdapter.cs` compatible with the existing observable contract unless an internal-only helper is sufficient.

### ✓ Step 3: Lock down combined-command and normalization behavior
Playback handles combined `Bxx` + `Dxx` rows, channel-order precedence, and empty-pattern destinations deterministically.
- Add targeted tests in `src/Trakr.Tests/PlaybackEngineTests.cs` for combined jump-and-break rows and last-wins channel-order behavior.
- Verify redirected playback still cooperates with `NormalizePosition()` when a destination order resolves to an empty pattern.
- Confirm unsupported effects remain observable and event dispatch ordering stays unchanged when supported flow effects are present.
- Refresh `src/Trakr/Program.cs` only as a lightweight demo example if the current hard-coded song should showcase the new behavior.