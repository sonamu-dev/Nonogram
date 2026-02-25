using Nonogram.Core.Domain;

namespace Nonogram.Tests;

public class SearchSolverTests
{
    [Fact]
    public void Solve_UniquePuzzle_ReturnsSingleSolution()
    {
        PuzzleDefinition puzzle = CreatePuzzle(
            width: 3,
            height: 3,
            rowClues:
            [
                [3],
                [1],
                [1]
            ],
            columnClues:
            [
                [1, 1],
                [2],
                [1]
            ]);
        BoardState initial = CreateBoard(
            width: 3,
            height: 3,
            [
                "???",
                "???",
                "???"
            ]);

        SearchSolveResult result = SearchSolver.Solve(puzzle, initial);

        Assert.Equal(SearchSolveStatus.Solved, result.Status);
        Assert.Equal(1, result.SolutionsFoundCount);
        Assert.NotNull(result.FirstSolution);
        AssertBoard(
            result.FirstSolution!,
            [
                "FFF",
                "EFE",
                "FEE"
            ]);
        Assert.True(result.Stats.NodesVisited > 0);
    }

    [Fact]
    public void Solve_MultiplePuzzle_StopsAtTwoSolutions()
    {
        PuzzleDefinition puzzle = CreatePuzzle(
            width: 2,
            height: 2,
            rowClues:
            [
                [1],
                [1]
            ],
            columnClues:
            [
                [1],
                [1]
            ]);
        BoardState initial = CreateBoard(
            width: 2,
            height: 2,
            [
                "??",
                "??"
            ]);

        SearchSolveResult result = SearchSolver.Solve(puzzle, initial, maxSolutions: 2);

        Assert.Equal(SearchSolveStatus.MultipleSolutions, result.Status);
        Assert.Equal(2, result.SolutionsFoundCount);
        Assert.NotNull(result.FirstSolution);
        Assert.True(result.Stats.GuessCount > 0);
    }

    [Fact]
    public void Solve_ContradictionPuzzle_ReturnsZeroSolutions()
    {
        PuzzleDefinition puzzle = CreatePuzzle(
            width: 2,
            height: 2,
            rowClues:
            [
                [],
                []
            ],
            columnClues:
            [
                [],
                []
            ]);
        BoardState initial = CreateBoard(
            width: 2,
            height: 2,
            [
                "F?",
                "??"
            ]);

        SearchSolveResult result = SearchSolver.Solve(puzzle, initial);

        Assert.Equal(SearchSolveStatus.Contradiction, result.Status);
        Assert.Equal(0, result.SolutionsFoundCount);
        Assert.Null(result.FirstSolution);
        Assert.True(result.Stats.NodesVisited > 0);
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

    private static BoardState CreateBoard(
        int width,
        int height,
        string[] rows)
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
                    _ => throw new ArgumentException("Unsupported cell token.", nameof(rows)),
                };
            }
        }

        return new BoardState(width, height, cells);
    }

    private static void AssertBoard(
        BoardState board,
        string[] expectedRows)
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
                    _ => throw new InvalidOperationException("Unsupported expected token."),
                };

                Assert.Equal(expected, board[row, column]);
            }
        }
    }
}
