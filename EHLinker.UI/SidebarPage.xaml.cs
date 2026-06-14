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
    internal SidebarPageViewModel ViewModel { get; } = new();

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
        string resourceFolderPath = PluginService.Context.ResourceFolderPath;
        var resourceLocator = new System.Uri($"ms-appx:///{resourceFolderPath}/EHLinker.UI/SidebarPage.xaml");
        Application.LoadComponent(this, resourceLocator, ComponentResourceLocation.Nested);
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        NavigationBundle.WindowContext.ReadingComicChanged += WindowContext_ReadingComicChanged;
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        ViewModel.Initialize();
        ViewModel.LoadComic(NavigationBundle.WindowContext.ReadingComic);
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        NavigationBundle.WindowContext.ReadingComicChanged -= WindowContext_ReadingComicChanged;
        ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SidebarPageViewModel.SelectedTabIndex):
                switch (ViewModel.SelectedTabIndex)
                {
                    case 0:
                        TabSelectorBar.SelectedItem = TabSelectorBarItemLinker;
                        break;
                    case 1:
                        TabSelectorBar.SelectedItem = TabSelectorBarItemInfo;
                        break;
                    default:
                        break;
                }

                break;
            default:
                break;
        }
    }

    private void WindowContext_ReadingComicChanged(IComicModel? comic)
    {
        ViewModel.LoadComic(comic);
    }

    private void TabSelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        SelectorBarItem selectedItem = sender.SelectedItem;
        if (selectedItem == TabSelectorBarItemLinker)
        {
            LinkerTabGrid.Visibility = Visibility.Visible;
            InfoTabGrid.Visibility = Visibility.Collapsed;
        }
        else if (selectedItem == TabSelectorBarItemInfo)
        {
            LinkerTabGrid.Visibility = Visibility.Collapsed;
            InfoTabGrid.Visibility = Visibility.Visible;
        }
    }

    private void SearchComicButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SearchComics(ViewModel.ComicSearchBoxText);
    }

    private void SearchComicPreviousPageButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SearchComicsPreviousPage();
    }

    private void SearchComicNextPageButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SearchComicsNextPage();
    }

    private void SearchComicResultListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 0)
        {
            return;
        }

        if (e.AddedItems[0] is not ComicSearchResultItemViewModel model)
        {
            return;
        }

        ViewModel.LinkComic(model.Source.Link);
    }
}
