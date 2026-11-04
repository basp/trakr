namespace Trakr.Core;

public sealed class Pattern
{
    public Pattern(IReadOnlyList<Row> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);

        Rows = rows.ToArray();
    }

    public IReadOnlyList<Row> Rows { get; }

    public int RowCount => Rows.Count;
}