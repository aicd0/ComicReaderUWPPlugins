// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;

using EHLinker.UI;
using EHLinker.UI.Models;

namespace EHLinker;

internal class PluginAbility : IPluginAbility
{
    private readonly object _browserLock = new();
    private BrowserHtmlFetcher? _browser;

    public async Task<ComicSearchResult> SearchComicsByKeyword(string keyword)
    {
        string url = $"https://e-hentai.org/?f_search={Uri.EscapeDataString(keyword)}";
        BrowserHtmlFetcher browser = await GetBrowser();
        string html = await browser.GetHtmlAsync(url);
        return ParseComicSearchResult(html);
    }

    public async Task<ComicSearchResult> SearchComicsByLink(string link)
    {
        BrowserHtmlFetcher browser = await GetBrowser();
        string html = await browser.GetHtmlAsync(link);
        return ParseComicSearchResult(html);
    }

    private async Task<BrowserHtmlFetcher> GetBrowser()
    {
        BrowserHtmlFetcher? browser = _browser;
        if (browser is not null)
        {
            return browser;
        }

        BrowserHtmlFetcher? tempBrowser = await BrowserHtmlFetcher.CreateAsync();
        try
        {
            lock (_browserLock)
            {
                browser = _browser;
                if (browser is not null)
                {
                    return browser;
                }

                browser = tempBrowser;
                _browser = tempBrowser;
                tempBrowser = null;
            }
        }
        finally
        {
            if (tempBrowser is not null)
            {
                await tempBrowser.DisposeAsync();
            }
        }

        return browser;
    }

    private static ComicSearchResult ParseComicSearchResult(string html)
    {
        return new()
        {
            Items = [],
            PreviousPageLink = null,
            NextPageLink = null,
        };
    }
}
