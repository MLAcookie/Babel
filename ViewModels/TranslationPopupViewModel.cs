using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Babel.Models;
using Babel.Services;

namespace Babel.ViewModels;

public partial class TranslationPopupViewModel : ObservableObject
{
    private readonly AppSettings _settings;
    private readonly IAiModelService _aiModel;
    private CancellationTokenSource? _cts;

    [ObservableProperty]
    private string _inputText = "";

    [ObservableProperty]
    private string _statusText = "";

    [ObservableProperty]
    private bool _isStatusVisible;

    [ObservableProperty]
    private bool _isInputEnabled = true;

    [ObservableProperty]
    private string _targetLanguage = "English";

    [ObservableProperty]
    private string _statusForeground = "#888888";

    private int _langIndex;

    public event Action<string?>? RequestClose;

    public TranslationPopupViewModel(AppSettings settings, IAiModelService aiModel)
    {
        _settings = settings;
        _aiModel = aiModel;

        _langIndex = _settings.TabLanguages.Count > 0
            ? Math.Max(_settings.TabLanguages.IndexOf(settings.TargetLanguage), 0)
            : -1;
        TargetLanguage = settings.TargetLanguage;
    }

    partial void OnIsInputEnabledChanged(bool value)
    {
        TranslateCommand.NotifyCanExecuteChanged();
        SwitchLanguageCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanTranslate))]
    private async Task TranslateAsync()
    {
        var text = InputText.Trim();
        if (string.IsNullOrEmpty(text)) return;

        IsInputEnabled = false;
        StatusText = "Translating...";
        StatusForeground = "#888888";
        IsStatusVisible = true;

        _cts = new CancellationTokenSource();

        try
        {
            var translated = await _aiModel.TranslateAsync(text, _settings, _cts.Token);
            RequestClose?.Invoke(translated);
        }
        catch (OperationCanceledException)
        {
            // dismissed — silently discard
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
            StatusForeground = "#FF6464";
            IsInputEnabled = true;
        }
    }

    private bool CanTranslate => IsInputEnabled;

    [RelayCommand(CanExecute = nameof(CanSwitchLanguage))]
    private void SwitchLanguage()
    {
        if (_settings.TabLanguages.Count == 0) return;

        _langIndex = (_langIndex + 1) % _settings.TabLanguages.Count;
        TargetLanguage = _settings.TabLanguages[_langIndex];
        _settings.TargetLanguage = TargetLanguage;
    }

    private bool CanSwitchLanguage => IsInputEnabled && _settings.TabLanguages.Count > 0;

    [RelayCommand]
    private void Close()
    {
        _cts?.Cancel();
        RequestClose?.Invoke(null);
    }

    public void Cancel()
    {
        _cts?.Cancel();
    }
}
