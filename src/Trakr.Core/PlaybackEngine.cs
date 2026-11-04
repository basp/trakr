namespace Trakr.Core;

public sealed class PlaybackEngine
{
    private readonly Song _song;
    private readonly IPlaybackEventSink _eventSink;
    private int _orderIndex;
    private int _rowIndex;
    private int _tick;
    private int _speed;
    private int _tempo;
    private int? _pendingOrderIndex;
    private int? _pendingRowIndex;

    public PlaybackEngine(Song song, IPlaybackEventSink? eventSink = null)
    {
        _song = song ?? throw new ArgumentNullException(nameof(song));
        _eventSink = eventSink ?? NullPlaybackEventSink.Instance;
        _speed = song.InitialSpeed;
        _tempo = song.InitialTempo;

        NormalizePosition();
    }

    public bool IsComplete { get; private set; }

    public int Speed => _speed;

    public int Tempo => _tempo;

    public PlaybackPosition Position => new(_orderIndex, IsComplete ? -1 : _song.Order[_orderIndex], _rowIndex, _tick);

    public void PlayToEnd()
    {
        while (Step())
        {
        }
    }

    public bool Step()
    {
        if (!TryStep(out var step) || step is null)
        {
            return false;
        }

        PlaybackStepEventSinkAdapter.Dispatch(step, _eventSink);
        return true;
    }

    public bool TryStep(out PlaybackStep? step)
    {
        if (IsComplete)
        {
            step = null;
            return false;
        }

        step = CreateStep();
        AdvancePosition();
        return true;
    }

    private PlaybackStep CreateStep()
    {
        var position = Position;
        if (_tick != 0)
        {
            return new PlaybackStep(position, Speed, Tempo, isRowStart: false);
        }

        var row = _song.GetPatternAtOrder(_orderIndex).Rows[_rowIndex];
        var channelEvents = new List<PlaybackStepChannelEvent>();
        var effects = new List<PlaybackStepEffect>();
        var speed = _speed;
        var tempo = _tempo;

        for (var channelIndex = 0; channelIndex < row.ChannelCount; channelIndex++)
        {
            var channelEvent = row.GetChannelEvent(channelIndex);
            if (channelEvent is null || channelEvent.IsEmpty)
            {
                continue;
            }

            channelEvents.Add(new PlaybackStepChannelEvent(channelIndex, channelEvent));

            if (channelEvent.Effect is not null)
            {
                effects.Add(new PlaybackStepEffect(channelIndex, channelEvent.Effect));
                ApplyTransportEffect(channelEvent.Effect, ref speed, ref tempo);
            }
        }

        _speed = speed;
        _tempo = tempo;

        if (FlowControlResolver.TryResolve(row, _orderIndex, out var flowControlTarget))
        {
            _pendingOrderIndex = flowControlTarget.OrderIndex;
            _pendingRowIndex = flowControlTarget.RowIndex;
        }
        else
        {
            _pendingOrderIndex = null;
            _pendingRowIndex = null;
        }

        return new PlaybackStep(position, Speed, Tempo, isRowStart: true, row, channelEvents, effects);
    }

    private void AdvancePosition()
    {
        _tick++;

        if (_tick < Speed)
        {
            return;
        }

        _tick = 0;

        if (_pendingOrderIndex is int pendingOrderIndex)
        {
            _orderIndex = pendingOrderIndex;
            _rowIndex = _pendingRowIndex ?? 0;
            _pendingOrderIndex = null;
            _pendingRowIndex = null;
            NormalizePosition();
            return;
        }

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

    private static void ApplyTransportEffect(EffectCommand effect, ref int speed, ref int tempo)
    {
        switch (effect.Code)
        {
            case "A" when effect.Parameter > 0:
                speed = effect.Parameter;
                break;
            case "F" when effect.Parameter == 0:
                break;
            case "F" when effect.Parameter < 0x20:
                speed = effect.Parameter;
                break;
            case "F":
                tempo = effect.Parameter;
                break;
        }
    }
}