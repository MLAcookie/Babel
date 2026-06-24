using System.Windows.Interop;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Babel.Services;

public class GlobalHotkeyService : IDisposable
{
    private const int HOTKEY_ID = 9001;
    private const int WM_HOTKEY = 0x0312;

    private HwndSource? _source;
    private bool _registered;

    public event Action? HotkeyPressed;

    public void Initialize()
    {
        var parameters = new HwndSourceParameters("Babel Hotkey")
        {
            Width = 0,
            Height = 0,
            WindowStyle = 0,
        };
        _source = new HwndSource(parameters);
        _source.AddHook(WndProc);

        _registered = PInvoke.RegisterHotKey(
            new HWND(_source.Handle),
            HOTKEY_ID,
            HOT_KEY_MODIFIERS.MOD_CONTROL | HOT_KEY_MODIFIERS.MOD_SHIFT | HOT_KEY_MODIFIERS.MOD_NOREPEAT,
            (uint)VIRTUAL_KEY.VK_OEM_3);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
        {
            HotkeyPressed?.Invoke();
            handled = true;
        }

        return IntPtr.Zero;
    }

    public void Dispose()
    {
        if (_registered && _source != null)
        {
            PInvoke.UnregisterHotKey(new HWND(_source.Handle), HOTKEY_ID);
            _registered = false;
        }

        if (_source != null)
        {
            _source.RemoveHook(WndProc);
            _source.Dispose();
            _source = null;
        }
    }
}