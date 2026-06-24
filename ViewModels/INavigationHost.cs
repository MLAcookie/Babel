namespace Babel.ViewModels;

public interface INavigationHost
{
    bool IsHomeVisible { get; }
    bool IsProfileConfigVisible { get; }
    bool IsAboutVisible { get; }
}
