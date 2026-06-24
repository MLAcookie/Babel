using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Babel.Models;
using Babel.Services;

namespace Babel.ViewModels;

public partial class SettingsViewModel : ObservableObject, INavigationHost
{
    private readonly AppSettings _settings;
    private readonly AppSettings _original;
    private readonly IAiModelService _aiModel;

    [ObservableProperty]
    private ObservableCollection<Profile> _profiles = [];

    [ObservableProperty]
    private Profile? _selectedProfile;

    [ObservableProperty]
    private string _apiKey = "";

    [ObservableProperty]
    private string _endpoint = "";

    [ObservableProperty]
    private string _model = "";

    [ObservableProperty]
    private ObservableCollection<string> _availableModels = [];

    [ObservableProperty]
    private bool _isLoadingModels;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasModelFetchError))]
    private string _modelFetchError = "";

    [ObservableProperty]
    private bool _isHomeVisible = true;

    [ObservableProperty]
    private bool _isProfileConfigVisible;

    [ObservableProperty]
    private bool _isAboutVisible;

    [ObservableProperty]
    private string _versionText = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string _errorMessage = "";

    [ObservableProperty]
    private ObservableCollection<TabLanguageItem> _tabLanguageItems = [];

    [ObservableProperty]
    private string _newLanguageInput = "";

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public bool HasModelFetchError => !string.IsNullOrEmpty(ModelFetchError);

    public event Action<bool>? RequestClose;

    public SettingsViewModel(AppSettings settings, IAiModelService aiModel)
    {
        _settings = settings;
        _original = settings.Clone();
        _aiModel = aiModel;

        VersionText = typeof(App).Assembly.GetName().Version?.ToString(3) ?? "1.0.0";

        Profiles = [.. settings.Profiles];
        SelectedProfile = settings.ActiveProfile;
        LoadFormFromSelectedProfile();
        LoadTabLanguages();
    }

    private void LoadTabLanguages()
    {
        var items = _settings.TabLanguages.Select(l => new TabLanguageItem
        {
            Name = l,
            IsEnabled = true,
        });

        TabLanguageItems = new ObservableCollection<TabLanguageItem>(items);
    }

    partial void OnSelectedProfileChanged(Profile? value)
    {
        if (value != null && _settings.ActiveProfileId != value.Id)
        {
            SaveFormToProfile();
            _settings.ActiveProfileId = value.Id;
        }

        LoadFormFromSelectedProfile();
        AvailableModels = [];
        ModelFetchError = "";
    }

    private void LoadFormFromSelectedProfile()
    {
        var p = _settings.ActiveProfile;
        if (p == null)
        {
            ApiKey = "";
            Endpoint = "";
            Model = "";
            return;
        }

        ApiKey = p.ApiKey;
        Endpoint = p.ApiEndpoint;
        Model = p.Model;
    }

    private void SaveFormToProfile()
    {
        _settings.ApiKey = ApiKey;
        _settings.ApiEndpoint = Endpoint;
        _settings.Model = Model;
    }

    private void SyncTabLanguagesToSettings()
    {
        _settings.TabLanguages = TabLanguageItems
            .Where(t => t.IsEnabled)
            .Select(t => t.Name)
            .ToList();
    }

    [RelayCommand]
    private async Task RefreshModelsAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(Endpoint))
        {
            ModelFetchError = "API Endpoint is required.";
            return;
        }

        IsLoadingModels = true;
        ModelFetchError = "";

        try
        {
            var models = await _aiModel.ListModelsAsync(Endpoint, ApiKey, ct);
            AvailableModels = new ObservableCollection<string>(models);

            if (!string.IsNullOrWhiteSpace(Model) && !AvailableModels.Contains(Model))
                AvailableModels.Add(Model);
        }
        catch (Exception ex)
        {
            ModelFetchError = $"Failed to fetch models: {ex.Message}";
        }
        finally
        {
            IsLoadingModels = false;
        }
    }

    [RelayCommand]
    private void Save()
    {
        SaveFormToProfile();
        SyncTabLanguagesToSettings();

        if (_settings.ActiveProfile == null)
        {
            ErrorMessage = "Please create and configure a profile.";
            return;
        }

        if (string.IsNullOrEmpty(_settings.ApiKey))
        {
            ErrorMessage = "API Key is required.";
            return;
        }

        ErrorMessage = "";
        _settings.Save();
        RequestClose?.Invoke(true);
    }

    [RelayCommand]
    private void Cancel()
    {
        RestoreOriginalState();
        Profiles = [.. _settings.Profiles];
        SelectedProfile = _settings.ActiveProfile;
        LoadFormFromSelectedProfile();
        LoadTabLanguages();
        RequestClose?.Invoke(false);
    }

    private void RestoreOriginalState()
    {
        _settings.Profiles = _original.Profiles;
        _settings.ActiveProfileId = _original.ActiveProfileId;
        _settings.TabLanguages = [.. _original.TabLanguages];
    }

    [RelayCommand]
    private void AddProfile()
    {
        var profile = new Profile
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = "New Profile",
        };
        _settings.Profiles.Add(profile);
        Profiles.Add(profile);
        SelectedProfile = profile;
        ShowProfileConfig();
    }

    [RelayCommand]
    private void DeleteProfile(Profile? profile)
    {
        if (profile == null)
            return;

        var idx = _settings.Profiles.IndexOf(profile);
        if (idx < 0) return;

        _settings.Profiles.RemoveAt(idx);
        Profiles.RemoveAt(idx);

        ClearActiveIfDeleted(profile);
        SelectFallbackProfile(idx);

        LoadFormFromSelectedProfile();
        ShowProfileConfig();
    }

    private void ClearActiveIfDeleted(Profile deleted)
    {
        if (_settings.ActiveProfileId != deleted.Id) return;

        _settings.ActiveProfileId = "";
        _settings.ApiKey = "";
        _settings.ApiEndpoint = "";
        _settings.Model = "";
    }

    private void SelectFallbackProfile(int deletedIdx)
    {
        if (_settings.ActiveProfile != null)
        {
            SelectedProfile = _settings.ActiveProfile;
        }
        else if (_settings.Profiles.Count > 0)
        {
            var newIdx = Math.Min(deletedIdx, _settings.Profiles.Count - 1);
            SelectedProfile = _settings.Profiles[newIdx];
        }
        else
        {
            SelectedProfile = null;
        }
    }

    [RelayCommand]
    private void ShowHome()
    {
        SaveFormToProfile();
        IsHomeVisible = true;
        IsProfileConfigVisible = false;
        IsAboutVisible = false;
    }

    [RelayCommand]
    private void ShowProfileConfig()
    {
        SaveFormToProfile();
        IsHomeVisible = false;
        IsProfileConfigVisible = true;
        IsAboutVisible = false;
    }

    [RelayCommand]
    private void ShowAbout()
    {
        SaveFormToProfile();
        IsHomeVisible = false;
        IsProfileConfigVisible = false;
        IsAboutVisible = true;
    }

    [RelayCommand]
    private void AddTabLanguage()
    {
        var name = NewLanguageInput.Trim();
        if (string.IsNullOrEmpty(name)) return;

        if (TabLanguageItems.Any(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase)))
            return;

        TabLanguageItems.Add(new TabLanguageItem
        {
            Name = name,
            IsEnabled = true,
        });
        NewLanguageInput = "";
    }

    [RelayCommand]
    private void RemoveTabLanguage(TabLanguageItem? item)
    {
        if (item != null)
            TabLanguageItems.Remove(item);
    }
}
