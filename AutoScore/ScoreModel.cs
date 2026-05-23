// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using ComicReaderUWP.SDK.Plugins.Comic;

using Shared;
using Shared.Utils;

namespace AutoScore;

internal class ScoreModel
{
    public const int MAX_SCORE = 1188;

    public bool IsRated { get; set; } = true;
    public int S11 { get; set; } = 0;
    public int S12 { get; set; } = 0;
    public int S13 { get; set; } = 0;
    public int S14 { get; set; } = 0;
    public int S15 { get; set; } = 0;
    public int S16 { get; set; } = 0;
    public int S21 { get; set; } = 0;
    public int S22 { get; set; } = 0;
    public int S23 { get; set; } = 0;
    public int S24 { get; set; } = 0;
    public int S25 { get; set; } = 0;
    public int S26 { get; set; } = 0;
    public int S31 { get; set; } = 0;

    private ScoreModel()
    {
    }

    public ScoreModel Clone()
    {
        return new()
        {
            IsRated = IsRated,
            S11 = S11,
            S12 = S12,
            S13 = S13,
            S14 = S14,
            S15 = S15,
            S16 = S16,
            S21 = S21,
            S22 = S22,
            S23 = S23,
            S24 = S24,
            S25 = S25,
            S26 = S26,
            S31 = S31,
        };
    }

    public void Save(IComicModel comic, ScoreSourceEnum source)
    {
        JsonModel jsonModel = new()
        {
            IsRated = IsRated,
            S11 = S11,
            S12 = S12,
            S13 = S13,
            S14 = S14,
            S15 = S15,
            S16 = S16,
            S21 = S21,
            S22 = S22,
            S23 = S23,
            S24 = S24,
            S25 = S25,
            S26 = S26,
            S31 = S31,
        };
        string jsonString = JsonSerializer.Serialize(jsonModel);
        SharedContext.PluginContext.RegistryDatabase
            .CreateKey(SourceToLibName(source))
            .Set(comic.Id.ToString(), jsonString);
    }

    public int GetAbsoluteScore()
    {
        float graphicScore =
            S11 +
            S12 +
            S13 +
            S14 +
            S15 +
            S16;
        float scriptScore =
            S21 +
            S22 +
            S23 +
            S24 +
            S25 +
            S26;
        int missingPages = S31;
        graphicScore = Math.Clamp(graphicScore / 10F, 0F, 5.4F);
        scriptScore = Math.Clamp(scriptScore / 10F, 0F, 5.4F);
        missingPages = Math.Clamp(missingPages, 0, 10);
        float scoreFloat = graphicScore * 120F + scriptScore * 100F;
        scoreFloat *= 1F - 0.04F * Math.Abs(graphicScore - scriptScore);
        scoreFloat *= 1F - 0.05F * missingPages;
        int score = (int)Math.Round(scoreFloat, MidpointRounding.AwayFromZero);
        return Math.Clamp(score, 0, MAX_SCORE);
    }

    public static ScoreModel FromEmpty()
    {
        return new();
    }

    public static ScoreModel? Load(IComicModel comic, ScoreSourceEnum source)
    {
        string? dbString = SharedContext.PluginContext.RegistryDatabase
            .CreateKey(SourceToLibName(source))
            .GetValue<string>(comic.Id.ToString());
        if (string.IsNullOrEmpty(dbString))
        {
            return null;
        }

        return FromDatabaseString(dbString);
    }

    private static ScoreModel? FromDatabaseString(string dbString)
    {
        JsonModel? jsonModel = null;
        try
        {
            jsonModel = JsonSerializer.Deserialize<JsonModel>(dbString);
        }
        catch (JsonException)
        {
        }

        if (jsonModel is not null)
        {
            return new()
            {
                IsRated = jsonModel.IsRated,
                S11 = jsonModel.S11,
                S12 = jsonModel.S12,
                S13 = jsonModel.S13,
                S14 = jsonModel.S14,
                S15 = jsonModel.S15,
                S16 = jsonModel.S16,
                S21 = jsonModel.S21,
                S22 = jsonModel.S22,
                S23 = jsonModel.S23,
                S24 = jsonModel.S24,
                S25 = jsonModel.S25,
                S26 = jsonModel.S26,
                S31 = jsonModel.S31,
            };
        }

        string[] parts = dbString.Split(',');
        if (parts.Length == 13)
        {
            try
            {
                return new ScoreModel
                {
                    IsRated = true,
                    S11 = int.Parse(parts[0]),
                    S12 = int.Parse(parts[1]),
                    S13 = int.Parse(parts[2]),
                    S14 = int.Parse(parts[3]),
                    S15 = int.Parse(parts[4]),
                    S16 = int.Parse(parts[5]),
                    S21 = int.Parse(parts[6]),
                    S22 = int.Parse(parts[7]),
                    S23 = int.Parse(parts[8]),
                    S24 = int.Parse(parts[9]),
                    S25 = int.Parse(parts[10]),
                    S26 = int.Parse(parts[11]),
                    S31 = int.Parse(parts[12]),
                };
            }
            catch (FormatException)
            {
            }
        }

        return null;
    }

    private static string SourceToLibName(ScoreSourceEnum source)
    {
        return source switch
        {
            ScoreSourceEnum.Main => RegistryNames.DETAIL_1,
            ScoreSourceEnum.Draft => RegistryNames.SCORE_DRAFT,
            _ => throw new ArgumentException("Invalid score source"),
        };
    }

    private class JsonModel
    {
        [JsonPropertyName("IsRated")]
        public bool IsRated { get; set; }

        [JsonPropertyName("S11")]
        public int S11 { get; set; }

        [JsonPropertyName("S12")]
        public int S12 { get; set; }

        [JsonPropertyName("S13")]
        public int S13 { get; set; }

        [JsonPropertyName("S14")]
        public int S14 { get; set; }

        [JsonPropertyName("S15")]
        public int S15 { get; set; }

        [JsonPropertyName("S16")]
        public int S16 { get; set; }

        [JsonPropertyName("S21")]
        public int S21 { get; set; }

        [JsonPropertyName("S22")]
        public int S22 { get; set; }

        [JsonPropertyName("S23")]
        public int S23 { get; set; }

        [JsonPropertyName("S24")]
        public int S24 { get; set; }

        [JsonPropertyName("S25")]
        public int S25 { get; set; }

        [JsonPropertyName("S26")]
        public int S26 { get; set; }

        [JsonPropertyName("S31")]
        public int S31 { get; set; }
    }

    public enum ScoreSourceEnum
    {
        Main,
        Draft,
    }
}
