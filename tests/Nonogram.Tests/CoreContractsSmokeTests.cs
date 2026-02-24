using Nonogram.Core.Contracts;

namespace Nonogram.Tests;

public class CoreContractsSmokeTests
{
    [Fact]
    public void CoreContracts_AreAccessible()
    {
        PuzzleSize size = new(5, 5);
        GenerationRequest request = new(size, DifficultyGrade.Easy, Seed: 42);

        Assert.Equal(5, request.Size.Width);
        Assert.Equal(5, request.Size.Height);
        Assert.Equal(DifficultyGrade.Easy, request.TargetDifficulty);
    }
}
