using System.Reflection;
using System.Runtime.InteropServices;
using Updatum;

namespace Shadr;

public partial class Form1 : Form
{
    private readonly NotifyIcon trayIcon;
    private static readonly UpdatumManager AppUpdater = new("aherrick", "Shadr");
    private static string AppVersion => Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";

    // P/Invoke for enabling click-through
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_LAYERED = 0x80000;
    private const int WS_EX_TRANSPARENT = 0x20;

#pragma warning disable SYSLIB1054 // Use LibraryImport - not needed for simple app
    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
#pragma warning restore SYSLIB1054

    public Form1()
    {
        InitializeComponent();

        trayIcon = new NotifyIcon()
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath),
            ContextMenuStrip = new ContextMenuStrip
            {
                Items =
                {
                    new ToolStripMenuItem("25%", null, (s, e) => SetBrightness(0.75)),
                    new ToolStripMenuItem("50%", null, (s, e) => SetBrightness(0.5)),
                    new ToolStripMenuItem("75%", null, (s, e) => SetBrightness(0.25)),
                    new ToolStripMenuItem("100%", null, (s, e) => SetBrightness(0.0)),
                    new ToolStripMenuItem("125%", null, (s, e) => SetBrightness(-0.25)),
                    new ToolStripSeparator(),
                    new ToolStripMenuItem(
                        "Check for Updates",
                        null,
                        async (s, e) => await CheckForUpdatesAsync()
                    ),
                    new ToolStripMenuItem("About", null, (s, e) => OpenAbout()),
                    new ToolStripMenuItem("Exit", null, (s, e) => Application.Exit()),
                },
            },
            Text = $"Shadr v{AppVersion}",
            Visible = true,
        };

        // Configure the form to act as an overlay
        FormBorderStyle = FormBorderStyle.None;
        BackColor = Color.Black;
        Opacity = 0.0; // Start with no overlay
        TopMost = true;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        WindowState = FormWindowState.Maximized;

        // Enable click-through and check for updates on startup
        Load += async (s, e) =>
        {
            EnableClickThrough();
            await CheckForUpdatesAsync(silent: true);
        };
    }

    private void SetBrightness(double opacity)
    {
        if (opacity < 0)
        {
            // Brightness above 100% - use white overlay
            BackColor = Color.White;
            Opacity = Math.Abs(opacity);
        }
        else
        {
            // Normal dimming - use black overlay
            BackColor = Color.Black;
            Opacity = opacity;
        }
    }

    private static async Task CheckForUpdatesAsync(bool silent = false)
    {
        var updateFound = await AppUpdater.CheckForUpdatesAsync();

        if (!updateFound)
        {
            if (!silent)
            {
                MessageBox.Show(
                    "Shadr is up to date!\n\nYou are running the latest version.",
                    "Shadr - No Updates",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            return;
        }

        var result = MessageBox.Show(
            $"A new version of Shadr is available!\n\nCurrent version: v{AppVersion}\nNew version: {AppUpdater.LatestRelease?.Name}\n\nWould you like to download and install the update now?",
            "Shadr - Update Available",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        );

        if (result == DialogResult.Yes)
        {
            var downloadedAsset = await AppUpdater.DownloadUpdateAsync();
            if (downloadedAsset is not null)
            {
                await AppUpdater.InstallUpdateAsync(downloadedAsset);
            }
        }
    }

    private void EnableClickThrough()
    {
        int exStyle = GetWindowLong(this.Handle, GWL_EXSTYLE);
        _ = SetWindowLong(this.Handle, GWL_EXSTYLE, exStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        trayIcon.Dispose(); // Clean up tray icon
        base.OnFormClosing(e);
    }

    private static void OpenAbout()
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://github.com/aherrick/Shadr",
                UseShellExecute = true
            });
        }
        catch
        {
            // Silently fail if browser can't open
        }
    }
}