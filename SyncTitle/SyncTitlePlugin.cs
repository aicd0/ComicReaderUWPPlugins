// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using ComicReaderUWP.SDK.Models;
using ComicReaderUWP.SDK.Plugins;
using ComicReaderUWP.SDK.Plugins.Comic;
using ComicReaderUWP.SDK.Plugins.UI;
using ComicReaderUWP.SDK.Plugins.UI.Menu;

using Microsoft.UI.Xaml.Controls;

using SyncTitle.Utils;

namespace SyncTitle;

public class SyncTitlePlugin : IPlugin
{
    string IPlugin.Name => "SyncTitle";

    string IPlugin.Publisher => "aicd0";

    string IPlugin.Version => "1.0";

    void IPlugin.Initialize(IPluginContext context)
    {
        PluginService.Initialize(context);
        context.MainPageMoreMenuItemCreator = new MainPageMoreMenuItemCreator(this);
    }

    private async Task SyncTitles()
    {
        IEnumerable<long> ids = await PluginService.PluginContext.SearchComics(string.Empty);
        List<IComicModel> failedComics = [];
        List<Tuple<IComicModel, string>> comicToNewPath = [];
        foreach (long id in ids)
        {
            IComicModel? comic = await PluginService.PluginContext.GetComic(id);
            if (comic is null || comic.IsHidden)
            {
                continue;
            }

            string path = comic.Location;
            string parentDir = Path.GetDirectoryName(path) ?? string.Empty;
            if (string.IsNullOrEmpty(parentDir))
            {
                failedComics.Add(comic);
                continue;
            }

            string? newName = SanitizeForNtfsFileName(comic.Title1);
            if (newName == null)
            {
                failedComics.Add(comic);
                continue;
            }

            string newPath;
            if (Directory.Exists(path))
            {
                newPath = Path.Combine(parentDir, newName);
            }
            else
            {
                failedComics.Add(comic);
                continue;
            }

            if (path == newPath)
            {
                continue;
            }

            comicToNewPath.Add(new(comic, newPath));
        }

        if (comicToNewPath.Count > 0)
        {
            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine("The following comics will be renamed:");
            foreach (Tuple<IComicModel, string> tuple in comicToNewPath)
            {
                IComicModel comic = tuple.Item1;
                string newPath = tuple.Item2;
                stringBuilder.AppendLine($"  {comic.Location}  =>  {newPath}");
            }

            DialogOptions options = new DialogOptions.Builder()
                .SetTitle("Warning")
                .SetContent(stringBuilder.ToString())
                .SetPrimaryButtonText("OK")
                .SetSecondaryButtonText("Cancel")
                .Build();
            if ((await PluginService.PluginContext.EnqueueDialog(options)).Result != ContentDialogResult.Primary)
            {
                return;
            }

            foreach (Tuple<IComicModel, string> tuple in comicToNewPath)
            {
                IComicModel comic = tuple.Item1;
                string newPath = tuple.Item2;
                if (!await comic.MoveToLocation(newPath))
                {
                    failedComics.Add(comic);
                }
            }
        }

        if (failedComics.Count > 0)
        {
            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine("Failed to process the following comics:");
            foreach (IComicModel comic in failedComics)
            {
                stringBuilder.AppendLine($"  {comic.Location}");
            }

            DialogOptions options = new DialogOptions.Builder()
                .SetTitle("Warning")
                .SetContent(stringBuilder.ToString())
                .SetPrimaryButtonText("OK")
                .Build();
            await PluginService.PluginContext.EnqueueDialog(options);
        }
    }

    private static string? SanitizeForNtfsFileName(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        // NTFS invalid characters: \ / : * ? " < > | and control chars (0-31)
        string invalidChars = new(Path.GetInvalidFileNameChars());
        string pattern = $"[{Regex.Escape(invalidChars)}]";

        // Replace invalid characters with '_'
        string sanitized = Regex.Replace(input, pattern, "_");

        // Remove trailing dots and spaces (NTFS does not allow these)
        sanitized = sanitized.TrimEnd('.', ' ');

        // NTFS max file name length is 255 characters
        if (sanitized.Length > 255)
        {
            return null;
        }

        // If result is empty, return a default name
        return string.IsNullOrWhiteSpace(sanitized) ? null : sanitized;
    }

    private class MainPageMoreMenuItemCreator(SyncTitlePlugin plugin) : ICommonMenuItemCreator
    {
        public IEnumerable<IMenuItem> CreateMenuItems(IWindowContext windowContext)
        {
            return [
                new SimpleMenuItem()
                {
                    Text = "Sync titles",
                    Click = () => CoroutineUtils.Run(() => PluginService.PluginContext.WithBusyState(plugin.SyncTitles)),
                }
            ];
        }
    }
}
