using System.Collections.Generic;
using System.Windows.Input;
using Microsoft.UI.Xaml.Input;
using Windows.System;

namespace Nonogram.App.Views.Pages;

public sealed partial class PlayPage : Page
{
    private readonly HashSet<int> _dragVisitedCells = new();

    private CancellationTokenSource? _generationCancellationTokenSource;
    private bool _isDragInputActive;
    private uint _activePointerId;
    private InputMode _activeDragMode = InputMode.Fill;

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

    private void OnFillModeChecked(object sender, RoutedEventArgs e)
    {
        ViewModel.SetInputMode(InputMode.Fill);
    }

    private void OnMarkEmptyModeChecked(object sender, RoutedEventArgs e)
    {
        ViewModel.SetInputMode(InputMode.MarkEmpty);
    }

    private void OnEraseModeChecked(object sender, RoutedEventArgs e)
    {
        ViewModel.SetInputMode(InputMode.Erase);
    }

    private void OnCellPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: CellViewModel cell } || !ViewModel.CanInteractWithBoard)
        {
            return;
        }

        var currentPoint = e.GetCurrentPoint(CellsGrid);
        bool isLeftButtonPressed = currentPoint.Properties.IsLeftButtonPressed;
        bool isRightButtonPressed = currentPoint.Properties.IsRightButtonPressed;
        if (!isLeftButtonPressed && !isRightButtonPressed)
        {
            return;
        }

        InputMode dragMode = isRightButtonPressed
            ? InputMode.MarkEmpty
            : ResolveLeftPointerMode(e.KeyModifiers);

        StartDragBatch(e.Pointer.PointerId, dragMode);
        ApplyDragInput(cell);

        CellsGrid.CapturePointer(e.Pointer);
        e.Handled = true;
    }

    private void OnCellPointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (!_isDragInputActive || e.Pointer.PointerId != _activePointerId)
        {
            return;
        }

        if (sender is not FrameworkElement { DataContext: CellViewModel cell })
        {
            return;
        }

        ApplyDragInput(cell);
        e.Handled = true;
    }

    private void OnCellsGridPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        EndDragBatch(e.Pointer.PointerId);
    }

    private void OnCellsGridPointerCanceled(object sender, PointerRoutedEventArgs e)
    {
        EndDragBatch(e.Pointer.PointerId);
    }

    private void OnCellsGridPointerCaptureLost(object sender, PointerRoutedEventArgs e)
    {
        EndDragBatch();
    }

    private void OnUndoKeyboardAcceleratorInvoked(
        KeyboardAccelerator sender,
        KeyboardAcceleratorInvokedEventArgs args)
    {
        ExecuteCommand(ViewModel.UndoCommand, args);
    }

    private void OnRedoKeyboardAcceleratorInvoked(
        KeyboardAccelerator sender,
        KeyboardAcceleratorInvokedEventArgs args)
    {
        ExecuteCommand(ViewModel.RedoCommand, args);
    }

    private static void ExecuteCommand(ICommand command, KeyboardAcceleratorInvokedEventArgs args)
    {
        if (!command.CanExecute(null))
        {
            return;
        }

        command.Execute(null);
        args.Handled = true;
    }

    private InputMode ResolveLeftPointerMode(VirtualKeyModifiers keyModifiers)
    {
        return keyModifiers.HasFlag(VirtualKeyModifiers.Shift)
            ? InputMode.Erase
            : ViewModel.CurrentInputMode;
    }

    private void StartDragBatch(uint pointerId, InputMode dragMode)
    {
        EndDragBatch();

        _isDragInputActive = true;
        _activePointerId = pointerId;
        _activeDragMode = dragMode;
        _dragVisitedCells.Clear();

        ViewModel.BeginInputBatch();
    }

    private void ApplyDragInput(CellViewModel cell)
    {
        if (!_isDragInputActive)
        {
            return;
        }

        if (!_dragVisitedCells.Add(cell.Index))
        {
            return;
        }

        ViewModel.ApplyInputToCell(cell, _activeDragMode);
    }

    private void EndDragBatch(uint pointerId = 0)
    {
        if (!_isDragInputActive)
        {
            return;
        }

        if (pointerId != 0 && pointerId != _activePointerId)
        {
            return;
        }

        _isDragInputActive = false;
        _activePointerId = 0;
        _dragVisitedCells.Clear();

        ViewModel.EndInputBatch();
        CellsGrid.ReleasePointerCaptures();
    }

    private void OnPageUnloaded(object sender, RoutedEventArgs e)
    {
        EndDragBatch();

        _generationCancellationTokenSource?.Cancel();
        _generationCancellationTokenSource?.Dispose();
        _generationCancellationTokenSource = null;
    }
}
