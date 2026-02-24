namespace Nonogram.Core.Domain;

public sealed class SolutionGrid
{
    private readonly bool[] cells;

    public SolutionGrid(int width, int height, IReadOnlyList<bool> cells)
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

        int expectedLength = checked(width * height);
        if (cells.Count != expectedLength)
        {
            throw new ArgumentException(
                $"Cell count must be {expectedLength} for a {width}x{height} grid.",
                nameof(cells));
        }

        this.cells = cells.ToArray();
        Width = width;
        Height = height;
    }

    public int Width { get; }

    public int Height { get; }

    public bool this[int row, int column]
    {
        get
        {
            if (row < 0 || row >= Height)
            {
                throw new ArgumentOutOfRangeException(nameof(row));
            }

            if (column < 0 || column >= Width)
            {
                throw new ArgumentOutOfRangeException(nameof(column));
            }

            return cells[(row * Width) + column];
        }
    }
}
