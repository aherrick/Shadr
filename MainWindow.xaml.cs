using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Shadr;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // Apply the click-through behavior
        var hwnd = new WindowInteropHelper(this).Handle;
        var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED);

        // Get the working area (usable area excluding the taskbar)
        var primaryScreen = Screen.PrimaryScreen;

        // If a full-screen app is active, cover the entire screen
        Left = primaryScreen.Bounds.Left;
        Top = primaryScreen.Bounds.Top;
        Width = primaryScreen.Bounds.Width;
        Height = primaryScreen.Bounds.Height;
    }

    // Constants for extended window styles
    private const int GWL_EXSTYLE = -20;

    private const int WS_EX_LAYERED = 0x00080000;
    private const int WS_EX_TRANSPARENT = 0x00000020;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
}