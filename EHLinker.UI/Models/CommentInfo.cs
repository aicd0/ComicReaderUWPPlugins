// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System;

namespace EHLinker.UI.Models;

public class CommentInfo
{
    public required DateTime PostTime { get; init; }

    public required string Sender { get; init; }

    public required string Content { get; init; }

    public required int Vote { get; init; }

    public required bool IsFromUploader { get; init; }
}
