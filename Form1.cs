using System.Reflection;
using Updatum;

namespace Shadr;

public partial class Form1 : Form
{
    private readonly NotifyIcon trayIcon;
    private readonly BrightnessHelper _brightnessHelper;
    private readonly ToolStripMenuItem[] _brightnessMenuItems;
    private readonly int[] _levels = [25, 50, 75, 100, 125, 150];
    private const int DefaultBrightness = 100;
    private int _currentBrightness = DefaultBrightness;

    private static readonly UpdatumManager AppUpdater = new("aherrick", "Shadr");

    private static string AppVersion =>
        Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";

    public Form1()
    {
        InitializeComponent();

        // Initialize brightness helper with this form as the overlay
        _brightnessHelper = new BrightnessHelper(this);

        // Create brightness menu items with checkmarks (loop-based)
        _brightnessMenuItems =
        [
            .. _levels.Select(level => new ToolStripMenuItem(
                $"{level}%",
                null,
                (s, e) => SetBrightnessWithCheck(level)
            )),
        ];

        trayIcon = new NotifyIcon()
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath),
            ContextMenuStrip = new ContextMenuStrip(),
            Text = $"Shadr v{AppVersion}",
            Visible = true,
        };

        // Build context menu
        trayIcon.ContextMenuStrip.Items.AddRange(_brightnessMenuItems);
        trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
        var startupMenuItem = new ToolStripMenuItem(
            "Run at Startup",
            null,
            (s, e) =>
            {
                /// if toggling startup setting succeeded, update checkmark
                if (StartupHelper.ToggleStartup())
                {
                    (s as ToolStripMenuItem).Checked = StartupHelper.IsEnabled();
                }
                else
                {
                    MessageBox.Show(
                        $"Failed to update startup setting.",
                        "Shadr - Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        )
        {
            Checked = StartupHelper.IsEnabled(),
        };
        trayIcon.ContextMenuStrip.Items.Add(startupMenuItem);
        trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
        trayIcon.ContextMenuStrip.Items.Add(
            new ToolStripMenuItem(
                "Check for Updates",
                null,
                async (s, e) => await CheckForUpdatesAsync()
            )
        );
        trayIcon.ContextMenuStrip.Items.Add(
            new ToolStripMenuItem("About", null, (s, e) => OpenAbout())
        );
        trayIcon.ContextMenuStrip.Items.Add(
            new ToolStripMenuItem("Exit", null, (s, e) => Application.Exit())
        );

        // Double-click tray icon to reset to 100%
        trayIcon.DoubleClick += (s, e) => SetBrightnessWithCheck(DefaultBrightness);

        // Set initial brightness and checkmark
        SetBrightnessWithCheck(DefaultBrightness);

        // Configure the form to act as an overlay (used for extreme dimming only)
        FormBorderStyle = FormBorderStyle.None;
        BackColor = Color.Black;
        Opacity = 0.0; // Start with no overlay
        TopMost = true;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        WindowState = FormWindowState.Maximized;

        // Check for updates on startup and enable click-through
        Load += async (s, e) =>
        {
            ClickThroughHelper.EnableClickThrough(Handle);
            await CheckForUpdatesAsync(silent: true);
        };
    }

    private void SetBrightnessWithCheck(int percentage)
    {
        _brightnessHelper.SetBrightness(percentage);
        _currentBrightness = percentage;

        // Update checkmarks
        for (int i = 0; i < _brightnessMenuItems.Length; i++)
        {
            _brightnessMenuItems[i].Checked = _levels[i] == percentage;
        }
    }

    private async Task CheckForUpdatesAsync(bool silent = false)
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
                    // Reset brightness before update kills the process
                    _brightnessHelper.Dispose();
                    await AppUpdater.InstallUpdateAsync(downloadedAsset);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Update error: {ex.Message}\n\n{ex.StackTrace}",
                "Shadr - Update Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
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
            System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://github.com/aherrick/Shadr",
                    UseShellExecute = true,
                }
            );
        }
        catch
        {
            // Silently fail if browser can't open
        }
    }
}