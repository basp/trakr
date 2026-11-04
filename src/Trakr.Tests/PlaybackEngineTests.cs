using Trakr.Core;

namespace Trakr.Tests;

public class PlaybackEngineTests
{
    [Fact]
    public void Step_advances_ticks_until_the_next_row_boundary()
    {
        var song = CreateSong(
            initialSpeed: 3,
            CreatePattern(
                CreateRow(),
                CreateRow()));

        var engine = new PlaybackEngine(song);

        Assert.Equal(new PlaybackPosition(0, 0, 0, 0), engine.Position);

        Assert.True(engine.Step());
        Assert.Equal(new PlaybackPosition(0, 0, 0, 1), engine.Position);

        Assert.True(engine.Step());
        Assert.Equal(new PlaybackPosition(0, 0, 0, 2), engine.Position);

        Assert.True(engine.Step());
        Assert.Equal(new PlaybackPosition(0, 0, 1, 0), engine.Position);
    }

    [Fact]
    public void Step_advances_to_the_next_pattern_after_the_last_row()
    {
        var song = new Song(
            new[]
            {
                CreatePattern(CreateRow()),
                CreatePattern(CreateRow())
            },
            new[] { 0, 1 },
            initialSpeed: 2);

        var engine = new PlaybackEngine(song);

        Assert.True(engine.Step());
        Assert.Equal(new PlaybackPosition(0, 0, 0, 1), engine.Position);

        Assert.True(engine.Step());
        Assert.Equal(new PlaybackPosition(1, 1, 0, 0), engine.Position);
        Assert.False(engine.IsComplete);

        Assert.True(engine.Step());
        Assert.True(engine.Step());
        Assert.True(engine.IsComplete);
        Assert.False(engine.Step());
        Assert.False(engine.TryStep(out var step));
        Assert.Null(step);
    }

    [Fact]
    public void TryStep_returns_row_start_step_with_ordered_channel_and_effect_payloads()
    {
        var song = CreateSong(
            initialSpeed: 4,
            CreatePattern(
                CreateRow(
                    new ChannelEvent(Note: 48, Instrument: 2, Effect: new EffectCommand("A", 0x0F)),
                    null,
                    new ChannelEvent(Volume: 24))));

        var engine = new PlaybackEngine(song);

        Assert.True(engine.TryStep(out var step));
        Assert.NotNull(step);

        var playbackStep = step!;

        Assert.Equal(new PlaybackPosition(0, 0, 0, 0), playbackStep.Position);
        Assert.Equal(15, playbackStep.Speed);
        Assert.Equal(song.InitialTempo, playbackStep.Tempo);
        Assert.True(playbackStep.IsRowStart);
        Assert.NotNull(playbackStep.Row);
        Assert.Equal(new[] { 0, 2 }, playbackStep.ChannelEvents.Select(channelEvent => channelEvent.ChannelIndex).ToArray());
        Assert.Equal(new[] { 0 }, playbackStep.Effects.Select(effect => effect.ChannelIndex).ToArray());
        Assert.Equal((byte)48, playbackStep.ChannelEvents[0].ChannelEvent.Note);
        Assert.Equal((byte)2, playbackStep.ChannelEvents[0].ChannelEvent.Instrument);
        Assert.Equal((byte)24, playbackStep.ChannelEvents[1].ChannelEvent.Volume);
        Assert.Equal("A", playbackStep.Effects[0].Effect.Code);
        Assert.Equal((byte)0x0F, playbackStep.Effects[0].Effect.Parameter);
    }

    [Fact]
    public void TryStep_returns_tick_only_step_for_non_zero_ticks()
    {
        var song = CreateSong(
            initialSpeed: 3,
            CreatePattern(
                CreateRow(
                    new ChannelEvent(Note: 60, Instrument: 1, Effect: new EffectCommand("F", 0x20)))));

        var engine = new PlaybackEngine(song);

        Assert.True(engine.TryStep(out _));
        Assert.True(engine.TryStep(out var step));
        Assert.NotNull(step);

        var playbackStep = step!;

        Assert.Equal(new PlaybackPosition(0, 0, 0, 1), playbackStep.Position);
        Assert.Equal(3, playbackStep.Speed);
        Assert.Equal(0x20, playbackStep.Tempo);
        Assert.False(playbackStep.IsRowStart);
        Assert.Null(playbackStep.Row);
        Assert.Empty(playbackStep.ChannelEvents);
        Assert.Empty(playbackStep.Effects);
    }

    [Fact]
    public void TryStep_returns_row_start_step_with_updated_speed_for_A_effect()
    {
        var song = CreateSong(
            initialSpeed: 6,
            CreatePattern(
                CreateRow(new ChannelEvent(Note: 48, Instrument: 1, Effect: new EffectCommand("A", 0x03))),
                CreateRow()));

        var engine = new PlaybackEngine(song);

        Assert.True(engine.TryStep(out var rowStartStep));
        Assert.NotNull(rowStartStep);

        var playbackStep = rowStartStep!;

        Assert.Equal(new PlaybackPosition(0, 0, 0, 0), playbackStep.Position);
        Assert.Equal(3, playbackStep.Speed);
        Assert.Equal(song.InitialTempo, playbackStep.Tempo);
        Assert.True(playbackStep.IsRowStart);
        Assert.Equal(new PlaybackPosition(0, 0, 0, 1), engine.Position);

        Assert.True(engine.TryStep(out var middleTickStep));
        Assert.NotNull(middleTickStep);
        Assert.Equal(new PlaybackPosition(0, 0, 0, 1), middleTickStep!.Position);
        Assert.Equal(3, middleTickStep.Speed);

        Assert.True(engine.TryStep(out var finalTickStep));
        Assert.NotNull(finalTickStep);
        Assert.Equal(new PlaybackPosition(0, 0, 0, 2), finalTickStep!.Position);
        Assert.Equal(3, finalTickStep.Speed);
        Assert.Equal(new PlaybackPosition(0, 0, 1, 0), engine.Position);
    }

    [Fact]
    public void Step_uses_low_F_effect_speed_for_the_current_row_length()
    {
        var song = CreateSong(
            initialSpeed: 5,
            CreatePattern(
                CreateRow(new ChannelEvent(Effect: new EffectCommand("F", 0x02))),
                CreateRow()));

        var engine = new PlaybackEngine(song);

        Assert.True(engine.Step());
        Assert.Equal(new PlaybackPosition(0, 0, 0, 1), engine.Position);

        Assert.True(engine.Step());
        Assert.Equal(new PlaybackPosition(0, 0, 1, 0), engine.Position);
    }

    [Fact]
    public void TryStep_returns_row_start_step_with_updated_tempo_for_high_F_effect()
    {
        var song = new Song(
            new[]
            {
                CreatePattern(
                    CreateRow(new ChannelEvent(Note: 60, Effect: new EffectCommand("F", 0x7D))))
            },
            new[] { 0 },
            initialSpeed: 4,
            initialTempo: 96);

        var engine = new PlaybackEngine(song);

        Assert.True(engine.TryStep(out var step));
        Assert.NotNull(step);

        var playbackStep = step!;

        Assert.Equal(new PlaybackPosition(0, 0, 0, 0), playbackStep.Position);
        Assert.Equal(4, playbackStep.Speed);
        Assert.Equal(0x7D, playbackStep.Tempo);
    }

    [Fact]
    public void TryStep_keeps_unsupported_effects_without_changing_transport()
    {
        var song = new Song(
            new[]
            {
                CreatePattern(
                    CreateRow(new ChannelEvent(Note: 57, Effect: new EffectCommand("D", 0x0F))))
            },
            new[] { 0 },
            initialSpeed: 4,
            initialTempo: 108);

        var engine = new PlaybackEngine(song);

        Assert.True(engine.TryStep(out var step));
        Assert.NotNull(step);

        var playbackStep = step!;

        Assert.Equal(4, playbackStep.Speed);
        Assert.Equal(108, playbackStep.Tempo);
        Assert.Single(playbackStep.Effects);
        Assert.Equal("D", playbackStep.Effects[0].Effect.Code);
        Assert.Equal((byte)0x0F, playbackStep.Effects[0].Effect.Parameter);
    }

    [Fact]
    public void TryStep_uses_the_last_supported_transport_command_in_channel_order()
    {
        var song = CreateSong(
            initialSpeed: 6,
            CreatePattern(
                CreateRow(
                    new ChannelEvent(Effect: new EffectCommand("A", 0x03)),
                    new ChannelEvent(Effect: new EffectCommand("F", 0x04)))));

        var engine = new PlaybackEngine(song);

        Assert.True(engine.TryStep(out var step));
        Assert.NotNull(step);

        var playbackStep = step!;

        Assert.Equal(4, playbackStep.Speed);
        Assert.Equal(song.InitialTempo, playbackStep.Tempo);
    }

    [Fact]
    public void Step_dispatches_channel_and_effect_events_in_deterministic_order()
    {
        var sink = new RecordingPlaybackEventSink();
        var song = CreateSong(
            6,
            96,
            CreatePattern(
                CreateRow(
                    new ChannelEvent(Note: 48, Instrument: 2, Effect: new EffectCommand("A", 0x03)),
                    null,
                    new ChannelEvent(Volume: 24, Effect: new EffectCommand("F", 0x7D)))));

        var engine = new PlaybackEngine(song, sink);

        Assert.True(engine.Step());

        Assert.Equal(
            new[]
            {
                "tick:0:0:0:0:3:125",
                "row:0:0:0",
                "channel:0:48:2:-:A:3",
                "effect:0:A:3",
                "channel:2:-:-:24:F:125",
                "effect:2:F:125"
            },
            sink.Events);
    }

    [Fact]
    public void PlaybackStepEventSinkAdapter_replays_row_start_content_in_legacy_order()
    {
        var sink = new RecordingPlaybackEventSink();
        var song = CreateSong(
            initialSpeed: 4,
            CreatePattern(
                CreateRow(
                    new ChannelEvent(Note: 48, Instrument: 2, Effect: new EffectCommand("A", 0x0F)),
                    null,
                    new ChannelEvent(Volume: 24))));

        var engine = new PlaybackEngine(song);

        Assert.True(engine.TryStep(out var step));
        Assert.NotNull(step);

        PlaybackStepEventSinkAdapter.Dispatch(step!, sink);

        Assert.Equal(
            new[]
            {
                "tick:0:0:0:0:15:125",
                "row:0:0:0",
                "channel:0:48:2:-:A:15",
                "effect:0:A:15",
                "channel:2:-:-:24:-:-"
            },
            sink.Events);
    }

    [Fact]
    public void Empty_patterns_in_the_order_are_skipped_before_playback_starts()
    {
        var song = new Song(
            new[]
            {
                CreatePattern(),
                CreatePattern(CreateRow(new ChannelEvent(Note: 60)))
            },
            new[] { 0, 1 },
            initialSpeed: 1);

        var engine = new PlaybackEngine(song);

        Assert.Equal(new PlaybackPosition(1, 1, 0, 0), engine.Position);
        Assert.False(engine.IsComplete);
    }

    private static Song CreateSong(int initialSpeed, params Pattern[] patterns)
    {
        return new Song(patterns, Enumerable.Range(0, patterns.Length).ToArray(), initialSpeed);
    }

    private static Song CreateSong(int initialSpeed, int initialTempo, params Pattern[] patterns)
    {
        return new Song(patterns, Enumerable.Range(0, patterns.Length).ToArray(), initialSpeed, initialTempo);
    }

    private static Pattern CreatePattern(params Row[] rows)
    {
        return new(rows);
    }

    private static Row CreateRow(params ChannelEvent?[] channelEvents)
    {
        return new(channelEvents);
    }

    private sealed class RecordingPlaybackEventSink : IPlaybackEventSink
    {
        public List<string> Events { get; } = new();

        public void OnTick(PlaybackPosition position, int speed, int tempo)
        {
            Events.Add($"tick:{position.OrderIndex}:{position.PatternIndex}:{position.RowIndex}:{position.Tick}:{speed}:{tempo}");
        }

        public void OnRow(PlaybackPosition position, Row row)
        {
            Events.Add($"row:{position.OrderIndex}:{position.PatternIndex}:{position.RowIndex}");
        }

        public void OnChannelEvent(PlaybackPosition position, int channelIndex, ChannelEvent channelEvent)
        {
            Events.Add($"channel:{channelIndex}:{Format(channelEvent.Note)}:{Format(channelEvent.Instrument)}:{Format(channelEvent.Volume)}:{channelEvent.Effect?.Code ?? "-"}:{channelEvent.Effect?.Parameter.ToString() ?? "-"}");
        }

        public void OnEffect(PlaybackPosition position, int channelIndex, EffectCommand effectCommand)
        {
            Events.Add($"effect:{channelIndex}:{effectCommand.Code}:{effectCommand.Parameter}");
        }

        private static string Format(byte? value)
        {
            return value?.ToString() ?? "-";
        }
    }
}
