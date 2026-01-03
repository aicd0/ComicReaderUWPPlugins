// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using ComicReader.SDK.Common.Utils;
using ComicReader.SDK.Plugins.Comic;

namespace AutoScore;

internal class AutoScoreCore
{
    private const string LIB_MAIN = "Main";
    private const string KEY_MIN_SCORE = "MinScore";
    private const string KEY_MAX_SCORE = "MaxScore";

    //
    // Properties
    //

    private int? _minScore = null;
    private int MinScore
    {
        get
        {
            if (_minScore.HasValue)
            {
                return _minScore.Value;
            }

            _minScore = AutoScorePlugin.Instance.Context.GetKVDatabase().GetCollection(LIB_MAIN).GetValueOrDefault(KEY_MIN_SCORE, -1);
            return _minScore.Value;
        }
        set
        {
            _minScore = value;
            AutoScorePlugin.Instance.Context.GetKVDatabase().GetCollection(LIB_MAIN).Set(KEY_MIN_SCORE, value);
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

            _maxScore = AutoScorePlugin.Instance.Context.GetKVDatabase().GetCollection(LIB_MAIN).GetValueOrDefault(KEY_MAX_SCORE, -1);
            return _maxScore.Value;
        }
        set
        {
            _maxScore = value;
            AutoScorePlugin.Instance.Context.GetKVDatabase().GetCollection(LIB_MAIN).Set(KEY_MAX_SCORE, value);
        }
    }

    //
    // Public Methods
    //

    public async Task EditComicScore(IComicModel comic)
    {
        if (!IsTargetComic(comic))
        {
            return;
        }

        ScoreModel scoreModel = GetComicScoreModel(comic) ?? new()
        {
            GrphicScore = 0F,
            ScriptScore = 0F,
            MissingPages = 0,
        };
        DetailedScoreModel detailedScoreModel = DetailedScoreModel.FromComicModel(comic) ?? new();
        var dialog = new EditScoreDialog(scoreModel, detailedScoreModel);
        await AutoScorePlugin.Instance.Context.EnqueueDialogAsync(dialog);
        detailedScoreModel.SaveToComicModel(comic);
        if (dialog.IsSaveClicked)
        {
            await comic.SetDescription(ReplaceFirstLine(comic.Description, scoreModel.ToDatabaseString()));
            await UpdateRating(comic);
        }
    }

    public async Task UpdateAllRatings()
    {
        IEnumerable<long> comicIds = await AutoScorePlugin.Instance.Context.SearchComics("%tag.\"Type\" = Manga");

        int minScore = -1;
        int maxScore = -1;
        foreach (long comicId in comicIds)
        {
            IComicModel? comic = await AutoScorePlugin.Instance.Context.GetComic(comicId);
            if (comic is null)
            {
                continue;
            }

            ScoreModel? scoreModel = GetComicScoreModel(comic);
            if (scoreModel is null)
            {
                continue;
            }

            int score = scoreModel.GetAbsoluteScore();
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
            IComicModel? comic = await AutoScorePlugin.Instance.Context.GetComic(comicId);
            if (comic is null)
            {
                continue;
            }

            await UpdateRating(comic);
        }
    }

    public async Task UpdateRating(IComicModel comic)
    {
        if (!IsTargetComic(comic))
        {
            return;
        }

        ScoreModel? scoreModel = GetComicScoreModel(comic);
        if (scoreModel is null)
        {
            await comic.SetRating(-1);
            await comic.SetCompletionStatus(CompletionStatusEnum.NotStarted);
            return;
        }

        int score = scoreModel.GetAbsoluteScore();
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

    //
    // Helpers
    //

    private static bool IsTargetComic(IComicModel comic)
    {
        return comic.Tags.FirstOrDefault(x => x.Name == "Type")?.Tags.Contains("Manga") ?? false;
    }

    private static ScoreModel? GetComicScoreModel(IComicModel comic)
    {
        string description = comic.Description.Trim();
        string firstLine = GetFirstLine(description);
        return ScoreModel.FromDatabaseString(firstLine);
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
}
