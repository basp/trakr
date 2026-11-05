# Tasks

## 1. Add a reusable JSON loader in `Trakr.Core`

- [x] 1.1 Add `src/Trakr.Core/SongJsonLoader.cs` to decode the JSON snapshot format into validated `Song` instances, and verify core loader coverage with `dotnet test src\Trakr.Tests\Trakr.Tests.csproj --filter SongJsonLoaderTests`.
- [x] 1.2 Add `src/Trakr.Tests/SongJsonLoaderTests.cs` for valid loads, sparse channels, malformed JSON, missing required arrays, and semantic validation failures, and verify those cases pass in the targeted loader test run.

## 2. Wire the console host to external song files

- [x] 2.1 Update `src/Trakr/Program.cs` to load a default sample JSON file and accept a first-argument path override, and verify `dotnet run --project src\Trakr\Trakr.csproj` loads and plays the sample successfully.
- [x] 2.2 Add `src/Trakr/demo-song.json` plus the `src/Trakr/Trakr.csproj` content entry needed to copy it to the output directory, and verify the app can resolve the bundled sample without source edits.

## 3. Document and verify the first song-loading pipeline

- [x] 3.1 Add `GUIDE.md` documenting the host/core boundary, JSON contract, runtime file assumptions, and extension points, and verify its file references match the implemented loader files.
- [x] 3.2 Run `dotnet test src\Trakr.Tests\Trakr.Tests.csproj` and verify the full suite passes with the loader and sample-song changes in place.

## Workflow follow-up

- Archive the change after implementation review is complete.
- Verify the archived `song-loading` spec remains aligned with the main spec tree.