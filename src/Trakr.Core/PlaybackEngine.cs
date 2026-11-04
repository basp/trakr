namespace Trakr.Core;

public sealed class PlaybackEngine
{
    private readonly Song _song;
    private readonly IPlaybackEventSink _eventSink;
    private int _orderIndex;
    private int _rowIndex;
    private int _tick;

    public PlaybackEngine(Song song, IPlaybackEventSink? eventSink = null)
    {
        _song = song ?? throw new ArgumentNullException(nameof(song));
        _eventSink = eventSink ?? NullPlaybackEventSink.Instance;
        Speed = song.InitialSpeed;
        Tempo = song.InitialTempo;

        NormalizePosition();
    }

    public bool IsComplete { get; private set; }

    public int Speed { get; }

    public int Tempo { get; }

    public PlaybackPosition Position => new(_orderIndex, IsComplete ? -1 : _song.Order[_orderIndex], _rowIndex, _tick);

    public void PlayToEnd()
    {
        while (Step())
        {
        }
    }

    public bool Step()
    {
        if (IsComplete)
        {
            return false;
        }

        var position = Position;
        var row = _song.GetPatternAtOrder(_orderIndex).Rows[_rowIndex];

        _eventSink.OnTick(position, Speed, Tempo);

        if (_tick == 0)
        {
            _eventSink.OnRow(position, row);

            for (var channelIndex = 0; channelIndex < row.ChannelCount; channelIndex++)
            {
                var channelEvent = row.GetChannelEvent(channelIndex);
                if (channelEvent is null || channelEvent.IsEmpty)
                {
                    continue;
                }

                _eventSink.OnChannelEvent(position, channelIndex, channelEvent);

                if (channelEvent.Effect is not null)
                {
                    _eventSink.OnEffect(position, channelIndex, channelEvent.Effect);
                }
            }
        }

        AdvancePosition();
        return true;
    }

    private void AdvancePosition()
    {
        _tick++;

        if (_tick < Speed)
        {
            return;
        }

        _tick = 0;
        _rowIndex++;

        var currentPattern = _song.GetPatternAtOrder(_orderIndex);
        if (_rowIndex < currentPattern.RowCount)
        {
            return;
        }

        _orderIndex++;
        _rowIndex = 0;
        NormalizePosition();
    }

    private void NormalizePosition()
    {
        while (_orderIndex < _song.Order.Count)
        {
            var pattern = _song.GetPatternAtOrder(_orderIndex);
            if (pattern.RowCount > 0)
            {
                return;
            }

            _orderIndex++;
        }

        IsComplete = true;
    }
}