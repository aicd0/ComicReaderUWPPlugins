// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System;
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

    public ClassicHttpClient()
    {
        var handler = new SocketsHttpHandler
        {
            AutomaticDecompression =
                DecompressionMethods.GZip |
                DecompressionMethods.Deflate |
                DecompressionMethods.Brotli,
            AllowAutoRedirect = true,
            UseCookies = true,
            CookieContainer = new CookieContainer(),
            PooledConnectionLifetime = TimeSpan.FromMinutes(10),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
            MaxConnectionsPerServer = 20,
            ConnectTimeout = TimeSpan.FromSeconds(15)
        };

        _client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(60)
        };

        ConfigureDefaultHeaders(_client);
    }

    private static void ConfigureDefaultHeaders(HttpClient client)
    {
        client.DefaultRequestHeaders.Clear();

        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
            "AppleWebKit/537.36 (KHTML, like Gecko) " +
            "Chrome/137.0.0.0 Safari/537.36");

        client.DefaultRequestHeaders.Accept.ParseAdd(
            "text/html,application/xhtml+xml,application/xml;q=0.9," +
            "image/avif,image/webp,*/*;q=0.8");

        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd(
            "en-US,en;q=0.9");

        client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip");
        client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("deflate");
        client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("br");

        client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
        {
            NoCache = true
        };
    }

    public async Task<string> GetHtmlAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            url);

        request.Version = HttpVersion.Version20;
        request.VersionPolicy =
            HttpVersionPolicy.RequestVersionOrLower;

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