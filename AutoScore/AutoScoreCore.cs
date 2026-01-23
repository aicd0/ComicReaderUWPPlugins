// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ComicReaderUWP.SDK.Plugins.Comic;

namespace AutoScore;

internal class AutoScoreCore
{
    private const string LIB_MAIN = "Main";

    //
    // Public Methods
    //

    public async Task EditComicScore(IComicModel comic)
    {
        if (!IsTargetComic(comic))
        {
            return;
        }

        ScoreModel scoreModel = ScoreModel.Load(comic) ?? new();
        var dialog = new EditScoreDialog(scoreModel);
        await AutoScorePlugin.Instance.Context.EnqueueDialogAsync(dialog);
        if (dialog.IsSaveClicked)
        {
            scoreModel.Save(comic);
            await UpdateRating(comic);
        }
    }

    public async Task UpdateAllRatings()
    {
        IEnumerable<long> comicIds = await AutoScorePlugin.Instance.Context.SearchComics("%tag.\"Type\" = Manga");

        List<IComicModel> updatingComics = [];
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

        var scoreModel = ScoreModel.Load(comic);
        if (scoreModel is null || !scoreModel.IsRated)
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

        score = CalculateRelativeScore(score);
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

    private static int CalculateRelativeScore(int absoluteScore)
    {
        float scoreFloat = absoluteScore * 100F / ScoreModel.MAX_SCORE;
        int score = (int)Math.Round(scoreFloat, MidpointRounding.AwayFromZero);
        score = Math.Clamp(score, 0, 100);
        return score;
    }
}
