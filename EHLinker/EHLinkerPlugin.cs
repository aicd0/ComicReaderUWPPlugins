// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System;

using ComicReaderUWP.SDK.Plugins;
using ComicReaderUWP.SDK.Plugins.UI;

using EHLinker.UI;
using EHLinker.UI.Views;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace EHLinker;

public partial class EHLinkerPlugin : IPlugin
{
    private const string HOST_SIDEBAR_PAGE = "ehlinker_sidebar";

    string IPlugin.Name => "EHLinker";

    string IPlugin.Publisher => "aicd0";

    string IPlugin.Version => "1.0";

    void IPlugin.Initialize(IPluginContext context)
    {
        PluginAbility ability = new();
        PluginService.Initialize(context, ability);
        ability.SetCookies(CookieManager.GetCookies());
        context.RegisterPage(HOST_SIDEBAR_PAGE, typeof(SidebarPage));
        context.RegisterSidebarPage(new SidebarPageProvider());
    }

    private class SidebarPageProvider : ISidebarPageProvider
    {
        public string Name => "EHLinker";

        public string Host => HOST_SIDEBAR_PAGE;

        public IconElement Icon => new BitmapIcon()
        {
            UriSource = new Uri($"ms-appx:///{PluginService.Context.ResourceFolderPath}/Assets/E-Hentai.png"),
            ShowAsMonochrome = true,
            Foreground = (Brush)Application.Current.Resources["TextFillColorPrimaryBrush"],
        };
    }
}
