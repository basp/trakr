namespace Trakr.Core;

public sealed class Song
{
    public Song(IReadOnlyList<Pattern> patterns, IReadOnlyList<int> order, int initialSpeed = 6, int initialTempo = 125)
    {
        ArgumentNullException.ThrowIfNull(patterns);
        ArgumentNullException.ThrowIfNull(order);

        if (initialSpeed <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(initialSpeed), initialSpeed, "Initial speed must be greater than zero.");
        }

        if (initialTempo <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(initialTempo), initialTempo, "Initial tempo must be greater than zero.");
        }

        var patternArray = patterns.ToArray();
        var orderArray = order.ToArray();

        foreach (var pattern in patternArray)
        {
            ArgumentNullException.ThrowIfNull(pattern);
        }

        foreach (var patternIndex in orderArray)
        {
            if (patternIndex < 0 || patternIndex >= patternArray.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(order), patternIndex, "Order references a pattern index that does not exist.");
            }
        }

        Patterns = patternArray;
        Order = orderArray;
        InitialSpeed = initialSpeed;
        InitialTempo = initialTempo;
    }

    public IReadOnlyList<Pattern> Patterns { get; }

    public IReadOnlyList<int> Order { get; }

    public int InitialSpeed { get; }

    public int InitialTempo { get; }

    public Pattern GetPatternAtOrder(int orderIndex)
    {
        if (orderIndex < 0 || orderIndex >= Order.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(orderIndex));
        }

        return Patterns[Order[orderIndex]];
    }
}