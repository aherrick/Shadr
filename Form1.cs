using System.Runtime.InteropServices;
using Updatum;

namespace Shadr;

public partial class Form1 : Form
{
    private readonly NotifyIcon trayIcon;
    private static readonly UpdatumManager AppUpdater = new("aherrick", "Shadr");

    public Form1()
    {
        InitializeComponent();

        trayIcon = new NotifyIcon()
        {
            Icon = new Icon("icon.ico"),
            ContextMenuStrip = new ContextMenuStrip
            {
                Items =
                {
                    new ToolStripMenuItem("25%", null, (s, e) => SetBrightness(0.75)),
                    new ToolStripMenuItem("50%", null, (s, e) => SetBrightness(0.5)),
                    new ToolStripMenuItem("75%", null, (s, e) => SetBrightness(0.25)),
                    new ToolStripMenuItem("100%", null, (s, e) => SetBrightness(0.0)),
                    new ToolStripSeparator(),
                    new ToolStripMenuItem("Check for Updates", null, async (s, e) => await CheckForUpdatesAsync()),
                    new ToolStripMenuItem("Exit", null, (s, e) => Application.Exit())
                },
                Text = "[[app-name-version]]",
            },
            Visible = true
        };

        // Configure the form to act as an overlay
        FormBorderStyle = FormBorderStyle.None;
        BackColor = Color.Black;
        Opacity = 0.0; // Start with no overlay
        TopMost = true;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        WindowState = FormWindowState.Maximized;

        // Enable click-through
        Load += (s, e) => EnableClickThrough();
        
        // Check for updates on startup (async, non-blocking)
        Load += async (s, e) => await CheckForUpdatesOnStartupAsync();
    }

    private void SetBrightness(double opacity)
    {
        Opacity = opacity; // Adjust overlay brightness
    }

    private async Task CheckForUpdatesOnStartupAsync() => await CheckForUpdatesAsync(silent: true);

    private async Task CheckForUpdatesAsync(bool silent = false)
    {
        var updateFound = await AppUpdater.CheckForUpdatesAsync();

        if (!updateFound)
        {
            if (!silent)
            {
                MessageBox.Show("You are running the latest version!", "No Updates", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            return;
        }

        var result = MessageBox.Show(
            $"A new version is available: {AppUpdater.LatestRelease?.Name}\n\nWould you like to download and install it now?",
            "Update Available",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            var downloadedAsset = await AppUpdater.DownloadUpdateAsync();
            await AppUpdater.InstallUpdateAsync(downloadedAsset);
        }
    }

    private void EnableClickThrough()
    {
        int exStyle = GetWindowLong(this.Handle, GWL_EXSTYLE);
        SetWindowLong(this.Handle, GWL_EXSTYLE, exStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        trayIcon.Dispose(); // Clean up tray icon
        base.OnFormClosing(e);
    }

    // P/Invoke for enabling click-through
    private const int GWL_EXSTYLE = -20;

    private const int WS_EX_LAYERED = 0x80000;
    private const int WS_EX_TRANSPARENT = 0x20;

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
}