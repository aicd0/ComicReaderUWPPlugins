// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace EHLinker;

internal partial class ClassicHttpClient : IDisposable
{
    private readonly HttpClient _client;
    private readonly CookieContainer _cookieContainer;

    public ClassicHttpClient()
    {
        _cookieContainer = new CookieContainer();

        var handler = new SocketsHttpHandler
        {
            AutomaticDecompression =
                DecompressionMethods.GZip |
                DecompressionMethods.Deflate |
                DecompressionMethods.Brotli,
            AllowAutoRedirect = true,
            UseCookies = true,
            CookieContainer = _cookieContainer,
            PooledConnectionLifetime = TimeSpan.FromMinutes(10),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
            MaxConnectionsPerServer = 20,
            ConnectTimeout = TimeSpan.FromSeconds(15)
        };

        var client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(60)
        };

        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/149.0.0.0 Safari/537.36");
        client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en,zh-CN;q=0.9,zh;q=0.8");
        client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip");
        client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("deflate");
        client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("br");

        client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
        {
            NoCache = true
        };

        _client = client;
    }

    public IReadOnlyDictionary<string, string> GetCookies(Uri uri)
    {
        CookieCollection cookies = _cookieContainer.GetCookies(uri);
        Dictionary<string, string> dict = [];
        foreach (Cookie cookie in cookies)
        {
            dict[cookie.Name] = cookie.Value;
        }

        return dict;
    }

    public void SetCookies(Uri uri, IReadOnlyDictionary<string, string> dict)
    {
        foreach (KeyValuePair<string, string> item in dict)
        {
            Cookie cookie = new(item.Key, item.Value);
            _cookieContainer.Add(uri, cookie);
        }
    }

    public async Task<string> GetHtmlAsync(string url, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        request.Version = HttpVersion.Version20;
        request.VersionPolicy = HttpVersionPolicy.RequestVersionOrLower;

        request.Headers.Add("Upgrade-Insecure-Requests", "1");

        using HttpResponseMessage response = await _client.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        await using Stream stream =
            await response.Content.ReadAsStreamAsync(cancellationToken);

        using var reader = new StreamReader(stream);

        return await reader.ReadToEndAsync(cancellationToken);
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}