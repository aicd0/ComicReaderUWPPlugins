// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using ComicReaderUWP.SDK.Models;

namespace EHLinker.UI.Utils;

internal static class Extensions
{
    //
    // Registry
    //

    public static T? GetValue<T>(this IRegistryKey registry, string key)
    {
        if (registry.TryGet(key, out T? value))
        {
            return value;
        }

        return default;
    }

    public static T GetValueOrDefault<T>(this IRegistryKey registry, string key, T defaultValue)
    {
        if (registry.TryGet(key, out T? value) && value is not null)
        {
            return value;
        }

        return defaultValue;
    }
}
