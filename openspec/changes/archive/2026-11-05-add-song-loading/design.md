# Design

## Context

`Song`, `Pattern`, `Row`, `ChannelEvent`, and `EffectCommand` already form a compact validated playback model inside `src/Trakr.Core`, while `src/Trakr/Program.cs` was still constructing one hard-coded demo song inline. See `proposal.md` for motivation; this design explains how the first file-backed input path should preserve that model boundary, keep parsing reusable for future hosts, and document the resulting architecture in `GUIDE.md`.

## Goals / Non-Goals

**Goals:**
- Keep file-path and file-stream concerns in `src/Trakr` while moving JSON parsing and model construction into `src/Trakr.Core`.
- Keep the first external format close to the existing in-memory model so the loader stays small and testable.
- Reuse existing model constructors for semantic validation instead of duplicating song rules in a second validation layer.
- Make the architecture and JSON contract explicit in `GUIDE.md` so future formats can extend the same boundary.

**Non-Goals:**
- Importing binary tracker formats such as `S3M`.
- Writing songs back to disk or introducing a general serialization framework.
- Changing playback semantics or console event-sink behavior beyond using loaded songs as input.

## Decisions

### Keep file access in the host and parsing in `Trakr.Core`
`src/Trakr/Program.cs` should choose the runtime file path, check for existence, and open the stream, but `SongJsonLoader` in `src/Trakr.Core` should own JSON decoding and model construction. This keeps parsing reusable for future hosts and lets tests exercise loading without depending on the console app.

**Alternatives considered:**
- Put the full loader in `src/Trakr`: rejected because it would couple parsing to the console host and make reuse harder.
- Introduce a separate parsed document model and builder layer immediately: rejected because the first format maps directly to the existing model and does not need extra indirection yet.

### Use a JSON snapshot that mirrors the current song model closely
The first external format should map directly onto transport settings, order data, patterns, rows, sparse channels, and effect commands. This keeps the initial parser small and preserves the existing nested model shape instead of inventing a domain-specific grammar up front.

**Alternatives considered:**
- A custom text DSL: rejected because it adds grammar and parser complexity before the first loading boundary is proven useful.
- A tracker-oriented text format: rejected because it front-loads import concerns that remain out of scope today.

### Separate structural validation from semantic validation
`SongJsonLoader` should reject malformed JSON and missing required structural fields such as `order`, `patterns`, `rows`, and `channels`. Semantic rules such as invalid order references or invalid flow-control targets should continue to fail through the existing `Song` and `EffectCommand` constructors so playback rules stay defined in one place.

**Alternatives considered:**
- Duplicate all semantic validation in the loader: rejected because it would split rule ownership and risk divergence from the core model.

### Ship a bundled sample file and document the boundary explicitly
The console app should use a checked-in `demo-song.json` by default, with `Trakr.csproj` copying it to the output directory so `dotnet run` works without extra setup. `GUIDE.md` should record the module boundary, JSON contract, runtime file assumptions, and extension points so the first input pipeline is easy to extend later.

**Alternatives considered:**
- Keep the sample only in source code: rejected because the whole goal is to make the input file-backed and editable without recompilation.

## Risks / Trade-offs

- [JSON optionality could drift away from the core model] → Keep the first schema close to the existing nested song shape and require key structural fields explicitly.
- [Validation responsibilities could become duplicated] → Let the loader own only syntax and required-structure checks, while `Song` and `EffectCommand` keep semantic rule enforcement.
- [Runtime sample-path behavior could be brittle across working directories] → Resolve the bundled sample from `AppContext.BaseDirectory`, keep override paths explicit, and document both behaviors in `GUIDE.md`.

## Migration Plan

1. Add `SongJsonLoader` and its JSON contract helpers in `src/Trakr.Core`, then cover valid and invalid loads in `src/Trakr.Tests/SongJsonLoaderTests.cs`.
2. Update `src/Trakr/Program.cs` and `src/Trakr/Trakr.csproj` so the console host loads `demo-song.json` by default and accepts a path override.
3. Check in `src/Trakr/demo-song.json`, write `GUIDE.md`, and verify the loader through `dotnet test src\Trakr.Tests\Trakr.Tests.csproj` plus `dotnet run --project src\Trakr\Trakr.csproj`.