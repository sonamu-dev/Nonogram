using Nonogram.Core.Domain;

namespace Nonogram.Tests;

public class LineSolverTests
{
    [Fact]
    public void Solve_EmptyClue_FillsAllCellsAsEmpty()
    {
        CellState[] input = [CellState.Unknown, CellState.Unknown, CellState.Unknown, CellState.Unknown, CellState.Unknown];

        LineSolveResult result = LineSolver.Solve(
            lineLength: 5,
            clue: Array.Empty<int>(),
            currentLine: input);

        Assert.False(result.IsContradiction);
        Assert.True(result.HasProgress);
        Assert.Equal(
            [CellState.Empty, CellState.Empty, CellState.Empty, CellState.Empty, CellState.Empty],
            result.UpdatedLine);
    }

    [Fact]
    public void Solve_FullLineClue_FillsAllCells()
    {
        CellState[] input = [CellState.Unknown, CellState.Unknown, CellState.Unknown, CellState.Unknown];

        LineSolveResult result = LineSolver.Solve(
            lineLength: 4,
            clue: [4],
            currentLine: input);

        Assert.False(result.IsContradiction);
        Assert.True(result.HasProgress);
        Assert.Equal(
            [CellState.Filled, CellState.Filled, CellState.Filled, CellState.Filled],
            result.UpdatedLine);
    }

    [Fact]
    public void Solve_SingleBlock_UsesCommonCellsAcrossCandidates()
    {
        CellState[] input = [CellState.Unknown, CellState.Filled, CellState.Unknown, CellState.Unknown, CellState.Unknown];

        LineSolveResult result = LineSolver.Solve(
            lineLength: 5,
            clue: [3],
            currentLine: input);

        Assert.False(result.IsContradiction);
        Assert.True(result.HasProgress);
        Assert.Equal(
            [CellState.Unknown, CellState.Filled, CellState.Filled, CellState.Unknown, CellState.Empty],
            result.UpdatedLine);
    }

    [Fact]
    public void Solve_MultiBlock_ResolvesMandatoryGapAndOverlap()
    {
        CellState[] input =
        [
            CellState.Filled,
            CellState.Unknown,
            CellState.Unknown,
            CellState.Unknown,
            CellState.Unknown,
            CellState.Unknown,
            CellState.Empty
        ];

        LineSolveResult result = LineSolver.Solve(
            lineLength: 7,
            clue: [2, 2],
            currentLine: input);

        Assert.False(result.IsContradiction);
        Assert.True(result.HasProgress);
        Assert.Equal(
            [
                CellState.Filled,
                CellState.Filled,
                CellState.Empty,
                CellState.Unknown,
                CellState.Filled,
                CellState.Unknown,
                CellState.Empty
            ],
            result.UpdatedLine);
    }

    [Fact]
    public void Solve_Contradiction_WhenNoCandidatePatternExists()
    {
        CellState[] input = [CellState.Empty, CellState.Empty, CellState.Empty, CellState.Empty];

        LineSolveResult result = LineSolver.Solve(
            lineLength: 4,
            clue: [1],
            currentLine: input);

        Assert.True(result.IsContradiction);
        Assert.False(result.HasProgress);
        Assert.Equal(input, result.UpdatedLine);
    }

    [Fact]
    public void Solve_GoldenCase_ReturnsExpectedLine()
    {
        CellState[] input =
        [
            CellState.Filled,
            CellState.Unknown,
            CellState.Unknown,
            CellState.Unknown,
            CellState.Filled
        ];

        LineSolveResult result = LineSolver.Solve(
            lineLength: 5,
            clue: [2, 1],
            currentLine: input);

        Assert.False(result.IsContradiction);
        Assert.True(result.HasProgress);
        Assert.Equal(
            [CellState.Filled, CellState.Filled, CellState.Empty, CellState.Empty, CellState.Filled],
            result.UpdatedLine);
    }

    [Fact]
    public void Solve_WithSameInput_IsDeterministic()
    {
        CellState[] input =
        [
            CellState.Unknown,
            CellState.Filled,
            CellState.Unknown,
            CellState.Empty,
            CellState.Unknown,
            CellState.Unknown
        ];
        int[] clue = [2, 1];

        LineSolveResult first = LineSolver.Solve(6, clue, input);
        LineSolveResult second = LineSolver.Solve(6, clue, input);

        Assert.Equal(first.IsContradiction, second.IsContradiction);
        Assert.Equal(first.HasProgress, second.HasProgress);
        Assert.Equal(first.UpdatedLine, second.UpdatedLine);
    }
}
