namespace Shadr;

public partial class ProgressForm : Form
{
    private readonly Label _statusLabel;
    private readonly ProgressBar _progressBar;

    public ProgressForm()
    {
        Text = "Downloading Update...";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ControlBox = false;
        StartPosition = FormStartPosition.CenterScreen;

        // High DPI support
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(450, 120);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            RowCount = 2,
            ColumnCount = 1
        };

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        _statusLabel = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomLeft,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 10)
        };

        _progressBar = new ProgressBar
        {
            Dock = DockStyle.Top,
            Height = 25
        };

        layout.Controls.Add(_statusLabel, 0, 0);
        layout.Controls.Add(_progressBar, 0, 1);
        Controls.Add(layout);
    }

    public void UpdateProgress(double percentage, string message)
    {
        if (InvokeRequired)
        {
            Invoke(() => UpdateProgress(percentage, message));
            return;
        }

        _progressBar.Value = Math.Clamp((int)percentage, 0, 100);
        _statusLabel.Text = message;
    }
}
