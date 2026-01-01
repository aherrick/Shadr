using System.Runtime.InteropServices;

namespace Shadr;

/// <summary>
/// Helper class for managing screen brightness using gamma ramp adjustments
/// and overlay techniques for extreme dimming.
/// </summary>
#pragma warning disable CA2101, SYSLIB1054 // P/Invoke marshalling - using DllImport for compatibility
public class BrightnessHelper : IDisposable
{
    #region P/Invoke for Gamma Ramp

    [DllImport("gdi32.dll")]
    private static extern bool SetDeviceGammaRamp(IntPtr hDC, ref GammaRamp lpRamp);

    [DllImport("gdi32.dll")]
    private static extern bool GetDeviceGammaRamp(IntPtr hDC, ref GammaRamp lpRamp);

    [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr CreateDC(
        string lpszDriver,
        string? lpszDevice,
        string? lpszOutput,
        IntPtr lpInitData
    );

    [DllImport("gdi32.dll")]
    private static extern bool DeleteDC(IntPtr hdc);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct GammaRamp
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public ushort[] Red;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public ushort[] Green;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public ushort[] Blue;
    }

    #endregion P/Invoke for Gamma Ramp
#pragma warning restore CA2101, SYSLIB1054

    private GammaRamp _originalGamma;
    private bool _originalGammaSaved;
    private readonly Form _overlayForm;
    private bool _disposed;

    /// <summary>
    /// The threshold below which we use overlay instead of gamma for consistent dimming.
    /// </summary>
    private const int OverlayThreshold = 100;

    /// <summary>
    /// Creates a new BrightnessHelper instance.
    /// </summary>
    /// <param name="overlayForm">The form to use for overlay-based dimming.</param>
    public BrightnessHelper(Form overlayForm)
    {
        _overlayForm = overlayForm;
        SaveOriginalGamma();
    }

    /// <summary>
    /// Sets the screen brightness to the specified percentage.
    /// </summary>
    /// <param name="percentage">Brightness percentage (25-150). 100 = normal.</param>
    public void SetBrightness(int percentage)
    {
        // Clamp to valid range
        percentage = Math.Clamp(percentage, 25, 150);

        if (percentage < OverlayThreshold)
        {
            // Use overlay for extreme dimming (better black levels)
            ResetGamma();
            ApplyOverlay(percentage);
        }
        else if (percentage == 100)
        {
            // Normal - reset everything
            ResetGamma();
            HideOverlay();
        }
        else
        {
            // Use gamma ramp for moderate adjustments
            HideOverlay();
            ApplyGamma(percentage);
        }
    }

    /// <summary>
    /// Resets brightness to normal (100%).
    /// </summary>
    public void Reset()
    {
        ResetGamma();
        HideOverlay();
    }

    #region Gamma Ramp Methods

    /// <summary>
    /// Saves the original gamma ramp so it can be restored later.
    /// </summary>
    private void SaveOriginalGamma()
    {
        IntPtr hdc = CreateDC("DISPLAY", null, null, IntPtr.Zero);
        if (hdc == IntPtr.Zero)
            return;

        try
        {
            _originalGamma = new GammaRamp
            {
                Red = new ushort[256],
                Green = new ushort[256],
                Blue = new ushort[256],
            };

            if (GetDeviceGammaRamp(hdc, ref _originalGamma))
            {
                _originalGammaSaved = true;
            }
        }
        finally
        {
            DeleteDC(hdc);
        }
    }

    /// <summary>
    /// Applies a gamma ramp for the specified brightness percentage.
    /// </summary>
    /// <param name="percentage">Brightness percentage (>100-150).
    /// Values above 100 increase brightness using gamma.
    /// </param>
    private static void ApplyGamma(int percentage)
    {
        IntPtr hdc = CreateDC("DISPLAY", null, null, IntPtr.Zero);
        if (hdc == IntPtr.Zero)
            return;

        try
        {
            var ramp = CreateGammaRamp(percentage / 100.0);
            SetDeviceGammaRamp(hdc, ref ramp);
        }
        finally
        {
            DeleteDC(hdc);
        }
    }

    /// <summary>
    /// Creates a gamma ramp for the specified brightness multiplier.
    /// </summary>
    /// <param name="brightness">Brightness multiplier (0.5 = 50%, 1.0 = 100%, 1.5 = 150%).</param>
    private static GammaRamp CreateGammaRamp(double brightness)
    {
        var ramp = new GammaRamp
        {
            Red = new ushort[256],
            Green = new ushort[256],
            Blue = new ushort[256],
        };

        for (int i = 0; i < 256; i++)
        {
            // Calculate adjusted value with brightness multiplier
            int value = (int)(i * 256 * brightness);

            // Clamp to valid range (0-65535)
            value = Math.Clamp(value, 0, 65535);

            ramp.Red[i] = (ushort)value;
            ramp.Green[i] = (ushort)value;
            ramp.Blue[i] = (ushort)value;
        }

        return ramp;
    }

    /// <summary>
    /// Resets the gamma ramp to the original values.
    /// </summary>
    private void ResetGamma()
    {
        if (!_originalGammaSaved)
            return;

        IntPtr hdc = CreateDC("DISPLAY", null, null, IntPtr.Zero);
        if (hdc == IntPtr.Zero)
            return;

        try
        {
            SetDeviceGammaRamp(hdc, ref _originalGamma);
        }
        finally
        {
            DeleteDC(hdc);
        }
    }

    #endregion Gamma Ramp Methods

    #region Overlay Methods

    /// <summary>
    /// Applies a black overlay for extreme dimming.
    /// </summary>
    /// <param name="percentage">Brightness percentage (lower = darker).</param>
    private void ApplyOverlay(int percentage)
    {
        // Convert percentage to opacity (50% brightness = 0.5 opacity black overlay)
        // 0% brightness = full black overlay (opacity 1.0)
        double opacity = 1.0 - (percentage / 100.0);
        opacity = Math.Clamp(opacity, 0.0, 1.0);

        _overlayForm.BackColor = Color.Black;
        _overlayForm.Opacity = opacity;
    }

    /// <summary>
    /// Hides the overlay form.
    /// </summary>
    private void HideOverlay()
    {
        _overlayForm.Opacity = 0.0;
    }

    #endregion Overlay Methods

    #region IDisposable

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        // Always reset gamma on dispose to restore normal display
        ResetGamma();

        _disposed = true;
    }

    ~BrightnessHelper()
    {
        Dispose(false);
    }

    #endregion IDisposable
}