// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System.ComponentModel;

using EHLinker.UI.Data;

namespace EHLinker.UI.Views;

internal partial class SettingsDialogViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private bool _importTagsAutomatically = false;
    public bool ImportTagsAutomatically
    {
        get => _importTagsAutomatically;
        set
        {
            _importTagsAutomatically = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImportTagsAutomatically)));
        }
    }

    private bool _importTagsWhenNotPresent = false;
    public bool ImportTagsWhenNotPresent
    {
        get => _importTagsWhenNotPresent;
        set
        {
            _importTagsWhenNotPresent = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImportTagsWhenNotPresent)));
        }
    }

    public void Initialize()
    {
        ImportTagsAutomatically = SettingsModel.ImportTagsAutomatically;
        ImportTagsWhenNotPresent = SettingsModel.ImportTagsWhenNotPresent;
    }

    public void SetImportTagsAutomatically(bool enabled)
    {
        if (enabled == ImportTagsAutomatically)
        {
            return;
        }

        ImportTagsAutomatically = enabled;
        SettingsModel.ImportTagsAutomatically = enabled;
    }

    public void SetImportTagsWhenNotPresent(bool enabled)
    {
        if (enabled == ImportTagsWhenNotPresent)
        {
            return;
        }

        ImportTagsWhenNotPresent = enabled;
        SettingsModel.ImportTagsWhenNotPresent = enabled;
    }
}
