using System.IO;
using Trakr;
using Trakr.Core;

var songPath = args.Length > 0
    ? Path.GetFullPath(args[0])
    : Path.Combine(AppContext.BaseDirectory, "demo-song.json");

if (!File.Exists(songPath))
{
    throw new FileNotFoundException("Song file was not found.", songPath);
}

using var stream = File.OpenRead(songPath);
var song = SongJsonLoader.Load(stream);

var engine = new PlaybackEngine(song);
var sink = new ConsolePlaybackEventSink();

Console.WriteLine($"Starting Trakr demo playback from '{songPath}'...");

while (engine.TryStep(out var step) && step is not null)
{
    PlaybackStepEventSinkAdapter.Dispatch(step, sink);
}

Console.WriteLine("Playback complete.");
