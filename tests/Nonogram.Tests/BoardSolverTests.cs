using Nonogram.Core.Domain;

namespace Nonogram.Tests;

public class BoardSolverTests
{
    [Fact]
    public void Solve_EmptyPuzzle_ResolvesAllCellsToEmpty()
    {
        PuzzleDefinition puzzle = CreatePuzzle(
            width: 5,
            height: 5,
            rowClues:
            [
                [],
                [],
                [],
                [],
                []
            ],
            columnClues:
            [
                [],
                [],
                [],
                [],
                []
            ]);
        BoardState board = CreateBoard(
            width: 5,
            height: 5,
            [
                "?????",
                "?????",
                "?????",
                "?????",
                "?????"
            ]);

        BoardSolveResult result = BoardSolver.Solve(puzzle, board);

        Assert.False(result.IsContradiction);
        Assert.True(result.HasProgress);
        AssertBoard(
            result.UpdatedBoard,
            [
                "EEEEE",
                "EEEEE",
                "EEEEE",
                "EEEEE",
                "EEEEE"
            ]);
    }

    [Fact]
    public void Solve_FullPuzzle_ResolvesAllCellsToFilled()
    {
        PuzzleDefinition puzzle = CreatePuzzle(
            width: 5,
            height: 5,
            rowClues:
            [
                [5],
                [5],
                [5],
                [5],
                [5]
            ],
            columnClues:
            [
                [5],
                [5],
                [5],
                [5],
                [5]
            ]);
        BoardState board = CreateBoard(
            width: 5,
            height: 5,
            [
                "?????",
                "?????",
                "?????",
                "?????",
                "?????"
            ]);

        BoardSolveResult result = BoardSolver.Solve(puzzle, board);

        Assert.False(result.IsContradiction);
        Assert.True(result.HasProgress);
        AssertBoard(
            result.UpdatedBoard,
            [
                "FFFFF",
                "FFFFF",
                "FFFFF",
                "FFFFF",
                "FFFFF"
            ]);
    }

    [Fact]
    public void Solve_WhenContradictionIsSeeded_ReturnsContradiction()
    {
        PuzzleDefinition puzzle = CreatePuzzle(
            width: 5,
            height: 5,
            rowClues:
            [
                [],
                [],
                [],
                [],
                []
            ],
            columnClues:
            [
                [],
                [],
                [],
                [],
                []
            ]);
        BoardState board = CreateBoard(
            width: 5,
            height: 5,
            [
                "??F??",
                "?????",
                "?????",
                "?????",
                "?????"
            ]);

        BoardSolveResult result = BoardSolver.Solve(puzzle, board);

        Assert.True(result.IsContradiction);
        Assert.False(result.HasProgress);
        Assert.Equal("Row 0", result.ContradictionSource);
    }

    [Fact]
    public void Solve_AlreadyStableBoard_ReturnsNoProgress()
    {
        PuzzleDefinition puzzle = CreatePuzzle(
            width: 5,
            height: 5,
            rowClues:
            [
                [5],
                [5],
                [5],
                [5],
                [5]
            ],
            columnClues:
            [
                [5],
                [5],
                [5],
                [5],
                [5]
            ]);
        BoardState board = CreateBoard(
            width: 5,
            height: 5,
            [
                "FFFFF",
                "FFFFF",
                "FFFFF",
                "FFFFF",
                "FFFFF"
            ]);

        BoardSolveResult result = BoardSolver.Solve(puzzle, board);

        Assert.False(result.IsContradiction);
        Assert.False(result.HasProgress);
        AssertBoard(
            result.UpdatedBoard,
            [
                "FFFFF",
                "FFFFF",
                "FFFFF",
                "FFFFF",
                "FFFFF"
            ]);
    }

    private static PuzzleDefinition CreatePuzzle(
        int width,
        int height,
        int[][] rowClues,
        int[][] columnClues)
    {
        IReadOnlyList<IReadOnlyList<int>> rows = rowClues
            .Select(clue => clue.Length == 0 ? Array.Empty<int>() : clue)
            .ToArray();
        IReadOnlyList<IReadOnlyList<int>> columns = columnClues
            .Select(clue => clue.Length == 0 ? Array.Empty<int>() : clue)
            .ToArray();

        return new PuzzleDefinition(width, height, rows, columns);
    }

    private static BoardState CreateBoard(int width, int height, string[] rows)
    {
        if (rows.Length != height)
        {
            throw new ArgumentException("Row count must match height.", nameof(rows));
        }

        CellState[] cells = new CellState[width * height];

        for (int row = 0; row < height; row++)
        {
            if (rows[row].Length != width)
            {
                throw new ArgumentException("Row width mismatch.", nameof(rows));
            }

            for (int column = 0; column < width; column++)
            {
                cells[(row * width) + column] = rows[row][column] switch
                {
                    '?' => CellState.Unknown,
                    'F' => CellState.Filled,
                    'E' => CellState.Empty,
                    _ => throw new ArgumentException("Unsupported cell token.", nameof(rows))
                };
            }
        }

        return new BoardState(width, height, cells);
    }

    private static void AssertBoard(BoardState board, string[] expectedRows)
    {
        Assert.Equal(expectedRows.Length, board.Height);
        Assert.All(expectedRows, row => Assert.Equal(board.Width, row.Length));

        for (int row = 0; row < board.Height; row++)
        {
            for (int column = 0; column < board.Width; column++)
            {
                CellState expected = expectedRows[row][column] switch
                {
                    '?' => CellState.Unknown,
                    'F' => CellState.Filled,
                    'E' => CellState.Empty,
                    _ => throw new InvalidOperationException("Unsupported expected token.")
                };

                Assert.Equal(expected, board[row, column]);
            }
        }
    }
}
