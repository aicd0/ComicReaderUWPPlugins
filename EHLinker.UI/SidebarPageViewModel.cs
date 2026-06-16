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

    private string _errorDialogMessage = string.Empty;
    public string ErrorDialogMessage
    {
        get => _errorDialogMessage;
        set
        {
            _errorDialogMessage = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ErrorDialogMessage)));
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

    private bool _disableSearchFilterChecked = false;
    public bool DisableSearchFilterChecked
    {
        get => _disableSearchFilterChecked;
        set
        {
            _disableSearchFilterChecked = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisableSearchFilterChecked)));
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

    private bool _searchResultEmptyVisible = false;
    public bool SearchResultEmptyVisible
    {
        get => _searchResultEmptyVisible;
        set
        {
            _searchResultEmptyVisible = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SearchResultEmptyVisible)));
        }
    }

    private bool _infoTabContentVisible = false;
    public bool InfoTabContentVisible
    {
        get => _infoTabContentVisible;
        set
        {
            _infoTabContentVisible = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InfoTabContentVisible)));
        }
    }

    private bool _infoTabRetryVisible = false;
    public bool InfoTabRetryVisible
    {
        get => _infoTabRetryVisible;
        set
        {
            _infoTabRetryVisible = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InfoTabRetryVisible)));
        }
    }

    private string _requestInfoErrorMessage = string.Empty;
    public string RequestInfoErrorMessage
    {
        get => _requestInfoErrorMessage;
        set
        {
            _requestInfoErrorMessage = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RequestInfoErrorMessage)));
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

    // Search
    private SearchArgs? _searchingArgs = null;
    private ComicSearchResult? _lastSearchResult;

    // Comic Link
    private IComicModel? _comic;
    private string? _link;
    private ComicDetailedInfo? _comicInfo;

    public void Initialize()
    {
        CoroutineUtils.Run(async () =>
        {
            await LoadComicInternal(null);
            UpdateSearchResult();
        });
    }

    public void LoadComic(IComicModel? comic)
    {
        if (comic == _comic)
        {
            return;
        }

        CoroutineUtils.Run(async () =>
        {
            await LoadComicInternal(comic);
        });
    }

    public void SearchComics(string keyword, bool disableFilters)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return;
        }

        SearchArgs searchArgs = new()
        {
            Keyword = keyword,
            DisableFilters = disableFilters,
        };

        CoroutineUtils.Run(async () =>
        {
            await SearchComicsInternal(searchArgs);
        });
    }

    public void SearchComicsPreviousPage()
    {
        ComicSearchResult? searchResult = _lastSearchResult;
        if (searchResult is null || string.IsNullOrEmpty(searchResult.PreviousPageLink))
        {
            return;
        }

        SearchArgs searchArgs = new()
        {
            Link = searchResult.PreviousPageLink,
        };

        CoroutineUtils.Run(async () =>
        {
            await SearchComicsInternal(searchArgs);
        });
    }

    public void SearchComicsNextPage()
    {
        ComicSearchResult? searchResult = _lastSearchResult;
        if (searchResult is null || string.IsNullOrEmpty(searchResult.NextPageLink))
        {
            return;
        }

        SearchArgs searchArgs = new()
        {
            Link = searchResult.NextPageLink,
        };

        CoroutineUtils.Run(async () =>
        {
            await SearchComicsInternal(searchArgs);
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

            if (comic != _comic)
            {
                // Out-of-date
                return;
            }

            _link = GetComicLink(comic);
            UpdateLinkStatus();

            if (string.IsNullOrEmpty(_link))
            {
                return;
            }

            ClearComicInfo();
            await RequestComicInfo(comic, _link);
        });
    }

    public void RetryRequestComicInfo()
    {
        if (_comic is null || string.IsNullOrEmpty(_link))
        {
            return;
        }

        CoroutineUtils.Run(async () =>
        {
            ClearComicInfo();
            await RequestComicInfo(_comic, _link);
        });
    }

    private async Task SearchComicsInternal(SearchArgs args)
    {
        if (_searchingArgs == args)
        {
            return;
        }

        _searchingArgs = args;
        ComicSearchResult result;
        try
        {
            if (args.Link is not null)
            {
                result = await PluginService.Ability.SearchComicsByLink(args.Link);
            }
            else if (args.Keyword is not null)
            {
                result = await PluginService.Ability.SearchComicsByKeyword(args.Keyword, args.DisableFilters);
            }
            else
            {
                throw new ArgumentException("Invalid search arguments.");
            }
        }
        catch (Exception ex)
        {
            if (args != _searchingArgs)
            {
                // Out-of-date
                return;
            }

            _searchingArgs = null;
            _lastSearchResult = null;
            UpdateSearchResult();
            ErrorDialogMessage = ex.Message;
            return;
        }

        if (args != _searchingArgs)
        {
            // Out-of-date
            return;
        }

        _searchingArgs = null;
        _lastSearchResult = result;
        UpdateSearchResult();
    }

    private async Task LoadComicInternal(IComicModel? comic)
    {
        ClearComic();
        _comic = comic;

        if (comic is null)
        {
            LinkStatusText = "Open a comic for linking.";
            return;
        }

        _link = GetComicLink(comic);
        UpdateLinkStatus();
        ComicSearchBoxText = ExtractSearchKeyword(comic);

        if (string.IsNullOrEmpty(_link))
        {
            SelectedTabIndex = 0;
            return;
        }

        await RequestComicInfo(comic, _link);
    }

    private async Task RequestComicInfo(IComicModel comic, string link)
    {
        ComicDetailedInfo comicInfo;
        try
        {
            comicInfo = await PluginService.Ability.RequestComicInfo(link);
        }
        catch (Exception ex)
        {
            if (comic != _comic || _comicInfo is not null)
            {
                // Out-of-date
                return;
            }

            RequestInfoErrorMessage = ex.Message;
            InfoTabRetryVisible = true;
            return;
        }

        if (comic != _comic || _comicInfo is not null)
        {
            // Out-of-date
            return;
        }

        _comicInfo = comicInfo;
        SelectedTabIndex = 1;
        UpdateComicInfo(comicInfo);
    }

    private void UpdateLinkStatus()
    {
        string? link = _link;
        if (string.IsNullOrEmpty(link))
        {
            LinkStatusText = $"This comic is not linked.";
        }
        else
        {
            LinkStatusText = $"This comic is linked with '{link}'.";
        }
    }

    private void UpdateSearchResult()
    {
        ComicSearchResult? searchResult = _lastSearchResult;

        if (searchResult is null)
        {
            SearchResultEmptyVisible = false;
            SearchComicPreviousPageEnabled = false;
            SearchComicNextPageEnabled = false;
            ComicSearchResultItems.Clear();
        }
        else
        {
            SearchResultEmptyVisible = searchResult.Items.Count == 0;
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
    }

    private void UpdateComicInfo(ComicDetailedInfo comicInfo)
    {
        InfoTabRetryVisible = false;
        InfoTabContentVisible = true;
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
        _link = null;
        ComicSearchBoxText = string.Empty;
        ClearComicInfo();
    }

    private void ClearComicInfo()
    {
        _comicInfo = null;
        InfoTabContentVisible = false;
        InfoTabRetryVisible = false;
        ComicTitle1 = string.Empty;
        ComicTitle2 = string.Empty;
        ComicCommentItems.Clear();
        RequestInfoErrorMessage = string.Empty;
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

    private record SearchArgs
    {
        public string? Keyword { get; init; } = null;
        public bool DisableFilters { get; init; } = false;
        public string? Link { get; init; } = null;
    }
}
