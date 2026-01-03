// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Controls;

namespace AutoScore;

internal sealed partial class EditScoreDialog : ContentDialog
{
    public EditScoreDialogViewModel ViewModel = new();

    public EditScoreDialog()
    {
        InitializeComponentForPlugin();

        ViewModel.Initialize();
    }

    public void InitializeComponentForPlugin()
    {
        if (_contentLoaded)
        {
            return;
        }

        _contentLoaded = true;
        string resourceFolderPath = AutoScorePlugin.Instance.Context.ResourceFolderPath;
        var resourceLocator = new System.Uri($"ms-appx:///{resourceFolderPath}/AutoScore/EditScoreDialog.xaml");
        Microsoft.UI.Xaml.Application.LoadComponent(this, resourceLocator, Microsoft.UI.Xaml.Controls.Primitives.ComponentResourceLocation.Nested);
    }
}
