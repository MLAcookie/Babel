using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Babel.Services;
using Babel.ViewModels;

namespace Babel.Views;

public partial class TranslationPopup : Window
{
    private readonly TranslationPopupViewModel _vm;
    private readonly ClipboardTextInjector _injector;
    private readonly Action<string?> _onRequestClose;
    private bool _closing;
    private bool _isTranslating;
    private const double MinPopupHeight = 36;
    private const double MaxPopupHeight = 200;
    private const double TextHeightPadding = 16;
    private const double HeightChangeThreshold = 2;
    private double _lastTextHeight;
    private Typeface _typeface = null!;
    private double _dpi;
    private Point _anchorPhysical;

    private static Typeface? s_cachedTypeface;
    private static double s_cachedDpi;

    public TranslationPopup(TranslationPopupViewModel vm, ClipboardTextInjector injector)
    {
        InitializeComponent();
        _vm = vm;
        _injector = injector;
        DataContext = vm;

        _onRequestClose = translatedText =>
        {
            if (translatedText != null && !_closing)
            {
                _closing = true;
                _isTranslating = false;
                Hide();
                _injector.InjectText(translatedText);
                Clipboard.SetText(translatedText);
            }
            else if (translatedText == null)
            {
                _closing = true;
            }

            Close();
        };

        vm.RequestClose += _onRequestClose;

        Closing += (_, _) => _closing = true;
        Closed += (_, _) =>
        {
            _vm.Cancel();
            _vm.RequestClose -= _onRequestClose;
        };
    }

    internal void CloseIfOpen()
    {
        if (_closing) return;
        _closing = true;
        Close();
    }

    public void SetAnchor(Point screenPointPhysical)
    {
        _anchorPhysical = screenPointPhysical;
        Left = -32000;
        Top = -32000;
    }

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        if (s_cachedTypeface != null)
        {
            _typeface = s_cachedTypeface;
            _dpi = s_cachedDpi;
        }
        else
        {
            _typeface = new Typeface(InputBox.FontFamily, InputBox.FontStyle, InputBox.FontWeight,
                InputBox.FontStretch);
            _dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;
            s_cachedTypeface = _typeface;
            s_cachedDpi = _dpi;
        }

        Deactivated += Window_Deactivated;
        InputBox.Focus();
        InputBox.SelectAll();

        var size = new Size(ActualWidth, ActualHeight);
        var pos = WindowPlacement.Calculate(size, _anchorPhysical, _dpi);
        Left = pos.X;
        Top = pos.Y;
    }

    private void Window_Deactivated(object? sender, EventArgs e)
    {
        if (!_isTranslating) CloseIfOpen();
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            _vm.CloseCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.Tab && _vm.SwitchLanguageCommand.CanExecute(null))
        {
            _vm.SwitchLanguageCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.Enter && _vm.TranslateCommand.CanExecute(null))
        {
            e.Handled = true;
            _isTranslating = true;
            _ = _vm.TranslateCommand.ExecuteAsync(null);
        }
    }

    private void InputBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (_typeface == null) return;
        var textHeight = MeasureTextHeight();
        if (Math.Abs(textHeight - _lastTextHeight) > HeightChangeThreshold)
        {
            _lastTextHeight = textHeight;
            var newHeight = Math.Clamp(textHeight + TextHeightPadding, MinPopupHeight, MaxPopupHeight);
            if (Math.Abs(InputBox.Height - newHeight) > HeightChangeThreshold)
            {
                InputBox.Height = newHeight;
            }
        }
    }

    private double MeasureTextHeight()
    {
        var text = InputBox.Text;
        if (string.IsNullOrEmpty(text)) return 0;

        var formatted = new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            _typeface,
            InputBox.FontSize,
            InputBox.Foreground,
            _dpi);

        formatted.MaxTextWidth = InputBox.ActualWidth - InputBox.Padding.Left - InputBox.Padding.Right - 4;
        return formatted.Height;
    }
}