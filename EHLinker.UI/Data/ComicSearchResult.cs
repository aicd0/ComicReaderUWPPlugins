// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace EHLinker.UI.Data;

public class ComicSearchResult
{
    public required IReadOnlyList<ComicBasicInfo> Items { get; init; }

    public required string? PreviousPageLink { get; init; }

    public required string? NextPageLink { get; init; }
}
