namespace Nonogram.Core.Domain;

public sealed record BoardSolveResult(
    BoardState UpdatedBoard,
    bool HasProgress,
    bool IsContradiction,
    string? ContradictionSource = null
);
