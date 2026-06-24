using CommunityToolkit.Mvvm.ComponentModel;

namespace Babel.Models;

public partial class TabLanguageItem : ObservableObject
{
    [ObservableProperty]
    private string _name = "";

    [ObservableProperty]
    private bool _isEnabled;
}
