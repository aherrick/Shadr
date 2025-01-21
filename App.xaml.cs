using System.Windows;

namespace Shadr;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private NotifyIcon _trayIcon;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var appNameVersion =
            Environment.GetEnvironmentVariable("APP_NAME_VERSION") ?? "Unknown version";

        // Create the tray icon
        _trayIcon = new NotifyIcon
        {
            Icon = new Icon("icon.ico"),
            Visible = true,
            Text = appNameVersion,
            ContextMenuStrip = CreateContextMenu()
        };

        // Ensure the tray icon is disposed when the application exits
        Exit += (sender, args) => _trayIcon.Dispose();
    }

    private ContextMenuStrip CreateContextMenu()
    {
        var menu = new ContextMenuStrip();

        menu.Items.Add("0%", null, (s, e) => SetBrightness(0));
        menu.Items.Add("25%", null, (s, e) => SetBrightness(25));
        menu.Items.Add("50%", null, (s, e) => SetBrightness(50));
        menu.Items.Add("75%", null, (s, e) => SetBrightness(75));
        menu.Items.Add("100%", null, (s, e) => SetBrightness(100));
        menu.Items.Add("Exit", null, (s, e) => Shutdown());

        return menu;
    }

    private static void SetBrightness(int brightness)
    {
        Current.MainWindow.Opacity = 1 - (brightness / 100.0);
    }
}