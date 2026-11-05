using System.Text.Json;

namespace Trakr.Core;

public static class SongJsonLoader
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static Song Load(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        JsonSongDocument document;

        try
        {
            document = JsonSerializer.Deserialize<JsonSongDocument>(stream, SerializerOptions)
                ?? throw new InvalidDataException("Song JSON payload cannot be empty.");
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException("Song JSON is malformed.", exception);
        }

        return CreateSong(document);
    }

    public static Song Load(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        JsonSongDocument document;

        try
        {
            document = JsonSerializer.Deserialize<JsonSongDocument>(json, SerializerOptions)
                ?? throw new InvalidDataException("Song JSON payload cannot be empty.");
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException("Song JSON is malformed.", exception);
        }

        return CreateSong(document);
    }

    private static Song CreateSong(JsonSongDocument document)
    {
        var initialSpeed = RequireValue(document.InitialSpeed, "initialSpeed");
        var initialTempo = RequireValue(document.InitialTempo, "initialTempo");
        var order = RequireArray(document.Order, "order").ToArray();
        var patterns = RequireArray(document.Patterns, "patterns")
            .Select((pattern, patternIndex) => CreatePattern(pattern ?? throw new InvalidDataException($"Song JSON field 'patterns[{patternIndex}]' cannot be null."), patternIndex))
            .ToArray();

        return new Song(patterns, order, initialSpeed, initialTempo);
    }

    private static Pattern CreatePattern(JsonPatternDocument document, int patternIndex)
    {
        var rows = RequireArray(document.Rows, $"patterns[{patternIndex}].rows")
            .Select((row, rowIndex) => CreateRow(row ?? throw new InvalidDataException($"Song JSON field 'patterns[{patternIndex}].rows[{rowIndex}]' cannot be null."), patternIndex, rowIndex))
            .ToArray();

        return new Pattern(rows);
    }

    private static Row CreateRow(JsonRowDocument document, int patternIndex, int rowIndex)
    {
        var channelEvents = RequireArray(document.Channels, $"patterns[{patternIndex}].rows[{rowIndex}].channels")
            .Select((channelEvent, channelIndex) => CreateChannelEvent(channelEvent, patternIndex, rowIndex, channelIndex))
            .ToArray();

        return new Row(channelEvents);
    }

    private static ChannelEvent? CreateChannelEvent(JsonChannelEventDocument? document, int patternIndex, int rowIndex, int channelIndex)
    {
        if (document is null)
        {
            return null;
        }

        EffectCommand? effect = null;
        if (document.Effect is not null)
        {
            effect = new EffectCommand(
                RequireString(document.Effect.Code, $"patterns[{patternIndex}].rows[{rowIndex}].channels[{channelIndex}].effect.code"),
                RequireValue(document.Effect.Parameter, $"patterns[{patternIndex}].rows[{rowIndex}].channels[{channelIndex}].effect.parameter"));
        }

        return new ChannelEvent(document.Note, document.Instrument, document.Volume, effect);
    }

    private static T RequireValue<T>(T? value, string fieldName)
        where T : struct
    {
        if (value is null)
        {
            throw new InvalidDataException($"Song JSON must contain '{fieldName}'.");
        }

        return value.Value;
    }

    private static string RequireString(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidDataException($"Song JSON must contain '{fieldName}'.");
        }

        return value;
    }

    private static IReadOnlyList<T> RequireArray<T>(IReadOnlyList<T>? value, string fieldName)
    {
        if (value is null)
        {
            throw new InvalidDataException($"Song JSON must contain '{fieldName}'.");
        }

        return value;
    }

    private sealed class JsonSongDocument
    {
        public int? InitialSpeed { get; init; }

        public int? InitialTempo { get; init; }

        public List<int>? Order { get; init; }

        public List<JsonPatternDocument?>? Patterns { get; init; }
    }

    private sealed class JsonPatternDocument
    {
        public List<JsonRowDocument?>? Rows { get; init; }
    }

    private sealed class JsonRowDocument
    {
        public List<JsonChannelEventDocument?>? Channels { get; init; }
    }

    private sealed class JsonChannelEventDocument
    {
        public byte? Note { get; init; }

        public byte? Instrument { get; init; }

        public byte? Volume { get; init; }

        public JsonEffectCommandDocument? Effect { get; init; }
    }

    private sealed class JsonEffectCommandDocument
    {
        public string? Code { get; init; }

        public byte? Parameter { get; init; }
    }
}