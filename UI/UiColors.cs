namespace MgaWwiseImporter.UI;

/// <summary>
/// アプリ全体の色定義。見た目の調整はこのクラスだけを編集する。
/// </summary>
internal static class UiColors
{
    // --- ウィンドウ／ログ ---
    public static readonly Color WindowBack = Color.FromArgb(30, 30, 30);
    public static readonly Color WindowFore = Color.White;
    public static readonly Color LogBack = Color.FromArgb(30, 30, 30);
    public static readonly Color LogDefault = Color.FromArgb(220, 220, 220);
    public static readonly Color LogHeader = Color.FromArgb(110, 180, 255);
    public static readonly Color LogWarning = Color.FromArgb(255, 180, 70);
    public static readonly Color LogError = Color.FromArgb(255, 110, 110);
    public static readonly Color LogMuted = Color.FromArgb(150, 150, 150);

    // --- 波形ビュー共通（初期背景はログエディタと同色） ---
    public static readonly Color WaveformBack = Color.FromArgb(30, 30, 30);
    public static readonly Color WaveFill = Color.White;
    public static readonly Color WaveCenter = Color.FromArgb(55, 55, 55);
    public static readonly Color EmptyHint = Color.FromArgb(140, 140, 140);
    public static readonly Color SeekCyan = Color.FromArgb(0, 245, 255);
    public static readonly Color MouseGuide = Color.FromArgb(220, 255, 255, 255);
    public static readonly Color BarLine = Color.FromArgb(90, 170, 170, 170);
    public static readonly Color AnacrusisLine = Color.FromArgb(160, 230, 190, 70);
    public static readonly Color TempoChangeLine = Color.FromArgb(180, 180, 255, 180);

    // --- ラベル行（背景／文字） ---
    public static readonly Color BarNumberBg = Color.FromArgb(43, 43, 45);
    public static readonly Color BarNumberFg = Color.FromArgb(235, 235, 235);
    public static readonly Color TempoBg = Color.FromArgb(17, 60, 29);
    public static readonly Color TempoFg = Color.FromArgb(230, 255, 230);
    public static readonly Color SignatureBg = Color.FromArgb(98, 51, 14);
    public static readonly Color SignatureFg = Color.FromArgb(255, 245, 230);
    public static readonly Color MarkerRowBg = Color.FromArgb(43, 43, 45);
    public static readonly Color MarkerFg = Color.FromArgb(245, 250, 255);
    public static readonly Color MarkerTriangle = Color.FromArgb(255, 90, 200, 255);
    public static readonly Color CycleRowBg = Color.FromArgb(28, 28, 30);
    public static readonly Color CycleRangeFill = Color.FromArgb(200, 180, 40, 40);
    public static readonly Color CycleFg = Color.FromArgb(255, 220, 220);
    // 波形エリア上のリージョン着色（赤／青交互・暗め）
    public static readonly Color RegionWaveFillRed = Color.FromArgb(95, 110, 22, 22);
    public static readonly Color RegionWaveFillBlue = Color.FromArgb(95, 18, 38, 105);
    public static readonly Color RegionWaveFillExcluded = Color.FromArgb(55, 50, 50, 55);
    public static readonly Color OutputPartFg = Color.FromArgb(255, 255, 255);
    public static readonly Color OutputPartShadow = Color.FromArgb(230, 0, 0, 0);
}
