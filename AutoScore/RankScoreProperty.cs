// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

using ComicReaderUWP.SDK.Plugins.Comic;
using ComicReaderUWP.SDK.Plugins.Property;

namespace AutoScore;

internal class RankScoreProperty : IVirtualProperty<IComicModel>
{
    public string Name => "RankScore";

    public string DisplayName => "Rank score";

    public IVirtualPropertySorter<IComicModel> CreateSorter<T>(IEnumerable<T> items) where T : IComicModel
    {
        return new RankScorePropertySorter<T>(items);
    }
}
