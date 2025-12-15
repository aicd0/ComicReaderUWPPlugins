using ComicReader.SDK.Common.DebugTools;
using ComicReader.SDK.Plugins;

namespace ComicReaderUWPPlugins;

public class SamplePlugin : IPlugin
{
    public void Initialize()
    {
        Logger.D("SamplePlugin", "Hello, world!");
    }
}
