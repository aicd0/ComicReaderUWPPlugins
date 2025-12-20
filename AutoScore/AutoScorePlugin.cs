// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using ComicReader.SDK.Common.Utils;
using ComicReader.SDK.Plugins;
using ComicReader.SDK.Plugins.Comic;
using ComicReader.SDK.Plugins.Menu;

namespace AutoScore;

public partial class AutoScorePlugin : IPlugin, IComicEditedHandler
{
    private const string TAG = nameof(AutoScorePlugin);
    private const string LIB_MAIN = "Main";
    private const string KEY_MIN_SCORE = "MinScore";
    private const string KEY_MAX_SCORE = "MaxScore";

    public string Name => TAG;

    private IPluginContext? _context;
    private IPluginContext Context => _context ?? throw new InvalidOperationException("Plugin not initialized.");

    public void Initialize(IPluginContext context)
    {
        _context = context;

        Context.RegisterComicEditedHandler(this);

        Context.RegisterMainPageMoreMenuItem(new SubItemMenuItem()
        {
            Text = "Auto score",
            Items = [
                new SimpleMenuItem()
                {
                    Text = "Update all ratings",
                    Click = () => CoroutineUtils.Start(UpdateAllRatings),
                }
            ],
        });
    }

    public void ComicEdited(IComicModel comic)
    {
        int maxScore = (int)Context.GetKVDatabase().GetLong(LIB_MAIN, KEY_MAX_SCORE, -1);
        int minScore = (int)Context.GetKVDatabase().GetLong(LIB_MAIN, KEY_MIN_SCORE, -1);
        CoroutineUtils.Start(() => UpdateRating(comic, minScore, maxScore));
    }

    private async Task UpdateAllRatings()
    {
        IEnumerable<long> comicIds = await Context.SearchComics("%tag.\"Type\" = Manga");

        int minScore = -1;
        int maxScore = -1;
        foreach (long comicId in comicIds)
        {
            IComicModel? comic = await Context.GetComicById(comicId);
            if (comic is null)
            {
                continue;
            }

            int score = CalculateAbsoluteScore(comic);
            if (score < 0)
            {
                continue;
            }

            if (maxScore < 0 || minScore < 0)
            {
                minScore = score;
                maxScore = score;
            }
            else
            {
                minScore = Math.Min(minScore, score);
                maxScore = Math.Max(maxScore, score);
            }
        }

        Context.GetKVDatabase().SetLong(LIB_MAIN, KEY_MIN_SCORE, minScore);
        Context.GetKVDatabase().SetLong(LIB_MAIN, KEY_MAX_SCORE, maxScore);

        foreach (long comicId in comicIds)
        {
            IComicModel? comic = await Context.GetComicById(comicId);
            if (comic is null)
            {
                continue;
            }

            await UpdateRating(comic, minScore, maxScore);
        }
    }

    private async Task UpdateRating(IComicModel comic, int minScore, int maxScore)
    {
        int score = CalculateAbsoluteScore(comic);
        if (score < 0)
        {
            await comic.SetRating(-1);
            await comic.SetCompletionStatus(CompletionStatusEnum.NotStarted);
            return;
        }

        if (minScore < 0 || score < minScore)
        {
            minScore = score;
            Context.GetKVDatabase().SetLong(LIB_MAIN, KEY_MIN_SCORE, minScore);
        }

        if (maxScore < 0 || score > maxScore)
        {
            maxScore = score;
            Context.GetKVDatabase().SetLong(LIB_MAIN, KEY_MAX_SCORE, maxScore);
        }

        score = CalculateRelativeScore(score, minScore, maxScore);
        await comic.SetRating(score);
        await comic.SetCompletionStatus(CompletionStatusEnum.Completed);
    }

    private static int CalculateRelativeScore(int absoluteScore, int minScore, int maxScore)
    {
        if (absoluteScore < minScore)
        {
            return 0;
        }

        if (absoluteScore > maxScore)
        {
            return 100;
        }

        float scoreFloat = (absoluteScore - minScore) * 100F / (maxScore - minScore);
        int score = (int)Math.Round(scoreFloat, MidpointRounding.AwayFromZero);
        score = Math.Clamp(score, 0, 100);
        return score;
    }

    private static int CalculateAbsoluteScore(IComicModel comic)
    {
        string description = comic.Description.Trim();
        string? firstLine = GetFirstLine(description);
        if (string.IsNullOrEmpty(firstLine))
        {
            return -1;
        }

        string[] pieces = firstLine.Split('/');
        if (pieces.Length != 3)
        {
            return -1;
        }

        string graphicScoreStr = pieces[0];
        string scriptScoreStr = pieces[1];
        string missingPagesStr = pieces[2];

        if (!float.TryParse(graphicScoreStr, out float graphicScore) || graphicScore < 0F || graphicScore > 5F)
        {
            return -1;
        }

        if (!float.TryParse(scriptScoreStr, out float scriptScore) || scriptScore < 0F || scriptScore > 5F)
        {
            return -1;
        }

        if (!int.TryParse(missingPagesStr, out int missingPages) || missingPages < 0 || missingPages > 10)
        {
            return -1;
        }

        return CalculateAbsoluteScore(graphicScore, scriptScore, missingPages);
    }

    private static int CalculateAbsoluteScore(float graphicScore, float scriptScore, int missingPages)
    {
        float scoreFloat = (graphicScore * 120F + scriptScore * 100F) * (1F - 0.04F * Math.Abs(graphicScore - scriptScore)) - 50 * missingPages;
        int score = (int)Math.Round(scoreFloat, MidpointRounding.AwayFromZero);
        return Math.Max(score, 0);
    }

    private static string? GetFirstLine(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        using StringReader reader = new(text);
        return reader.ReadLine();
    }
}
