# Trakr
A very basic music tracker project that guides the user into audio programming. The initial goal is not full `S3M` support or
perfect playback parity. Instead, the project should use APIs and data structures that can represent `S3M`-style concepts well
enough that fuller support can be added later without major redesign.

## Initial Thoughts
* The tracker should model classic trackers like `FastTracker II` and `ScreamTracker`, and its APIs should stay compatible with the kinds of concepts the `S3M` format needs. We do not need to parse `S3M` or fully implement every playback behavior in the first versions.
* Ideally, the tracker runs in a terminal. For initial testing, it can start as a console application with tracing output.
Later, we can evaluate a richer terminal UI such as `Terminal.Gui` (`tui-cs`) or an alternative.
* Audio output can come later. Early versions should focus on architecture, sequencing, and testability, using stubs or
tracing instead of real sound where helpful.
* We may eventually write more of the audio effects ourselves, but we do not need to build low-level audio processing from
scratch. A third-party library such as `NAudio` can sit behind an abstraction when we are ready for real playback.
* We want unit tests whenever we can. The code should be structured so core tracker logic is testable in isolation, with IO
and audio dependencies hidden behind seams that can be stubbed, proxied, or mocked.

## Final Thoughts
If, at any point, you have concerns about the path we are taking, by all means, feel free to push back.