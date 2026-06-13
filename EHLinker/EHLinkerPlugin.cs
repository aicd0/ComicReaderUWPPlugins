// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

using ComicReaderUWP.SDK.Plugins;
using ComicReaderUWP.SDK.Plugins.UI;

using EHLinker.UI;

using Microsoft.UI.Xaml.Controls;

namespace EHLinker;

public partial class EHLinkerPlugin : IPlugin
{
    private const string HOST_SIDEBAR_PAGE = "ehlinker_sidebar";

    public string Name => "EHLinker";

    public string Publisher => "aicd0";

    public string Version => "1.0";

    public IReadOnlyCollection<string> SharedAssemblies => ["EHLinker.UI"];

    public void Initialize(IPluginContext context)
    {
        PluginService.Initialize(context);
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
