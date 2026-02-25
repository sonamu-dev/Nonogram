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

    private int _seedCounter = unchecked((int)DateTime.UtcNow.Ticks);
    private int _gridWidth;
    private int _gridHeight;
    private string _statusMessage = "Click New Puzzle to start.";
    private bool _isGenerating;
    private bool _isSolved;
    private CellState[]? _solutionBoard;

    public event PropertyChangedEventHandler? PropertyChanged;

    public PlayViewModel()
    {
        CycleCellCommand = new RelayCommand<CellViewModel>(
            execute: cell =>
            {
                if (cell is not null)
                {
                    CycleCellState(cell);
                }
            });
    }

    public ObservableCollection<ClueLineViewModel> RowClues => _rowClues;

    public ObservableCollection<ClueLineViewModel> ColumnClues => _columnClues;

    public ObservableCollection<CellViewModel> Cells => _cells;

    public ICommand CycleCellCommand { get; }

    public int GridWidth => _gridWidth <= 0 ? 1 : _gridWidth;

    public int GridHeight => _gridHeight <= 0 ? 1 : _gridHeight;

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

    public void CycleCellState(CellViewModel cell)
    {
        ArgumentNullException.ThrowIfNull(cell);

        if (!CanInteractWithBoard)
        {
            return;
        }

        cell.State = cell.State switch
        {
            CellState.Unknown => CellState.Filled,
            CellState.Filled => CellState.Empty,
            _ => CellState.Unknown
        };

        EvaluateSolvedState();
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

        IsSolved = false;
        OnPropertyChanged(nameof(GridWidth));
        OnPropertyChanged(nameof(GridHeight));
        OnPropertyChanged(nameof(HasPuzzle));
        OnPropertyChanged(nameof(CanInteractWithBoard));
        OnPropertyChanged(nameof(BoardVisibility));
        OnPropertyChanged(nameof(EmptyBoardVisibility));
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
}
