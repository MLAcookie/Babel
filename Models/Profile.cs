using CommunityToolkit.Mvvm.ComponentModel;

namespace Babel.Models;

public partial class Profile : ObservableObject
{
    [ObservableProperty]
    private string _id = "";

    [ObservableProperty]
    private string _name = "";

    [ObservableProperty]
    private string _apiKey = "";

    [ObservableProperty]
    private string _apiEndpoint = "";

    [ObservableProperty]
    private string _model = "";
}
