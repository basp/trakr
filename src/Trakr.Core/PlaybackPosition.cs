namespace Trakr.Core;

public readonly record struct PlaybackPosition(int OrderIndex, int PatternIndex, int RowIndex, int Tick);