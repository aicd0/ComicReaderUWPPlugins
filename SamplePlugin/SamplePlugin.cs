// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using ComicReader.SDK.Common.DebugTools;
using ComicReader.SDK.Plugins;
using ComicReader.SDK.Plugins.Menu;

namespace SamplePlugin;

public class SamplePlugin : IPlugin
{
    private const string TAG = nameof(SamplePlugin);

    public string Name => TAG;

    public void Initialize(IPluginContext context)
    {
        Logger.D(TAG, "Hello, world!");

        context.RegisterMainPageMoreMenuItem(new SimpleMenuItem()
        {
            Text = "Sample plugin",
            Click = () =>
            {
                Logger.D(TAG, "Sample plugin clicked");
            }
        });
    }
}
