// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using ComicReaderUWP.SDK.Plugins.Comic;

using EHLinker.UI.Models;
using EHLinker.UI.Utils;

namespace EHLinker.UI;

internal partial class SidebarPageViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string _linkStatusText = string.Empty;
    public string LinkStatusText
    {
        get => _linkStatusText;
        set
        {
            _linkStatusText = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LinkStatusText)));
        }
    }

    private string _comicSearchBoxText = string.Empty;
    public string ComicSearchBoxText
    {
        get => _comicSearchBoxText;
        set
        {
            _comicSearchBoxText = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ComicSearchBoxText)));
        }
    }

    private bool _searchComicPreviousPageEnabled = false;
    public bool SearchComicPreviousPageEnabled
    {
        get => _searchComicPreviousPageEnabled;
        set
        {
            _searchComicPreviousPageEnabled = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SearchComicPreviousPageEnabled)));
        }
    }

    private bool _searchComicNextPageEnabled = false;
    public bool SearchComicNextPageEnabled
    {
        get => _searchComicNextPageEnabled;
        set
        {
            _searchComicNextPageEnabled = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SearchComicNextPageEnabled)));
        }
    }

    public ObservableCollection<ComicSearchResultItemViewModel> ComicSearchResultItems { get; } = [];

    private IComicModel? _comic;
    private ComicSearchResult? _lastSearchResult;

    public void Initialize()
    {
        UpdateSearchResult();
    }

    public void LoadComic(IComicModel? comic)
    {
        if (comic == _comic)
        {
            return;
        }

        _comic = comic;

        if (comic is null)
        {
            LinkStatusText = "Open a comic for linking.";
            ComicSearchBoxText = string.Empty;
            return;
        }

        ComicSearchBoxText = ExtractSearchKeyword(comic);

        string? link = GetComicLink(comic);
        if (string.IsNullOrEmpty(link))
        {
            LinkStatusText = $"The comic is linked with '{link}'.";
        }
        else
        {
            LinkStatusText = $"The comic is not linked.";
        }
    }

    public void SearchComics(string keyword)
    {
        CoroutineUtils.Run(async () =>
        {
            ComicSearchResult result;
            try
            {
                result = await PluginService.Ability.SearchComicsByKeyword(keyword);
            }
            catch (Exception ex)
            {
                LinkStatusText = ex.Message;
                return;
            }

            _lastSearchResult = result;
            UpdateSearchResult();
        });
    }

    public void SearchComicsPreviousPage()
    {
        CoroutineUtils.Run(async () =>
        {
            ComicSearchResult? searchResult = _lastSearchResult;
            if (searchResult is null || string.IsNullOrEmpty(searchResult.PreviousPageLink))
            {
                return;
            }

            await SearchComicsByLink(searchResult.PreviousPageLink);
        });
    }

    public void SearchComicsNextPage()
    {
        CoroutineUtils.Run(async () =>
        {
            ComicSearchResult? searchResult = _lastSearchResult;
            if (searchResult is null || string.IsNullOrEmpty(searchResult.NextPageLink))
            {
                return;
            }

            await SearchComicsByLink(searchResult.NextPageLink);
        });
    }

    private async Task SearchComicsByLink(string link)
    {
        ComicSearchResult result;
        try
        {
            result = await PluginService.Ability.SearchComicsByLink(link);
        }
        catch (Exception ex)
        {
            LinkStatusText = ex.Message;
            return;
        }

        _lastSearchResult = result;
        UpdateSearchResult();
    }

    private void UpdateSearchResult()
    {
        ComicSearchResult? searchResult = _lastSearchResult;

        if (searchResult is null)
        {
            SearchComicPreviousPageEnabled = false;
            SearchComicNextPageEnabled = false;
            return;
        }

        SearchComicPreviousPageEnabled = string.IsNullOrEmpty(searchResult.PreviousPageLink);
        SearchComicNextPageEnabled = string.IsNullOrEmpty(searchResult.NextPageLink);

        ComicSearchResultItems.Clear();
        foreach (ComicBasicInfo item in searchResult.Items)
        {
            ComicSearchResultItems.Add(new()
            {
                Title = item.Title,
                Description = $"{item.Category} | {item.PageCount} | {item.UploadTime} | {item.Rating}",
            });
        }
    }

    private static string ExtractSearchKeyword(IComicModel comic)
    {
        string title = comic.Title1;
        Match match = COMIC_TITLE_REGEX().Match(title);

        if (!match.Success)
        {
            return title;
        }

        return match.Groups[1].Value;
    }

    private static string? GetComicLink(IComicModel comic)
    {
        IReadOnlyDictionary<string, string> links = comic.Links;

        if (!links.TryGetValue(PluginConstants.LINK_NAME_EHENTAI, out string? link1))
        {
            return null;
        }

        if (!links.TryGetValue(PluginConstants.LINK_NAME_EXHENTAI, out string? link2))
        {
            return null;
        }

        if (link1 != link2)
        {
            return null;
        }

        return link1;
    }

    [GeneratedRegex(@"^\d{8}\s*-\s*(.+)$")]
    private static partial Regex COMIC_TITLE_REGEX();
}
