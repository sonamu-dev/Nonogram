using Nonogram.Core.Domain;

namespace Nonogram.Tests;

public class ClueGeneratorTests
{
    [Fact]
    public void CreatePuzzleDefinition_EmptyLine_UsesArrayEmpty()
    {
        SolutionGrid grid = CreateGrid("00000");

        PuzzleDefinition puzzle = ClueGenerator.CreatePuzzleDefinition(grid);

        Assert.Single(puzzle.RowClues);
        Assert.Same(Array.Empty<int>(), puzzle.RowClues[0]);
        Assert.All(puzzle.ColumnClues, columnClue => Assert.Same(Array.Empty<int>(), columnClue));
    }

    [Fact]
    public void CreatePuzzleDefinition_FilledLine_ReturnsSingleBlock()
    {
        SolutionGrid grid = CreateGrid("11111");

        PuzzleDefinition puzzle = ClueGenerator.CreatePuzzleDefinition(grid);

        Assert.Equal(new[] { 5 }, puzzle.RowClues[0]);
        Assert.All(puzzle.ColumnClues, columnClue => Assert.Equal(new[] { 1 }, columnClue));
    }

    [Fact]
    public void CreatePuzzleDefinition_MultiBlockPattern_ReturnsBlockLengths()
    {
        SolutionGrid grid = CreateGrid("0011100110");

        PuzzleDefinition puzzle = ClueGenerator.CreatePuzzleDefinition(grid);

        Assert.Equal(new[] { 3, 2 }, puzzle.RowClues[0]);
    }

    [Fact]
    public void CreatePuzzleDefinition_5x5GoldenCase_ReturnsExpectedRowAndColumnClues()
    {
        SolutionGrid grid = CreateGrid(
            "11010",
            "00111",
            "10001",
            "01110",
            "00000");

        PuzzleDefinition puzzle = ClueGenerator.CreatePuzzleDefinition(grid);

        int[][] expectedRows =
        [
            [2, 1],
            [3],
            [1, 1],
            [3],
            []
        ];

        int[][] expectedColumns =
        [
            [1, 1],
            [1, 1],
            [1, 1],
            [2, 1],
            [2]
        ];

        Assert.Equal(5, puzzle.Width);
        Assert.Equal(5, puzzle.Height);
        AssertClues(expectedRows, puzzle.RowClues);
        AssertClues(expectedColumns, puzzle.ColumnClues);
        Assert.Same(Array.Empty<int>(), puzzle.RowClues[4]);
    }

    private static void AssertClues(int[][] expected, IReadOnlyList<IReadOnlyList<int>> actual)
    {
        Assert.Equal(expected.Length, actual.Count);

        for (int line = 0; line < expected.Length; line++)
        {
            Assert.Equal(expected[line], actual[line]);
        }
    }

    private static SolutionGrid CreateGrid(params string[] rows)
    {
        if (rows.Length == 0)
        {
            throw new ArgumentException("At least one row is required.", nameof(rows));
        }

        int width = rows[0].Length;
        if (width == 0)
        {
            throw new ArgumentException("Row width must be greater than zero.", nameof(rows));
        }

        bool[] cells = new bool[width * rows.Length];

        for (int row = 0; row < rows.Length; row++)
        {
            if (rows[row].Length != width)
            {
                throw new ArgumentException("All rows must have the same width.", nameof(rows));
            }

            for (int column = 0; column < width; column++)
            {
                char token = rows[row][column];
                cells[(row * width) + column] = token switch
                {
                    '1' => true,
                    '0' => false,
                    _ => throw new ArgumentException(
                        $"Unsupported token '{token}'. Use only '0' or '1'.",
                        nameof(rows))
                };
            }
        }

        return new SolutionGrid(width, rows.Length, cells);
    }
}
