namespace Nonogram.Core.Domain;

public sealed class BoardState
{
    private readonly CellState[] _cells;

    public BoardState(int width, int height, IReadOnlyList<CellState> cells)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be greater than zero.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be greater than zero.");
        }

        ArgumentNullException.ThrowIfNull(cells);

        int expectedCount = checked(width * height);
        if (cells.Count != expectedCount)
        {
            throw new ArgumentException(
                $"Cell count must be {expectedCount} for a {width}x{height} board.",
                nameof(cells));
        }

        _cells = cells.ToArray();
        Width = width;
        Height = height;
    }

    public int Width { get; }

    public int Height { get; }

    public CellState this[int row, int column]
    {
        get
        {
            ValidateCoordinates(row, column);
            return _cells[GetRowMajorIndex(row, column)];
        }
    }

    public CellState[] ToArray() => _cells.ToArray();

    private int GetRowMajorIndex(int row, int column) => (row * Width) + column;

    private void ValidateCoordinates(int row, int column)
    {
        if (row < 0 || row >= Height)
        {
            throw new ArgumentOutOfRangeException(nameof(row));
        }

        if (column < 0 || column >= Width)
        {
            throw new ArgumentOutOfRangeException(nameof(column));
        }
    }
}
