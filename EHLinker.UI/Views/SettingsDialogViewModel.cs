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

    public void Initialize()
    {
        ImportTagsAutomatically = SettingsModel.ImportTagsAutomatically;
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
}
