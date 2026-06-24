using System.Windows.Controls;
using Babel.ViewModels;

namespace Babel.Views.Settings;

public partial class ProfilePanel : UserControl
{
    public ProfilePanel()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is not SettingsViewModel vm) return;

        ApiKeyBox.Password = vm.ApiKey;
        ApiKeyBox.PasswordChanged += (_, _) => vm.ApiKey = ApiKeyBox.Password;
    }
}