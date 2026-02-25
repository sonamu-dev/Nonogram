namespace Nonogram.App.Views.Pages;

public sealed partial class PlayPage : Page
{
    private CancellationTokenSource? _generationCancellationTokenSource;

    public PlayPage()
    {
        ViewModel = new PlayViewModel();
        InitializeComponent();
        DataContext = ViewModel;
        Unloaded += OnPageUnloaded;
    }

    public PlayViewModel ViewModel { get; }

    private async void OnNewPuzzleClicked(object sender, RoutedEventArgs e)
    {
        if (ViewModel.IsGenerating)
        {
            return;
        }

        _generationCancellationTokenSource?.Dispose();
        _generationCancellationTokenSource = new CancellationTokenSource();

        try
        {
            await ViewModel.GenerateNewPuzzleAsync(_generationCancellationTokenSource.Token);
        }
        finally
        {
            _generationCancellationTokenSource?.Dispose();
            _generationCancellationTokenSource = null;
        }
    }

    private void OnCancelClicked(object sender, RoutedEventArgs e)
    {
        _generationCancellationTokenSource?.Cancel();
    }

    private void OnCellItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is CellViewModel cell)
        {
            ViewModel.CycleCellState(cell);
        }
    }

    private void OnPageUnloaded(object sender, RoutedEventArgs e)
    {
        _generationCancellationTokenSource?.Cancel();
        _generationCancellationTokenSource?.Dispose();
        _generationCancellationTokenSource = null;
    }
}
