using Trakr.Core;

namespace Trakr;

internal sealed class ConsolePlaybackEventSink : IPlaybackEventSink
{
    public void OnTick(PlaybackPosition position, int speed, int tempo)
    {
        Console.WriteLine($"Tick O{position.OrderIndex:D2} P{position.PatternIndex:D2} R{position.RowIndex:D2} T{position.Tick:D2} speed={speed} tempo={tempo}");
    }

    public void OnRow(PlaybackPosition position, Row row)
    {
        Console.WriteLine($"  Row O{position.OrderIndex:D2} P{position.PatternIndex:D2} R{position.RowIndex:D2} channels={row.ChannelCount}");
    }

    public void OnChannelEvent(PlaybackPosition position, int channelIndex, ChannelEvent channelEvent)
    {
        Console.WriteLine($"    Channel {channelIndex:D2} note={Format(channelEvent.Note)} instrument={Format(channelEvent.Instrument)} volume={Format(channelEvent.Volume)} effect={Format(channelEvent.Effect)}");
    }

    public void OnEffect(PlaybackPosition position, int channelIndex, EffectCommand effectCommand)
    {
        Console.WriteLine($"      Effect  {channelIndex:D2} {effectCommand.Code}{effectCommand.Parameter:X2} (placeholder)");
    }

    private static string Format(byte? value)
    {
        return value?.ToString() ?? "--";
    }

    private static string Format(EffectCommand? effectCommand)
    {
        return effectCommand is null ? "--" : $"{effectCommand.Code}{effectCommand.Parameter:X2}";
    }
}