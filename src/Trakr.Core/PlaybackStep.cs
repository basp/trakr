namespace Trakr.Core;

public sealed class PlaybackStep
{
    public PlaybackStep(
        PlaybackPosition position,
        int speed,
        int tempo,
        bool isRowStart,
        Row? row = null,
        IReadOnlyList<PlaybackStepChannelEvent>? channelEvents = null,
        IReadOnlyList<PlaybackStepEffect>? effects = null)
    {
        if (speed < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(speed), "Speed must be positive.");
        }

        if (tempo < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(tempo), "Tempo must be positive.");
        }

        if (isRowStart && row is null)
        {
            throw new ArgumentException("A row-start step must include row data.", nameof(row));
        }

        if (!isRowStart && row is not null)
        {
            throw new ArgumentException("Only row-start steps can include row data.", nameof(row));
        }

        if (!isRowStart && channelEvents is { Count: > 0 })
        {
            throw new ArgumentException("Only row-start steps can include channel events.", nameof(channelEvents));
        }

        if (!isRowStart && effects is { Count: > 0 })
        {
            throw new ArgumentException("Only row-start steps can include effects.", nameof(effects));
        }

        var copiedChannelEvents = Copy(channelEvents);
        var copiedEffects = Copy(effects);

        ValidateEffectAlignment(copiedChannelEvents, copiedEffects);

        Position = position;
        Speed = speed;
        Tempo = tempo;
        IsRowStart = isRowStart;
        Row = row;
        ChannelEvents = copiedChannelEvents;
        Effects = copiedEffects;
    }

    public PlaybackPosition Position { get; }

    public int Speed { get; }

    public int Tempo { get; }

    public bool IsRowStart { get; }

    public Row? Row { get; }

    public IReadOnlyList<PlaybackStepChannelEvent> ChannelEvents { get; }

    public IReadOnlyList<PlaybackStepEffect> Effects { get; }

    private static IReadOnlyList<T> Copy<T>(IReadOnlyList<T>? items)
    {
        return items is null or { Count: 0 }
            ? Array.Empty<T>()
            : items.ToArray();
    }

    private static void ValidateEffectAlignment(
        IReadOnlyList<PlaybackStepChannelEvent> channelEvents,
        IReadOnlyList<PlaybackStepEffect> effects)
    {
        var channelEventIndex = 0;

        foreach (var effect in effects)
        {
            while (channelEventIndex < channelEvents.Count &&
                   channelEvents[channelEventIndex].ChannelIndex < effect.ChannelIndex)
            {
                channelEventIndex++;
            }

            if (channelEventIndex >= channelEvents.Count ||
                channelEvents[channelEventIndex].ChannelIndex != effect.ChannelIndex)
            {
                throw new ArgumentException("Effects must align with channel event order.", nameof(effects));
            }
        }
    }
}