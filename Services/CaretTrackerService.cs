using System.Diagnostics;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using DRect = System.Drawing.Rectangle;

namespace Babel.Services;

public sealed class CaretTrackerService : IDisposable
{
    private readonly UIA3Automation _uia3;

    public CaretTrackerService()
    {
        _uia3 = new UIA3Automation();
    }

    public System.Windows.Point? GetCaretPosition()
    {
        var element = _uia3.FocusedElement();
        if (element == null) return null;

        var rect = TryGetViaText2(element);
        if (rect != null) return new System.Windows.Point(rect.Value.X, rect.Value.Y);

        return null;
    }

    private static DRect? TryGetViaText2(AutomationElement element)
    {
        var result = TryText2OnElement(element);
        if (result != null) return result;

        foreach (var descendant in EnumerateDescendants(element))
        {
            result = TryText2OnElement(descendant);
            if (result != null) return result;
        }

        return null;
    }

    private static DRect? TryText2OnElement(AutomationElement element)
    {
        try
        {
            var tp2 = element.Patterns.Text2;
            if (tp2 == null || !tp2.IsSupported) return null;

            var caretRange = tp2.Pattern.GetCaretRange(out _);
            if (caretRange == null) return null;

            var rects = caretRange.GetBoundingRectangles();
            if (rects.Length == 0) return null;

            return rects[0];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"CaretTracker Text2 failed: {ex.Message}");
            return null;
        }
    }

    private static IEnumerable<AutomationElement> EnumerateDescendants(AutomationElement element)
    {
        AutomationElement[] descendants;
        try
        {
            descendants = element.FindAllDescendants();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"CaretTracker FindAllDescendants failed: {ex.Message}");
            yield break;
        }

        foreach (var descendant in descendants)
        {
            yield return descendant;
        }
    }

    public void Dispose()
    {
        _uia3.Dispose();
    }
}
