namespace Nonogram.Core.Contracts;

public enum DifficultyGrade
{
    Easy,
    Normal,
    Hard,
    Expert
}

public sealed record PuzzleSize(int Width, int Height);

public sealed record PuzzleDefinition(
    IReadOnlyList<IReadOnlyList<int>> RowHints,
    IReadOnlyList<IReadOnlyList<int>> ColumnHints);

public sealed record GenerationRequest(
    PuzzleSize Size,
    DifficultyGrade? TargetDifficulty = null,
    int? Seed = null);

public sealed record GenerationProgress(int AttemptedCandidates, int AcceptedCandidates);

public sealed record SolverStep(string Strategy, string Description);

public sealed record SolverResult(
    bool Solved,
    bool UsedGuessing,
    int BacktrackingCost,
    IReadOnlyList<SolverStep> Steps);

public sealed record DifficultyAssessment(
    DifficultyGrade Grade,
    string Summary,
    IReadOnlyList<string> Evidence);

public sealed record GenerationResult(
    PuzzleDefinition Puzzle,
    DifficultyAssessment Difficulty);

public interface IPuzzleSolver
{
    Task<SolverResult> SolveAsync(
        PuzzleDefinition puzzle,
        CancellationToken cancellationToken = default);
}

public interface IDifficultyAnalyzer
{
    Task<DifficultyAssessment> AnalyzeAsync(
        PuzzleDefinition puzzle,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default);
}

public interface IPuzzleGenerator
{
    Task<GenerationResult> GenerateAsync(
        GenerationRequest request,
        IProgress<GenerationProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
