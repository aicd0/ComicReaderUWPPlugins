// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace EHLinker;

public static partial class ProxyService
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct WINHTTP_CURRENT_USER_IE_PROXY_CONFIG
    {
        [MarshalAs(UnmanagedType.Bool)]
        public bool AutoDetect;

        public IntPtr AutoConfigUrl;
        public IntPtr Proxy;
        public IntPtr ProxyBypass;
    }

    [DllImport("winhttp.dll", SetLastError = true)]
    private static extern bool WinHttpGetIEProxyConfigForCurrentUser(out WINHTTP_CURRENT_USER_IE_PROXY_CONFIG config);

    [LibraryImport("kernel32.dll")]
    private static partial IntPtr GlobalFree(IntPtr hMem);

    public static ProxyInfo? GetProxyInfo()
    {
        if (!WinHttpGetIEProxyConfigForCurrentUser(out WINHTTP_CURRENT_USER_IE_PROXY_CONFIG cfg))
        {
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        }

        try
        {
            string? proxyString = Marshal.PtrToStringUni(cfg.Proxy);
            string? bypassString = Marshal.PtrToStringUni(cfg.ProxyBypass);

            if (string.IsNullOrWhiteSpace(proxyString))
            {
                return null;
            }

            // Example values:
            // "proxy.company.com:8080"
            // "http=proxy1:8080;https=proxy2:8080"

            string proxyAddress = ExtractProxyAddress(proxyString);

            return new()
            {
                Address = proxyAddress,
                BypassList = string.IsNullOrWhiteSpace(bypassString) ? [] : bypassString.Split(';', StringSplitOptions.RemoveEmptyEntries),
            };
        }
        finally
        {
            if (cfg.Proxy != IntPtr.Zero)
            {
                GlobalFree(cfg.Proxy);
            }

            if (cfg.ProxyBypass != IntPtr.Zero)
            {
                GlobalFree(cfg.ProxyBypass);
            }

            if (cfg.AutoConfigUrl != IntPtr.Zero)
            {
                GlobalFree(cfg.AutoConfigUrl);
            }
        }
    }

    private static string ExtractProxyAddress(string proxy)
    {
        // Handles:
        // proxy:8080
        // http=proxy:8080;https=proxy2:8080

        if (!proxy.Contains('='))
        {
            return proxy.StartsWith("http",
                StringComparison.OrdinalIgnoreCase)
                ? proxy
                : $"http://{proxy}";
        }

        foreach (string part in proxy.Split(';'))
        {
            if (part.StartsWith("https=",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "http://" + part[6..];
            }

            if (part.StartsWith("http=",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "http://" + part[5..];
            }
        }

        throw new InvalidOperationException($"Unsupported proxy format: {proxy}");
    }

    public class ProxyInfo
    {
        public required string Address { get; init; }
        public required string[]? BypassList { get; init; }

        public override bool Equals(object? obj)
        {
            if (obj is not ProxyInfo info)
            {
                return false;
            }

            return
                Address == info.Address &&
                StructuralComparisons.StructuralEqualityComparer.Equals(BypassList, info.BypassList);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                Address,
                BypassList is null ? 0 : StructuralComparisons.StructuralEqualityComparer.GetHashCode(BypassList));
        }

        public static bool operator ==(ProxyInfo? left, ProxyInfo? right)
        {
            if (left is null)
            {
                return right is null;
            }

            return left.Equals(right);
        }

        public static bool operator !=(ProxyInfo? left, ProxyInfo? right)
        {
            return !(left == right);
        }

        public IWebProxy? CreateProxy()
        {
            WebProxy proxy = new(Address);

            if (BypassList is not { Length: > 0 } bypassList)
            {
                return proxy;
            }

            // WinINET stores proxy exceptions as wildcard patterns ("*zhihu.com", "<local>"), while
            // WebProxy expects regular expressions, so they have to be converted instead of assigned directly.
            List<string> regexList = [];
            bool bypassLocal = false;

            foreach (string item in bypassList)
            {
                string entry = item.Trim();
                if (entry.Length == 0)
                {
                    continue;
                }

                if (entry.Equals("<local>", StringComparison.OrdinalIgnoreCase))
                {
                    bypassLocal = true;
                    continue;
                }

                regexList.Add(ConvertBypassEntry(entry));
            }

            proxy.BypassProxyOnLocal = bypassLocal;
            if (regexList.Count > 0)
            {
                proxy.BypassList = [.. regexList];
            }

            return proxy;
        }

        /// <summary>
        /// Converts a WinINET proxy exception entry into a regular expression matching the
        /// "scheme://host[:port]" representation that <see cref="WebProxy"/> compares against.
        /// </summary>
        private static string ConvertBypassEntry(string entry)
        {
            StringBuilder builder = new();
            builder.Append("^[a-z][a-z0-9+.\\-]*://");

            string pattern = entry;
            if (pattern.StartsWith("*.", StringComparison.Ordinal))
            {
                // "*.example.com" should also match "example.com" itself.
                pattern = pattern[2..];
            }

            if (pattern.Length == 0)
            {
                // "*.", a wildcard without a host part, matches every host.
                pattern = "*";
            }

            if (pattern[0] != '*')
            {
                // "example.com" should also match subdomains such as "www.example.com".
                builder.Append("(?:[^/]*\\.)?");
            }

            string[] parts = pattern.Split('*');
            for (int i = 0; i < parts.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(".*");
                }

                builder.Append(Regex.Escape(parts[i]));
            }

            builder.Append("(?::\\d+)?$");
            return builder.ToString();
        }
    }
}
