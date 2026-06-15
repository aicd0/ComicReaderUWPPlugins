// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;

using EHLinker.UI.Models;

namespace EHLinker.UI;

public interface IPluginAbility
{
    Task<ComicSearchResult> SearchComicsByKeyword(string keyword, bool disableFilters);

    Task<ComicSearchResult> SearchComicsByLink(string link);

    Task<ComicDetailedInfo> RequestComicInfo(string link);
}
