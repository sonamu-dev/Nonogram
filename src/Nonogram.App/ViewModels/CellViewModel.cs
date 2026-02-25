using System.ComponentModel;
using System.Runtime.CompilerServices;
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
        }
    }

    public string DisplayText => _state switch
    {
        CellState.Filled => "F",
        CellState.Empty => "E",
        _ => "?"
    };

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
