// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Playwright;

namespace EHLinker;

internal sealed class BrowserHtmlFetcher : IAsyncDisposable
{
    public static async Task<BrowserHtmlFetcher> CreateAsync()
    {
        IPlaywright playwright = await Playwright.CreateAsync();

        IBrowser browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });

        IBrowserContext context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize
            {
                Width = 1920,
                Height = 1080
            },
            UserAgent =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
                "AppleWebKit/537.36 (KHTML, like Gecko) " +
                "Chrome/137.0.0.0 Safari/537.36",
            Locale = "en-US",
            DeviceScaleFactor = 1,
            IgnoreHTTPSErrors = false
        });

        return new BrowserHtmlFetcher(playwright, browser, context);
    }

    private readonly IPlaywright _playwright;
    private readonly IBrowser _browser;
    private readonly IBrowserContext _context;

    private BrowserHtmlFetcher(IPlaywright playwright, IBrowser browser, IBrowserContext context)
    {
        _playwright = playwright;
        _browser = browser;
        _context = context;
    }

    public async Task<string> GetHtmlAsync(string url, CancellationToken cancellationToken = default)
    {
        IPage page = await _context.NewPageAsync();

        try
        {
            await page.GotoAsync(
                url,
                new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.NetworkIdle,
                    Timeout = 60000
                });

            return await page.ContentAsync();
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
        await _browser.DisposeAsync();
        _playwright.Dispose();
    }
}
