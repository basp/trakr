---
sessionId: session-261104-235539-3pk5
---

# Requirements

### Overview & Goals
Create a new OpenSpec change that defines and documents tracker-style speed/tempo effect execution for the existing playback engine, without implementing code in this workflow.

### Scope
**In Scope**
- Create a new change using the existing default `spec-driven` schema in `openspec/config.yaml`.
- Treat this as a modification of the existing `openspec/specs/song-playback/spec.md` capability, not a new capability tree.
- Define behavior for tracker-style transport effects in playback:
  - `Axx` updates playback speed.
  - `Fxx` follows tracker-style split semantics, with low values affecting speed and higher values affecting tempo.
- Ground the change in the current playback flow exposed by `src/Trakr.Core/PlaybackEngine.cs`, `src/Trakr.Core/PlaybackStep.cs`, `src/Trakr.Core/IPlaybackEventSink.cs`, `src/Trakr/Program.cs`, and `src/Trakr.Tests/PlaybackEngineTests.cs`.
- Produce the planning artifacts required for implementation readiness: `proposal.md`, `specs/song-playback/spec.md`, `design.md`, and `tasks.md`.

**Out of Scope**
- Implementing effect execution in `src/` during this workflow.
- Adding pattern jump/break, volume, or other non-transport effects.
- Creating a new capability path unrelated to `song-playback`.
- Defining console formatting as part of the stable contract.

### User Stories
- As a developer, I want the playback spec to define how speed and tempo effects alter transport so future implementation work has clear acceptance criteria.
- As a maintainer, I want the change scoped to `song-playback` so effect behavior stays aligned with the existing playback contract instead of fragmenting into premature specs.

### Functional Requirements
- The change must describe when row-start effect commands are interpreted relative to tick stepping in `PlaybackEngine`.
- The change must specify observable outcomes for `PlaybackPosition`, `PlaybackStep.Speed`, `PlaybackStep.Tempo`, and emitted tick/effect events when `Axx` and `Fxx` are present.
- The change must preserve current deterministic event ordering already documented in `song-playback`.
- The tasks artifact must break future implementation into concrete testable milestones tied to the current code structure.

# Technical Design

### Current Implementation
- `src/Trakr.Core/ChannelEvent.cs` stores an optional `EffectCommand` per channel event.
- `src/Trakr.Core/EffectCommand.cs` normalizes effect codes but does not assign behavior.
- `src/Trakr.Core/PlaybackEngine.cs` currently exposes fixed `Speed` and `Tempo` from `Song.InitialSpeed` and `Song.InitialTempo`; it forwards row-start effect payloads into `PlaybackStep` but never applies them to transport state.
- `src/Trakr.Core/PlaybackStep.cs` and `src/Trakr.Core/PlaybackStepEventSinkAdapter.cs` preserve and emit effect payloads in deterministic row-start order.
- `src/Trakr/Program.cs` already demonstrates `A03`, `D0F`, and `F7D`, while `src/Trakr/ConsolePlaybackEventSink.cs` labels effect output as placeholder behavior.
- `src/Trakr.Tests/PlaybackEngineTests.cs` currently verifies sequencing and effect emission order, but not effect execution semantics.
- `openspec/specs/song-playback/spec.md` currently covers song validity, position stepping, empty-pattern skipping, structured playback steps, and deterministic event ordering, so transport-effect behavior should extend this existing capability.

### Key Decisions
- **Modify `song-playback` instead of adding a new capability.** Effect execution changes the same playback contract already owned by `PlaybackEngine`, `PlaybackStep`, and the existing `song-playback` spec.
- **Document tracker-style `Axx`/`Fxx` semantics.** This matches the user’s chosen direction and aligns with `IDEA.md`, which favors tracker-compatible concepts over ad hoc rules.
- **Keep the proposal limited to transport effects.** Pattern-control and other effect families remain separate future changes.

### Proposed Changes
- Create a new OpenSpec change named `add-speed-tempo-effects` using the existing `spec-driven` schema.
- Author `proposal.md` to explain why the current effect model is incomplete and why the first effect delta should stay within transport semantics.
- Author the spec delta at `specs/song-playback/spec.md` to add requirements/scenarios for:
  - Row-start interpretation of `Axx` and `Fxx`.
  - How new speed/tempo values affect the current or next tick/row boundary.
  - How playback reports updated transport values through `PlaybackStep` and `IPlaybackEventSink`.
  - How unsupported effect codes remain outside this change.
- Author `design.md` to describe the current gap in `PlaybackEngine`, the intended execution point for transport effects, and the contract boundary between effect interpretation and event emission.
- Author `tasks.md` to map follow-up implementation work onto the existing codebase, centered on `PlaybackEngineTests.cs`, `PlaybackEngine.cs`, `Program.cs`, and any supporting model updates if needed.

### File Structure
- New change directory under `openspec/changes/<new-change-name>/`.
- `openspec/changes/<new-change-name>/proposal.md`
- `openspec/changes/<new-change-name>/design.md`
- `openspec/changes/<new-change-name>/tasks.md`
- `openspec/changes/<new-change-name>/specs/song-playback/spec.md`

### Risks
- `Fxx` tracker-style threshold semantics must be stated explicitly to avoid ambiguity between speed and tempo handling.
- The spec must distinguish current observed behavior from planned behavior, since the current engine only forwards effect metadata.
- The proposal should avoid promising broader tracker compatibility than the existing model supports today.

# Testing

### Validation Approach
- Compare the planned spec delta against the existing playback contract in `openspec/specs/song-playback/spec.md` to ensure the new change extends, rather than contradicts, current sequencing rules.
- Cross-check artifact contents against `src/Trakr.Core/PlaybackEngine.cs`, `src/Trakr.Core/PlaybackStep.cs`, `src/Trakr.Core/IPlaybackEventSink.cs`, `src/Trakr/Program.cs`, and `src/Trakr.Tests/PlaybackEngineTests.cs` so every requirement is anchored in concrete code seams.
- Validate that the produced change contains all required planning artifacts for the `spec-driven` schema and that the artifact dependency chain is complete.

### Key Scenarios
- Row-start `Axx` changes the effective playback speed exposed by later playback steps.
- Row-start `Fxx` changes either speed or tempo according to tracker-style rules, with expected transport values visible in tick output.
- Existing deterministic event ordering remains intact while transport values change.
- No new requirements accidentally pull in pattern-control or audio-output behavior.

### Test Changes
- Future implementation tasks should call for new or updated tests in `src/Trakr.Tests/PlaybackEngineTests.cs` covering speed/tempo effect execution and any resulting position/tick semantics.

# Delivery Steps

### ✓ Step 1: Define the OpenSpec scope and capability delta for transport effects
The new change is scoped to tracker-style speed/tempo effect behavior within the existing `song-playback` capability.
- Confirm the change name and create it under `openspec/changes/` using the default `spec-driven` schema.
- Reuse `openspec/specs/song-playback/spec.md` as the capability target instead of introducing a new capability path.
- Capture the motivation and scope in `proposal.md`, anchored to the current gap between `EffectCommand` modeling and `PlaybackEngine` behavior.

### ✓ Step 2: Draft the spec and design artifacts around the current playback architecture
The change documents exactly how `Axx` and `Fxx` should affect playback transport and where that behavior belongs in the architecture.
- Extend the spec delta in `specs/song-playback/spec.md` with transport-effect requirements and observable scenarios.
- Describe in `design.md` how `src/Trakr.Core/PlaybackEngine.cs`, `src/Trakr.Core/PlaybackStep.cs`, `src/Trakr.Core/IPlaybackEventSink.cs`, and `src/Trakr/Program.cs` relate to the new contract.
- Record assumptions and limits so unsupported effect families remain clearly out of scope.

### ✓ Step 3: Produce implementation-ready tasks and verify artifact completeness
The change package ends with concrete implementation tasks and a complete, schema-aligned artifact set.
- Write `tasks.md` as staged follow-up work tied to `src/Trakr.Tests/PlaybackEngineTests.cs`, `src/Trakr.Core/PlaybackEngine.cs`, and related playback surfaces.
- Check that all required planning artifacts for the change exist and align with the repository’s established OpenSpec conventions from the archived `sync-openspec-with-current-project-state` change.
- Ensure the final artifact set is ready for later apply work without making any `src/` code changes in this planning workflow.