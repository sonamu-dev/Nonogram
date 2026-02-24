namespace Nonogram.Core.Domain;

public static class LineSolver
{
    public static LineSolveResult Solve(int lineLength, int[] clue, CellState[] currentLine)
    {
        if (lineLength <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lineLength), "Line length must be greater than zero.");
        }

        ArgumentNullException.ThrowIfNull(clue);
        ArgumentNullException.ThrowIfNull(currentLine);

        if (currentLine.Length != lineLength)
        {
            throw new ArgumentException("Current line length must match lineLength.", nameof(currentLine));
        }

        if (clue.Any(value => value <= 0))
        {
            throw new ArgumentException("All clue values must be greater than zero.", nameof(clue));
        }

        CellState[] updatedLine = (CellState[])currentLine.Clone();
        bool[] workingPattern = new bool[lineLength];
        int[] minimumTailLength = BuildMinimumTailLength(clue);
        PatternAccumulator accumulator = new(lineLength);

        EnumeratePatterns(
            clue,
            currentLine,
            lineLength,
            clueIndex: 0,
            searchStart: 0,
            workingPattern,
            minimumTailLength,
            accumulator);

        if (accumulator.ValidPatternCount == 0)
        {
            return new LineSolveResult(updatedLine, IsContradiction: true, HasProgress: false);
        }

        bool hasProgress = false;
        for (int index = 0; index < lineLength; index++)
        {
            CellState nextState = updatedLine[index];
            if (accumulator.AlwaysFilled[index])
            {
                nextState = CellState.Filled;
            }
            else if (accumulator.AlwaysEmpty[index])
            {
                nextState = CellState.Empty;
            }

            if (nextState != updatedLine[index])
            {
                updatedLine[index] = nextState;
                hasProgress = true;
            }
        }

        return new LineSolveResult(updatedLine, IsContradiction: false, HasProgress: hasProgress);
    }

    private static void EnumeratePatterns(
        int[] clue,
        CellState[] currentLine,
        int lineLength,
        int clueIndex,
        int searchStart,
        bool[] workingPattern,
        int[] minimumTailLength,
        PatternAccumulator accumulator)
    {
        if (clueIndex == clue.Length)
        {
            if (IsCompatibleWithCurrentLine(currentLine, workingPattern))
            {
                accumulator.Accept(workingPattern);
            }

            return;
        }

        int blockLength = clue[clueIndex];
        int maxStart = lineLength - minimumTailLength[clueIndex];

        for (int start = searchStart; start <= maxStart; start++)
        {
            if (!IsEmptyRangeCompatible(currentLine, searchStart, start))
            {
                continue;
            }

            if (!IsFilledRangeCompatible(currentLine, start, blockLength))
            {
                continue;
            }

            for (int offset = 0; offset < blockLength; offset++)
            {
                workingPattern[start + offset] = true;
            }

            int nextSearchStart = start + blockLength + 1;
            EnumeratePatterns(
                clue,
                currentLine,
                lineLength,
                clueIndex + 1,
                nextSearchStart,
                workingPattern,
                minimumTailLength,
                accumulator);

            for (int offset = 0; offset < blockLength; offset++)
            {
                workingPattern[start + offset] = false;
            }
        }
    }

    private static int[] BuildMinimumTailLength(int[] clue)
    {
        if (clue.Length == 0)
        {
            return Array.Empty<int>();
        }

        int[] minimumTailLength = new int[clue.Length];
        int runningLength = 0;

        for (int index = clue.Length - 1; index >= 0; index--)
        {
            runningLength += clue[index];
            if (index < clue.Length - 1)
            {
                runningLength += 1;
            }

            minimumTailLength[index] = runningLength;
        }

        return minimumTailLength;
    }

    private static bool IsEmptyRangeCompatible(CellState[] currentLine, int start, int endExclusive)
    {
        for (int index = start; index < endExclusive; index++)
        {
            if (currentLine[index] == CellState.Filled)
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsFilledRangeCompatible(CellState[] currentLine, int start, int length)
    {
        for (int offset = 0; offset < length; offset++)
        {
            if (currentLine[start + offset] == CellState.Empty)
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsCompatibleWithCurrentLine(CellState[] currentLine, bool[] pattern)
    {
        for (int index = 0; index < currentLine.Length; index++)
        {
            if (currentLine[index] == CellState.Filled && !pattern[index])
            {
                return false;
            }

            if (currentLine[index] == CellState.Empty && pattern[index])
            {
                return false;
            }
        }

        return true;
    }

    private sealed class PatternAccumulator
    {
        public PatternAccumulator(int lineLength)
        {
            AlwaysFilled = Enumerable.Repeat(true, lineLength).ToArray();
            AlwaysEmpty = Enumerable.Repeat(true, lineLength).ToArray();
        }

        public int ValidPatternCount { get; private set; }

        public bool[] AlwaysFilled { get; }

        public bool[] AlwaysEmpty { get; }

        public void Accept(bool[] pattern)
        {
            if (ValidPatternCount == 0)
            {
                for (int index = 0; index < pattern.Length; index++)
                {
                    AlwaysFilled[index] = pattern[index];
                    AlwaysEmpty[index] = !pattern[index];
                }

                ValidPatternCount = 1;
                return;
            }

            for (int index = 0; index < pattern.Length; index++)
            {
                AlwaysFilled[index] &= pattern[index];
                AlwaysEmpty[index] &= !pattern[index];
            }

            ValidPatternCount++;
        }
    }
}
