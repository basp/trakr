# song-playback Specification

## Purpose

Defines the in-memory tracker song model and deterministic playback flow that the current Trakr prototype already exposes through its core library, demo app, and tests.

## Requirements

### Requirement: Song definitions reject invalid playback setup
The system MUST reject song definitions whose playback settings, order references, or supported flow-control destinations are invalid before playback begins.

#### Scenario: Initial speed must be positive
- **WHEN** a caller creates a song with an initial speed less than 1
- **THEN** the system rejects the song definition with an error

#### Scenario: Initial tempo must be positive
- **WHEN** a caller creates a song with an initial tempo less than 1
- **THEN** the system rejects the song definition with an error

#### Scenario: Order entries must reference existing patterns
- **WHEN** a caller creates a song whose order includes a pattern index outside the available pattern list
- **THEN** the system rejects the song definition with an error

#### Scenario: Order jump must target an existing order
- **WHEN** a caller creates a song whose row-start `Bxx` command targets an order index outside the song order list
- **THEN** the system rejects the song definition with an error before playback begins

#### Scenario: Pattern break must have a destination order
- **WHEN** a caller creates a song whose final ordered row contains a row-start `Dxx` command without a `Bxx` command on the same row
- **THEN** the system rejects the song definition with an error because no next sequential order exists for the break destination

#### Scenario: Pattern break row must exist in the destination order
- **WHEN** a caller creates a song whose row-start flow commands target a destination row index outside the destination order's pattern row count
- **THEN** the system rejects the song definition with an error before playback begins

### Requirement: Playback advances ticks within a row
The system MUST expose playback position by order, pattern, row, and tick, and each playback step MUST advance tick position until the effective row speed for that row is reached.

#### Scenario: Tick advances before row boundary
- **WHEN** playback starts at the first row with a speed greater than 1 and the caller steps playback once
- **THEN** the reported position stays on the same row and advances to the next tick

#### Scenario: Row advances after configured speed
- **WHEN** the caller continues stepping playback until the number of steps equals the configured speed for that row
- **THEN** the reported position advances to the next row and resets tick to zero

#### Scenario: Row-start speed effect changes the current row length
- **WHEN** playback emits tick zero for a row that contains a supported `Axx` command or an `Fxx` command with `xx` below `0x20`
- **THEN** the system uses the new speed for the remaining tick progression of that same row and advances to the next row after the updated number of ticks is reached

### Requirement: Playback follows the song order and completes after the final row
The system MUST move from one ordered pattern to the next when the current pattern is exhausted, unless supported row-start flow-control commands redirect playback to a different order or row after the current row completes. It MUST still mark playback complete after the last ordered row that can be played is processed.

#### Scenario: Playback advances to the next ordered pattern
- **WHEN** playback reaches the last row of a pattern and the song order contains another pattern
- **THEN** the next step begins at row zero of the next ordered pattern

#### Scenario: Playback completes after the last ordered row
- **WHEN** playback finishes the final row of the final ordered pattern
- **THEN** the system marks playback complete and further steps return no additional playback progress

#### Scenario: Order jump redirects playback after the current row
- **WHEN** a row-start channel event carries `B01`
- **THEN** the current row still finishes normally
- **AND** the next playable position starts at order `1`, row `0`

#### Scenario: Pattern break redirects playback to a row in the next order
- **WHEN** a row-start channel event carries `D01` and no `Bxx` command overrides the destination order
- **THEN** the current row still finishes normally
- **AND** the next playable position starts at row `1` of the next sequential order

#### Scenario: Order jump and pattern break combine on the same row
- **WHEN** one row-start row includes both `B01` and `D01`
- **THEN** the current row still finishes normally
- **AND** the next playable position starts at order `1`, row `1`

### Requirement: Empty ordered patterns are skipped during position normalization
The system MUST skip ordered patterns that contain zero rows whenever it normalizes playback position before emitting playable events, including positions reached through supported flow-control redirects.

#### Scenario: Leading empty patterns are skipped before first playback step
- **WHEN** the song order starts with one or more empty patterns followed by a non-empty pattern
- **THEN** the initial playback position targets the first non-empty ordered pattern

#### Scenario: Empty patterns between playable patterns do not emit playback content
- **WHEN** playback advances from a finished pattern into one or more ordered patterns with zero rows
- **THEN** the next playable position skips those empty patterns and continues at the next non-empty pattern

#### Scenario: Order jump lands on an empty ordered pattern
- **WHEN** a row-start `Bxx` command redirects playback to an order whose pattern contains zero rows
- **THEN** the next playable position skips that empty pattern and continues at the next non-empty ordered pattern

### Requirement: Structured playback steps expose per-tick playback facts
The system MUST expose one structured playback result for each successful tick. Each result MUST include playback position, the effective speed, and the effective tempo for that tick, and on tick zero it MUST include the row-start channel and effect payloads needed to reproduce the legacy callback sequence for that tick.

#### Scenario: Row-start step includes ordered row-start payloads
- **WHEN** a caller requests the structured playback result for a tick-zero row start
- **THEN** the result includes the current row plus ordered non-empty channel and effect payloads for that tick

#### Scenario: Non-zero tick step omits row-start payloads
- **WHEN** a caller requests the structured playback result for a non-zero tick within a row
- **THEN** the result contains the tick position and transport settings without row-start channel or effect payloads

#### Scenario: Row-start transport effects are reflected immediately
- **WHEN** a tick-zero row contains one or more supported transport effects
- **THEN** the structured playback result for that same tick reports the final speed and tempo selected for the row before playback advances further

#### Scenario: Row-start transport changes apply before later flow redirects
- **WHEN** a tick-zero row includes both supported transport effects and supported flow-control effects
- **THEN** the structured playback result for that tick reports the final speed and tempo selected for the current row
- **AND** any flow-control redirect changes only the next playback position after the current row completes

### Requirement: Tracker-style flow-control effects redirect playback at row boundaries
The system MUST interpret supported flow-control effects only on tick zero. `Bxx` MUST schedule playback to continue at order `xx` after the current row finishes, and `Dxx` MUST schedule playback to continue at row `xx` using the raw effect value as a zero-based row index. When `Dxx` appears without `Bxx`, the destination order MUST be the next sequential order.

#### Scenario: Pattern break uses the raw row index directly
- **WHEN** a row-start channel event carries `D01`
- **THEN** the next redirected position uses destination row `1` without decimal or tracker-digit reinterpretation

#### Scenario: Jumping to the final order can still complete playback
- **WHEN** a row-start `Bxx` command redirects playback to the final order in the song
- **THEN** playback continues from that valid destination and still completes after the final reachable row is processed

### Requirement: Supported flow-control commands preserve deterministic channel-order precedence
The system MUST scan supported row-start flow-control commands in ascending channel order. If multiple supported commands on one row affect the same destination field, the last command encountered for that field MUST determine the redirected order or row. Supported and unsupported effects MUST remain observable in the existing row-start payloads and event callbacks.

#### Scenario: Later channels override earlier flow destinations
- **WHEN** one row-start row contains multiple supported `Bxx` and `Dxx` commands on different channels
- **THEN** the redirected order uses the last supported `Bxx` encountered in channel order
- **AND** the redirected row uses the last supported `Dxx` encountered in channel order

#### Scenario: Supported flow effects remain observable in event order
- **WHEN** a row-start row contains supported flow-control effects alongside unsupported effect codes
- **THEN** the structured row-start payloads and emitted effect events preserve those effects in deterministic channel order while only the supported flow commands redirect playback

### Requirement: Tracker-style transport effects update playback at row start
The system MUST interpret supported transport effects only on tick zero. `Axx` MUST set speed to `xx`, `F00` MUST leave transport unchanged, `Fxx` with `xx` below `0x20` MUST set speed to `xx`, and `Fxx` with `xx` at or above `0x20` MUST set tempo to `xx`.

#### Scenario: Speed command updates speed on the current row start
- **WHEN** a row-start channel event carries `A03`
- **THEN** the tick-zero playback result and emitted tick event report speed `3` for that row and later ticks use the updated row length

#### Scenario: Tempo command updates tempo on the current row start
- **WHEN** a row-start channel event carries an `Fxx` command whose `xx` value is at or above `0x20`
- **THEN** the tick-zero playback result and emitted tick event report the updated tempo while preserving the current row speed unless another supported command changes it

#### Scenario: Later row-start transport commands override earlier ones in channel order
- **WHEN** multiple supported transport effects appear on the same tick-zero row
- **THEN** the system interprets them in ascending channel order and the last command affecting speed or tempo determines the value reported for that row start and later ticks

### Requirement: Unsupported effects remain observable without changing transport
The system MUST continue to expose and emit row-start effect payloads for unsupported effect codes, and those unsupported effects MUST NOT change playback speed or tempo.

#### Scenario: Unsupported effect code remains metadata only
- **WHEN** a tick-zero row includes an effect command outside the supported `Axx` and `Fxx` transport rules
- **THEN** the system preserves that effect in the structured row-start payloads and emitted effect events while leaving the reported speed and tempo unchanged

### Requirement: Playback events are emitted in deterministic row-start order
The system MUST emit a tick event on every step, and on tick zero it MUST emit row, channel, and effect events in channel order for non-empty channel data only.

#### Scenario: Tick event is emitted for every playback step
- **WHEN** a caller steps playback at any playable position
- **THEN** the system emits a tick event for that position

#### Scenario: Row-start events follow channel order
- **WHEN** a row contains non-empty channel events and playback steps on tick zero
- **THEN** the system emits the row event first, then each non-empty channel event in ascending channel order, and emits effect events immediately after the channel event that carries the effect