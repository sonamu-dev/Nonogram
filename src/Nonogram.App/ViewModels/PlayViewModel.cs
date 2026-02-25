using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.UI.Xaml;
using Nonogram.Core.Domain;

namespace Nonogram.App.ViewModels;

public sealed class PlayViewModel : INotifyPropertyChanged
{
    private const int DefaultBoardSize = 10;

    private readonly ObservableCollection<ClueLineViewModel> _rowClues = new();
    private readonly ObservableCollection<ClueLineViewModel> _columnClues = new();
    private readonly ObservableCollection<CellViewModel> _cells = new();
    private readonly Stack<HistoryBatch> _undoHistory = new();
    private readonly Stack<HistoryBatch> _redoHistory = new();

    private readonly RelayCommand _undoCommand;
    private readonly RelayCommand _redoCommand;
    private readonly RelayCommand _resetCommand;

    private int _seedCounter = unchecked((int)DateTime.UtcNow.Ticks);
    private int _gridWidth;
    private int _gridHeight;
    private string _statusMessage = "Click New Puzzle to start.";
    private bool _isGenerating;
    private bool _isSolved;
    private InputMode _currentInputMode = InputMode.Fill;
    private CellState[]? _solutionBoard;
    private List<CellChange>? _activeBatchChanges;
    private HashSet<int>? _activeBatchTouchedIndices;

    public event PropertyChangedEventHandler? PropertyChanged;

    public PlayViewModel()
    {
        _undoCommand = new RelayCommand(Undo, () => CanUndo);
        _redoCommand = new RelayCommand(Redo, () => CanRedo);
        _resetCommand = new RelayCommand(ResetBoard, () => CanReset);

        UndoCommand = _undoCommand;
        RedoCommand = _redoCommand;
        ResetCommand = _resetCommand;
    }

    public ObservableCollection<ClueLineViewModel> RowClues => _rowClues;

    public ObservableCollection<ClueLineViewModel> ColumnClues => _columnClues;

    public ObservableCollection<CellViewModel> Cells => _cells;

    public ICommand UndoCommand { get; }

    public ICommand RedoCommand { get; }

    public ICommand ResetCommand { get; }

    public int GridWidth => _gridWidth <= 0 ? 1 : _gridWidth;

    public int GridHeight => _gridHeight <= 0 ? 1 : _gridHeight;

    public InputMode CurrentInputMode => _currentInputMode;

    public bool IsFillModeSelected => _currentInputMode == InputMode.Fill;

    public bool IsMarkEmptyModeSelected => _currentInputMode == InputMode.MarkEmpty;

    public bool IsEraseModeSelected => _currentInputMode == InputMode.Erase;

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetField(ref _statusMessage, value);
    }

    public bool IsGenerating
    {
        get => _isGenerating;
        private set
        {
            if (!SetField(ref _isGenerating, value))
            {
                return;
            }

            OnPropertyChanged(nameof(CanStartNewPuzzle));
            OnPropertyChanged(nameof(CanCancelGeneration));
            OnPropertyChanged(nameof(CanInteractWithBoard));
            OnPropertyChanged(nameof(GenerationVisibility));
            UpdateHistoryState();
        }
    }

    public bool IsSolved
    {
        get => _isSolved;
        private set
        {
            if (!SetField(ref _isSolved, value))
            {
                return;
            }

            OnPropertyChanged(nameof(SolvedMessage));
            OnPropertyChanged(nameof(SolvedVisibility));
        }
    }

    public bool HasPuzzle => _cells.Count > 0;

    public bool CanStartNewPuzzle => !IsGenerating;

    public bool CanCancelGeneration => IsGenerating;

    public bool CanInteractWithBoard => !IsGenerating && HasPuzzle;

    public bool CanUndo => !IsGenerating && _undoHistory.Count > 0;

    public bool CanRedo => !IsGenerating && _redoHistory.Count > 0;

    public bool CanReset => !IsGenerating && HasPuzzle && _cells.Any(cell => cell.State != CellState.Unknown);

    public Visibility GenerationVisibility => IsGenerating
        ? Visibility.Visible
        : Visibility.Collapsed;

    public Visibility BoardVisibility => HasPuzzle
        ? Visibility.Visible
        : Visibility.Collapsed;

    public Visibility EmptyBoardVisibility => HasPuzzle
        ? Visibility.Collapsed
        : Visibility.Visible;

    public string SolvedMessage => IsSolved ? "Solved!" : string.Empty;

    public Visibility SolvedVisibility => IsSolved
        ? Visibility.Visible
        : Visibility.Collapsed;

    public void SetInputMode(InputMode mode)
    {
        if (_currentInputMode == mode)
        {
            return;
        }

        _currentInputMode = mode;
        OnPropertyChanged(nameof(CurrentInputMode));
        OnPropertyChanged(nameof(IsFillModeSelected));
        OnPropertyChanged(nameof(IsMarkEmptyModeSelected));
        OnPropertyChanged(nameof(IsEraseModeSelected));
    }

    public async Task GenerateNewPuzzleAsync(CancellationToken cancellationToken)
    {
        if (IsGenerating)
        {
            return;
        }

        IsGenerating = true;
        IsSolved = false;
        StatusMessage = "Generating puzzle...";

        try
        {
            int seed = Interlocked.Increment(ref _seedCounter);
            int boardSize = DefaultBoardSize;

            GeneratorOptions options = new(
                width: boardSize,
                height: boardSize,
                seed: seed,
                targetFillRatio: 0.45,
                maxAttempts: 400,
                maxTime: TimeSpan.FromSeconds(10),
                includeSolutionBoard: true);

            PuzzleGenerationResult generationResult = await Task.Run(
                () => PuzzleGenerator.Generate(options, cancellationToken));

            if (generationResult.IsSuccess && generationResult.Puzzle is not null)
            {
                ApplyGeneratedPuzzle(generationResult.Puzzle);
                StatusMessage =
                    $"Ready: {boardSize}x{boardSize}, seed {seed}, {generationResult.Puzzle.DifficultyLabel}, attempts {generationResult.AttemptCount}.";
                return;
            }

            StatusMessage = generationResult.FailureReason switch
            {
                PuzzleGenerationFailureReason.Cancelled => "Generation cancelled.",
                PuzzleGenerationFailureReason.MaxTimeReached => "Generation timed out. Try New Puzzle again.",
                PuzzleGenerationFailureReason.MaxAttemptsReached => "No unique puzzle found. Try New Puzzle again.",
                _ => "Puzzle generation failed."
            };
        }
        catch (Exception ex)
        {
            StatusMessage = $"Generation failed: {ex.Message}";
        }
        finally
        {
            IsGenerating = false;
        }
    }

    public void BeginInputBatch()
    {
        if (!CanInteractWithBoard || _activeBatchChanges is not null)
        {
            return;
        }

        _activeBatchChanges = new List<CellChange>();
        _activeBatchTouchedIndices = new HashSet<int>();
    }

    public void ApplyInputToCell(CellViewModel cell, InputMode? overrideMode = null)
    {
        ArgumentNullException.ThrowIfNull(cell);

        if (!CanInteractWithBoard)
        {
            return;
        }

        bool ownsBatch = _activeBatchChanges is null;
        if (ownsBatch)
        {
            BeginInputBatch();
        }

        bool didChange = ApplyStateToCell(
            cell,
            MapInputModeToState(overrideMode ?? _currentInputMode));

        if (didChange)
        {
            EvaluateSolvedState();
            UpdateHistoryState();
        }

        if (ownsBatch)
        {
            EndInputBatch();
        }
    }

    public void EndInputBatch()
    {
        if (_activeBatchChanges is null)
        {
            return;
        }

        if (_activeBatchChanges.Count > 0)
        {
            _undoHistory.Push(new HistoryBatch(_activeBatchChanges.ToArray()));
            _redoHistory.Clear();
        }

        _activeBatchChanges = null;
        _activeBatchTouchedIndices = null;

        EvaluateSolvedState();
        UpdateHistoryState();
    }

    private void Undo()
    {
        if (!CanUndo)
        {
            return;
        }

        HistoryBatch batch = _undoHistory.Pop();
        ApplyBatch(batch, reverse: true);
        _redoHistory.Push(batch);

        EvaluateSolvedState();
        UpdateHistoryState();
    }

    private void Redo()
    {
        if (!CanRedo)
        {
            return;
        }

        HistoryBatch batch = _redoHistory.Pop();
        ApplyBatch(batch, reverse: false);
        _undoHistory.Push(batch);

        EvaluateSolvedState();
        UpdateHistoryState();
    }

    private void ResetBoard()
    {
        if (!CanReset)
        {
            return;
        }

        BeginInputBatch();

        foreach (CellViewModel cell in _cells)
        {
            ApplyStateToCell(cell, CellState.Unknown);
        }

        EndInputBatch();
    }

    private bool ApplyStateToCell(CellViewModel cell, CellState targetState)
    {
        if (_activeBatchChanges is null || _activeBatchTouchedIndices is null)
        {
            return false;
        }

        if (!_activeBatchTouchedIndices.Add(cell.Index))
        {
            return false;
        }

        CellState previousState = cell.State;
        if (previousState == targetState)
        {
            return false;
        }

        _activeBatchChanges.Add(new CellChange(cell.Index, previousState, targetState));
        cell.State = targetState;
        return true;
    }

    private static CellState MapInputModeToState(InputMode inputMode)
    {
        return inputMode switch
        {
            InputMode.Fill => CellState.Filled,
            InputMode.MarkEmpty => CellState.Empty,
            InputMode.Erase => CellState.Unknown,
            _ => CellState.Unknown
        };
    }

    private void ApplyBatch(HistoryBatch batch, bool reverse)
    {
        foreach (CellChange change in batch.Changes)
        {
            CellState nextState = reverse ? change.PreviousState : change.NextState;
            _cells[change.CellIndex].State = nextState;
        }
    }

    private void ApplyGeneratedPuzzle(GeneratedPuzzle generatedPuzzle)
    {
        _gridWidth = generatedPuzzle.Puzzle.Width;
        _gridHeight = generatedPuzzle.Puzzle.Height;
        _solutionBoard = generatedPuzzle.SolutionBoard is null
            ? null
            : (CellState[])generatedPuzzle.SolutionBoard.Clone();

        _rowClues.Clear();
        foreach (IReadOnlyList<int> clue in generatedPuzzle.Puzzle.RowClues)
        {
            _rowClues.Add(new ClueLineViewModel(ToRowClueText(clue)));
        }

        _columnClues.Clear();
        foreach (IReadOnlyList<int> clue in generatedPuzzle.Puzzle.ColumnClues)
        {
            _columnClues.Add(new ClueLineViewModel(ToColumnClueText(clue)));
        }

        _cells.Clear();
        int cellCount = checked(generatedPuzzle.Puzzle.Width * generatedPuzzle.Puzzle.Height);
        for (int index = 0; index < cellCount; index++)
        {
            _cells.Add(new CellViewModel(index));
        }

        _undoHistory.Clear();
        _redoHistory.Clear();
        _activeBatchChanges = null;
        _activeBatchTouchedIndices = null;

        IsSolved = false;
        OnPropertyChanged(nameof(GridWidth));
        OnPropertyChanged(nameof(GridHeight));
        OnPropertyChanged(nameof(HasPuzzle));
        OnPropertyChanged(nameof(CanInteractWithBoard));
        OnPropertyChanged(nameof(BoardVisibility));
        OnPropertyChanged(nameof(EmptyBoardVisibility));
        UpdateHistoryState();
    }

    private void EvaluateSolvedState()
    {
        if (_solutionBoard is null || _solutionBoard.Length != _cells.Count)
        {
            IsSolved = false;
            return;
        }

        for (int index = 0; index < _cells.Count; index++)
        {
            CellState currentState = _cells[index].State;
            if (currentState == CellState.Unknown)
            {
                IsSolved = false;
                return;
            }

            if (currentState != _solutionBoard[index])
            {
                IsSolved = false;
                return;
            }
        }

        IsSolved = true;
    }

    private void UpdateHistoryState()
    {
        OnPropertyChanged(nameof(CanUndo));
        OnPropertyChanged(nameof(CanRedo));
        OnPropertyChanged(nameof(CanReset));

        _undoCommand.RaiseCanExecuteChanged();
        _redoCommand.RaiseCanExecuteChanged();
        _resetCommand.RaiseCanExecuteChanged();
    }

    private static string ToRowClueText(IReadOnlyList<int> clue)
    {
        return clue.Count == 0 ? "0" : string.Join(" ", clue);
    }

    private static string ToColumnClueText(IReadOnlyList<int> clue)
    {
        return clue.Count == 0 ? "0" : string.Join("\n", clue);
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private readonly record struct CellChange(int CellIndex, CellState PreviousState, CellState NextState);

    private sealed class HistoryBatch
    {
        public HistoryBatch(IReadOnlyList<CellChange> changes)
        {
            Changes = changes;
        }

        public IReadOnlyList<CellChange> Changes { get; }
    }
}
