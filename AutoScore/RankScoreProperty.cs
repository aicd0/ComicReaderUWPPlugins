// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

using ComicReader.SDK.Plugins.Comic;
using ComicReader.SDK.Plugins.Property;

namespace AutoScore;

internal class RankScoreProperty : IVirtualProperty<IComicModel>
{
    private readonly Dictionary<IComicModel, ComicInfo> _comicInfoMap = [];

    public string Name => "RankScore";

    public string DisplayName => "Rank score";

    public double? AsNumber(IComicModel item)
    {
        if (!_comicInfoMap.TryGetValue(item, out ComicInfo? info))
        {
            return null;
        }

        return info.Score;
    }

    public IEnumerable<string> GetGroupNames(IComicModel item)
    {
        if (!_comicInfoMap.TryGetValue(item, out ComicInfo? info))
        {
            return ["Ungrouped"];
        }

        int rankGroup = (info.Rank - 1) / 20;
        int minRank = rankGroup * 20 + 1;
        int maxRank = rankGroup * 20 + 20;
        return [$"{minRank} ~ {maxRank}"];
    }

    public void Initialize<T>(IEnumerable<T> items) where T : IComicModel
    {
        _comicInfoMap.Clear();
        List<T> sorted = [.. items.Where(x => x.Rating >= 0).OrderByDescending(x => x.Rating)];
        int rank = 0;
        int lastRating = 0;
        for (int i = 0; i < sorted.Count; i++)
        {
            T item = sorted[i];
            if (i == 0 || item.Rating != lastRating)
            {
                rank = i + 1;
            }

            lastRating = item.Rating;
            double score = 1000.0 * Math.Pow(rank, -0.3);
            _comicInfoMap[item] = new()
            {
                Score = score,
                Rank = rank,
            };
        }
    }

    public IEnumerable<T> SortGroups<T>(IEnumerable<T> items) where T : IItemGroup<IComicModel>
    {
        return items.OrderBy(x =>
        {
            if (!_comicInfoMap.TryGetValue(x.Items[0], out ComicInfo? info))
            {
                return int.MinValue;
            }

            return info.Score;
        });
    }

    public IEnumerable<T> SortItems<T>(IEnumerable<T> items) where T : IComicModel
    {
        return items.OrderBy(x =>
        {
            if (!_comicInfoMap.TryGetValue(x, out ComicInfo? info))
            {
                return int.MinValue;
            }

            return info.Score;
        });
    }

    private class ComicInfo
    {
        public double Score { get; init; }
        public int Rank { get; init; }
    }
}
