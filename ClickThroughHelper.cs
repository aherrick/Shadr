using System.Runtime.InteropServices;

namespace Shadr;

/// <summary>
/// Helper class for managing window click-through behavior.
/// </summary>
public static class ClickThroughHelper
{
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_LAYERED = 0x80000;
    private const int WS_EX_TRANSPARENT = 0x20;

#pragma warning disable SYSLIB1054
    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
#pragma warning restore SYSLIB1054

    /// <summary>
    /// Enables click-through on the specified window.
    /// </summary>
    public static void EnableClickThrough(IntPtr windowHandle)
    {
        int exStyle = GetWindowLong(windowHandle, GWL_EXSTYLE);
        _ = SetWindowLong(windowHandle, GWL_EXSTYLE, exStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED);
    }

}
