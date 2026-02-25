namespace Nonogram.Core.Domain;

public sealed record SearchSolveStats(
    int NodesVisited,
    int MaxDepth,
    int GuessCount
);
