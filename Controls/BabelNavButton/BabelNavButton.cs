using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Babel.Models;
using Babel.ViewModels;

namespace Babel.Controls;

public class BabelNavButton : Button
{
    static BabelNavButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(BabelNavButton),
            new FrameworkPropertyMetadata(typeof(BabelNavButton)));
    }

    public static readonly DependencyProperty PageProperty =
        DependencyProperty.Register(nameof(Page), typeof(NavPage), typeof(BabelNavButton),
            new PropertyMetadata(NavPage.Home, OnPageChanged));

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(BabelNavButton),
            new PropertyMetadata(false));

    public NavPage Page
    {
        get => (NavPage)GetValue(PageProperty);
        set => SetValue(PageProperty, value);
    }

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    private INotifyPropertyChanged? _viewModel;

    public BabelNavButton()
    {
        DataContextChanged += OnDataContextChanged;
        Loaded += (_, _) => UpdateActiveState();
    }

    private static void OnPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is BabelNavButton btn)
            btn.UpdateActiveState();
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_viewModel != null)
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;

        _viewModel = e.NewValue as INotifyPropertyChanged;

        if (_viewModel != null)
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;

        UpdateActiveState();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(INavigationHost.IsHomeVisible)
            or nameof(INavigationHost.IsProfileConfigVisible)
            or nameof(INavigationHost.IsAboutVisible))
            UpdateActiveState();
    }

    private void UpdateActiveState()
    {
        if (DataContext is not INavigationHost host)
        {
            IsActive = false;
            return;
        }

        IsActive = Page switch
        {
            NavPage.Home => host.IsHomeVisible,
            NavPage.Profile => host.IsProfileConfigVisible,
            NavPage.About => host.IsAboutVisible,
            _ => false,
        };
    }
}
