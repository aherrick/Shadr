using System.Reflection;
using System.Runtime.InteropServices;
using Updatum;

namespace Shadr;

public partial class Form1 : Form
{
    private readonly NotifyIcon trayIcon;
    private readonly BrightnessHelper _brightnessHelper;
    
    private static readonly UpdatumManager AppUpdater = new("aherrick", "Shadr")
    {
        FetchOnlyLatestRelease = true,
        InstallUpdateSingleFileExecutableName = "Shadr",
    };
    
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

        // Initialize brightness helper with this form as the overlay
        _brightnessHelper = new BrightnessHelper(this);

        trayIcon = new NotifyIcon()
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath),
            ContextMenuStrip = new ContextMenuStrip
            {
                Items =
                {
                    new ToolStripMenuItem("0%", null, (s, e) => _brightnessHelper.SetBrightness(0)),
                    new ToolStripMenuItem("25%", null, (s, e) => _brightnessHelper.SetBrightness(25)),
                    new ToolStripMenuItem("50%", null, (s, e) => _brightnessHelper.SetBrightness(50)),
                    new ToolStripMenuItem("75%", null, (s, e) => _brightnessHelper.SetBrightness(75)),
                    new ToolStripMenuItem("100%", null, (s, e) => _brightnessHelper.SetBrightness(100)),
                    new ToolStripMenuItem("125%", null, (s, e) => _brightnessHelper.SetBrightness(125)),
                    new ToolStripMenuItem("150%", null, (s, e) => _brightnessHelper.SetBrightness(150)),
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

        // Configure the form to act as an overlay (used for extreme dimming only)
        FormBorderStyle = FormBorderStyle.None;
        BackColor = Color.Black;
        Opacity = 0.0; // Start with no overlay
        TopMost = true;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        WindowState = FormWindowState.Maximized;

        // Click anywhere on the overlay to reset to 50% (escape from 0% darkness)
        MouseDown += (s, e) =>
        {
            if (_brightnessHelper.GetCurrentBrightness() == 0)
            {
                _brightnessHelper.SetBrightness(50);
            }
        };

        // Enable click-through and check for updates on startup
        Load += async (s, e) =>
        {
            EnableClickThrough();
            await CheckForUpdatesAsync(silent: true);
        };
    }

    private static async Task CheckForUpdatesAsync(bool silent = false)
    {
        try
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

            var release = AppUpdater.LatestRelease!;
            var result = MessageBox.Show(
                $"A new version of Shadr is available!\n\nCurrent version: v{AppVersion}\nNew version: {release.TagName}\n\nWould you like to download and install the update now?",
                "Shadr - Update Available",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                var downloadedAsset = await AppUpdater.DownloadUpdateAsync();
                if (downloadedAsset != null)
                {
                    await AppUpdater.InstallUpdateAsync(downloadedAsset);
                }
            }
        }
        catch
        {
            // Silently fail - updates are not critical
        }
    }

    private void EnableClickThrough()
    {
        int exStyle = GetWindowLong(this.Handle, GWL_EXSTYLE);
        _ = SetWindowLong(this.Handle, GWL_EXSTYLE, exStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _brightnessHelper.Dispose(); // Reset gamma and clean up
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