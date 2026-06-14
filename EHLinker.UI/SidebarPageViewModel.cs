// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using ComicReaderUWP.SDK.Plugins.Comic;

using EHLinker.UI.Models;
using EHLinker.UI.Utils;

namespace EHLinker.UI;

internal partial class SidebarPageViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private int _selectedTabIndex = 0;
    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set
        {
            _selectedTabIndex = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedTabIndex)));
        }
    }

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

    private string _comicTitle1 = string.Empty;
    public string ComicTitle1
    {
        get => _comicTitle1;
        set
        {
            _comicTitle1 = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ComicTitle1)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ComicTitle1Visible)));
        }
    }

    public bool ComicTitle1Visible => !string.IsNullOrEmpty(ComicTitle1);

    private string _comicTitle2 = string.Empty;
    public string ComicTitle2
    {
        get => _comicTitle2;
        set
        {
            _comicTitle2 = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ComicTitle2)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ComicTitle2Visible)));
        }
    }

    public bool ComicTitle2Visible => !string.IsNullOrEmpty(ComicTitle2);

    public ObservableCollection<ComicSearchResultItemViewModel> ComicSearchResultItems { get; } = [];
    public ObservableCollection<CommentItemViewModel> ComicCommentItems { get; } = [];

    private IComicModel? _comic;
    private ComicSearchResult? _lastSearchResult;
    private ComicDetailedInfo? _comicInfo;

    public void Initialize()
    {
        ClearComic();
    }

    public void LoadComic(IComicModel? comic)
    {
        if (comic == _comic)
        {
            return;
        }

        CoroutineUtils.Run(async () =>
        {
            ClearComic();
            _comic = comic;
            UpdateLinkStatus();
            await RequestComicInfo();

            if (_comicInfo is null)
            {
                SelectedTabIndex = 0;
            }
            else
            {
                SelectedTabIndex = 1;
            }
        });
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

    public void LinkComic(string link)
    {
        IComicModel? comic = _comic;
        if (comic is null)
        {
            return;
        }

        CoroutineUtils.Run(async () =>
        {
            var links = comic.Links.ToDictionary();
            links[PluginConstants.LINK_NAME_EHENTAI] = link.Replace(PluginConstants.URL_PREFIX_EXHENTAI, PluginConstants.URL_PREFIX_EHENTAI);
            links[PluginConstants.LINK_NAME_EXHENTAI] = link.Replace(PluginConstants.URL_PREFIX_EHENTAI, PluginConstants.URL_PREFIX_EXHENTAI);
            await comic.SetLinks(links);
            UpdateLinkStatus();
            await RequestComicInfo();
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

    private async Task RequestComicInfo()
    {
        IComicModel? comic = _comic;
        if (comic is null)
        {
            return;
        }

        if (_comicInfo is not null)
        {
            return;
        }

        string? link = GetComicLink(comic);
        if (string.IsNullOrEmpty(link))
        {
            return;
        }

        ComicDetailedInfo comicInfo;
        try
        {
            comicInfo = await PluginService.Ability.RequestComicInfo(link);
        }
        catch (Exception ex)
        {
            ComicTitle1 = ex.Message;
            return;
        }

        _comicInfo = comicInfo;
        UpdateComicInfo();
    }

    private void UpdateLinkStatus()
    {
        IComicModel? comic = _comic;
        if (comic is null)
        {
            return;
        }

        ComicSearchBoxText = ExtractSearchKeyword(comic);

        string? link = GetComicLink(comic);
        if (string.IsNullOrEmpty(link))
        {
            LinkStatusText = $"This comic is not linked.";
            return;
        }

        LinkStatusText = $"This comic is linked with '{link}'.";
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

        SearchComicPreviousPageEnabled = !string.IsNullOrEmpty(searchResult.PreviousPageLink);
        SearchComicNextPageEnabled = !string.IsNullOrEmpty(searchResult.NextPageLink);

        ComicSearchResultItems.Clear();
        foreach (ComicBasicInfo item in searchResult.Items)
        {
            ComicSearchResultItems.Add(new()
            {
                Title = item.Title,
                Description = $"{item.Category} | {item.PageCount} | {item.Link}",
                Source = item,
            });
        }
    }

    private void UpdateComicInfo()
    {
        ComicDetailedInfo? comicInfo = _comicInfo;
        if (comicInfo is null)
        {
            return;
        }

        ComicTitle1 = comicInfo.Title1;
        ComicTitle2 = comicInfo.Title2;

        ComicCommentItems.Clear();
        foreach (CommentInfo comment in comicInfo.Comments)
        {
            string voteText = comment.IsFromUploader ? "Uploader" : comment.Vote.ToString();
            string postTimeText = comment.PostTime.ToString();
            ComicCommentItems.Add(new()
            {
                Sender = comment.Sender,
                Content = comment.Content,
                Detail = $"{voteText} | {postTimeText}",
            });
        }
    }

    private void ClearComic()
    {
        _comic = null;
        _comicInfo = null;
        LinkStatusText = "Open a comic for linking.";
        ComicSearchBoxText = string.Empty;
        ComicSearchResultItems.Clear();
        ComicTitle1 = string.Empty;
        ComicTitle2 = string.Empty;
        ComicCommentItems.Clear();
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

        if (link1 != link2.Replace(PluginConstants.URL_PREFIX_EXHENTAI, PluginConstants.URL_PREFIX_EHENTAI))
        {
            return null;
        }

        return link1;
    }

    [GeneratedRegex(@"^\d{8}\s*-\s*(.+)$")]
    private static partial Regex COMIC_TITLE_REGEX();
}
