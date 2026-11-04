namespace Trakr.Core;

public sealed record PlaybackStepChannelEvent
{
    public PlaybackStepChannelEvent(int channelIndex, ChannelEvent channelEvent)
    {
        if (channelIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(channelIndex));
        }

        ArgumentNullException.ThrowIfNull(channelEvent);

        if (channelEvent.IsEmpty)
        {
            throw new ArgumentException("Channel event cannot be empty.", nameof(channelEvent));
        }

        ChannelIndex = channelIndex;
        ChannelEvent = channelEvent;
    }

    public int ChannelIndex { get; }

    public ChannelEvent ChannelEvent { get; }
}