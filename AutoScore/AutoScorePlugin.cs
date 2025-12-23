// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using ComicReader.SDK.Common.Utils;
using ComicReader.SDK.Plugins;
using ComicReader.SDK.Plugins.Comic;
using ComicReader.SDK.Plugins.Common;
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

    private int? _minScore = null;
    private int MinScore
    {
        get
        {
            if (_minScore.HasValue)
            {
                return _minScore.Value;
            }

            _minScore = Context.GetKVDatabase().GetCollection(LIB_MAIN).GetValueOrDefault(KEY_MIN_SCORE, -1);
            return _minScore.Value;
        }
        set
        {
            _minScore = value;
            Context.GetKVDatabase().GetCollection(LIB_MAIN).Set(KEY_MIN_SCORE, value);
        }
    }

    private int? _maxScore = null;
    private int MaxScore
    {
        get
        {
            if (_maxScore.HasValue)
            {
                return _maxScore.Value;
            }

            _maxScore = (int)Context.GetKVDatabase().GetCollection(LIB_MAIN).GetValueOrDefault(KEY_MAX_SCORE, -1);
            return _maxScore.Value;
        }
        set
        {
            _maxScore = value;
            Context.GetKVDatabase().GetCollection(LIB_MAIN).Set(KEY_MAX_SCORE, value);
        }
    }

    public void Initialize(IPluginContext context)
    {
        _context = context;
        Context.SetComicEditedHandler(this);
        Context.SetMainPageMoreMenuItemCreator(new MainPageMoreMenuItemCreator(this));
        Context.SetComicMenuItemCreator(new ComicMoreMenuItemCreator(this));
    }

    public void ComicEdited(IComicModel comic)
    {
        CoroutineUtils.Start(() => UpdateRating(comic));
    }

    private async Task EditComicScore(IComicModel comic)
    {
        ScoreModel scoreModel = GetComicScoreModel(comic) ?? new()
        {
            GrphicScore = 0F,
            ScriptScore = 0F,
            MissingPages = 0,
        };
        string graphicScoreStr = Math.Round(scoreModel.GrphicScore, 2, MidpointRounding.AwayFromZero).ToString("0.##");
        string scriptScoreStr = Math.Round(scoreModel.ScriptScore, 2, MidpointRounding.AwayFromZero).ToString("0.##");
        string missingPagesStr = scoreModel.MissingPages.ToString();
        string text = await GetTextFromNotepadAsync($@"{graphicScoreStr}/{scriptScoreStr}/{missingPagesStr}

Grphic score/Script score/Missing pages

9 Impressive
8 Excellent
6 Good
4 Fair
2 Poor
0 Terrible

Graphic (0-5):
Storyboard
Character design
Body proportions
Facial expressions
Details
Art style

Script (0-5):
Plot
Pacing
Dialogue
Character development
Originality
Themes
");
        text = GetFirstLine(text);
        await comic.SetDescription(ReplaceFirstLine(comic.Description, text));
        await UpdateRating(comic);
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

            ScoreModel? scoreModel = GetComicScoreModel(comic);
            if (scoreModel is null)
            {
                continue;
            }

            int score = CalculateAbsoluteScore(scoreModel);
            if (score == 0)
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

        MinScore = minScore;
        MaxScore = maxScore;

        foreach (long comicId in comicIds)
        {
            IComicModel? comic = await Context.GetComicById(comicId);
            if (comic is null)
            {
                continue;
            }

            await UpdateRating(comic);
        }
    }

    private async Task UpdateRating(IComicModel comic)
    {
        ScoreModel? scoreModel = GetComicScoreModel(comic);
        if (scoreModel is null)
        {
            await comic.SetRating(-1);
            await comic.SetCompletionStatus(CompletionStatusEnum.NotStarted);
            return;
        }

        int score = CalculateAbsoluteScore(scoreModel);
        if (score == 0)
        {
            await comic.SetRating(-1);
            await comic.SetCompletionStatus(CompletionStatusEnum.Completed);
            return;
        }

        if (MinScore < 0 || score < MinScore)
        {
            MinScore = score;
        }

        if (MaxScore < 0 || score > MaxScore)
        {
            MaxScore = score;
        }

        score = CalculateRelativeScore(score, MinScore, MaxScore);
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

    private static int CalculateAbsoluteScore(ScoreModel scoreModel)
    {
        float graphicScore = scoreModel.GrphicScore;
        float scriptScore = scoreModel.ScriptScore;
        int missingPages = scoreModel.MissingPages;
        float scoreFloat = (graphicScore * 120F + scriptScore * 100F) * (1F - 0.04F * Math.Abs(graphicScore - scriptScore)) - 50 * missingPages;
        int score = (int)Math.Round(scoreFloat, MidpointRounding.AwayFromZero);
        return Math.Max(score, 0);
    }

    private static ScoreModel? GetComicScoreModel(IComicModel comic)
    {
        string description = comic.Description.Trim();
        string firstLine = GetFirstLine(description);
        if (string.IsNullOrEmpty(firstLine))
        {
            return null;
        }

        string[] pieces = firstLine.Split('/');
        if (pieces.Length != 3)
        {
            return null;
        }

        string graphicScoreStr = pieces[0];
        string scriptScoreStr = pieces[1];
        string missingPagesStr = pieces[2];

        if (!float.TryParse(graphicScoreStr, out float graphicScore) || graphicScore < 0F || graphicScore > 5F)
        {
            return null;
        }

        if (!float.TryParse(scriptScoreStr, out float scriptScore) || scriptScore < 0F || scriptScore > 5F)
        {
            return null;
        }

        if (!int.TryParse(missingPagesStr, out int missingPages) || missingPages < 0 || missingPages > 10)
        {
            return null;
        }

        return new ScoreModel
        {
            GrphicScore = graphicScore,
            ScriptScore = scriptScore,
            MissingPages = missingPages,
        };
    }

    private static string GetFirstLine(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        using StringReader reader = new(text);
        return reader.ReadLine() ?? string.Empty;
    }

    private static string ReplaceFirstLine(string input, string newFirstLine)
    {
        if (string.IsNullOrEmpty(input))
        {
            return newFirstLine;
        }

        int index = input.IndexOfAny(['\r', '\n']);
        return index == -1 ? newFirstLine : string.Concat(newFirstLine, input.AsSpan(index));
    }

    private static async Task<string> GetTextFromNotepadAsync(string initialText)
    {
        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, initialText);
        var process = Process.Start("notepad.exe", tempFile);
        await Task.Run(() => process.WaitForExit());
        string text = File.ReadAllText(tempFile);
        File.Delete(tempFile);
        return text;
    }

    private class ScoreModel
    {
        public float GrphicScore { get; init; }
        public float ScriptScore { get; init; }
        public int MissingPages { get; init; }
    }

    private class MainPageMoreMenuItemCreator(AutoScorePlugin plugin) : ICommonMenuItemCreator
    {
        public IEnumerable<IMenuItem> CreateMenuItems()
        {
            return [
                new SimpleMenuItem()
                {
                    Text = "Update scores",
                    Click = () => CoroutineUtils.Start(plugin.UpdateAllRatings),
                }
            ];
        }
    }

    private class ComicMoreMenuItemCreator(AutoScorePlugin plugin) : IComicMenuItemCreator
    {
        public IEnumerable<IMenuItem> CreateMenuItems(IComicModel primary, IEnumerable<IComicModel> selection)
        {
            return [
                new SimpleMenuItem()
                {
                    Text = "Edit score",
                    Click = () => CoroutineUtils.Start(() => plugin.EditComicScore(primary)),
                }
            ];
        }
    }
}
