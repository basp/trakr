namespace Trakr.Core;

public sealed class Row
{
    public Row(IReadOnlyList<ChannelEvent?> channelEvents)
    {
        ArgumentNullException.ThrowIfNull(channelEvents);

        ChannelEvents = channelEvents.ToArray();
    }

    public IReadOnlyList<ChannelEvent?> ChannelEvents { get; }

    public int ChannelCount => ChannelEvents.Count;

    public ChannelEvent? GetChannelEvent(int channelIndex)
    {
        if (channelIndex < 0 || channelIndex >= ChannelEvents.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(channelIndex));
        }

        return ChannelEvents[channelIndex];
    }
}