namespace Trakr.Core;

public sealed class NullPlaybackEventSink : IPlaybackEventSink
{
    public static NullPlaybackEventSink Instance { get; } = new();

    private NullPlaybackEventSink()
    {
    }

    public void OnTick(PlaybackPosition position, int speed, int tempo)
    {
    }

    public void OnRow(PlaybackPosition position, Row row)
    {
    }

    public void OnChannelEvent(PlaybackPosition position, int channelIndex, ChannelEvent channelEvent)
    {
    }

    public void OnEffect(PlaybackPosition position, int channelIndex, EffectCommand effectCommand)
    {
    }
}