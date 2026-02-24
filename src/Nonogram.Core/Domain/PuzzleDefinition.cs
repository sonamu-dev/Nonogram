namespace Nonogram.Core.Domain;

public sealed record PuzzleDefinition
{
    public PuzzleDefinition(
        int width,
        int height,
        IReadOnlyList<IReadOnlyList<int>> rowClues,
        IReadOnlyList<IReadOnlyList<int>> columnClues)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be greater than zero.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be greater than zero.");
        }

        ArgumentNullException.ThrowIfNull(rowClues);
        ArgumentNullException.ThrowIfNull(columnClues);

        if (rowClues.Count != height)
        {
            throw new ArgumentException(
                $"Row clue count must match height ({height}).",
                nameof(rowClues));
        }

        if (columnClues.Count != width)
        {
            throw new ArgumentException(
                $"Column clue count must match width ({width}).",
                nameof(columnClues));
        }

        Width = width;
        Height = height;
        RowClues = rowClues;
        ColumnClues = columnClues;
    }

    public int Width { get; }

    public int Height { get; }

    public IReadOnlyList<IReadOnlyList<int>> RowClues { get; }

    public IReadOnlyList<IReadOnlyList<int>> ColumnClues { get; }
}
