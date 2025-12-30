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

    public static bool ToggleStartup() => SetEnabled(!IsEnabled());
}