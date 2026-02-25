namespace Nonogram.Core.Domain;

public sealed record SearchSolveResult(
    SearchSolveStatus Status,
    int SolutionsFoundCount,
    BoardState? FirstSolution,
    SearchSolveStats Stats
);
