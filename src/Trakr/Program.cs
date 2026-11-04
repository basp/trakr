using Trakr;
using Trakr.Core;

var song = new Song(
    new[]
    {
        new Pattern(
            new[]
            {
                new Row(
                    new ChannelEvent?[]
                    {
                        new(Note: 48, Instrument: 1, Effect: new EffectCommand("A", 0x03)),
                        new(Note: 55, Instrument: 2)
                    }),
                new Row(
                    new ChannelEvent?[]
                    {
                        new(Volume: 32),
                        null
                    }),
                new Row(
                    new ChannelEvent?[]
                    {
                        null,
                        new(Note: 57, Instrument: 2, Effect: new EffectCommand("D", 0x0F))
                    })
            }),
        new Pattern(
            new[]
            {
                new Row(
                    new ChannelEvent?[]
                    {
                        new(Note: 50, Instrument: 1),
                        new(Volume: 24, Effect: new EffectCommand("F", 0x7D))
                    }),
                new Row(
                    new ChannelEvent?[]
                    {
                        null,
                        new(Note: 62, Instrument: 3)
                    })
            })
    },
    new[] { 0, 1 },
    initialSpeed: 3,
    initialTempo: 125);

var engine = new PlaybackEngine(song);
var sink = new ConsolePlaybackEventSink();

Console.WriteLine("Starting Trakr demo playback...");

while (engine.TryStep(out var step) && step is not null)
{
    PlaybackStepEventSinkAdapter.Dispatch(step, sink);
}

Console.WriteLine("Playback complete.");
