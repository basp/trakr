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
    }

    [Fact]
    public void Step_dispatches_channel_and_effect_events_in_deterministic_order()
    {
        var sink = new RecordingPlaybackEventSink();
        var song = CreateSong(
            initialSpeed: 4,
            CreatePattern(
                CreateRow(
                    new ChannelEvent(Note: 48, Instrument: 2, Effect: new EffectCommand("A", 0x0F)),
                    null,
                    new ChannelEvent(Volume: 24))));

        var engine = new PlaybackEngine(song, sink);

        Assert.True(engine.Step());

        Assert.Equal(
            new[]
            {
                "tick:0:0:0:0",
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
            Events.Add($"tick:{position.OrderIndex}:{position.PatternIndex}:{position.RowIndex}:{position.Tick}");
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
