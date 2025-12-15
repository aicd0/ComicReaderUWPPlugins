// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;

using ComicReader.SDK.Plugins;

namespace AutoScore;

public partial class AutoScorePlugin : IPlugin, IBeforeComicUpdatingHandler
{
    private const string TAG = nameof(AutoScorePlugin);

    public string Name => TAG;

    public void Initialize(IPluginContext context)
    {
        context.RegisterBeforeComicUpdatingHandler(this);
    }

    public void OnComicUpdating(IComicModel comic)
    {
        UpdateScore(comic);
    }

    private static void UpdateScore(IComicModel comic)
    {
        string description = comic.Description.Trim();
        string? firstLine = GetFirstLine(description);
        if (string.IsNullOrEmpty(firstLine))
        {
            return;
        }

        string[] pieces = firstLine.Split('/');
        if (pieces.Length < 3)
        {
            return;
        }

        string graphicScoreStr = pieces[0];
        string scriptScoreStr = pieces[1];
        string biasPercentageStr = pieces[2];
        string missingPagesStr = pieces.Length >= 4 ? pieces[3] : "0";
        if (pieces.Length > 4)
        {
            return;
        }

        if (!int.TryParse(graphicScoreStr, out int graphicScore) || graphicScore <= 0 || graphicScore > 5)
        {
            return;
        }

        if (!int.TryParse(scriptScoreStr, out int scriptScore) || scriptScore <= 0 || scriptScore > 5)
        {
            return;
        }

        if (!int.TryParse(biasPercentageStr, out int biasPercentage) || biasPercentage < 0 || biasPercentage > 100)
        {
            return;
        }

        if (!int.TryParse(missingPagesStr, out int missingPages) || missingPages < 0)
        {
            return;
        }

        int score = CalculateScore(graphicScore, scriptScore, biasPercentage, missingPages);
        if (score == comic.Rating)
        {
            return;
        }

        comic.SetRating(score);
    }

    private static int CalculateScore(int graphicScore, int scriptScore, int biasPercentage, int missingPages)
    {
        int minBound;
        int maxBound;
        switch (graphicScore * 10 + scriptScore)
        {
            case 11:
                minBound = 0;
                maxBound = 20;
                break;
            case 12:
                minBound = 0;
                maxBound = 25;
                break;
            case 13:
                minBound = 0;
                maxBound = 30;
                break;
            case 14:
                minBound = 10;
                maxBound = 35;
                break;
            case 15:
                minBound = 20;
                maxBound = 40;
                break;
            case 21:
                minBound = 20;
                maxBound = 40;
                break;
            case 22:
                minBound = 25;
                maxBound = 45;
                break;
            case 23:
                minBound = 30;
                maxBound = 50;
                break;
            case 24:
                minBound = 35;
                maxBound = 55;
                break;
            case 25:
                minBound = 40;
                maxBound = 60;
                break;
            case 31:
                minBound = 40;
                maxBound = 50;
                break;
            case 32:
                minBound = 45;
                maxBound = 60;
                break;
            case 33:
                minBound = 50;
                maxBound = 70;
                break;
            case 34:
                minBound = 55;
                maxBound = 75;
                break;
            case 35:
                minBound = 60;
                maxBound = 80;
                break;
            case 41:
                minBound = 50;
                maxBound = 60;
                break;
            case 42:
                minBound = 60;
                maxBound = 70;
                break;
            case 43:
                minBound = 70;
                maxBound = 80;
                break;
            case 44:
                minBound = 75;
                maxBound = 90;
                break;
            case 45:
                minBound = 80;
                maxBound = 95;
                break;
            case 51:
                minBound = 60;
                maxBound = 70;
                break;
            case 52:
                minBound = 70;
                maxBound = 80;
                break;
            case 53:
                minBound = 80;
                maxBound = 90;
                break;
            case 54:
                minBound = 90;
                maxBound = 95;
                break;
            case 55:
                minBound = 95;
                maxBound = 100;
                break;
            default:
                throw new System.Exception();
        }

        float scoreFloat = minBound + (maxBound - minBound) * (float)biasPercentage / 100F;
        int score = (int)Math.Round(scoreFloat, MidpointRounding.AwayFromZero);
        score = Math.Clamp(score, 0, 100);
        score -= missingPages * 5;
        score = Math.Clamp(score, 0, 100);
        return score;
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
