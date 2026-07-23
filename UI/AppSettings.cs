using System.Globalization;
using MgaWwiseIMImporter.Wave;

namespace MgaWwiseIMImporter.UI;

/// <summary>
/// アプリ全体の作業設定（exe 横 INI の [App]）。プロジェクト切替では変わらない。
/// </summary>
internal sealed class AppSettings
{
    public const string Section = "App";

    public bool AlwaysOnTop { get; set; }

    /// <summary>UI／ログの表示言語（既定 ja）。</summary>
    public UiLanguage UiLanguage { get; set; } = UiLanguage.Japanese;

    /// <summary>
    /// アップデート案内をスキップしたリモート SemVer（空なら未スキップ）。
    /// より新しい版が出たら再度案内する。
    /// </summary>
    public string SkippedUpdateVersion { get; set; } = string.Empty;

    /// <summary>ツールチップ表示（既定オン）。</summary>
    public bool ShowToolTips { get; set; } = true;

    /// <summary>再生出力 API（既定 WaveOut）。</summary>
    public AudioOutputApi AudioApi { get; set; } = AudioOutputApi.WaveOut;

    /// <summary>出力デバイス識別子（API 依存。空はシステム既定）。</summary>
    public string AudioDeviceId { get; set; } = string.Empty;

    /// <summary>波形表示エリア高さの倍率（1 / 2 / 3。既定 1）。</summary>
    public int WaveformHeightScale { get; set; } = 1;

    public AudioOutputSettings ToAudioOutputSettings() => new(AudioApi, AudioDeviceId ?? string.Empty);

    public static AppSettings Load()
    {
        var values = IniFile.ReadSection(Section);
        var settings = Parse(values);
        if (!HasKnownKeys(values))
        {
            settings.Save();
        }

        return settings;
    }

    public void Save() => WriteValues(ToDictionary());

    public void SaveAlwaysOnTop(bool enabled)
    {
        AlwaysOnTop = enabled;
        Save();
    }

    public void SaveUiLanguage(UiLanguage language)
    {
        UiLanguage = language;
        Save();
    }

    public void SaveSkippedUpdateVersion(string? semVer)
    {
        SkippedUpdateVersion = AppVersion.NormalizeTag(semVer);
        Save();
    }

    public void SaveAudioOutput(AudioOutputApi api, string? deviceId)
    {
        AudioApi = api;
        AudioDeviceId = deviceId ?? string.Empty;
        Save();
    }

    public void SaveShowToolTips(bool enabled)
    {
        ShowToolTips = enabled;
        Save();
    }

    public void SaveWaveformHeightScale(int scale)
    {
        WaveformHeightScale = NormalizeWaveformHeightScale(scale);
        Save();
    }

    private Dictionary<string, string> ToDictionary() => new(StringComparer.OrdinalIgnoreCase)
    {
        ["AlwaysOnTop"] = AlwaysOnTop ? "1" : "0",
        ["UiLanguage"] = UiStrings.ToIniValue(UiLanguage),
        ["SkippedUpdateVersion"] = SkippedUpdateVersion ?? string.Empty,
        ["ShowToolTips"] = ShowToolTips ? "1" : "0",
        ["AudioApi"] = AudioOutputSettings.ToIniValue(AudioApi),
        ["AudioDeviceId"] = AudioDeviceId ?? string.Empty,
        ["WaveformHeightScale"] = WaveformHeightScale.ToString(CultureInfo.InvariantCulture),
    };

    private static void WriteValues(Dictionary<string, string> values)
    {
        IniFile.WriteSection(Section, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["AlwaysOnTop"] = values.TryGetValue("AlwaysOnTop", out var alwaysOnTop) ? alwaysOnTop : "0",
            ["UiLanguage"] = values.TryGetValue("UiLanguage", out var language) ? language : "ja",
            ["SkippedUpdateVersion"] = values.TryGetValue("SkippedUpdateVersion", out var skipped)
                ? skipped
                : string.Empty,
            ["ShowToolTips"] = values.TryGetValue("ShowToolTips", out var showToolTips) ? showToolTips : "1",
            ["AudioApi"] = values.TryGetValue("AudioApi", out var audioApi) ? audioApi : "WaveOut",
            ["AudioDeviceId"] = values.TryGetValue("AudioDeviceId", out var deviceId) ? deviceId : string.Empty,
            ["WaveformHeightScale"] = values.TryGetValue("WaveformHeightScale", out var scale)
                ? NormalizeWaveformHeightScale(scale).ToString(CultureInfo.InvariantCulture)
                : "1",
        });
    }

    private static AppSettings Parse(Dictionary<string, string> values) => new()
    {
        AlwaysOnTop = ReadBool(values, "AlwaysOnTop", defaultValue: false),
        UiLanguage = values.TryGetValue("UiLanguage", out var languageText)
            ? UiStrings.ParseLanguage(languageText)
            : UiLanguage.Japanese,
        SkippedUpdateVersion = values.TryGetValue("SkippedUpdateVersion", out var skipped)
            ? AppVersion.NormalizeTag(skipped)
            : string.Empty,
        ShowToolTips = ReadBool(values, "ShowToolTips", defaultValue: true),
        AudioApi = values.TryGetValue("AudioApi", out var audioApiText)
            ? AudioOutputSettings.ParseApi(audioApiText)
            : AudioOutputApi.WaveOut,
        AudioDeviceId = values.TryGetValue("AudioDeviceId", out var deviceId)
            ? deviceId
            : string.Empty,
        WaveformHeightScale = values.TryGetValue("WaveformHeightScale", out var scaleText)
            ? NormalizeWaveformHeightScale(scaleText)
            : 1,
    };

    private static bool HasKnownKeys(Dictionary<string, string> values) =>
        values.ContainsKey("AlwaysOnTop")
        || values.ContainsKey("UiLanguage")
        || values.ContainsKey("SkippedUpdateVersion")
        || values.ContainsKey("ShowToolTips")
        || values.ContainsKey("AudioApi")
        || values.ContainsKey("AudioDeviceId")
        || values.ContainsKey("WaveformHeightScale");

    /// <summary>波形高さ倍率を 1〜3 に正規化する。</summary>
    public static int NormalizeWaveformHeightScale(int scale) =>
        scale is >= 1 and <= 3 ? scale : 1;

    private static int NormalizeWaveformHeightScale(string? text)
    {
        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var scale))
        {
            return NormalizeWaveformHeightScale(scale);
        }

        return 1;
    }

    private static bool ReadBool(Dictionary<string, string> values, string key, bool defaultValue)
    {
        if (!values.TryGetValue(key, out var text))
        {
            return defaultValue;
        }

        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
        {
            return number != 0;
        }

        return bool.TryParse(text, out var flag) ? flag : defaultValue;
    }
}
