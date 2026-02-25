namespace Nonogram.Core.Domain;

public static class PuzzleGenerator
{
    private const int EasyNodeThreshold = 8;
    private const int MediumNodeThreshold = 64;
    private const int EasyDepthThreshold = 1;
    private const int MediumDepthThreshold = 4;
    private const int MediumGuessThreshold = 2;

    public static PuzzleGenerationResult Generate(
        GeneratorOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        Random random = new(options.Seed);
        DateTime startedAtUtc = DateTime.UtcNow;

        for (int attempt = 1; attempt <= options.MaxAttempts; attempt++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return PuzzleGenerationResult.Failure(
                    PuzzleGenerationFailureReason.Cancelled,
                    attemptCount: attempt - 1);
            }

            if (HasExceededMaxTime(options.MaxTime, startedAtUtc))
            {
                return PuzzleGenerationResult.Failure(
                    PuzzleGenerationFailureReason.MaxTimeReached,
                    attemptCount: attempt - 1);
            }

            bool[] solutionCells = CreateRandomSolutionCells(options, random);
            SolutionGrid solutionGrid = new(options.Width, options.Height, solutionCells);
            PuzzleDefinition puzzle = ClueGenerator.CreatePuzzleDefinition(solutionGrid);
            BoardState emptyBoard = CreateUnknownBoard(options.Width, options.Height);
            SearchSolveResult solveResult = SearchSolver.Solve(
                puzzle,
                emptyBoard,
                maxSolutions: 2);

            if (solveResult.SolutionsFoundCount != 1)
            {
                continue;
            }

            CellState[]? solutionBoard = options.IncludeSolutionBoard
                ? ConvertToCellStateBoard(solutionCells)
                : null;

            GeneratedPuzzleDifficulty difficulty = ClassifyDifficulty(solveResult.Stats);
            GeneratedPuzzle generatedPuzzle = new(
                puzzle,
                solutionBoard,
                difficulty,
                solveResult.Stats);

            return PuzzleGenerationResult.Success(generatedPuzzle, attempt);
        }

        return PuzzleGenerationResult.Failure(
            PuzzleGenerationFailureReason.MaxAttemptsReached,
            options.MaxAttempts);
    }

    private static bool HasExceededMaxTime(TimeSpan? maxTime, DateTime startedAtUtc)
    {
        if (maxTime is null)
        {
            return false;
        }

        TimeSpan elapsed = DateTime.UtcNow - startedAtUtc;
        return elapsed >= maxTime.Value;
    }

    private static bool[] CreateRandomSolutionCells(GeneratorOptions options, Random random)
    {
        int cellCount = checked(options.Width * options.Height);
        bool[] cells = new bool[cellCount];

        if (options.TargetFillRatio is null)
        {
            for (int index = 0; index < cellCount; index++)
            {
                cells[index] = random.NextDouble() < 0.5d;
            }

            return cells;
        }

        int fillCount = (int)Math.Round(
            options.TargetFillRatio.Value * cellCount,
            MidpointRounding.AwayFromZero);

        int[] shuffledIndices = CreateShuffledIndices(cellCount, random);
        for (int index = 0; index < fillCount; index++)
        {
            cells[shuffledIndices[index]] = true;
        }

        return cells;
    }

    private static int[] CreateShuffledIndices(int count, Random random)
    {
        int[] indices = new int[count];
        for (int index = 0; index < count; index++)
        {
            indices[index] = index;
        }

        for (int index = count - 1; index > 0; index--)
        {
            int swapIndex = random.Next(index + 1);
            (indices[index], indices[swapIndex]) = (indices[swapIndex], indices[index]);
        }

        return indices;
    }

    private static BoardState CreateUnknownBoard(int width, int height)
    {
        CellState[] cells = new CellState[checked(width * height)];
        Array.Fill(cells, CellState.Unknown);
        return new BoardState(width, height, cells);
    }

    private static CellState[] ConvertToCellStateBoard(bool[] solutionCells)
    {
        CellState[] converted = new CellState[solutionCells.Length];
        for (int index = 0; index < solutionCells.Length; index++)
        {
            converted[index] = solutionCells[index]
                ? CellState.Filled
                : CellState.Empty;
        }

        return converted;
    }

    private static GeneratedPuzzleDifficulty ClassifyDifficulty(SearchSolveStats stats)
    {
        if (stats.GuessCount == 0 &&
            stats.MaxDepth <= EasyDepthThreshold &&
            stats.NodesVisited <= EasyNodeThreshold)
        {
            return GeneratedPuzzleDifficulty.Easy;
        }

        if (stats.GuessCount <= MediumGuessThreshold &&
            stats.MaxDepth <= MediumDepthThreshold &&
            stats.NodesVisited <= MediumNodeThreshold)
        {
            return GeneratedPuzzleDifficulty.Medium;
        }

        return GeneratedPuzzleDifficulty.Hard;
    }
}
