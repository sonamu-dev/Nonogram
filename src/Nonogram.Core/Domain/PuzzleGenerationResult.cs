namespace Nonogram.Core.Domain;

public sealed record PuzzleGenerationResult
{
    private PuzzleGenerationResult(
        GeneratedPuzzle? puzzle,
        int attemptCount,
        PuzzleGenerationFailureReason? failureReason)
    {
        if (attemptCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(attemptCount),
                "AttemptCount cannot be negative.");
        }

        Puzzle = puzzle;
        AttemptCount = attemptCount;
        FailureReason = failureReason;
    }

    public GeneratedPuzzle? Puzzle { get; }

    public int AttemptCount { get; }

    public PuzzleGenerationFailureReason? FailureReason { get; }

    public bool IsSuccess => Puzzle is not null;

    public static PuzzleGenerationResult Success(
        GeneratedPuzzle puzzle,
        int attemptCount)
    {
        ArgumentNullException.ThrowIfNull(puzzle);

        if (attemptCount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(attemptCount),
                "AttemptCount must be greater than zero for success.");
        }

        return new PuzzleGenerationResult(puzzle, attemptCount, failureReason: null);
    }

    public static PuzzleGenerationResult Failure(
        PuzzleGenerationFailureReason failureReason,
        int attemptCount)
    {
        return new PuzzleGenerationResult(puzzle: null, attemptCount, failureReason);
    }
}
