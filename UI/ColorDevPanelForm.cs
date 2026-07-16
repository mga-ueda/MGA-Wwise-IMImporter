namespace MgaWwiseIMImporter.UI;

/// <summary>
/// 開発者向け色調整パネル。開いたままメイン画面を見ながら変更できる。
/// </summary>
internal sealed class ColorDevPanelForm : Form
{
    private readonly Dictionary<string, Panel> _swatches = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Label> _hexLabels = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, NumericUpDown> _alphaInputs = new(StringComparer.OrdinalIgnoreCase);
    private bool _suppressAlphaEvents;

    public event EventHandler? ColorsChanged;

    public ColorDevPanelForm()
    {
        Text = "色調整（開発者）";
        FormBorderStyle = FormBorderStyle.SizableToolWindow;
        StartPosition = FormStartPosition.Manual;
        MinimumSize = new Size(420, 320);
        Size = new Size(480, 640);
        ShowInTaskbar = false;
        KeyPreview = true;
        BackColor = Color.FromArgb(40, 40, 42);
        ForeColor = Color.FromArgb(230, 230, 230);
        Font = new Font("Yu Gothic UI", 9F);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(8),
        };
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));

        var scroll = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = Color.FromArgb(32, 32, 34),
        };

        var list = new TableLayoutPanel
        {
            ColumnCount = 4,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Top,
            Padding = new Padding(4),
        };
        list.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150f));
        list.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48f));
        list.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90f));
        list.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

        for (var i = 0; i < UiColors.Entries.Count; i++)
        {
            var entry = UiColors.Entries[i];
            list.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
            list.RowCount = i + 1;

            var nameLabel = new Label
            {
                Text = entry.Label,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true,
            };

            var swatch = new Panel
            {
                Width = 40,
                Height = 22,
                Margin = new Padding(4, 4, 4, 4),
                Cursor = Cursors.Hand,
                BorderStyle = BorderStyle.FixedSingle,
                Tag = entry.Key,
            };
            swatch.Click += (_, _) => PickColor(entry.Key);
            _swatches[entry.Key] = swatch;

            var alpha = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 255,
                Width = 70,
                Margin = new Padding(4, 3, 4, 3),
                Tag = entry.Key,
            };
            alpha.ValueChanged += Alpha_ValueChanged;
            _alphaInputs[entry.Key] = alpha;

            var hex = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Consolas", 9F),
                ForeColor = Color.FromArgb(180, 180, 180),
            };
            _hexLabels[entry.Key] = hex;

            list.Controls.Add(nameLabel, 0, i);
            list.Controls.Add(swatch, 1, i);
            list.Controls.Add(alpha, 2, i);
            list.Controls.Add(hex, 3, i);
        }

        scroll.Controls.Add(list);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Padding = new Padding(0, 4, 0, 0),
        };

        var closeButton = new Button { Text = "閉じる", AutoSize = true, FlatStyle = FlatStyle.System };
        closeButton.Click += (_, _) => Close();

        var resetButton = new Button { Text = "既定に戻す", AutoSize = true, FlatStyle = FlatStyle.System };
        resetButton.Click += (_, _) =>
        {
            UiColors.ResetToDefaults();
            UiColors.SaveToIni();
            RefreshRows();
            ColorsChanged?.Invoke(this, EventArgs.Empty);
        };

        var saveButton = new Button { Text = "INI に保存", AutoSize = true, FlatStyle = FlatStyle.System };
        saveButton.Click += (_, _) =>
        {
            UiColors.SaveToIni();
            Text = "色調整（開発者） — 保存済み";
        };

        buttons.Controls.Add(closeButton);
        buttons.Controls.Add(resetButton);
        buttons.Controls.Add(saveButton);

        root.Controls.Add(scroll, 0, 0);
        root.Controls.Add(buttons, 0, 1);
        Controls.Add(root);

        RefreshRows();
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape)
        {
            Close();
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    public void RefreshRows()
    {
        _suppressAlphaEvents = true;
        try
        {
            foreach (var entry in UiColors.Entries)
            {
                var color = entry.Get();
                if (_swatches.TryGetValue(entry.Key, out var swatch))
                {
                    swatch.BackColor = Color.FromArgb(255, color.R, color.G, color.B);
                }

                if (_hexLabels.TryGetValue(entry.Key, out var hex))
                {
                    hex.Text = UiColors.FormatColor(color);
                }

                if (_alphaInputs.TryGetValue(entry.Key, out var alpha))
                {
                    alpha.Value = color.A;
                }
            }
        }
        finally
        {
            _suppressAlphaEvents = false;
        }
    }

    private void Alpha_ValueChanged(object? sender, EventArgs e)
    {
        if (_suppressAlphaEvents || sender is not NumericUpDown alpha || alpha.Tag is not string key)
        {
            return;
        }

        var entry = FindEntry(key);
        if (entry is null)
        {
            return;
        }

        var current = entry.Get();
        var next = Color.FromArgb((int)alpha.Value, current.R, current.G, current.B);
        ApplyColor(entry, next, saveImmediately: true);
    }

    private void PickColor(string key)
    {
        var entry = FindEntry(key);
        if (entry is null)
        {
            return;
        }

        var current = entry.Get();
        using var dialog = new ColorDialog
        {
            AllowFullOpen = true,
            AnyColor = true,
            FullOpen = true,
            Color = Color.FromArgb(current.R, current.G, current.B),
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var next = Color.FromArgb(current.A, dialog.Color.R, dialog.Color.G, dialog.Color.B);
        ApplyColor(entry, next, saveImmediately: true);
    }

    private void ApplyColor(UiColorEntry entry, Color color, bool saveImmediately)
    {
        entry.Set(color);
        RefreshRows();
        if (saveImmediately)
        {
            UiColors.SaveToIni();
        }

        ColorsChanged?.Invoke(this, EventArgs.Empty);
    }

    private static UiColorEntry? FindEntry(string key) =>
        UiColors.Entries.FirstOrDefault(e => string.Equals(e.Key, key, StringComparison.OrdinalIgnoreCase));
}
