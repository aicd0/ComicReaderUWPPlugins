// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

using ComicReaderUWP.SDK.Plugins.Comic;
using ComicReaderUWP.SDK.Plugins.Property;

namespace AutoScore;

internal class RankScorePropertySorter<A> : IVirtualPropertySorter<IComicModel> where A : IComicModel
{
    private readonly Dictionary<IComicModel, ComicInfo> _comicInfoMap = [];

    public RankScorePropertySorter(IEnumerable<A> items)
    {
        List<A> sorted = [.. items.Where(x => x.Rating >= 0).OrderByDescending(x => x.Rating)];
        int rank = 0;
        int lastRating = 0;
        for (int i = 0; i < sorted.Count; i++)
        {
            A item = sorted[i];
            if (i == 0 || item.Rating != lastRating)
            {
                rank = i + 1;
            }

            lastRating = item.Rating;
            double rankRatio = (double)(rank - 1) / sorted.Count;
            double score = 1000.0 * Math.Pow(2.0, -10.0 * rankRatio);
            _comicInfoMap[item] = new()
            {
                Score = score,
                Rank = rank,
            };
        }
    }

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
