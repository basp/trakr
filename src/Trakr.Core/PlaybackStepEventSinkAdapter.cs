namespace Trakr.Core;

public static class PlaybackStepEventSinkAdapter
{
    public static void Dispatch(PlaybackStep step, IPlaybackEventSink eventSink)
    {
        ArgumentNullException.ThrowIfNull(step);
        ArgumentNullException.ThrowIfNull(eventSink);

        eventSink.OnTick(step.Position, step.Speed, step.Tempo);

        if (!step.IsRowStart || step.Row is null)
        {
            return;
        }

        eventSink.OnRow(step.Position, step.Row);

        var effectIndex = 0;

        foreach (var channelEvent in step.ChannelEvents)
        {
            eventSink.OnChannelEvent(step.Position, channelEvent.ChannelIndex, channelEvent.ChannelEvent);

            while (effectIndex < step.Effects.Count &&
                   step.Effects[effectIndex].ChannelIndex == channelEvent.ChannelIndex)
            {
                var effect = step.Effects[effectIndex];
                eventSink.OnEffect(step.Position, effect.ChannelIndex, effect.Effect);
                effectIndex++;
            }
        }
    }
}