# Tasks

## 1. Lock down transport-effect expectations in tests

- [x] 1.1 Extend `src/Trakr.Tests/PlaybackEngineTests.cs` with `TryStep` and `Step` coverage for row-start `Axx` speed changes and low-value `Fxx` speed changes, and verify the targeted playback-engine tests pass.
- [x] 1.2 Add `src/Trakr.Tests/PlaybackEngineTests.cs` coverage for high-value `Fxx` tempo changes, unsupported effect pass-through, and same-row channel-order precedence, and verify the targeted playback-engine tests pass.

## 2. Implement tracker-style transport effects in the playback engine

- [x] 2.1 Update `src/Trakr.Core/PlaybackEngine.cs` to maintain mutable transport state and apply supported tick-zero `Axx`/`Fxx` effects in ascending channel order, and verify the new speed/tempo tests pass.
- [x] 2.2 Keep `PlaybackStep`/`IPlaybackEventSink` observation aligned by ensuring tick-zero `PlaybackStep` and emitted tick events expose the updated transport values without changing row-start event ordering, and verify the related event-order assertions pass.

## 3. Confirm demo and regression behavior

- [x] 3.1 Review `src/Trakr/Program.cs` and `src/Trakr/ConsolePlaybackEventSink.cs` against the implemented transport rules, updating only what is needed to keep the demo output consistent, and verify the demo still runs through the sample song.
- [x] 3.2 Run `dotnet test` for `src/Trakr.Tests` and verify the full playback test suite passes without regressions.

## Workflow follow-up

- Archive the change after implementation review is complete.
- Verify the archived `song-playback` spec merges the new transport-effect requirements cleanly.