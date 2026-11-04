namespace Trakr.Core;

public interface IPlaybackEventSink
{
    void OnTick(PlaybackPosition position, int speed, int tempo);

    void OnRow(PlaybackPosition position, Row row);

    void OnChannelEvent(PlaybackPosition position, int channelIndex, ChannelEvent channelEvent);

    void OnEffect(PlaybackPosition position, int channelIndex, EffectCommand effectCommand);
}