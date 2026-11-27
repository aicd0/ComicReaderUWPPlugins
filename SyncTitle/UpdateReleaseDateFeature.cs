// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using ComicReaderUWP.SDK.Models;
using ComicReaderUWP.SDK.Plugins.Comic;

namespace SyncTitle;

internal class UpdateReleaseDateFeature
{
    private const string TAG = nameof(UpdateReleaseDateFeature);

    private static readonly Regex sDatePrefixRegex = new(@"^(\d{8}) - (.+)$", RegexOptions.Compiled);

    public async Task UpdateReleaseDates()
    {
        IEnumerable<long> ids = await PluginService.PluginContext.SearchComics(string.Empty);
        List<Tuple<IComicModel, DateTime, string>> comicToUpdate = [];
        foreach (long id in ids)
        {
            IComicModel? comic = await PluginService.PluginContext.GetComic(id);
            if (comic is null || comic.IsHidden)
            {
                continue;
            }

            Match match = sDatePrefixRegex.Match(comic.Title1 ?? string.Empty);
            if (!match.Success)
            {
                continue;
            }

            DateTime releaseDate = ClampToValidDate(match.Groups[1].Value);

            string newTitle = match.Groups[2].Value.Trim();
            if (string.IsNullOrEmpty(newTitle))
            {
                continue;
            }

            comicToUpdate.Add(new(comic, releaseDate, newTitle));
        }

        if (comicToUpdate.Count == 0)
        {
            return;
        }

        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine("The following comics will be updated:");
        foreach (Tuple<IComicModel, DateTime, string> tuple in comicToUpdate)
        {
            IComicModel comic = tuple.Item1;
            DateTime releaseDate = tuple.Item2;
            string newTitle = tuple.Item3;
            DateTime? currentReleaseDate = comic.ReleaseDate;
            stringBuilder.AppendLine($"  {comic.Location}");
            stringBuilder.AppendLine($"    Title: {comic.Title1}  =>  {newTitle}");
            stringBuilder.AppendLine($"    Release date: {(currentReleaseDate.HasValue ? currentReleaseDate.Value.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture) : "none")}  =>  {releaseDate.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture)}");
        }

        DialogOptions options = new DialogOptions.Builder()
            .SetTitle("Warning")
            .SetContent(stringBuilder.ToString())
            .SetPrimaryButtonText("OK")
            .SetSecondaryButtonText("Cancel")
            .Build();
        if ((await PluginService.PluginContext.EnqueueDialog(options)) != DialogResult.Primary)
        {
            return;
        }

        List<IComicModel> failedComics = [];
        foreach (Tuple<IComicModel, DateTime, string> tuple in comicToUpdate)
        {
            IComicModel comic = tuple.Item1;
            DateTime releaseDate = tuple.Item2;
            string newTitle = tuple.Item3;
            try
            {
                await comic.SetReleaseDate(releaseDate);
                await comic.SetTitle1(newTitle);
            }
            catch (Exception e)
            {
                PluginService.PluginContext.Logger.F(TAG, $"Failed to update comic {comic.Id}: {e.Message}", e);
                failedComics.Add(comic);
            }
        }

        if (failedComics.Count > 0)
        {
            StringBuilder failedStringBuilder = new();
            failedStringBuilder.AppendLine("Failed to process the following comics:");
            foreach (IComicModel comic in failedComics)
            {
                failedStringBuilder.AppendLine($"  {comic.Location}");
            }

            DialogOptions failedOptions = new DialogOptions.Builder()
                .SetTitle("Warning")
                .SetContent(failedStringBuilder.ToString())
                .SetPrimaryButtonText("OK")
                .Build();
            await PluginService.PluginContext.EnqueueDialog(failedOptions);
        }
    }

    private static DateTime ClampToValidDate(string value)
    {
        // Out-of-range components are clamped to the nearest valid date, e.g. 20160099 => 2016/01/31.
        int year = int.Parse(value[..4], CultureInfo.InvariantCulture);
        int month = int.Parse(value[4..6], CultureInfo.InvariantCulture);
        int day = int.Parse(value[6..8], CultureInfo.InvariantCulture);

        year = Math.Clamp(year, DateTime.MinValue.Year, DateTime.MaxValue.Year);
        month = Math.Clamp(month, 1, 12);
        day = Math.Clamp(day, 1, DateTime.DaysInMonth(year, month));

        return new DateTime(year, month, day);
    }
}
