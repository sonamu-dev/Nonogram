namespace Nonogram.Core.Domain;

public sealed record GeneratedPuzzle
{
    public GeneratedPuzzle(
        PuzzleDefinition puzzle,
        CellState[]? solutionBoard,
        GeneratedPuzzleDifficulty difficultyLabel,
        SearchSolveStats stats)
    {
        ArgumentNullException.ThrowIfNull(puzzle);
        ArgumentNullException.ThrowIfNull(stats);

        if (solutionBoard is not null && solutionBoard.Length != checked(puzzle.Width * puzzle.Height))
        {
            throw new ArgumentException(
                "Solution board size must match puzzle dimensions.",
                nameof(solutionBoard));
        }

        Puzzle = puzzle;
        SolutionBoard = solutionBoard is null ? null : (CellState[])solutionBoard.Clone();
        DifficultyLabel = difficultyLabel;
        Stats = stats;
    }

    public PuzzleDefinition Puzzle { get; }

    public CellState[]? SolutionBoard { get; }

    public GeneratedPuzzleDifficulty DifficultyLabel { get; }

    public SearchSolveStats Stats { get; }
}
