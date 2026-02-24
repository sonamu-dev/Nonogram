namespace Nonogram.Core.Domain;

public sealed record LineSolveResult(
    CellState[] UpdatedLine,
    bool IsContradiction,
    bool HasProgress);
