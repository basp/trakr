using Trakr.Core;

namespace Trakr.Tests;

public class SongJsonLoaderTests
{
    [Fact]
    public void Load_builds_a_song_from_valid_json()
    {
        var song = SongJsonLoader.Load(
            """
            {
              "initialSpeed": 3,
              "initialTempo": 125,
              "order": [0],
              "patterns": [
                {
                  "rows": [
                    {
                      "channels": [
                        { "note": 48, "instrument": 1, "volume": 32, "effect": { "code": "A", "parameter": 3 } }
                      ]
                    }
                  ]
                }
              ]
            }
            """);

        Assert.Equal(3, song.InitialSpeed);
        Assert.Equal(125, song.InitialTempo);
        Assert.Equal(new[] { 0 }, song.Order);
        Assert.Single(song.Patterns);

        var channelEvent = Assert.Single(song.Patterns[0].Rows[0].ChannelEvents);
        Assert.NotNull(channelEvent);
        Assert.Equal((byte)48, channelEvent!.Note);
        Assert.Equal((byte)1, channelEvent.Instrument);
        Assert.Equal((byte)32, channelEvent.Volume);
        Assert.Equal("A", channelEvent.Effect?.Code);
        Assert.Equal((byte)3, channelEvent.Effect?.Parameter);
    }

    [Fact]
    public void Load_preserves_sparse_channels_and_normalizes_effect_codes()
    {
        var song = SongJsonLoader.Load(
            """
            {
              "initialSpeed": 6,
              "initialTempo": 125,
              "order": [0, 1],
              "patterns": [
                {
                  "rows": [
                    {
                      "channels": [
                        { "effect": { "code": "d", "parameter": 1 } },
                        null
                      ]
                    }
                  ]
                },
                {
                  "rows": [
                    { "channels": [ null ] },
                    { "channels": [ { "note": 60 } ] }
                  ]
                }
              ]
            }
            """);

        var firstRow = song.Patterns[0].Rows[0];

        Assert.Equal(2, firstRow.ChannelCount);
        Assert.Equal("D", firstRow.GetChannelEvent(0)?.Effect?.Code);
        Assert.Equal((byte)1, firstRow.GetChannelEvent(0)?.Effect?.Parameter);
        Assert.Null(firstRow.GetChannelEvent(1));
    }

    [Fact]
    public void Load_rejects_malformed_json()
    {
        var exception = Assert.Throws<InvalidDataException>(() => SongJsonLoader.Load("{"));

        Assert.Equal("Song JSON is malformed.", exception.Message);
        Assert.IsType<System.Text.Json.JsonException>(exception.InnerException);
    }

    [Theory]
    [InlineData("""
    {
      "initialSpeed": 6,
      "initialTempo": 125,
      "patterns": []
    }
    """, "Song JSON must contain 'order'.")]
    [InlineData("""
    {
      "initialSpeed": 6,
      "initialTempo": 125,
      "order": [0]
    }
    """, "Song JSON must contain 'patterns'.")]
    public void Load_rejects_missing_required_top_level_arrays(string json, string expectedMessage)
    {
        var exception = Assert.Throws<InvalidDataException>(() => SongJsonLoader.Load(json));

        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Load_rejects_order_entries_that_reference_missing_patterns()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => SongJsonLoader.Load(
            """
            {
              "initialSpeed": 6,
              "initialTempo": 125,
              "order": [1],
              "patterns": [
                {
                  "rows": [
                    { "channels": [ null ] }
                  ]
                }
              ]
            }
            """));
    }

    [Fact]
    public void Load_rejects_invalid_flow_control_targets()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => SongJsonLoader.Load(
            """
            {
              "initialSpeed": 6,
              "initialTempo": 125,
              "order": [0, 1],
              "patterns": [
                {
                  "rows": [
                    {
                      "channels": [
                        { "effect": { "code": "B", "parameter": 1 } },
                        { "effect": { "code": "D", "parameter": 1 } }
                      ]
                    }
                  ]
                },
                {
                  "rows": [
                    { "channels": [ null ] }
                  ]
                }
              ]
            }
            """));
    }
}