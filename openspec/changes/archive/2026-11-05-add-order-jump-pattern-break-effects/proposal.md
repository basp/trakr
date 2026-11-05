# Proposal

## Why

The playback engine already preserves row-start effect payloads, but it still treats tracker-style flow commands as metadata only. Defining `Bxx` order jump and `Dxx` pattern break behavior now closes that gap with a spec-first contract before more playback features build on incomplete control-flow semantics.

## What Changes

- Extend playback requirements so row-start `Bxx` commands redirect playback to a different order after the current row completes.
- Define row-start `Dxx` pattern break behavior using the raw effect parameter as a zero-based destination row.
- Specify how `Bxx` and `Dxx` combine on the same row, including channel-order last-wins behavior for supported flow commands.
- Require invalid jump or break destinations to be rejected before playback begins while preserving existing transport-effect and unsupported-effect observability.

## Capabilities

### New Capabilities

- None.

### Modified Capabilities

- `song-playback`: Add tracker-style `Bxx` order jump and `Dxx` pattern break requirements within the existing deterministic playback contract.

## Impact

- Affects `src/Trakr.Core/Song.cs`, which currently validates playback setup before playback begins.
- Affects `src/Trakr.Core/PlaybackEngine.cs`, where row-start effects are scanned and playback position advances across orders, rows, and ticks.
- Affects `src/Trakr.Tests/PlaybackEngineTests.cs`, which needs coverage for valid and invalid flow-control behavior, precedence, and normalization.
- Affects `src/Trakr/Program.cs` as a demo surface that already carries a `D` effect and should remain valid under the new playback rules.