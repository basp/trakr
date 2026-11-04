# Spec Delta

## Purpose

Defines the in-memory tracker song model and deterministic playback flow that the current Trakr prototype already exposes through its core library, demo app, and tests.

## ADDED Requirements

### Requirement: Song definitions reject invalid playback setup
The system MUST reject song definitions whose playback settings or order references are invalid before playback begins.

#### Scenario: Initial speed must be positive
- **WHEN** a caller creates a song with an initial speed less than 1
- **THEN** the system rejects the song definition with an error

#### Scenario: Initial tempo must be positive
- **WHEN** a caller creates a song with an initial tempo less than 1
- **THEN** the system rejects the song definition with an error

#### Scenario: Order entries must reference existing patterns
- **WHEN** a caller creates a song whose order includes a pattern index outside the available pattern list
- **THEN** the system rejects the song definition with an error

### Requirement: Playback advances ticks within a row
The system MUST expose playback position by order, pattern, row, and tick, and each playback step MUST advance tick position until the configured row speed is reached.

#### Scenario: Tick advances before row boundary
- **WHEN** playback starts at the first row with a speed greater than 1 and the caller steps playback once
- **THEN** the reported position stays on the same row and advances to the next tick

#### Scenario: Row advances after configured speed
- **WHEN** the caller continues stepping playback until the number of steps equals the configured speed
- **THEN** the reported position advances to the next row and resets tick to zero

### Requirement: Playback follows the song order and completes after the final row
The system MUST move from one ordered pattern to the next when the current pattern is exhausted, and it MUST mark playback complete after the last ordered row is processed.

#### Scenario: Playback advances to the next ordered pattern
- **WHEN** playback reaches the last row of a pattern and the song order contains another pattern
- **THEN** the next step begins at row zero of the next ordered pattern

#### Scenario: Playback completes after the last ordered row
- **WHEN** playback finishes the final row of the final ordered pattern
- **THEN** the system marks playback complete and further steps return no additional playback progress

### Requirement: Empty ordered patterns are skipped during position normalization
The system MUST skip ordered patterns that contain zero rows whenever it normalizes playback position before emitting playable events.

#### Scenario: Leading empty patterns are skipped before first playback step
- **WHEN** the song order starts with one or more empty patterns followed by a non-empty pattern
- **THEN** the initial playback position targets the first non-empty ordered pattern

#### Scenario: Empty patterns between playable patterns do not emit playback content
- **WHEN** playback advances from a finished pattern into one or more ordered patterns with zero rows
- **THEN** the next playable position skips those empty patterns and continues at the next non-empty pattern

### Requirement: Playback events are emitted in deterministic row-start order
The system MUST emit a tick event on every step, and on tick zero it MUST emit row, channel, and effect events in channel order for non-empty channel data only.

#### Scenario: Tick event is emitted for every playback step
- **WHEN** a caller steps playback at any playable position
- **THEN** the system emits a tick event for that position

#### Scenario: Row-start events follow channel order
- **WHEN** a row contains non-empty channel events and playback steps on tick zero
- **THEN** the system emits the row event first, then each non-empty channel event in ascending channel order, and emits effect events immediately after the channel event that carries the effect