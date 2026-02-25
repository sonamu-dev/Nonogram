using Nonogram.Core.Domain;

namespace Nonogram.Tests;

public class PuzzleGeneratorTests
{
    [Fact]
    public void Generate_WithFixedSeed_ReturnsDeterministicPuzzle()
    {
        GeneratorOptions options = new(
            width: 4,
            height: 4,
            seed: 20260225,
            targetFillRatio: 0.5,
            maxAttempts: 250);

        PuzzleGenerationResult first = PuzzleGenerator.Generate(options);
        PuzzleGenerationResult second = PuzzleGenerator.Generate(options);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.NotNull(first.Puzzle);
        Assert.NotNull(second.Puzzle);

        GeneratedPuzzle firstPuzzle = first.Puzzle!;
        GeneratedPuzzle secondPuzzle = second.Puzzle!;

        Assert.Equal(first.AttemptCount, second.AttemptCount);
        AssertClues(firstPuzzle.Puzzle.RowClues, secondPuzzle.Puzzle.RowClues);
        AssertClues(firstPuzzle.Puzzle.ColumnClues, secondPuzzle.Puzzle.ColumnClues);
        Assert.Equal(firstPuzzle.DifficultyLabel, secondPuzzle.DifficultyLabel);
        Assert.Equal(firstPuzzle.Stats, secondPuzzle.Stats);
        Assert.Equal(firstPuzzle.SolutionBoard, secondPuzzle.SolutionBoard);
    }

    [Fact]
    public void Generate_WhenSuccessful_ResultPuzzleHasUniqueSolution()
    {
        GeneratorOptions options = new(
            width: 5,
            height: 5,
            seed: 7,
            targetFillRatio: 0.45,
            maxAttempts: 400);

        PuzzleGenerationResult generationResult = PuzzleGenerator.Generate(options);

        Assert.True(generationResult.IsSuccess);
        Assert.NotNull(generationResult.Puzzle);

        GeneratedPuzzle generatedPuzzle = generationResult.Puzzle!;
        BoardState emptyBoard = CreateUnknownBoard(
            generatedPuzzle.Puzzle.Width,
            generatedPuzzle.Puzzle.Height);
        SearchSolveResult verifyResult = SearchSolver.Solve(
            generatedPuzzle.Puzzle,
            emptyBoard,
            maxSolutions: 2);

        Assert.Equal(1, verifyResult.SolutionsFoundCount);
        Assert.Equal(SearchSolveStatus.Solved, verifyResult.Status);
    }

    [Fact]
    public void Generate_MaxAttemptsTooLow_ReturnsFailure()
    {
        GeneratorOptions options = new(
            width: 2,
            height: 2,
            seed: 1,
            targetFillRatio: 0.5,
            maxAttempts: 1);

        PuzzleGenerationResult result = PuzzleGenerator.Generate(options);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Puzzle);
        Assert.Equal(1, result.AttemptCount);
        Assert.Equal(PuzzleGenerationFailureReason.MaxAttemptsReached, result.FailureReason);
    }

    [Fact]
    public void Generate_WhenCancelled_ReturnsCancelledFailureReason()
    {
        GeneratorOptions options = new(
            width: 5,
            height: 5,
            seed: 1234,
            targetFillRatio: 0.5,
            maxAttempts: 100);
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();

        PuzzleGenerationResult result = PuzzleGenerator.Generate(
            options,
            cancellationTokenSource.Token);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Puzzle);
        Assert.Equal(0, result.AttemptCount);
        Assert.Equal(PuzzleGenerationFailureReason.Cancelled, result.FailureReason);
    }

    private static BoardState CreateUnknownBoard(int width, int height)
    {
        CellState[] cells = new CellState[checked(width * height)];
        Array.Fill(cells, CellState.Unknown);
        return new BoardState(width, height, cells);
    }

    private static void AssertClues(
        IReadOnlyList<IReadOnlyList<int>> expected,
        IReadOnlyList<IReadOnlyList<int>> actual)
    {
        Assert.Equal(expected.Count, actual.Count);

        for (int index = 0; index < expected.Count; index++)
        {
            Assert.Equal(expected[index], actual[index]);
        }
    }
}
