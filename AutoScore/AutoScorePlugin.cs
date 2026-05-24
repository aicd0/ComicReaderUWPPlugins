// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

using AutoScore.UI;
using AutoScore.UI.Utils;

using ComicReaderUWP.SDK.Plugins;
using ComicReaderUWP.SDK.Plugins.Comic;
using ComicReaderUWP.SDK.Plugins.Common;
using ComicReaderUWP.SDK.Plugins.Menu;

namespace AutoScore;

public partial class AutoScorePlugin : IPlugin
{
    public string Name => "AutoScore";

    public string Publisher => "aicd0";

    public string Version => "1.0";

    public IReadOnlyCollection<string> SharedAssemblies => ["AutoScore.UI"];

    private readonly AutoScoreCore _core = new();

    public void Initialize(IPluginContext context)
    {
        PluginService.Initialize(context);
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
                    Click = () => CoroutineUtils.Run(() => PluginService.PluginContext.Busy(plugin._core.UpdateAllRatings)),
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
