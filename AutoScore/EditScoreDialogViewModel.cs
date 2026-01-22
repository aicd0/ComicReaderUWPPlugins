// Copyright (c) aicd0. All rights reserved.
// Licensed under the MIT License.

using System.ComponentModel;

namespace AutoScore;

internal partial class EditScoreDialogViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set
        {
            if (_title != value)
            {
                _title = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
            }
        }
    }

    private string _description = string.Empty;
    public string Description
    {
        get => _description;
        set
        {
            if (_description != value)
            {
                _description = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Description)));
            }
        }
    }

    private int _s11Selection = 0;
    public int S11Selection
    {
        get => _s11Selection;
        set
        {
            if (_s11Selection != value)
            {
                _s11Selection = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(S11Selection)));
            }
        }
    }

    private int _s12Selection = 0;
    public int S12Selection
    {
        get => _s12Selection;
        set
        {
            if (_s12Selection != value)
            {
                _s12Selection = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(S12Selection)));
            }
        }
    }

    private int _s13Selection = 0;
    public int S13Selection
    {
        get => _s13Selection;
        set
        {
            if (_s13Selection != value)
            {
                _s13Selection = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(S13Selection)));
            }
        }
    }

    private int _s14Selection = 0;
    public int S14Selection
    {
        get => _s14Selection;
        set
        {
            if (_s14Selection != value)
            {
                _s14Selection = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(S14Selection)));
            }
        }
    }

    private int _s15Selection = 0;
    public int S15Selection
    {
        get => _s15Selection;
        set
        {
            if (_s15Selection != value)
            {
                _s15Selection = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(S15Selection)));
            }
        }
    }

    private int _s16Selection = 0;
    public int S16Selection
    {
        get => _s16Selection;
        set
        {
            if (_s16Selection != value)
            {
                _s16Selection = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(S16Selection)));
            }
        }
    }

    private int _s21Selection = 0;
    public int S21Selection
    {
        get => _s21Selection;
        set
        {
            if (_s21Selection != value)
            {
                _s21Selection = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(S21Selection)));
            }
        }
    }

    private int _s22Selection = 0;
    public int S22Selection
    {
        get => _s22Selection;
        set
        {
            if (_s22Selection != value)
            {
                _s22Selection = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(S22Selection)));
            }
        }
    }

    private int _s23Selection = 0;
    public int S23Selection
    {
        get => _s23Selection;
        set
        {
            if (_s23Selection != value)
            {
                _s23Selection = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(S23Selection)));
            }
        }
    }

    private int _s24Selection = 0;
    public int S24Selection
    {
        get => _s24Selection;
        set
        {
            if (_s24Selection != value)
            {
                _s24Selection = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(S24Selection)));
            }
        }
    }

    private int _s25Selection = 0;
    public int S25Selection
    {
        get => _s25Selection;
        set
        {
            if (_s25Selection != value)
            {
                _s25Selection = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(S25Selection)));
            }
        }
    }

    private int _s26Selection = 0;
    public int S26Selection
    {
        get => _s26Selection;
        set
        {
            if (_s26Selection != value)
            {
                _s26Selection = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(S26Selection)));
            }
        }
    }

    private int _s31Selection = 0;
    public int S31Selection
    {
        get => _s31Selection;
        set
        {
            if (_s31Selection != value)
            {
                _s31Selection = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(S31Selection)));
            }
        }
    }

    private bool _isRatingVisible = false;
    public bool IsRatingVisible
    {
        get => _isRatingVisible;
        set
        {
            if (_isRatingVisible != value)
            {
                _isRatingVisible = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRatingVisible)));
            }
        }
    }

    private ScoreModel? _oldScoreModel;
    private ScoreModel? _newScoreModel;

    public void Initialize(ScoreModel scoreModel)
    {
        Title = "Edit score";
        _oldScoreModel = scoreModel.Clone();
        _newScoreModel = scoreModel;
        S11Selection = scoreModel.S11;
        S12Selection = scoreModel.S12;
        S13Selection = scoreModel.S13;
        S14Selection = scoreModel.S14;
        S15Selection = scoreModel.S15;
        S16Selection = scoreModel.S16;
        S21Selection = scoreModel.S21;
        S22Selection = scoreModel.S22;
        S23Selection = scoreModel.S23;
        S24Selection = scoreModel.S24;
        S25Selection = scoreModel.S25;
        S26Selection = scoreModel.S26;
        S31Selection = scoreModel.S31;
        IsRatingVisible = scoreModel.IsRated;
        UpdateScore();
    }

    public void ResetScore()
    {
        S11Selection = 0;
        S12Selection = 0;
        S13Selection = 0;
        S14Selection = 0;
        S15Selection = 0;
        S16Selection = 0;
        S21Selection = 0;
        S22Selection = 0;
        S23Selection = 0;
        S24Selection = 0;
        S25Selection = 0;
        S26Selection = 0;
        S31Selection = 0;
        UpdateScore();
    }

    public void SetS11(int index)
    {
        _s11Selection = index;
        UpdateScore();
    }

    public void SetS12(int index)
    {
        _s12Selection = index;
        UpdateScore();
    }

    public void SetS13(int index)
    {
        _s13Selection = index;
        UpdateScore();
    }

    public void SetS14(int index)
    {
        _s14Selection = index;
        UpdateScore();
    }

    public void SetS15(int index)
    {
        _s15Selection = index;
        UpdateScore();
    }

    public void SetS16(int index)
    {
        _s16Selection = index;
        UpdateScore();
    }

    public void SetS21(int index)
    {
        _s21Selection = index;
        UpdateScore();
    }

    public void SetS22(int index)
    {
        _s22Selection = index;
        UpdateScore();
    }

    public void SetS23(int index)
    {
        _s23Selection = index;
        UpdateScore();
    }

    public void SetS24(int index)
    {
        _s24Selection = index;
        UpdateScore();
    }

    public void SetS25(int index)
    {
        _s25Selection = index;
        UpdateScore();
    }

    public void SetS26(int index)
    {
        _s26Selection = index;
        UpdateScore();
    }

    public void SetS31(int index)
    {
        _s31Selection = index;
        UpdateScore();
    }

    public void SetRatingVisible(bool visible)
    {
        _isRatingVisible = visible;
        UpdateScore();
    }

    private void UpdateScore()
    {
        ScoreModel? oldScoreModel = _oldScoreModel;
        ScoreModel? newScoreModel = _newScoreModel;
        if (oldScoreModel is null || newScoreModel is null)
        {
            return;
        }

        newScoreModel.S11 = _s11Selection;
        newScoreModel.S12 = _s12Selection;
        newScoreModel.S13 = _s13Selection;
        newScoreModel.S14 = _s14Selection;
        newScoreModel.S15 = _s15Selection;
        newScoreModel.S16 = _s16Selection;
        newScoreModel.S21 = _s21Selection;
        newScoreModel.S22 = _s22Selection;
        newScoreModel.S23 = _s23Selection;
        newScoreModel.S24 = _s24Selection;
        newScoreModel.S25 = _s25Selection;
        newScoreModel.S26 = _s26Selection;
        newScoreModel.S31 = _s31Selection;
        newScoreModel.IsRated = _isRatingVisible;

        int oldScore = oldScoreModel.GetAbsoluteScore();
        int newScore = newScoreModel.GetAbsoluteScore();
        Description = $"Old score: {oldScore}\nNew score: {newScore}";
    }
}
