// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

using ComicReader.SDK.Common.Utils;
using ComicReader.SDK.Plugins;
using ComicReader.SDK.Plugins.Comic;
using ComicReader.SDK.Plugins.Common;
using ComicReader.SDK.Plugins.Menu;

namespace AutoScore;

public partial class AutoScorePlugin : IPlugin, IComicEditedHandler
{
    private static AutoScorePlugin? _instance = null;
    public static AutoScorePlugin Instance => _instance ?? throw new InvalidOperationException("Plugin not initialized.");

    public string Name => "AutoScore";

    public string Publisher => "aicd0";

    public int MajorVersion => 1;

    public int MinorVersion => 2;

    private IPluginContext? _context;
    public IPluginContext Context => _context ?? throw new InvalidOperationException("Plugin not initialized.");

    private readonly AutoScoreCore _core = new();

    public void Initialize(IPluginContext context)
    {
        _instance = this;
        _context = context;
        Context.SetComicEditedHandler(this);
        Context.SetMainPageMoreMenuItemCreator(new MainPageMoreMenuItemCreator(this));
        Context.SetComicMenuItemCreator(new ComicMoreMenuItemCreator(this));
        Context.RegisterComicVirtualProperty(new RankScoreProperty());
    }

    public void ComicEdited(IComicModel comic)
    {
        CoroutineUtils.Start(() => _core.UpdateRating(comic));
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
                    Click = () => CoroutineUtils.Start(() => plugin.Context.Busy(plugin._core.UpdateAllRatings)),
                },
                new SimpleMenuItem()
                {
                    Text = "Show dialog",
                    Click = () => CoroutineUtils.Start(async () =>
                    {
                        await plugin.Context.EnqueueDialogAsync(new EditScoreDialog());
                    }),
                }
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
                    Click = () => CoroutineUtils.Start(() => plugin._core.EditComicScore(primary)),
                }
            ];
        }
    }
}
