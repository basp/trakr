# Proposal

## Why

The playback engine already preserves row-start effect payloads, but it never applies them to transport state. Defining tracker-style `Axx` and `Fxx` behavior now closes that gap with a spec-first contract before implementation work starts to diverge.

## What Changes

- Extend playback requirements so row-start `Axx` commands update playback speed using tracker-style transport semantics.
- Define tracker-style `Fxx` split behavior so lower values change speed and higher values change tempo.
- Specify when transport effects are interpreted relative to tick stepping and row-start event emission.
- Clarify the observable impact on `PlaybackStep`, playback position progression, and event sink output while leaving non-transport effects out of scope.

## Capabilities

### New Capabilities

- None.

### Modified Capabilities

- `song-playback`: Add transport-effect requirements for tracker-style `Axx` and `Fxx` execution within the existing deterministic playback contract.

## Impact

- Affects `src/Trakr.Core/PlaybackEngine.cs`, where speed and tempo state are currently fixed after song initialization.
- Affects `src/Trakr.Core/PlaybackStep.cs` and `src/Trakr.Core/IPlaybackEventSink.cs`, which expose the observable transport values and row-start effect data.
- Affects `src/Trakr.Tests/PlaybackEngineTests.cs`, which will need coverage for speed and tempo execution semantics.
- Affects `src/Trakr/Program.cs` and `src/Trakr/ConsolePlaybackEventSink.cs` as demo surfaces that currently show effect payloads without applying transport behavior.