using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Babel.Services;

public sealed class ClipboardTextInjector
{
    private HWND _targetWindow = HWND.Null;

    public void CaptureForegroundWindow()
    {
        _targetWindow = PInvoke.GetForegroundWindow();
    }

    public Point GetCursorScreenPosition()
    {
        PInvoke.GetCursorPos(out var pt);
        return new Point(pt.X, pt.Y);
    }

    public unsafe void InjectText(string text)
    {
        if (string.IsNullOrEmpty(text) || _targetWindow.IsNull)
            return;

        var savedText = string.Empty;
        try
        {
            if (Clipboard.ContainsText())
                savedText = Clipboard.GetText();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Clipboard save failed: {ex.Message}");
        }

        Clipboard.SetText(text);
        Thread.Sleep(30);

        if (!_targetWindow.IsNull)
        {
            _ = PInvoke.SetForegroundWindow(_targetWindow);
            Thread.Sleep(50);
        }

        var inputSize = Marshal.SizeOf<INPUT>();
        var inputs = stackalloc INPUT[2];

        inputs[0].type = INPUT_TYPE.INPUT_KEYBOARD;
        inputs[0].Anonymous.ki.wVk = VIRTUAL_KEY.VK_CONTROL;
        inputs[1].type = INPUT_TYPE.INPUT_KEYBOARD;
        inputs[1].Anonymous.ki.wVk = VIRTUAL_KEY.VK_V;
        _ = PInvoke.SendInput(2, inputs, inputSize);
        Thread.Sleep(30);

        inputs[0].Anonymous.ki.dwFlags = KEYBD_EVENT_FLAGS.KEYEVENTF_KEYUP;
        inputs[1].Anonymous.ki.dwFlags = KEYBD_EVENT_FLAGS.KEYEVENTF_KEYUP;
        _ = PInvoke.SendInput(2, inputs, inputSize);

        Thread.Sleep(100);

        try
        {
            if (!string.IsNullOrEmpty(savedText))
                Clipboard.SetText(savedText);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Clipboard restore failed: {ex.Message}");
        }
    }
}