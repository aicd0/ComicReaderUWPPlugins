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

internal sealed partial class ClassicHttpClient : IDisposable
{
    private const int MIN_QUERY_PROXY_INTERVAL = 5000;

    private readonly object _lock = new();
    private RefCounted<WrappedHttpClient>? _clientRef;
    private ProxyService.ProxyInfo? _proxyInfo;
    private long _lastQueryProxyTime = 0;

    public IReadOnlyDictionary<string, string> GetCookies(Uri uri)
    {
        using RefCounted<WrappedHttpClient> clientRef = RefClient();
        WrappedHttpClient client = clientRef.Value;
        return client.GetCookies(uri);
    }

    public void SetCookies(Uri uri, IReadOnlyDictionary<string, string> dict)
    {
        using RefCounted<WrappedHttpClient> clientRef = RefClient();
        WrappedHttpClient client = clientRef.Value;
        client.SetCookies(uri, dict);
    }

    public async Task<string> GetHtmlAsync(string url, CancellationToken cancellationToken = default)
    {
        using RefCounted<WrappedHttpClient> clientRef = RefClient();
        WrappedHttpClient client = clientRef.Value;

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Version = HttpVersion.Version20;
        request.VersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
        request.Headers.Add("Upgrade-Insecure-Requests", "1");

        using HttpResponseMessage response = await client.Client.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        response.EnsureSuccessStatusCode();

        await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(cancellationToken);
    }

    public void Dispose()
    {
        lock (_lock)
        {
            _clientRef?.Unref();
            _clientRef = null;
        }
    }

    private RefCounted<WrappedHttpClient> RefClient()
    {
        long time = Environment.TickCount64;

        bool proxyMayChanged = time - _lastQueryProxyTime >= MIN_QUERY_PROXY_INTERVAL;
        ProxyService.ProxyInfo? newProxyInfo = null;
        if (proxyMayChanged)
        {
            newProxyInfo = ProxyService.GetProxyInfo();
            _lastQueryProxyTime = time;
        }

        lock (_lock)
        {
            bool proxyChanged = proxyMayChanged && newProxyInfo != _proxyInfo;
            if (_clientRef is not null && !proxyChanged)
            {
                _clientRef.Ref();
                return _clientRef;
            }

            RefCounted<WrappedHttpClient>? oldClientRef = _clientRef;
            WrappedHttpClient? oldClient = oldClientRef?.Value;

            CookieContainer newCookieContainer = oldClient?.CloneCookies() ?? new CookieContainer();
            WrappedHttpClient newClient = new(newCookieContainer, newProxyInfo?.CreateProxy());
            RefCounted<WrappedHttpClient> newClientRef = new(newClient);

            _proxyInfo = newProxyInfo;
            _clientRef = newClientRef;
            oldClientRef?.Unref();

            newClientRef.Ref();
            return newClientRef;
        }
    }

    private sealed partial class WrappedHttpClient : IDisposable
    {
        public readonly HttpClient Client;

        public readonly CookieContainer _cookieContainer;

        public WrappedHttpClient(CookieContainer cookieContainer, IWebProxy? proxy)
        {
            _cookieContainer = cookieContainer;

            var socksHttpHandler = new SocketsHttpHandler
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
                ConnectTimeout = TimeSpan.FromSeconds(15),
                UseProxy = proxy is not null,
                Proxy = proxy,
            };

            var client = new HttpClient(socksHttpHandler)
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

            Client = client;
        }

        public void Dispose()
        {
            Client.Dispose();
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

        public CookieContainer CloneCookies()
        {
            CookieCollection cookies = _cookieContainer.GetAllCookies();
            CookieContainer cookieContainer = new();
            foreach (Cookie cookie in cookies)
            {
                cookieContainer.Add(cookie);
            }

            return cookieContainer;
        }
    }
}
