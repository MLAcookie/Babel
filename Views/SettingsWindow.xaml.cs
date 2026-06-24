using System.Windows;
using Babel.ViewModels;

namespace Babel.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow(SettingsViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        vm.RequestClose += saved =>
        {
            DialogResult = saved;
            Close();
        };
    }
}