// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System;

using ComicReaderUWP.SDK.Models;
using ComicReaderUWP.SDK.Plugins.Comic;
using ComicReaderUWP.SDK.Plugins.UI;

using EHLinker.UI.Data;
using EHLinker.UI.Utils;
using EHLinker.UI.ViewModels;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Navigation;

namespace EHLinker.UI.Views;

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
        string resourceFolderPath = PluginService.Context.PluginRootDirectoryPath;
        var resourceLocator = new System.Uri($"ms-appx:///{resourceFolderPath}/EHLinker.UI/Views/SidebarPage.xaml");
        Application.LoadComponent(this, resourceLocator, ComponentResourceLocation.Nested);
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        NavigationBundle.WindowContext.ReadingComicChanged += WindowContext_ReadingComicChanged;
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
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
            case nameof(SidebarPageViewModel.ErrorDialogMessage):
                {
                    DialogOptions.Builder options = new();
                    options.SetTitle("EHLinker error").SetContent(ViewModel.ErrorDialogMessage).SetPrimaryButtonText("OK");
                    NavigationBundle.WindowContext.EnqueueDialog(options.Build());
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
        ViewModel.SearchComics(ViewModel.ComicSearchBoxText, ViewModel.DisableSearchFilterChecked);
    }

    private void DisableSearchFilterCheckBox_Click(object sender, RoutedEventArgs e)
    {
        bool isChecked = ((CheckBox)sender).IsChecked == true;
        SettingsModel.DisableFilters = isChecked;
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        CoroutineUtils.Run(async () =>
        {
            var dialog = new SettingsDialog();
            await NavigationBundle.WindowContext.EnqueueDialog(dialog);
        });
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

    private void ListView_ContextRequested(UIElement sender, Microsoft.UI.Xaml.Input.ContextRequestedEventArgs args)
    {
        if (args.OriginalSource is not FrameworkElement fe)
        {
            return;
        }

        if (fe.DataContext is not ComicSearchResultItemViewModel model)
        {
            return;
        }

        args.Handled = true;

        var flyout = new MenuFlyout();

        {
            var item = new MenuFlyoutItem
            {
                Text = "Open in browser",
                Icon = new FontIcon
                {
                    Glyph = "\uE909",
                },
            };
            item.Click += (_, _) =>
            {
                CoroutineUtils.Run(async () =>
                {
                    await Windows.System.Launcher.LaunchUriAsync(new Uri(model.Source.Link));
                });
            };
            flyout.Items.Add(item);
        }

        if (args.TryGetPosition(fe, out Windows.Foundation.Point point))
        {
            flyout.ShowAt(fe, new FlyoutShowOptions { Position = point });
        }
        else
        {
            flyout.ShowAt(fe);
        }
    }

    private void RequestInfoRetryButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.RetryRequestComicInfo();
    }
}
