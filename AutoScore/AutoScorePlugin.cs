// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

using ComicReaderUWP.SDK.Plugins;
using ComicReaderUWP.SDK.Plugins.Comic;
using ComicReaderUWP.SDK.Plugins.Common;
using ComicReaderUWP.SDK.Plugins.Menu;

using Shared;
using Shared.Utils;

namespace AutoScore;

public partial class AutoScorePlugin : IPlugin
{
    private static AutoScorePlugin? _instance = null;
    public static AutoScorePlugin Instance => _instance ?? throw new InvalidOperationException("Plugin not initialized.");

    public string Name => "AutoScore";

    public string Publisher => "aicd0";

    public int MajorVersion => 1;

    public int MinorVersion => 2;

    private readonly AutoScoreCore _core = new();

    public void Initialize(IPluginContext context)
    {
        _instance = this;
        SharedContext.Initialize(context);
        context.SetMainPageMoreMenuItemCreator(new MainPageMoreMenuItemCreator(this));
        context.SetComicMenuItemCreator(new ComicMoreMenuItemCreator(this));
        context.RegisterComicVirtualProperty(new RankScoreProperty());
    }

    //
    // Types
    //

    private class MainPageMoreMenuItemCreator(AutoScorePlugin plugin) : ICommonMenuItemCreator
    {
        public IEnumerable<IMenuItem> CreateMenuItems(IUIContext uiContext)
        {
            return [
                new SimpleMenuItem()
                {
                    Text = "Update scores",
                    Click = () => CoroutineUtils.Run(() => SharedContext.PluginContext.Busy(plugin._core.UpdateAllRatings)),
                },
            ];
        }
    }

    private class ComicMoreMenuItemCreator(AutoScorePlugin plugin) : IComicMenuItemCreator
    {
        public IEnumerable<IMenuItem> CreateMenuItems(IUIContext uiContext, IComicModel primary, IEnumerable<IComicModel> selection)
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
