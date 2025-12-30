using Microsoft.Win32;

namespace Shadr;

internal static class StartupHelper
{
    private const string StartupRegistryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "Shadr";

    public static bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(StartupRegistryKey, false);
        var value = key?.GetValue(AppName) as string;
        return !string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Checks if startup is enabled but pointing to the wrong executable path.
    /// This can happen after an update when the EXE name changes.
    /// </summary>
    public static bool NeedsPathUpdate()
    {
        using var key = Registry.CurrentUser.OpenSubKey(StartupRegistryKey, false);
        var value = key?.GetValue(AppName) as string;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        var currentPath = $"\"{Application.ExecutablePath}\"";
        return !value.Equals(currentPath, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Enables or disables startup and updates the path to the current executable.
    /// </summary>
    public static bool SetEnabled(bool enabled)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(StartupRegistryKey, true);
            if (key == null)
                return false;

            if (enabled)
                key.SetValue(AppName, $"\"{Application.ExecutablePath}\"");
            else
                key.DeleteValue(AppName, false);

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Toggles startup on/off. Returns true if the operation succeeded.
    /// </summary>
    public static bool ToggleStartup() => SetEnabled(!IsEnabled());

    /// <summary>
    /// Updates the startup path to the current executable. Call after an update.
    /// </summary>
    public static bool UpdateStartupPath() => SetEnabled(true);
}