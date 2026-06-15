// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using ComicReaderUWP.SDK.Plugins;
using ComicReaderUWP.SDK.Plugins.UI;

using EHLinker.UI;

using Microsoft.UI.Xaml.Controls;

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

        public IconElement Icon => new FontIcon() { Glyph = "\uE90A" };
    }
}
