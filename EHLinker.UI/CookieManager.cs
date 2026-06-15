// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text;

using ComicReaderUWP.SDK.Models;
namespace EHLinker.UI;

public static class CookieManager
{
    public static Dictionary<string, string> GetCookies()
    {
        IRegistryKey cookiesRegistry = PluginService.Context.RegistryDatabase.CreateKey(PluginConstants.REGISTRY_COOKIES);
        Dictionary<string, string> cookies = [];
        foreach (string key in cookiesRegistry.Keys)
        {
            if (cookiesRegistry.TryGet(key, out string? value))
            {
                cookies.Add(key, value);
            }
        }

        return cookies;
    }

    public static string GetCookiesAsString()
    {
        Dictionary<string, string> cookies = GetCookies();
        StringBuilder cookieString = new();
        bool splitor = false;
        foreach (KeyValuePair<string, string> item in cookies)
        {
            if (splitor)
            {
                cookieString.Append(';');
            }

            cookieString.Append(item.Key).Append('=').Append(item.Value);
            splitor = true;
        }

        return cookieString.ToString();
    }

    public static void SaveCookies(IReadOnlyDictionary<string, string> cookies)
    {
        PluginService.Context.RegistryDatabase.RemoveKey(PluginConstants.REGISTRY_COOKIES);
        IRegistryKey cookiesRegistry = PluginService.Context.RegistryDatabase.CreateKey(PluginConstants.REGISTRY_COOKIES);
        foreach (KeyValuePair<string, string> item in cookies)
        {
            cookiesRegistry.Set(item.Key, item.Value);
        }
    }

    public static void SaveCookies(string cookieString)
    {
        string[] fields = cookieString.Split(';');
        Dictionary<string, string> cookies = [];
        foreach (string field in fields)
        {
            int idx = field.IndexOf('=');
            if (idx < 0)
            {
                continue;
            }

            string key = field[..idx].Trim();
            string value = field[(idx + 1)..].Trim();
            cookies[key] = value;
        }

        SaveCookies(cookies);
    }
}
