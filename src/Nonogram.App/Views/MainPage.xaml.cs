namespace Nonogram.App.Views;

public sealed partial class MainPage : Page
{
    private static readonly IReadOnlyDictionary<string, Type> PageMap = new Dictionary<string, Type>(StringComparer.Ordinal)
    {
        ["PlayPage"] = typeof(PlayPage),
        ["LibraryPage"] = typeof(LibraryPage),
        ["GeneratePage"] = typeof(GeneratePage),
        ["SettingsPage"] = typeof(SettingsPage)
    };

    public MainPage()
    {
        InitializeComponent();

        if (RootNavigationView.MenuItems.FirstOrDefault() is NavigationViewItem firstMenuItem)
        {
            RootNavigationView.SelectedItem = firstMenuItem;
            NavigateTo(firstMenuItem.Tag as string);
        }
    }

    private void OnNavigationSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer?.Tag is string pageKey)
        {
            NavigateTo(pageKey);
        }
    }

    private void NavigateTo(string? pageKey)
    {
        if (pageKey is null || !PageMap.TryGetValue(pageKey, out Type? pageType))
        {
            return;
        }

        if (ContentFrame.CurrentSourcePageType != pageType)
        {
            ContentFrame.Navigate(pageType);
        }
    }
}
