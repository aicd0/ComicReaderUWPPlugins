// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

using ComicReaderUWP.SDK.Plugins;
using ComicReaderUWP.SDK.Plugins.UI;
using ComicReaderUWP.SDK.Plugins.UI.Menu;

using Microsoft.UI.Xaml.Controls;

using SyncTitle.Utils;

namespace SyncTitle;

public class SyncTitlePlugin : IPlugin
{
    private readonly SyncTitleFeature _syncTitleFeature = new();
    private readonly UpdateReleaseDateFeature _updateReleaseDateFeature = new();

    string IPlugin.Name => "SyncTitle";

    string IPlugin.Publisher => "aicd0";

    string IPlugin.Description => "";

    IconSource? IPlugin.Icon => null;

    string IPlugin.Version => "1.2.0";

    void IPlugin.Initialize(IPluginContext context)
    {
        PluginService.Initialize(context);
        context.MainPageMoreMenuItemCreator = new MainPageMoreMenuItemCreator(this);
    }

    private class MainPageMoreMenuItemCreator(SyncTitlePlugin plugin) : ICommonMenuItemCreator
    {
        public IEnumerable<IMenuItem> CreateMenuItems(IWindowContext windowContext)
        {
            return (IMenuItem[])[
                new SimpleMenuItem()
                {
                    Text = "Sync titles",
                    Click = () => CoroutineUtils.Run(() => PluginService.PluginContext.WithBusyState(plugin._syncTitleFeature.SyncTitles)),
                },
                new SimpleMenuItem()
                {
                    Text = "Update release date",
                    Click = () => CoroutineUtils.Run(() => PluginService.PluginContext.WithBusyState(plugin._updateReleaseDateFeature.UpdateReleaseDates)),
                }
            ];
        }
    }
}
