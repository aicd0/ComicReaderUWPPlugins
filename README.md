# Comic Reader UWP Plugins

This project contains plugins that can work with Comic Reader UWP. If you are simply looking for plugins to download, visit [Releases](https://github.com/aicd0/ComicReaderUWPPlugins/releases) page. If you are a plugin developer looking for instructions, see [Development](#Development) section.

## Plugin list

| Name | Description |
| ---- | ----------- |
| DuplicationDetection | Detects similar images in a comic using PHash. Display results in a dialog. |
| EHLinker | Automatically import comic tags from E-Hentai. Display gallery comments in the sidebar. |

## Development

Prerequisites:
- In case you haven't, build and run Comic Reader UWP app following the instructions [here](https://github.com/aicd0/ComicReaderUWP).
- Set up a local NuGet package source. You can set a folder on your computer as your local NuGet package source in Visual Studio -> Tools -> Options -> NuGet Package Manager -> Sources -> Package sources. We assume that `D:\NuGetPackages` is the folder.
- Build `ComicReaderUWP.SDK` project in Release configuration. Copy the generated `ComicReaderUWP.SDK.x.x.x.nupkg` from output folder to `D:\NuGetPackages`. 
- Get the code:
    ```
    git clone git@github.com:aicd0/ComicReaderUWPPlugins.git
    ```
- Open [ComicReaderUWPPlugins.sln](ComicReaderUWPPlugins.sln) in Visual Studio to build all plugins.

To create a new plugin:

- Add a new .NET Class Library project to the solution. Use your plugin name as the project name. Leave target .NET framework the default setting (we are going to change that later).
- Assuming the name of your new plugin is `YourPlugin`. Copy everything from [SamplePlugin.csproj](SamplePlugin/SamplePlugin.csproj) to `YourPlugin\YourPlugin.csproj`. Replace text `SamplePlugin` with `YourPlugin`.
- Copy [PluginMeta.json](SamplePlugin/PluginMeta.json) from `SamplePlugin\` to `YourPlugin\`. This file is essential for a plugin to work.
- Now implementing the plugin: Create a new class `YourPlugin.cs` which implements `IPlugin` interface. Provide all information the interface needs, then implement your custom routine in `void Initialize(IPluginContext context)`. `Initialize` is the entry point for all plugins, performs any registrations here when the app launches. Take [SamplePlugin.json](SamplePlugin/SamplePlugin.cs) as an example:
    ```
    public class SamplePlugin : IPlugin
    {
        string IPlugin.Name => "SamplePlugin";

        string IPlugin.Publisher => "aicd0";

        string IPlugin.Description => "This is a sample plugin.";

        IconSource? IPlugin.Icon => new FontIconSource() { Glyph = "\uE74C" };

        string IPlugin.Version => "1.0";

        void IPlugin.Initialize(IPluginContext context)
        {
            context.Logger.D("SamplePlugin", "Hello, world!");
        }
    }
    ```

To debug a plugin:

- Build the plugin in Debug configuration. Install the plugin then restart the app.
- In Visual Studio, click Debug -> Attach to Process (or "Reattach to ComicReaderUWP.exe" if you have already attached before), then select the running ComicReaderUWP.exe process, click "Attach".
- Now you can set breakpoints for the plugin.

## Contributing
If a plugin is not working properly, you can [submit an issue on GitHub](https://github.com/aicd0/ComicReaderUWPPlugins/issues/new). If you want to create a new plugin, implement new features, or simply fix an issue, you can create a [pull request](https://github.com/aicd0/ComicReaderUWPPlugins/pulls).

## License
Licensed under the [MIT License](LICENSE.md).
