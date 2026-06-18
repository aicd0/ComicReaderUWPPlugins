// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;

using EHLinker.UI.Data;

namespace EHLinker.UI;

public interface IPluginAbility
{
    IReadOnlyDictionary<string, string> GetCookies();

    void SetCookies(IReadOnlyDictionary<string, string> cookies);

    Task<ComicSearchResult> SearchComicsByKeyword(string keyword, bool disableFilters);

    Task<ComicSearchResult> SearchComicsByLink(string link);

    Task<ComicDetailedInfo> RequestComicInfo(string link);
}
