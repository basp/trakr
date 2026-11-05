# song-loading Specification

## Purpose

Defines the first external song-input pipeline for Trakr so hosts can load validated `Song` instances from a JSON snapshot format instead of constructing demo songs directly in source code.

## Requirements

### Requirement: Hosts can load songs from JSON before playback begins
The system MUST load a valid JSON song definition into a fully validated `Song` instance before playback starts.

#### Scenario: Console host runs the bundled sample song
- **WHEN** the console app starts without a song-path override
- **THEN** it loads the checked-in sample JSON song and plays it through the existing playback pipeline

#### Scenario: Console host accepts a song-path override
- **WHEN** the caller supplies a first command-line argument with a file path
- **THEN** the console app loads that file instead of the bundled sample

### Requirement: The JSON snapshot maps directly to the current song model
The system MUST support JSON fields for transport settings, order data, patterns, rows, per-channel note/instrument/volume data, and optional effect commands.

#### Scenario: Sparse channel data remains representable
- **WHEN** a row contains `null` channel entries in JSON
- **THEN** the loaded song preserves those sparse channel slots without inventing placeholder note data

#### Scenario: Effect codes normalize through the existing model
- **WHEN** a channel effect code is written in mixed case in JSON
- **THEN** the loaded song stores the normalized effect code produced by `EffectCommand`

### Requirement: Structural input errors fail fast
The system MUST reject malformed JSON and missing required structural fields before playback begins.

#### Scenario: Malformed JSON is rejected
- **WHEN** a caller supplies syntactically invalid JSON
- **THEN** song loading fails immediately with an actionable error

#### Scenario: Missing required arrays are rejected
- **WHEN** a caller omits required structural fields such as `order` or `patterns`
- **THEN** song loading fails immediately with an actionable error

### Requirement: Semantic validation remains in the core song model
The system MUST continue to enforce semantic song rules through the existing model constructors rather than duplicating that validation in the host.

#### Scenario: Invalid order references are rejected during loading
- **WHEN** a JSON song references a pattern index outside the available pattern list
- **THEN** loading fails because the constructed `Song` is invalid

#### Scenario: Invalid playback flow targets are rejected during loading
- **WHEN** a JSON song contains supported flow-control commands with invalid destinations
- **THEN** loading fails because the constructed `Song` is invalid before playback begins