namespace Trakr.Core;

public sealed record PlaybackStepEffect
{
    public PlaybackStepEffect(int channelIndex, EffectCommand effect)
    {
        if (channelIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(channelIndex));
        }

        ArgumentNullException.ThrowIfNull(effect);

        ChannelIndex = channelIndex;
        Effect = effect;
    }

    public int ChannelIndex { get; }

    public EffectCommand Effect { get; }
}