// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using EHLinker.UI.Models;

namespace EHLinker.UI;

internal class ComicSearchResultItemViewModel
{
    public required string Title { get; set; }

    public required string Description { get; set; }

    public required ComicBasicInfo Source { get; set; }
}
