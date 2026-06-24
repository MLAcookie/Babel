using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Babel.Controls;

public class ClosableTag : ToggleButton
{
    static ClosableTag()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ClosableTag),
            new FrameworkPropertyMetadata(typeof(ClosableTag)));
    }

    public static readonly DependencyProperty CloseCommandProperty =
        DependencyProperty.Register(nameof(CloseCommand), typeof(ICommand), typeof(ClosableTag));

    public ICommand? CloseCommand
    {
        get => (ICommand?)GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }

    public static readonly DependencyProperty CloseCommandParameterProperty =
        DependencyProperty.Register(nameof(CloseCommandParameter), typeof(object), typeof(ClosableTag));

    public object? CloseCommandParameter
    {
        get => GetValue(CloseCommandParameterProperty);
        set => SetValue(CloseCommandParameterProperty, value);
    }
}