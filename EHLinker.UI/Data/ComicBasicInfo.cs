// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

namespace EHLinker.UI.Data;

public class ComicBasicInfo
{
    public required string Title { get; init; }

    public required string Link { get; init; }

    public required ComicCategory Category { get; init; }

    public required int PageCount { get; init; }
}
