// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Net;
using System.Runtime.InteropServices;

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
            return new WebProxy(Address)
            {
                BypassList = BypassList,
            };
        }
    }
}
