# Proposal

## Why

The repository already implements a small but coherent playback core in `Trakr.Core` and demonstrates it through the console app and tests, but OpenSpec does not yet describe that behavior. Capturing the current state now gives future changes a concrete baseline and reduces the risk of planning against outdated assumptions.

## What Changes

- Add the project's first capability spec for in-memory song playback behavior, covering song validity, deterministic tick/row sequencing, empty-pattern skipping, and event emission order.
- Document the design boundary between the immutable playback model in `Trakr.Core`, the console demo in `Trakr`, and the current xUnit validation in `Trakr.Tests`.
- Add implementation tasks for validating and reviewing the new OpenSpec materials against the current code and tests.

## Capabilities

### New Capabilities
- `song-playback`: Defines the current in-memory tracker song model and deterministic playback sequencing and event emission behavior.

### Modified Capabilities
- None.

## Impact

- Adds new OpenSpec materials under this change, including `specs/song-playback/spec.md`.
- Grounds the spec in `src/Trakr.Core/Song.cs`, `src/Trakr.Core/Pattern.cs`, `src/Trakr.Core/Row.cs`, `src/Trakr.Core/PlaybackEngine.cs`, and the playback event contracts.
- Uses `src/Trakr/Program.cs` and `src/Trakr/ConsolePlaybackEventSink.cs` as demo/reference behavior and `src/Trakr.Tests/UnitTest1.cs` as validation evidence.