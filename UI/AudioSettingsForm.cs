using MgaWwiseIMImporter.Wave;

namespace MgaWwiseIMImporter.UI;

/// <summary>
/// 再生出力 API とデバイスを選ぶ設定ダイアログ。
/// </summary>
internal sealed class AudioSettingsForm : Form
{
    private readonly DarkDropDownComboBox _apiCombo;
    private readonly DarkDropDownComboBox _deviceCombo;
    private readonly Label _apiLabel;
    private readonly Label _deviceLabel;
    private readonly RoundedButton _okButton;
    private readonly RoundedButton _cancelButton;
    private bool _suppressDeviceReload;

    public AudioOutputSettings SelectedSettings { get; private set; }

    public AudioSettingsForm(AudioOutputSettings current)
    {
        SelectedSettings = current;

        Text = UiStrings.DialogAudioSettingsTitle;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        KeyPreview = true;
        AutoScaleMode = AutoScaleMode.Font;
        Font = new Font("Yu Gothic UI", 9F);
        ClientSize = new Size(440, 230);
        BackColor = UiColors.ForControlBack(UiColors.DialogBodyBack);
        ForeColor = UiColors.DialogFore;
        Padding = new Padding(20);

        const int left = 20;
        const int fieldWidth = 400;
        const int comboHeight = 30;
        const int labelToField = 8;
        const int sectionGap = 22;

        _apiLabel = new Label
        {
            AutoSize = true,
            Location = new Point(left, 20),
            Text = UiStrings.LabelAudioApi,
            ForeColor = UiColors.DialogFore,
            BackColor = BackColor,
        };

        _apiCombo = new DarkDropDownComboBox
        {
            Location = new Point(left, _apiLabel.Location.Y + 18 + labelToField),
            Width = fieldWidth,
            Height = comboHeight,
            Font = Font,
        };
        _apiCombo.ApplyColors();
        _apiCombo.Items.Add(new ApiItem(AudioOutputApi.WaveOut, UiStrings.LabelAudioApiWaveOut));
        _apiCombo.Items.Add(new ApiItem(AudioOutputApi.Wasapi, UiStrings.LabelAudioApiWasapi));
        _apiCombo.Items.Add(new ApiItem(AudioOutputApi.Asio, UiStrings.LabelAudioApiAsio));
        _apiCombo.SelectedIndexChanged += (_, _) =>
        {
            if (!_suppressDeviceReload)
            {
                ReloadDevices(preserveSelection: false);
            }
        };

        var deviceLabelY = _apiCombo.Location.Y + comboHeight + sectionGap;
        _deviceLabel = new Label
        {
            AutoSize = true,
            Location = new Point(left, deviceLabelY),
            Text = UiStrings.LabelAudioDevice,
            ForeColor = UiColors.DialogFore,
            BackColor = BackColor,
        };

        _deviceCombo = new DarkDropDownComboBox
        {
            Location = new Point(left, deviceLabelY + 18 + labelToField),
            Width = fieldWidth,
            Height = comboHeight,
            Font = Font,
        };
        _deviceCombo.ApplyColors();

        const int buttonWidth = 108;
        const int buttonHeight = 34;
        const int buttonGap = 12;
        var buttonY = _deviceCombo.Location.Y + comboHeight + sectionGap + 4;
        var cancelX = left + fieldWidth - buttonWidth;
        var okX = cancelX - buttonGap - buttonWidth;

        _okButton = CreateDialogButton(UiStrings.ButtonAudioSettingsOk, new Point(okX, buttonY), buttonWidth, buttonHeight);
        _okButton.DialogResult = DialogResult.OK;
        _okButton.Click += OkButton_Click;

        _cancelButton = CreateDialogButton(UiStrings.ButtonAudioSettingsCancel, new Point(cancelX, buttonY), buttonWidth, buttonHeight);
        _cancelButton.DialogResult = DialogResult.Cancel;

        ClientSize = new Size(left * 2 + fieldWidth, buttonY + buttonHeight + 20);

        Controls.Add(_apiLabel);
        Controls.Add(_apiCombo);
        Controls.Add(_deviceLabel);
        Controls.Add(_deviceCombo);
        Controls.Add(_okButton);
        Controls.Add(_cancelButton);

        AcceptButton = _okButton;
        CancelButton = _cancelButton;

        SelectApi(current.Api);
        ReloadDevices(preserveSelection: true, preferredDeviceId: current.DeviceId);
    }

    private RoundedButton CreateDialogButton(string text, Point location, int width, int height)
    {
        var button = new RoundedButton
        {
            Text = text,
            Size = new Size(width, height),
            Location = location,
            CornerRadius = 6,
            Font = Font,
            Padding = new Padding(12, 4, 12, 4),
            TabStop = true,
        };
        ApplyDialogButtonColors(button);
        return button;
    }

    private static void ApplyDialogButtonColors(RoundedButton button)
    {
        var fill = UiColors.ForControlBack(UiColors.ProjectBarInputBack);
        var hover = UiColors.ForControlBack(UiColors.TransportHoverBack);
        var pressed = UiColors.ForControlBack(UiColors.TransportPressedBack);
        var border = UiColors.ForControlBack(UiColors.ChromeBorder);

        button.BackColor = fill;
        button.ForeColor = UiColors.ProjectBarInputFore;
        button.HoverBackColor = hover;
        button.PressedBackColor = pressed;
        button.DisabledBackColor = fill;
        button.DisabledForeColor = UiColors.ActionButtonDisabledFore;
        button.BorderColor = border;
        button.HoverBorderColor = border;
        button.PressedBorderColor = border;
        button.DisabledBorderColor = UiColors.ForControlBack(UiColors.ActionButtonDisabledBorder);
        button.BorderSize = 1;
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        if (Owner is { TopMost: true })
        {
            TopMost = true;
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            DialogResult = DialogResult.Cancel;
            Close();
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }

    private void OkButton_Click(object? sender, EventArgs e)
    {
        var api = GetSelectedApi();
        var deviceId = string.Empty;
        if (_deviceCombo.SelectedItem is DeviceItem device)
        {
            deviceId = device.Id;
        }

        SelectedSettings = new AudioOutputSettings(api, deviceId);
    }

    private void SelectApi(AudioOutputApi api)
    {
        _suppressDeviceReload = true;
        try
        {
            for (var i = 0; i < _apiCombo.Items.Count; i++)
            {
                if (_apiCombo.Items[i] is ApiItem item && item.Api == api)
                {
                    _apiCombo.SelectedIndex = i;
                    return;
                }
            }

            _apiCombo.SelectedIndex = 0;
        }
        finally
        {
            _suppressDeviceReload = false;
        }
    }

    private AudioOutputApi GetSelectedApi() =>
        _apiCombo.SelectedItem is ApiItem item ? item.Api : AudioOutputApi.WaveOut;

    private void ReloadDevices(bool preserveSelection, string? preferredDeviceId = null)
    {
        var api = GetSelectedApi();
        var devices = AudioOutputFactory.EnumerateDevices(api);
        var keepId = preferredDeviceId;
        if (preserveSelection
            && keepId is null
            && _deviceCombo.SelectedItem is DeviceItem selected)
        {
            keepId = selected.Id;
        }

        _deviceCombo.BeginUpdate();
        try
        {
            _deviceCombo.Items.Clear();
            foreach (var device in devices)
            {
                _deviceCombo.Items.Add(new DeviceItem(device.Id, device.DisplayName));
            }

            if (_deviceCombo.Items.Count == 0)
            {
                return;
            }

            var index = 0;
            if (!string.IsNullOrEmpty(keepId))
            {
                for (var i = 0; i < _deviceCombo.Items.Count; i++)
                {
                    if (_deviceCombo.Items[i] is DeviceItem item
                        && string.Equals(item.Id, keepId, StringComparison.OrdinalIgnoreCase))
                    {
                        index = i;
                        break;
                    }
                }
            }

            _deviceCombo.SelectedIndex = index;
        }
        finally
        {
            _deviceCombo.EndUpdate();
        }
    }

    private sealed class ApiItem(AudioOutputApi api, string label)
    {
        public AudioOutputApi Api { get; } = api;

        public override string ToString() => label;
    }

    private sealed class DeviceItem(string id, string displayName)
    {
        public string Id { get; } = id;

        public override string ToString() => displayName;
    }
}
