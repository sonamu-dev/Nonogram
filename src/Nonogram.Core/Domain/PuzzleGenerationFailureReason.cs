namespace Nonogram.Core.Domain;

public enum PuzzleGenerationFailureReason
{
    MaxAttemptsReached = 0,
    MaxTimeReached = 1,
    Cancelled = 2
}
