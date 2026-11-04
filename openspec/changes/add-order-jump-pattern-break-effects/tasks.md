# Tasks

## 1. Validate supported flow-control targets before playback

- [x] 1.1 Extend `src/Trakr.Core/Song.cs` with upfront validation for supported `Bxx` and `Dxx` destinations, and add `src/Trakr.Tests/PlaybackEngineTests.cs` coverage for invalid order targets, missing break destination orders, invalid break rows, and valid edge destinations.

## 2. Implement row-start flow-control redirects in playback

- [x] 2.1 Update `src/Trakr.Core/PlaybackEngine.cs` to resolve supported `Bxx`/`Dxx` commands during tick-zero row scanning, store pending order and row overrides, and apply them only after the current row completes.
- [x] 2.2 Keep row-start transport behavior and observable row-start effect ordering aligned by reusing shared flow-target resolution and verifying that the current row still reports its final speed and tempo before any redirect takes effect.

## 3. Lock down precedence, normalization, and demo behavior

- [x] 3.1 Add `src/Trakr.Tests/PlaybackEngineTests.cs` coverage for combined jump-and-break rows, channel-order last-wins behavior, empty-pattern jump normalization, and preserved supported/unsupported effect observability.
- [x] 3.2 Review `src/Trakr/Program.cs` against the new validation rules and run `dotnet test src\Trakr.Tests\Trakr.Tests.csproj` plus `dotnet run --project src\Trakr\Trakr.csproj` to verify the playback suite and demo behavior remain correct.

## Workflow follow-up

- Archive the change after implementation review is complete.
- Verify the archived `song-playback` spec merges the new flow-control requirements cleanly.