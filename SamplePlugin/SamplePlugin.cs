// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using ComicReaderUWP.SDK.Plugins;

namespace SamplePlugin;

public class SamplePlugin : IPlugin
{
    private const string TAG = nameof(SamplePlugin);

    string IPlugin.Name => "SamplePlugin";

    string IPlugin.Publisher => "aicd0";

    string IPlugin.Version => "1.0";

    void IPlugin.Initialize(IPluginContext context)
    {
        context.Logger.D(TAG, "Hello, world!");
    }
}
