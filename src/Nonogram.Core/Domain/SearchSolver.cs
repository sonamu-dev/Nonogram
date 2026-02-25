namespace Nonogram.Core.Domain;

public static class SearchSolver
{
    public static SearchSolveResult Solve(PuzzleDefinition puzzle, BoardState initialBoard, int maxSolutions = 2)
    {
        ArgumentNullException.ThrowIfNull(puzzle);
        ArgumentNullException.ThrowIfNull(initialBoard);

        if (maxSolutions <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxSolutions), "maxSolutions must be greater than zero.");
        }

        SearchContext context = new(maxSolutions);
        SearchRecursive(
            puzzle,
            initialBoard,
            depth: 0,
            context);

        SearchSolveStatus status = context.SolutionsFound switch
        {
            0 => SearchSolveStatus.Contradiction,
            1 => SearchSolveStatus.Solved,
            _ => SearchSolveStatus.MultipleSolutions,
        };

        return new SearchSolveResult(
            status,
            context.SolutionsFound,
            context.FirstSolution,
            new SearchSolveStats(
                context.NodesVisited,
                context.MaxDepth,
                context.GuessCount
            )
        );
    }

    private static void SearchRecursive(
        PuzzleDefinition puzzle,
        BoardState currentBoard,
        int depth,
        SearchContext context)
    {
        if (context.SolutionsFound >= context.MaxSolutions)
        {
            return;
        }

        context.NodesVisited++;
        if (depth > context.MaxDepth)
        {
            context.MaxDepth = depth;
        }

        BoardSolveResult propagation = BoardSolver.Solve(puzzle, currentBoard);
        if (propagation.IsContradiction)
        {
            return;
        }

        BoardState stabilizedBoard = propagation.UpdatedBoard;
        CellState[] stabilizedCells = stabilizedBoard.ToArray();
        int branchIndex = FindFirstUnknownIndex(stabilizedCells);
        if (branchIndex < 0)
        {
            context.SolutionsFound++;
            context.FirstSolution ??= stabilizedBoard;
            return;
        }

        context.GuessCount++;

        SearchRecursive(
            puzzle,
            CreateBranchedBoard(stabilizedBoard, stabilizedCells, branchIndex, CellState.Filled),
            depth + 1,
            context);

        if (context.SolutionsFound >= context.MaxSolutions)
        {
            return;
        }

        SearchRecursive(
            puzzle,
            CreateBranchedBoard(stabilizedBoard, stabilizedCells, branchIndex, CellState.Empty),
            depth + 1,
            context);
    }

    private static int FindFirstUnknownIndex(CellState[] cells)
    {
        for (int index = 0; index < cells.Length; index++)
        {
            if (cells[index] == CellState.Unknown)
            {
                return index;
            }
        }

        return -1;
    }

    private static BoardState CreateBranchedBoard(
        BoardState stabilizedBoard,
        CellState[] stabilizedCells,
        int branchIndex,
        CellState branchState)
    {
        CellState[] branchedCells = (CellState[])stabilizedCells.Clone();
        branchedCells[branchIndex] = branchState;
        return new BoardState(stabilizedBoard.Width, stabilizedBoard.Height, branchedCells);
    }

    private sealed class SearchContext
    {
        public SearchContext(int maxSolutions)
        {
            MaxSolutions = maxSolutions;
        }

        public int MaxSolutions { get; }

        public int NodesVisited { get; set; }

        public int MaxDepth { get; set; }

        public int GuessCount { get; set; }

        public int SolutionsFound { get; set; }

        public BoardState? FirstSolution { get; set; }
    }
}
