using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Nonogram.Core.Domain;

namespace Nonogram.App.ViewModels;

public sealed class CellViewModel : INotifyPropertyChanged
{
    private CellState _state = CellState.Unknown;

    public CellViewModel(int index)
    {
        Index = index;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public int Index { get; }

    public CellState State
    {
        get => _state;
        set
        {
            if (_state == value)
            {
                return;
            }

            _state = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayText));
            OnPropertyChanged(nameof(FilledVisibility));
            OnPropertyChanged(nameof(EmptyVisibility));
        }
    }

    public string DisplayText => _state switch
    {
        CellState.Filled => "F",
        CellState.Empty => "E",
        _ => "?"
    };

    public Visibility FilledVisibility => _state == CellState.Filled
        ? Visibility.Visible
        : Visibility.Collapsed;

    public Visibility EmptyVisibility => _state == CellState.Empty
        ? Visibility.Visible
        : Visibility.Collapsed;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
