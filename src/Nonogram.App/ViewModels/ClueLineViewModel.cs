namespace Nonogram.App.ViewModels;

public sealed class ClueLineViewModel
{
    public ClueLineViewModel(string clueText)
    {
        ClueText = clueText;
    }

    public string ClueText { get; }
}
