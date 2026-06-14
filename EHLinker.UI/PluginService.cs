// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System;

using ComicReaderUWP.SDK.Plugins;

namespace EHLinker.UI;

public static class PluginService
{
    private static IPluginContext? _pluginContext;
    private static IPluginAbility? _pluginAbility;

    public static IPluginContext Context => _pluginContext ?? throw new InvalidOperationException("The plugin is not initialized.");
    public static IPluginAbility Ability => _pluginAbility ?? throw new InvalidOperationException("The plugin is not initialized.");

    public static void Initialize(IPluginContext pluginContext, IPluginAbility ability)
    {
        if (_pluginContext is not null || _pluginAbility is not null)
        {
            throw new InvalidOperationException("The plugin is already initialized.");
        }

        _pluginContext = pluginContext;
        _pluginAbility = ability;
    }
}
