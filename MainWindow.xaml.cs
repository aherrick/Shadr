using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Shadr;

public partial class MainWindow : Window
{
    #region Win Event Helpers

    private const uint EVENT_SYSTEM_FOREGROUND = 0x0003; // Event when foreground window changes
    private const uint WINEVENT_OUTOFCONTEXT = 0x0000;

    private delegate void WinEventDelegate(
        IntPtr hWinEventHook,
        uint eventType,
        IntPtr hwnd,
        int idObject,
        int idChild,
        uint dwEventThread,
        uint dwmsEventTime
    );

    private WinEventDelegate _winEventDelegate;

    [DllImport("user32.dll")]
    private static extern IntPtr SetWinEventHook(
        uint eventMin,
        uint eventMax,
        IntPtr hmodWinEventProc,
        WinEventDelegate lpfnWinEventProc,
        uint idProcess,
        uint idThread,
        uint dwFlags
    );

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    // Constants for extended window styles
    private const int GWL_EXSTYLE = -20;

    private const int WS_EX_LAYERED = 0x00080000;
    private const int WS_EX_TRANSPARENT = 0x00000020;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    #endregion Win Event Helpers

    public MainWindow()
    {
        InitializeComponent();
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Apply the click-through behavior
        var hwnd = new WindowInteropHelper(this).Handle;
        var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED);

        // Set up a global hook to monitor active window changes
        _winEventDelegate = new WinEventDelegate(WinEventCallback);
        SetWinEventHook(
            EVENT_SYSTEM_FOREGROUND,
            EVENT_SYSTEM_FOREGROUND,
            IntPtr.Zero,
            _winEventDelegate,
            0,
            0,
            WINEVENT_OUTOFCONTEXT
        );

        // Set initial overlay size
        UpdateOverlay();
    }

    private void WinEventCallback(
        IntPtr hWinEventHook,
        uint eventType,
        IntPtr hwnd,
        int idObject,
        int idChild,
        uint dwEventThread,
        uint dwmsEventTime
    )
    {
        // Update overlay whenever the foreground window changes
        UpdateOverlay();
    }

    private void UpdateOverlay()
    {
        var primaryScreen = Screen.PrimaryScreen;

        if (IsFullScreenAppRunning(primaryScreen))
        {
            // If a full-screen app is active, cover the entire screen
            Left = primaryScreen.Bounds.Left;
            Top = primaryScreen.Bounds.Top;
            Width = primaryScreen.Bounds.Width;
            Height = primaryScreen.Bounds.Height;
        }
        else
        {
            // Otherwise, exclude the taskbar
            var workingArea = primaryScreen.WorkingArea;
            Left = workingArea.Left;
            Top = workingArea.Top;
            Width = workingArea.Width;
            Height = workingArea.Height;
        }
    }

    private static bool IsFullScreenAppRunning(Screen screen)
    {
        var activeWindow = GetForegroundWindow();
        GetWindowRect(activeWindow, out RECT rect);

        // Check if the active window occupies the entire screen
        return rect.Left <= screen.Bounds.Left
            && rect.Top <= screen.Bounds.Top
            && rect.Right >= screen.Bounds.Right
            && rect.Bottom >= screen.Bounds.Bottom;
    }
}