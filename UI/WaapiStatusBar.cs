namespace MgaWwiseIMImporter.UI;

/// <summary>
/// エディタ下の WAAPI / Wwise 接続ステータス表示。
/// </summary>
internal sealed class WaapiStatusBar : Panel
{
    private readonly Label _titleLabel;
    private readonly Label _detailLabel;
    private readonly FlatOptionCheckBox _keepTargetCheckBox;
    private readonly DarkToolTip _toolTip = new();
    private readonly Font _badgeFont = new("Yu Gothic UI", 9F, FontStyle.Bold);

    private string _badgeText = "—";
    private Color _badgeBack = Color.Transparent;
    private Color _badgeFore = Color.Gray;
    private bool _badgeFilled;
    private Rectangle _badgeFillBounds;
    private Rectangle _badgeTextBounds;
    private bool _selectionMissing;
    private bool _keepTargetWarning;
    private bool _suppressKeepTargetEvents;

    public WaapiStatusBar()
    {
        Height = 30;
        Dock = DockStyle.Bottom;
        Padding = new Padding(10, 0, 10, 0);
        TabStop = false;
        DoubleBuffered = true;

        _titleLabel = new Label
        {
            AutoSize = true,
            Text = "WAAPI",
            Font = new Font("Yu Gothic UI", 9F, FontStyle.Bold),
            Location = new Point(10, 7),
            TabStop = false,
        };

        _detailLabel = new Label
        {
            AutoEllipsis = true,
            Text = string.Empty,
            Font = new Font("Yu Gothic UI", 9F),
            Location = new Point(100, 7),
            TabStop = false,
        };

        _keepTargetCheckBox = new FlatOptionCheckBox
        {
            AutoSize = true,
            Font = new Font("Yu Gothic UI", 9F),
            Text = "Keep Target",
            TabStop = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
        };
        _keepTargetCheckBox.CheckedChanged += (_, _) =>
        {
            if (_suppressKeepTargetEvents)
            {
                return;
            }

            KeepTargetChanged?.Invoke(this, EventArgs.Empty);
        };

        Controls.Add(_keepTargetCheckBox);
        Controls.Add(_detailLabel);
        Controls.Add(_titleLabel);
        Resize += (_, _) => LayoutLabels();
        Paint += OnPaint;
        ApplyColors();
        ApplyToolTips();
        SetPending();
    }

    /// <summary>Keep Target チェックの変更。</summary>
    public event EventHandler? KeepTargetChanged;

    public bool KeepTargetChecked
    {
        get => _keepTargetCheckBox.Checked;
        set
        {
            if (_keepTargetCheckBox.Checked == value)
            {
                return;
            }

            _suppressKeepTargetEvents = true;
            try
            {
                _keepTargetCheckBox.Checked = value;
            }
            finally
            {
                _suppressKeepTargetEvents = false;
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _badgeFont.Dispose();
            _toolTip.Dispose();
        }

        base.Dispose(disposing);
    }

    public void ApplyColors()
    {
        BackColor = UiColors.ForControlBack(UiColors.StatusBarBack);
        _titleLabel.ForeColor = UiColors.StatusBarTitleFore;
        _titleLabel.BackColor = BackColor;
        _detailLabel.BackColor = BackColor;
        _keepTargetCheckBox.BackColor = BackColor;
        _keepTargetCheckBox.ForeColor = UiColors.StatusBarDetailFore;
        _keepTargetCheckBox.ApplyColors();

        if (_badgeText == "CONNECT")
        {
            SetBadgeConnected();
            ApplyDetailForeColor(connected: true);
        }
        else if (_badgeText == "DISCONNECT")
        {
            SetBadgeDisconnected();
            ApplyDetailForeColor(connected: false);
        }
        else
        {
            SetBadgeNeutral();
            _detailLabel.ForeColor = UiColors.StatusBarTitleFore;
        }

        Invalidate();
    }

    private void ApplyToolTips()
    {
        _toolTip.SetToolTip(
            _keepTargetCheckBox,
            "オンにした時点の作成先パスをアプリ側で固定します。"
            + " その後 Wwise 上で選択を変えても、表示と EXPORT 先はこの Keep パスのままです。"
            + " 起動時／EXPORT 前には可能なら Wwise 上でも同じパスを再選択します。"
            + " 接続中は Keep 先へ書き出す旨を警告色で表示します。");
    }

    private void SetBadgeConnected()
    {
        _badgeText = "CONNECT";
        _badgeBack = UiColors.StatusBarConnectedBadgeBack;
        _badgeFore = Color.White;
        _badgeFilled = true;
    }

    private void SetBadgeDisconnected()
    {
        _badgeText = "DISCONNECT";
        _badgeBack = UiColors.StatusBarDisconnectedBadgeBack;
        _badgeFore = Color.White;
        _badgeFilled = true;
    }

    private void SetBadgeNeutral()
    {
        _badgeBack = BackColor;
        _badgeFore = UiColors.StatusBarTitleFore;
        _badgeFilled = false;
    }

    private void ApplyDetailForeColor(bool connected)
    {
        if (!connected)
        {
            _detailLabel.ForeColor = UiColors.StatusBarErrorDetailFore;
            return;
        }

        if (_selectionMissing)
        {
            _detailLabel.ForeColor = UiColors.StatusBarErrorDetailFore;
            return;
        }

        // Keep Target 中は「このパスへ書き出す」ことを警告色で明示する（未選択エラーにはしない）。
        _detailLabel.ForeColor = _keepTargetWarning
            ? UiColors.LogWarning
            : UiColors.StatusBarDetailFore;
    }

    public void SetPending()
    {
        _selectionMissing = false;
        _badgeText = "…";
        SetBadgeNeutral();
        _detailLabel.Text = "確認中…";
        _detailLabel.ForeColor = UiColors.StatusBarTitleFore;
        LayoutLabels();
    }

    public void SetSkipped()
    {
        _selectionMissing = false;
        _badgeText = "—";
        SetBadgeNeutral();
        _detailLabel.Text = "起動時チェックオフ";
        _detailLabel.ForeColor = UiColors.StatusBarTitleFore;
        LayoutLabels();
    }

    public void SetResult(WaapiProbeResult result)
    {
        if (result.Ok)
        {
            _selectionMissing = !result.HasSelection;
            _keepTargetWarning = false;
            SetBadgeConnected();
            ApplyDetailForeColor(connected: true);
        }
        else
        {
            _selectionMissing = false;
            _keepTargetWarning = false;
            SetBadgeDisconnected();
            ApplyDetailForeColor(connected: false);
        }

        _detailLabel.Text = result.FormatStatusDetail();
        LayoutLabels();
    }

    /// <summary>
    /// 接続維持中の表示更新。
    /// <paramref name="keepTarget"/> が true のときは表示パスを Keep 先として扱い、
    /// Wwise 上の選択有無ではエラーにしない（警告色で Keep 先を明示）。
    /// </summary>
    public void UpdateSelection(
        string wwiseVersion,
        string projectName,
        string selectedPath,
        bool keepTarget = false)
    {
        _keepTargetWarning = keepTarget && selectedPath.Length > 0;
        _selectionMissing = keepTarget
            ? selectedPath.Length == 0
            : string.IsNullOrEmpty(selectedPath);
        SetBadgeConnected();
        ApplyDetailForeColor(connected: true);

        var parts = new List<string>();
        if (wwiseVersion.Length > 0)
        {
            parts.Add(wwiseVersion);
        }

        if (projectName.Length > 0)
        {
            parts.Add(projectName);
        }

        if (keepTarget)
        {
            parts.Add(selectedPath.Length > 0
                ? $"Keep → {selectedPath}"
                : "Keep → （未設定）");
        }
        else
        {
            parts.Add(string.IsNullOrEmpty(selectedPath) ? "（未選択）" : selectedPath);
        }

        _detailLabel.Text = string.Join("  ·  ", parts);
        LayoutLabels();
    }

    private void LayoutLabels()
    {
        var titleMidY = Math.Max(0, (ClientSize.Height - _titleLabel.PreferredHeight) / 2);
        _titleLabel.Location = new Point(Padding.Left, titleMidY);

        const int padX = 8;
        const int padY = 3;
        // Yu Gothic UI はメトリクス上の中央より文字が下に見えるため、塗りだけ少し下げる。
        const int fillNudgeY = 2;
        var textSize = TextRenderer.MeasureText(
            _badgeText,
            _badgeFont,
            Size.Empty,
            TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);
        var textTop = titleMidY
            + Math.Max(0, (_titleLabel.PreferredHeight - textSize.Height) / 2);
        var badgeWidth = textSize.Width + padX * 2;
        var badgeLeft = _titleLabel.Right + 8;
        _badgeTextBounds = new Rectangle(badgeLeft, textTop, badgeWidth, textSize.Height);
        _badgeFillBounds = new Rectangle(
            badgeLeft,
            textTop - padY + fillNudgeY,
            badgeWidth,
            textSize.Height + padY * 2);

        var keepSize = _keepTargetCheckBox.GetPreferredSize(Size.Empty);
        var keepLeft = Math.Max(
            _badgeFillBounds.Right + 12,
            ClientSize.Width - Padding.Right - keepSize.Width);
        var keepTop = Math.Max(0, (ClientSize.Height - keepSize.Height) / 2);
        _keepTargetCheckBox.Location = new Point(keepLeft, keepTop);
        _keepTargetCheckBox.Size = keepSize;

        var detailX = _badgeFillBounds.Right + 12;
        var detailRight = keepLeft - 8;
        _detailLabel.Location = new Point(detailX, titleMidY);
        _detailLabel.Width = Math.Max(0, detailRight - detailX);
        _detailLabel.Height = _detailLabel.PreferredHeight;
        Invalidate();
    }

    private void OnPaint(object? sender, PaintEventArgs e)
    {
        using var pen = new Pen(UiColors.StatusBarBorder);
        e.Graphics.DrawLine(pen, 0, 0, Width, 0);

        if (_badgeFilled)
        {
            using var brush = new SolidBrush(_badgeBack);
            e.Graphics.FillRectangle(brush, _badgeFillBounds);
        }

        TextRenderer.DrawText(
            e.Graphics,
            _badgeText,
            _badgeFont,
            _badgeTextBounds,
            _badgeFore,
            TextFormatFlags.HorizontalCenter
            | TextFormatFlags.VerticalCenter
            | TextFormatFlags.NoPrefix
            | TextFormatFlags.NoPadding
            | TextFormatFlags.SingleLine);
    }
}
