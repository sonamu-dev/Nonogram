namespace Nonogram.Core.Domain;

public static class BoardSolver
{
    public static BoardSolveResult Solve(PuzzleDefinition puzzle, BoardState currentBoard)
    {
        ArgumentNullException.ThrowIfNull(puzzle);
        ArgumentNullException.ThrowIfNull(currentBoard);

        if (puzzle.Width != currentBoard.Width || puzzle.Height != currentBoard.Height)
        {
            throw new ArgumentException("Puzzle and board dimensions must match.");
        }

        int boardWidth = puzzle.Width;
        int boardHeight = puzzle.Height;

        CellState[] workingCells = currentBoard.ToArray();
        bool madeAnyProgress = false;

        int[][] rowClues = puzzle.RowClues.Select(NormalizeClue).ToArray();
        int[][] columnClues = puzzle.ColumnClues.Select(NormalizeClue).ToArray();

        while (true)
        {
            bool progressedInPass = false;

            for (int row = 0; row < boardHeight; row++)
            {
                CellState[] rowLine = ReadRow(workingCells, boardWidth, row);
                LineSolveResult rowResult = LineSolver.Solve(boardWidth, rowClues[row], rowLine);
                if (rowResult.IsContradiction)
                {
                    return new BoardSolveResult(
                        new BoardState(boardWidth, boardHeight, workingCells),
                        HasProgress: madeAnyProgress,
                        IsContradiction: true,
                        ContradictionSource: $"Row {row}");
                }

                if (ApplyRow(workingCells, boardWidth, row, rowResult.UpdatedLine))
                {
                    progressedInPass = true;
                    madeAnyProgress = true;
                }
            }

            for (int column = 0; column < boardWidth; column++)
            {
                CellState[] columnLine = ReadColumn(workingCells, boardWidth, boardHeight, column);
                LineSolveResult columnResult = LineSolver.Solve(boardHeight, columnClues[column], columnLine);
                if (columnResult.IsContradiction)
                {
                    return new BoardSolveResult(
                        new BoardState(boardWidth, boardHeight, workingCells),
                        HasProgress: madeAnyProgress,
                        IsContradiction: true,
                        ContradictionSource: $"Column {column}");
                }

                if (ApplyColumn(workingCells, boardWidth, boardHeight, column, columnResult.UpdatedLine))
                {
                    progressedInPass = true;
                    madeAnyProgress = true;
                }
            }

            if (!progressedInPass)
            {
                break;
            }
        }

        return new BoardSolveResult(
            new BoardState(boardWidth, boardHeight, workingCells),
            HasProgress: madeAnyProgress,
            IsContradiction: false);
    }

    private static int[] NormalizeClue(IReadOnlyList<int> clue)
    {
        if (clue.Count == 0)
        {
            return Array.Empty<int>();
        }

        return clue.ToArray();
    }

    private static CellState[] ReadRow(CellState[] cells, int width, int row)
    {
        CellState[] line = new CellState[width];
        Array.Copy(cells, row * width, line, 0, width);
        return line;
    }

    private static CellState[] ReadColumn(CellState[] cells, int width, int height, int column)
    {
        CellState[] line = new CellState[height];
        for (int row = 0; row < height; row++)
        {
            line[row] = cells[(row * width) + column];
        }

        return line;
    }

    private static bool ApplyRow(CellState[] cells, int width, int row, CellState[] updatedLine)
    {
        bool changed = false;
        int start = row * width;

        for (int column = 0; column < width; column++)
        {
            int index = start + column;
            if (cells[index] != updatedLine[column])
            {
                cells[index] = updatedLine[column];
                changed = true;
            }
        }

        return changed;
    }

    private static bool ApplyColumn(CellState[] cells, int width, int height, int column, CellState[] updatedLine)
    {
        bool changed = false;

        for (int row = 0; row < height; row++)
        {
            int index = (row * width) + column;
            if (cells[index] != updatedLine[row])
            {
                cells[index] = updatedLine[row];
                changed = true;
            }
        }

        return changed;
    }
}
