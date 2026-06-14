// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System;

namespace EHLinker.UI.Models;

public class ComicBasicInfo
{
    public required string Title { get; init; }

    public required ComicCategory Category { get; init; }

    public required DateTimeOffset UploadTime { get; init; }

    public required int PageCount { get; init; }

    public required float Rating { get; init; }
}
