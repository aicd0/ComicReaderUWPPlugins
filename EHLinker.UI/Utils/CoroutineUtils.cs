// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;

namespace EHLinker.UI.Utils;

public static class CoroutineUtils
{
    private const string TAG = nameof(CoroutineUtils);

    public static void Run(Func<Task> task)
    {
        task().ContinueWith(t =>
        {
            AggregateException? exception = t.Exception;
            if (exception is not null)
            {
                PluginService.Context.Logger.F(TAG, exception.Message, exception);
            }
        });
    }
}
