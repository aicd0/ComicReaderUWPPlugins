// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Controls;

namespace AutoScore;

internal sealed partial class EditScoreDialog : ContentDialog
{
    public EditScoreDialogViewModel ViewModel = new();

    public bool IsSaveClicked { get; private set; } = false;

    public EditScoreDialog(ScoreModel scoreModel)
    {
        InitializeComponentForPlugin();

        ViewModel.Initialize(scoreModel);
    }

    private void ResetButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ViewModel.ResetScore();
    }

    private void SaveButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        IsSaveClicked = true;
        Hide();
    }

    private void CancelButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Hide();
    }

    private void S11_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel.SetS11(((RadioButtons)sender).SelectedIndex);
    }

    private void S12_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel.SetS12(((RadioButtons)sender).SelectedIndex);
    }

    private void S13_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel.SetS13(((RadioButtons)sender).SelectedIndex);
    }

    private void S14_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel.SetS14(((RadioButtons)sender).SelectedIndex);
    }

    private void S15_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel.SetS15(((RadioButtons)sender).SelectedIndex);
    }

    private void S16_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel.SetS16(((RadioButtons)sender).SelectedIndex);
    }

    private void S21_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel.SetS21(((RadioButtons)sender).SelectedIndex);
    }

    private void S22_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel.SetS22(((RadioButtons)sender).SelectedIndex);
    }

    private void S23_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel.SetS23(((RadioButtons)sender).SelectedIndex);
    }

    private void S24_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel.SetS24(((RadioButtons)sender).SelectedIndex);
    }

    private void S25_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel.SetS25(((RadioButtons)sender).SelectedIndex);
    }

    private void S26_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel.SetS26(((RadioButtons)sender).SelectedIndex);
    }

    private void S31_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ViewModel.SetS31(((RadioButtons)sender).SelectedIndex);
    }

    private void RatingVisibilityCheckBox_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ViewModel.SetRatingVisible(((CheckBox)sender).IsChecked ?? false);
    }

    private void InitializeComponentForPlugin()
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
