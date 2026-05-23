// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using ComicReaderUWP.SDK.Plugins;

using Shared;

namespace SamplePlugin;

public class SamplePlugin : IPlugin
{
    private const string TAG = nameof(SamplePlugin);

    public string Name => "SamplePlugin";

    public string Publisher => "aicd0";

    public int MajorVersion => 1;

    public int MinorVersion => 0;

    public void Initialize(IPluginContext context)
    {
        SharedContext.Initialize(context);
        context.Logger.D(TAG, "Hello, world!");
    }
}
