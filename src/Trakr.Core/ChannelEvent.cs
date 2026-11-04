namespace Trakr.Core;

public sealed record ChannelEvent(byte? Note = null, byte? Instrument = null, byte? Volume = null, EffectCommand? Effect = null)
{
    public bool IsEmpty => Note is null && Instrument is null && Volume is null && Effect is null;
}