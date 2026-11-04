# Spec Delta

## MODIFIED Requirements

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

## ADDED Requirements

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