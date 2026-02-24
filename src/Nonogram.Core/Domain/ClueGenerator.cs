namespace Nonogram.Core.Domain;

public static class ClueGenerator
{
    public static PuzzleDefinition CreatePuzzleDefinition(SolutionGrid solutionGrid)
    {
        ArgumentNullException.ThrowIfNull(solutionGrid);

        IReadOnlyList<IReadOnlyList<int>> rowClues = GenerateRowClues(solutionGrid);
        IReadOnlyList<IReadOnlyList<int>> columnClues = GenerateColumnClues(solutionGrid);
        return new PuzzleDefinition(solutionGrid.Width, solutionGrid.Height, rowClues, columnClues);
    }

    public static IReadOnlyList<IReadOnlyList<int>> GenerateRowClues(SolutionGrid solutionGrid)
    {
        ArgumentNullException.ThrowIfNull(solutionGrid);

        IReadOnlyList<int>[] rowClues = new IReadOnlyList<int>[solutionGrid.Height];
        for (int row = 0; row < solutionGrid.Height; row++)
        {
            int localRow = row;
            rowClues[row] = BuildLineClues(solutionGrid.Width, column => solutionGrid[localRow, column]);
        }

        return rowClues;
    }

    public static IReadOnlyList<IReadOnlyList<int>> GenerateColumnClues(SolutionGrid solutionGrid)
    {
        ArgumentNullException.ThrowIfNull(solutionGrid);

        IReadOnlyList<int>[] columnClues = new IReadOnlyList<int>[solutionGrid.Width];
        for (int column = 0; column < solutionGrid.Width; column++)
        {
            int localColumn = column;
            columnClues[column] = BuildLineClues(solutionGrid.Height, row => solutionGrid[row, localColumn]);
        }

        return columnClues;
    }

    private static IReadOnlyList<int> BuildLineClues(int length, Func<int, bool> isFilledAt)
    {
        List<int>? clues = null;
        int streak = 0;

        for (int index = 0; index < length; index++)
        {
            if (isFilledAt(index))
            {
                streak++;
                continue;
            }

            if (streak > 0)
            {
                clues ??= new List<int>();
                clues.Add(streak);
                streak = 0;
            }
        }

        if (streak > 0)
        {
            clues ??= new List<int>();
            clues.Add(streak);
        }

        return clues is null ? Array.Empty<int>() : clues.ToArray();
    }
}
