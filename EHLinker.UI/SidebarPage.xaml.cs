// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using ComicReaderUWP.SDK.Plugins.Comic;
using ComicReaderUWP.SDK.Plugins.UI;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Navigation;

namespace EHLinker.UI;

public sealed partial class SidebarPage : Page
{
    private IPageNavigationBundle? _navigationBundle;
    private IPageNavigationBundle NavigationBundle => _navigationBundle!;

    public SidebarPage()
    {
        InitializeComponentForPlugin();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        switch (e.NavigationMode)
        {
            case NavigationMode.New:
            case NavigationMode.Back:
            case NavigationMode.Forward:
                _navigationBundle = (IPageNavigationBundle)e.Parameter;
                break;
            case NavigationMode.Refresh:
                break;
        }
    }

    private void InitializeComponentForPlugin()
    {
        if (_contentLoaded)
        {
            return;
        }

        _contentLoaded = true;
        string resourceFolderPath = PluginService.PluginContext.ResourceFolderPath;
        var resourceLocator = new System.Uri($"ms-appx:///{resourceFolderPath}/EHLinker.UI/SidebarPage.xaml");
        Application.LoadComponent(this, resourceLocator, ComponentResourceLocation.Nested);
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        NavigationBundle.WindowContext.ReadingComicChanged += WindowContext_ReadingComicChanged;
        TabSelectorBar.SelectedItem = TabSelectorBarItemLinker;
        LoadComic(NavigationBundle.WindowContext.ReadingComic);
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        NavigationBundle.WindowContext.ReadingComicChanged -= WindowContext_ReadingComicChanged;
    }

    private void WindowContext_ReadingComicChanged(IComicModel? comic)
    {
        LoadComic(comic);
    }

    private void TabSelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        SelectorBarItem selectedItem = sender.SelectedItem;
        if (selectedItem == TabSelectorBarItemLinker)
        {
            LinkerTabGrid.Visibility = Visibility.Visible;
            CommentsTabGrid.Visibility = Visibility.Collapsed;
        }
        else if (selectedItem == TabSelectorBarItemComments)
        {
            LinkerTabGrid.Visibility = Visibility.Collapsed;
            CommentsTabGrid.Visibility = Visibility.Visible;
        }
    }

    private void LoadComic(IComicModel? comic)
    {
        TestTextBlock.Text = comic?.Title1 ?? "<null>";
    }
}
