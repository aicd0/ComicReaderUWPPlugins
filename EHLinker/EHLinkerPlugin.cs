// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System;

using ComicReaderUWP.SDK.Plugins;
using ComicReaderUWP.SDK.Plugins.UI;

using EHLinker.UI;
using EHLinker.UI.Views;

using Microsoft.UI.Xaml.Controls;

namespace EHLinker;

public partial class EHLinkerPlugin : IPlugin
{
    private const string HOST_SIDEBAR_PAGE = "ehlinker_sidebar";

    string IPlugin.Name => "EHLinker";

    string IPlugin.Publisher => "aicd0";

    string IPlugin.Description => "Retrieve comic information from E-Hentai.";

    IconSource? IPlugin.Icon => CreatePluginIcon();

    string IPlugin.Version => "1.0";

    void IPlugin.Initialize(IPluginContext context)
    {
        PluginAbility ability = new();
        PluginService.Initialize(context, ability);
        ability.SetCookies(CookieManager.GetCookies());
        context.RegisterPage(HOST_SIDEBAR_PAGE, typeof(SidebarPage));
        context.RegisterSidebarPage(new SidebarPageProvider());
    }

    private static BitmapIconSource CreatePluginIcon()
    {
        return new BitmapIconSource()
        {
            UriSource = new Uri($"ms-appx:///{PluginService.Context.PluginRootDirectoryPath}/Assets/E-Hentai.png"),
            ShowAsMonochrome = true,
        };
    }

    private class SidebarPageProvider : ISidebarPageProvider
    {
        public string Name => "EHLinker";

        public string Host => HOST_SIDEBAR_PAGE;

        IconSource ISidebarPageProvider.Icon => CreatePluginIcon();
    }
}
