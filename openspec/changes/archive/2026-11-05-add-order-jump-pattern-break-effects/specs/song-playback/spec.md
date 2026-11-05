# Spec Delta

## MODIFIED Requirements

### Requirement: Song definitions reject invalid playback setup
The system MUST reject song definitions whose playback settings, order references, or supported flow-control destinations are invalid before playback begins.

#### Scenario: Order jump must target an existing order
- **WHEN** a caller creates a song whose row-start `Bxx` command targets an order index outside the song order list
- **THEN** the system rejects the song definition with an error before playback begins

#### Scenario: Pattern break must have a destination order
- **WHEN** a caller creates a song whose final ordered row contains a row-start `Dxx` command without a `Bxx` command on the same row
- **THEN** the system rejects the song definition with an error because no next sequential order exists for the break destination

#### Scenario: Pattern break row must exist in the destination order
- **WHEN** a caller creates a song whose row-start flow commands target a destination row index outside the destination order's pattern row count
- **THEN** the system rejects the song definition with an error before playback begins

### Requirement: Playback follows the song order and completes after the final row
The system MUST move from one ordered pattern to the next when the current pattern is exhausted, unless supported row-start flow-control commands redirect playback to a different order or row after the current row completes. It MUST still mark playback complete after the last ordered row that can be played is processed.

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

#### Scenario: Order jump lands on an empty ordered pattern
- **WHEN** a row-start `Bxx` command redirects playback to an order whose pattern contains zero rows
- **THEN** the next playable position skips that empty pattern and continues at the next non-empty ordered pattern

### Requirement: Structured playback steps expose per-tick playback facts
The system MUST expose one structured playback result for each successful tick. Each result MUST include playback position, the effective speed, and the effective tempo for that tick, and on tick zero it MUST include the row-start channel and effect payloads needed to reproduce the legacy callback sequence for that tick.

#### Scenario: Row-start transport changes apply before later flow redirects
- **WHEN** a tick-zero row includes both supported transport effects and supported flow-control effects
- **THEN** the structured playback result for that tick reports the final speed and tempo selected for the current row
- **AND** any flow-control redirect changes only the next playback position after the current row completes

## ADDED Requirements

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