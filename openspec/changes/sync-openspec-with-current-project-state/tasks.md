# Tasks

## 1. Review the baseline playback capability

- [x] 1.1 Review `proposal.md` and `specs/song-playback/spec.md` against `src/Trakr.Core/Song.cs`, `src/Trakr.Core/Pattern.cs`, `src/Trakr.Core/Row.cs`, and `src/Trakr.Core/PlaybackEngine.cs`, and verify every stated requirement matches implemented behavior.
- [x] 1.2 Compare the spec scenarios with `src/Trakr.Tests/UnitTest1.cs` and `src/Trakr/Program.cs`, and verify the wording captures current sequencing behavior without promising audio output, file parsing, or effect-processing semantics.

## 2. Validate the OpenSpec change package

- [x] 2.1 Run `openspec validate sync-openspec-with-current-project-state --type change` and verify the proposal, spec delta, design, and tasks satisfy the `spec-driven` schema.
- [x] 2.2 Review the final change status and verify `openspec status --change sync-openspec-with-current-project-state` reports all required planning artifacts as done.

## Workflow follow-up

- Archive the change after review approval.
- Verify the archived result creates `openspec/specs/song-playback/spec.md` as the project's baseline capability spec.