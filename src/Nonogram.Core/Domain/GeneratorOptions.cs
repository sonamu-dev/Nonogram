namespace Nonogram.Core.Domain;

public sealed record GeneratorOptions
{
    public GeneratorOptions(
        int width,
        int height,
        int seed,
        double? targetFillRatio = null,
        int maxAttempts = 200,
        TimeSpan? maxTime = null,
        bool includeSolutionBoard = true)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be greater than zero.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be greater than zero.");
        }

        if (targetFillRatio is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(targetFillRatio),
                "TargetFillRatio must be between 0 and 1.");
        }

        if (maxAttempts <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxAttempts),
                "MaxAttempts must be greater than zero.");
        }

        if (maxTime.HasValue && maxTime.Value < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxTime),
                "MaxTime cannot be negative.");
        }

        Width = width;
        Height = height;
        Seed = seed;
        TargetFillRatio = targetFillRatio;
        MaxAttempts = maxAttempts;
        MaxTime = maxTime;
        IncludeSolutionBoard = includeSolutionBoard;
    }

    public int Width { get; }

    public int Height { get; }

    public int Seed { get; }

    public double? TargetFillRatio { get; }

    public int MaxAttempts { get; }

    public TimeSpan? MaxTime { get; }

    public bool IncludeSolutionBoard { get; }
}
