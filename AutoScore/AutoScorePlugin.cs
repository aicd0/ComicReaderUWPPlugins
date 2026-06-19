// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

using AutoScore.UI;
using AutoScore.UI.Utils;

using ComicReaderUWP.SDK.Plugins;
using ComicReaderUWP.SDK.Plugins.Comic;
using ComicReaderUWP.SDK.Plugins.UI;
using ComicReaderUWP.SDK.Plugins.UI.Menu;

using Microsoft.UI.Xaml.Controls;

namespace AutoScore;

public partial class AutoScorePlugin : IPlugin
{
    string IPlugin.Name => "AutoScore";

    string IPlugin.Publisher => "aicd0";

    string IPlugin.Description => "";

    IconSource? IPlugin.Icon => null;

    string IPlugin.Version => "1.0";

    private readonly AutoScoreCore _core = new();

    void IPlugin.Initialize(IPluginContext context)
    {
        PluginService.Initialize(context);
        context.MainPageMoreMenuItemCreator = new MainPageMoreMenuItemCreator(this);
        context.ComicMenuItemCreator = new ComicMoreMenuItemCreator(this);
        context.RegisterComicVirtualProperty(new RankScoreProperty());
    }

    //
    // Types
    //

    private class MainPageMoreMenuItemCreator(AutoScorePlugin plugin) : ICommonMenuItemCreator
    {
        public IEnumerable<IMenuItem> CreateMenuItems(IWindowContext windowContext)
        {
            return [
                new SimpleMenuItem()
                {
                    Text = "Update scores",
                    Click = () => CoroutineUtils.Run(() => PluginService.PluginContext.WithBusyState(plugin._core.UpdateAllRatings)),
                },
            ];
        }
    }

    private class ComicMoreMenuItemCreator(AutoScorePlugin plugin) : IComicMenuItemCreator
    {
        public IEnumerable<IMenuItem> CreateMenuItems(IWindowContext windowContext, IComicModel primary, IEnumerable<IComicModel> selection)
        {
            return [
                new SimpleMenuItem()
                {
                    Text = "Edit score",
                    Click = () => CoroutineUtils.Run(() => plugin._core.EditComicScore(primary)),
                }
            ];
        }
    }
}
