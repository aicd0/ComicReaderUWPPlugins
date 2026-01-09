// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using ComicReaderUWP.SDK.Common.Utils;
using ComicReaderUWP.SDK.Plugins.Comic;

namespace AutoScore;

internal class DetailedScoreModel
{
    private const string LIB_DETAIL_1 = "Detail1";

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

    public void SaveToComicModel(IComicModel comic)
    {
        string dbString = ToDatabaseString();
        AutoScorePlugin.Instance.Context.GetKVDatabase().GetCollection(LIB_DETAIL_1).Set(comic.Id.ToString(), dbString);
    }

    private string ToDatabaseString()
    {
        return $"{S11},{S12},{S13},{S14},{S15},{S16},{S21},{S22},{S23},{S24},{S25},{S26},{S31}";
    }

    public static DetailedScoreModel? FromComicModel(IComicModel comic)
    {
        string? dbString = AutoScorePlugin.Instance.Context.GetKVDatabase().GetCollection(LIB_DETAIL_1).GetValue<string>(comic.Id.ToString());
        if (string.IsNullOrEmpty(dbString))
        {
            return null;
        }

        return FromDatabaseString(dbString);
    }

    private static DetailedScoreModel? FromDatabaseString(string dbString)
    {
        string[] parts = dbString.Split(',');
        if (parts.Length != 13)
        {
            return null;
        }

        return new DetailedScoreModel
        {
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
}
