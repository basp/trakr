# Design

## Context

See `proposal.md` for motivation. The current repository has no baseline capability specs, but it already has a coherent playback core in `src/Trakr.Core`: `Song`, `Pattern`, and `Row` model the in-memory sequence, while `PlaybackEngine` steps through order, row, and tick state and publishes events through `IPlaybackEventSink`. `src/Trakr/Program.cs` and `src/Trakr/ConsolePlaybackEventSink.cs` show the current demo surface, and `src/Trakr.Tests/UnitTest1.cs` captures the strongest observable behaviors already locked down by tests.

## Goals / Non-Goals

**Goals:**
- Add the first OpenSpec capability for the current playback core without changing application code.
- Describe behavior at the contract level using observable position changes, completion semantics, and emitted event order.
- Keep the capability boundary narrow enough that future parsing, audio output, and richer tracker behavior can be added as separate capabilities.

**Non-Goals:**
- Introducing new playback features, audio rendering, file import/export, or terminal UI requirements.
- Treating console output formatting as a stable external contract.
- Splitting current behavior into multiple specs before the codebase grows enough to justify it.

## Decisions

1. Introduce a single capability named `song-playback`.
   - Rationale: the implemented surface is small and cohesive; song validity rules feed directly into deterministic playback sequencing and event emission.
   - Alternative considered: separate `song-definition` and `playback-events` capabilities. Rejected because current behavior is still compact and heavily coupled in one core workflow.

2. Anchor the spec to behavior already visible through tests or the public core API.
   - Rationale: `src/Trakr.Tests/UnitTest1.cs` and the public `Trakr.Core` types provide a reliable contract baseline, while `IDEA.md` intentionally describes future direction rather than implemented guarantees.
   - Alternative considered: speculating about S3M compatibility, effect semantics, or audio playback. Rejected because those behaviors are not implemented yet.

3. Use a flat spec layout under `openspec/specs/song-playback/`.
   - Rationale: there are no existing capability groups, so a flat durable name matches the project's current organization and avoids premature taxonomy.
   - Alternative considered: introducing a broader domain folder such as `playback/song-playback`. Rejected because there is no existing hierarchy to preserve.

4. Document the boundary between core behavior and demo-only behavior.
   - Rationale: the console app is useful context, but the spec should stabilize sequencing semantics, not log message formatting.
   - Alternative considered: treating the console trace output as the primary contract. Rejected because it would overfit a demo surface that may change without altering core behavior.

## Risks / Trade-offs

- [Risk] The new spec could accidentally freeze future-facing ideas from `IDEA.md` as current guarantees. → Mitigation: only specify behavior already present in `Trakr.Core` or covered by tests.
- [Risk] A single capability may grow too broad as parsing, effects, and audio output arrive. → Mitigation: keep the current purpose narrowly focused on in-memory playback and split later when new behaviors become independent.
- [Risk] Some current constructor validations are not yet covered by automated tests. → Mitigation: capture them in the spec now and add or expand tests in future implementation changes when those areas are touched.