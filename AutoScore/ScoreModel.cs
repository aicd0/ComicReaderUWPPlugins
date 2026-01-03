// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System;

namespace AutoScore;

internal class ScoreModel
{
    public float GrphicScore { get; set; }
    public float ScriptScore { get; set; }
    public int MissingPages { get; set; }

    public int GetAbsoluteScore()
    {
        float graphicScore = GrphicScore;
        float scriptScore = ScriptScore;
        int missingPages = MissingPages;
        float scoreFloat = graphicScore * 120F + scriptScore * 100F;
        scoreFloat *= 1F - 0.04F * Math.Abs(graphicScore - scriptScore);
        scoreFloat *= 1F - 0.05F * missingPages;
        int score = (int)Math.Round(scoreFloat, MidpointRounding.AwayFromZero);
        return Math.Max(score, 0);
    }

    public ScoreModel Clone()
    {
        return new()
        {
            GrphicScore = GrphicScore,
            ScriptScore = ScriptScore,
            MissingPages = MissingPages
        };
    }

    public string ToDatabaseString()
    {
        string graphicScore = Math.Round(GrphicScore, 1, MidpointRounding.AwayFromZero).ToString("0.#");
        string scriptScore = Math.Round(ScriptScore, 1, MidpointRounding.AwayFromZero).ToString("0.#");
        return $"{graphicScore}/{scriptScore}/{MissingPages}";
    }

    public static ScoreModel? FromDatabaseString(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        string[] pieces = value.Split('/');
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
}
