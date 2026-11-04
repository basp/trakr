# Trakr
A very basic music tracker project that guides the user into audio programming. It does the bare minimum but does support all the features that the `S3M` format allows. However, that doesn't mean that all features have to be implemented fully - just make sure we have an interface and that we can stub any really complicated routines out for later. 

## Initial Thoughts
* The tracker should model the classic trackers like `FastTracker II` and `ScreamTracker` and the APIs should support the `S3M` format. This doesn't mean we actually need to parse that format but just make sure our API can support it.
* Ideally, the tracker runs in a terminal. For initial testing, we can even run it as a console application with sort of tracing output. In the long run, we probably want a proper UI using something like `Terminal.Gui` (`tui-cs`) or an alternative. This is not set in stone and I'd like some feedback on options.
* Ideally, we want to write a lot of the audio effects ourselves but we don't need to write the actually processing, we're going to rely on a third-party library like `NAudio` or something similar.
* We want to have unit tests whenever we can. We also want to write the code in such a way that we can rely on unit tests instead of integration tests. This suggests that we should keep in mind that we want to stub/proxy/mock a lot of our IO test objects.

## Final Thoughts
If, at any point, you have concerns about the path we are taking, by all means, feel free to push back.