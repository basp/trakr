# Proposal

## Why

`Trakr` still hard-coded a demo `Song` directly in `Program.cs`, which meant changing playback input required source edits and left the project without a reusable input pipeline. Adding a first external loader now creates a durable boundary between host file access and core song construction before more formats or hosts arrive.

## What Changes

- Introduce a new `song-loading` capability for loading validated songs from a JSON snapshot format.
- Add a reusable `SongJsonLoader` in `src/Trakr.Core` that maps JSON into `Song`, `Pattern`, `Row`, `ChannelEvent`, and `EffectCommand`.
- Update the console host to load a checked-in sample JSON file by default and accept an optional first command-line argument as a path override.
- Add focused tests for valid loads, malformed JSON, missing required structure, and semantic validation failures.
- Document the host/core boundary, JSON contract, and design decisions in a new root-level `GUIDE.md`.

## Capabilities

### New Capabilities
- `song-loading`: Load validated `Song` instances from a JSON snapshot format before playback begins.

### Modified Capabilities
- None.

## Impact

- Affects `src/Trakr.Core/SongJsonLoader.cs`, which adds the reusable JSON-to-model loading boundary.
- Affects `src/Trakr/Program.cs` and `src/Trakr/Trakr.csproj`, which now resolve a runtime JSON song file instead of building a hard-coded demo song in source.
- Affects `src/Trakr.Tests/SongJsonLoaderTests.cs`, which verifies successful loads and failure modes.
- Adds `src/Trakr/demo-song.json` and `GUIDE.md` as the checked-in sample and architecture documentation for the new capability.