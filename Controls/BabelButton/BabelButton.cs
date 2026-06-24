using System.Windows;
using System.Windows.Controls;

namespace Babel.Controls;

public class BabelButton : Button
{
    static BabelButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(BabelButton),
            new FrameworkPropertyMetadata(typeof(BabelButton)));
    }
}
