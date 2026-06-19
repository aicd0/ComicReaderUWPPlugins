// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.IO;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace EHLinker.UI.Views;

public sealed partial class SettingsDialog : ContentDialog
{
    internal SettingsDialogViewModel ViewModel = new();

    public SettingsDialog()
    {
        InitializeComponentForPlugin();
        ViewModel.Initialize();
    }

    private void InitializeComponentForPlugin()
    {
        if (_contentLoaded)
        {
            return;
        }

        _contentLoaded = true;
        string resourceFolderPath = PluginService.Context.ResourceFolderPath;
        var resourceLocator = new System.Uri($"ms-appx:///{resourceFolderPath}/EHLinker.UI/Views/SettingsDialog.xaml");
        Application.LoadComponent(this, resourceLocator, ComponentResourceLocation.Nested);
    }

    private void ImportTagsAutomaticallyCheckBox_Click(object sender, RoutedEventArgs e)
    {
        bool isChecked = ((CheckBox)sender).IsChecked == true;
        ViewModel.SetImportTagsAutomatically(isChecked);
    }

    private void ImportTagsWhenNotPresentCheckBox_Click(object sender, RoutedEventArgs e)
    {
        bool isChecked = ((CheckBox)sender).IsChecked == true;
        ViewModel.SetImportTagsWhenNotPresent(isChecked);
    }

    private void EditCookiesButton_Click(object sender, RoutedEventArgs e)
    {
        string cookieString = CookieManager.GetCookiesAsString();

        string tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            File.WriteAllText(tempFile, cookieString);
            using Process notepad = Process.Start(new ProcessStartInfo
            {
                FileName = "notepad.exe",
                Arguments = $"\"{tempFile}\"",
                UseShellExecute = true
            }) ?? throw new InvalidOperationException("Failed to start Notepad.");
            notepad.WaitForExit();
            cookieString = File.ReadAllText(tempFile);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }

        CookieManager.SaveCookies(cookieString);
        PluginService.Ability.SetCookies(CookieManager.GetCookies());
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }
}
