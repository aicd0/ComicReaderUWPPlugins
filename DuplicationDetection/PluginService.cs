// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System;

using ComicReaderUWP.SDK.Plugins;

namespace DuplicationDetection;

internal static class PluginService
{
    private static IPluginContext? _pluginContext;
    public static IPluginContext PluginContext => _pluginContext ?? throw new InvalidOperationException("Plugin not initialized.");

    public static void Initialize(IPluginContext pluginContext)
    {
        if (_pluginContext is not null)
        {
            throw new InvalidOperationException("Plugin already initialized.");
        }

        _pluginContext = pluginContext;
    }
}
