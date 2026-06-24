using System.Windows;
using Babel.ViewModels;
using Babel.Views;

namespace Babel.Services;

public sealed class PopupCoordinator
{
    private readonly ClipboardTextInjector _injector;
    private readonly CaretTrackerService _caretTracker;
    private TranslationPopup? _currentPopup;

    public PopupCoordinator(ClipboardTextInjector injector, CaretTrackerService caretTracker)
    {
        _injector = injector;
        _caretTracker = caretTracker;
    }

    public void ShowPopup(TranslationPopupViewModel vm)
    {
        if (_currentPopup != null)
        {
            _currentPopup.CloseIfOpen();
            _currentPopup = null;
        }

        _injector.CaptureForegroundWindow();

        var pos = _caretTracker.GetCaretPosition()
                  ?? _injector.GetCursorScreenPosition();

        // Tech debt: PopupCoordinator directly constructs TranslationPopup (Views layer).
        // WPF Windows with parameterized constructors don't fit cleanly into DI containers.
        // If popup creation ever needs more indirection, introduce an IPopupFactory.
        var popup = new TranslationPopup(vm, _injector);
        popup.SetAnchor(pos);
        popup.Show();
        popup.Activate();

        popup.Closed += (_, _) =>
        {
            if (_currentPopup == popup)
                _currentPopup = null;
        };
        _currentPopup = popup;
    }

    public void CloseCurrentPopup()
    {
        if (_currentPopup != null)
        {
            _currentPopup.CloseIfOpen();
            _currentPopup = null;
        }
    }
}