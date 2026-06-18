// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

using EHLinker.UI.Utils;

namespace EHLinker.UI.Data;

public static class SettingsModel
{
    private const string KEY_AUTO_IMPORT_TAGS = "ImportTagsAutomatically";
    private const string KEY_DISABLE_FILTERS = "DisableFilters";

    public static bool ImportTagsAutomatically
    {
        get => PluginService.Context.RegistryDatabase.CreateKey(PluginConstants.REGISTRY_SETTINGS).GetValueOrDefault(KEY_AUTO_IMPORT_TAGS, true);
        set => PluginService.Context.RegistryDatabase.CreateKey(PluginConstants.REGISTRY_SETTINGS).Set(KEY_AUTO_IMPORT_TAGS, value);
    }

    public static bool DisableFilters
    {
        get => PluginService.Context.RegistryDatabase.CreateKey(PluginConstants.REGISTRY_SETTINGS).GetValueOrDefault(KEY_DISABLE_FILTERS, true);
        set => PluginService.Context.RegistryDatabase.CreateKey(PluginConstants.REGISTRY_SETTINGS).Set(KEY_DISABLE_FILTERS, value);
    }
}
