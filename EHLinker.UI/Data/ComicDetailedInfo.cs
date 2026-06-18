// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace EHLinker.UI.Data;

public class ComicDetailedInfo
{
    public required string Title1 { get; init; }

    public required string Title2 { get; init; }

    public required IReadOnlyDictionary<string, IReadOnlySet<string>> Tags { get; init; }

    public required IReadOnlyList<CommentInfo> Comments { get; init; }
}
