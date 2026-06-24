using System.Runtime.InteropServices;
using System.Windows;
using Windows.Win32;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Babel.Services;

public static class WindowPlacement
{
    private const double Offset = 8;

    public static Point Calculate(Size desiredSizeDip, Point anchorPhysical, double dpiScale)
    {
        var anchorPt = new System.Drawing.Point((int)anchorPhysical.X, (int)anchorPhysical.Y);
        var monitor = PInvoke.MonitorFromPoint(anchorPt, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);

        var anchor = new Point(anchorPhysical.X / dpiScale, anchorPhysical.Y / dpiScale);

        var info = new MONITORINFO { cbSize = (uint)Marshal.SizeOf<MONITORINFO>() };
        PInvoke.GetMonitorInfo(monitor, ref info);

        var workArea = new Rect(
            info.rcWork.left / dpiScale, info.rcWork.top / dpiScale,
            (info.rcWork.right - info.rcWork.left) / dpiScale,
            (info.rcWork.bottom - info.rcWork.top) / dpiScale);

        double left = anchor.X;
        double top = anchor.Y + Offset;

        if (top + desiredSizeDip.Height > workArea.Bottom)
            top = anchor.Y - desiredSizeDip.Height - Offset;
        if (left + desiredSizeDip.Width > workArea.Right)
            left = workArea.Right - desiredSizeDip.Width - Offset;
        if (left < workArea.Left)
            left = workArea.Left + Offset;
        if (top < workArea.Top)
            top = workArea.Top + Offset;
        if (top + desiredSizeDip.Height > workArea.Bottom)
            top = workArea.Bottom - desiredSizeDip.Height - Offset;

        return new Point(left, top);
    }
}
