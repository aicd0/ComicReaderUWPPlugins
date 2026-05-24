// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

using ComicReaderUWP.SDK.Plugins;

namespace SamplePlugin;

public class SamplePlugin : IPlugin
{
    private const string TAG = nameof(SamplePlugin);

    public string Name => "SamplePlugin";

    public string Publisher => "aicd0";

    public string Version => "1.0";

    public IReadOnlyCollection<string> SharedAssemblies => [];

    public void Initialize(IPluginContext context)
    {
        context.Logger.D(TAG, "Hello, world!");
    }
}
