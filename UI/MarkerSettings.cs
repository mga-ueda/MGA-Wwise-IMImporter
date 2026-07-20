using System.Globalization;
using MgaWwiseIMImporter.Wave;

namespace MgaWwiseIMImporter.UI;

/// <summary>
/// マウスドラッグで付与するマーカーのスナップ単位。
/// </summary>
internal enum MarkerGridOverrideMode
{
    /// <summary>見た目のグリッド単位（従来仕様）。</summary>
    Default,

    /// <summary>表示状態に関わらず常に小節単位。</summary>
    Bar,

    /// <summary>表示状態に関わらず常に拍単位。</summary>
    Beat,
}

/// <summary>
/// マーカー付与オプション（実行時は [Project.*] に保存。旧 [Markers] は移行用）。
/// </summary>
internal sealed class MarkerSettings
{
    public const string Section = "Markers";
    private const int CurrentSettingsVersion = 3;

    /// <summary>ドラッグ付与時のスナップ単位。</summary>
    public MarkerGridOverrideMode GridOverride { get; set; } = MarkerGridOverrideMode.Default;

    /// <summary>連番の桁数（幅・上限。0 で連番なし）。</summary>
    public int CommentDigits { get; set; } = 3;

    /// <summary>Digits で指定した桁数まで 0 埋めするか（桁数に依らず同じ）。</summary>
    public bool CommentZeroPad { get; set; } = true;

    public bool CommentPrefixEnabled { get; set; }

    public string CommentPrefix { get; set; } = string.Empty;

    public bool CommentSuffixEnabled { get; set; }

    public string CommentSuffix { get; set; } = string.Empty;

    /// <summary>接頭語・接尾語と連番を繋ぐ文字を使うか（入力があれば有効）。</summary>
    public bool CommentJoinerEnabled { get; set; }

    public string CommentJoiner { get; set; } = string.Empty;

    /// <summary>塊（書き出しパート）ごとに連番をリセットするか。</summary>
    public bool CommentResetPerPart { get; set; } = true;

    public const int CommentDigitsMin = 0;
    public const int CommentDigitsMax = 6;

    /// <summary>
    /// 旧チェックボックス設定を入力有無へ正規化する。
    /// Enabled=false の値は破棄し、Enabled は文字列の有無に同期する。
    /// </summary>
    public void SyncCommentOptionalEnabledFlags()
    {
        CommentPrefixEnabled = CommentPrefix.Length > 0;
        CommentSuffixEnabled = CommentSuffix.Length > 0;
        CommentJoinerEnabled = CommentJoiner.Length > 0;
    }

    /// <summary>コメント生成側（Wave 層）に渡す確定値へ変換する。</summary>
    public MarkerCommentRule ToCommentRule()
    {
        SyncCommentOptionalEnabledFlags();
        return new(
            Digits: Math.Clamp(CommentDigits, CommentDigitsMin, CommentDigitsMax),
            ZeroPad: CommentZeroPad,
            Prefix: CommentPrefix,
            Suffix: CommentSuffix,
            Joiner: CommentJoiner,
            ResetPerPart: CommentResetPerPart);
    }

    public static MarkerSettings Load()
    {
        var values = IniFile.ReadSection(Section);
        var settings = new MarkerSettings();
        var settingsVersion = values.TryGetValue("SettingsVersion", out var versionText)
            && int.TryParse(versionText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var version)
                ? version
                : 0;

        if (values.TryGetValue("GridOverride", out var grid)
            && Enum.TryParse<MarkerGridOverrideMode>(grid, ignoreCase: true, out var gridMode))
        {
            settings.GridOverride = gridMode;
        }

        if (values.TryGetValue("CommentDigits", out var digitsText)
            && int.TryParse(digitsText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var digits))
        {
            settings.CommentDigits = Math.Clamp(digits, CommentDigitsMin, CommentDigitsMax);
        }

        settings.CommentZeroPad = ReadBool(values, "CommentZeroPad", settings.CommentZeroPad);
        var prefixEnabled = ReadBool(values, "CommentPrefixEnabled", defaultValue: true);
        var suffixEnabled = ReadBool(values, "CommentSuffixEnabled", settings.CommentSuffixEnabled);
        var joinerEnabled = ReadBool(values, "CommentJoinerEnabled", settings.CommentJoinerEnabled);
        settings.CommentResetPerPart = ReadBool(values, "CommentResetPerPart", settings.CommentResetPerPart);

        if (values.TryGetValue("CommentPrefix", out var prefix))
        {
            settings.CommentPrefix = prefix;
        }

        if (values.TryGetValue("CommentSuffix", out var suffix))
        {
            settings.CommentSuffix = suffix;
        }

        if (values.TryGetValue("CommentJoiner", out var joiner))
        {
            settings.CommentJoiner = joiner;
        }

        // 旧版で自動生成された既定値を、新しい既定値へ一度だけ移行する。
        if (settingsVersion < CurrentSettingsVersion)
        {
            settings.CommentPrefix = string.Empty;
            settings.CommentSuffix = string.Empty;
            settings.CommentZeroPad = true;
            settings.CommentResetPerPart = true;
            settings.Save();
        }
        else
        {
            // 旧チェックボックス OFF の値は入力なしとして扱う。
            if (!prefixEnabled)
            {
                settings.CommentPrefix = string.Empty;
            }

            if (!suffixEnabled)
            {
                settings.CommentSuffix = string.Empty;
            }

            if (!joinerEnabled)
            {
                settings.CommentJoiner = string.Empty;
            }
        }

        settings.SyncCommentOptionalEnabledFlags();
        return settings;
    }

    public void Save()
    {
        IniFile.WriteSection(Section, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["SettingsVersion"] = CurrentSettingsVersion.ToString(CultureInfo.InvariantCulture),
            ["GridOverride"] = GridOverride.ToString(),
            ["CommentDigits"] = Math.Clamp(CommentDigits, CommentDigitsMin, CommentDigitsMax)
                .ToString(CultureInfo.InvariantCulture),
            ["CommentZeroPad"] = CommentZeroPad ? "1" : "0",
            ["CommentPrefixEnabled"] = CommentPrefixEnabled ? "1" : "0",
            ["CommentPrefix"] = CommentPrefix,
            ["CommentSuffixEnabled"] = CommentSuffixEnabled ? "1" : "0",
            ["CommentSuffix"] = CommentSuffix,
            ["CommentJoinerEnabled"] = CommentJoinerEnabled ? "1" : "0",
            ["CommentJoiner"] = CommentJoiner,
            ["CommentResetPerPart"] = CommentResetPerPart ? "1" : "0",
        });
    }

    /// <summary>旧グローバル [Markers] を除去する（値は [Project.*] へ移行済み）。</summary>
    public static void StripLegacySection()
    {
        IniFile.RemoveSection(Section);
    }

    private static bool ReadBool(
        Dictionary<string, string> values,
        string key,
        bool defaultValue)
    {
        if (!values.TryGetValue(key, out var text))
        {
            return defaultValue;
        }

        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
        {
            return number != 0;
        }

        if (bool.TryParse(text, out var flag))
        {
            return flag;
        }

        return defaultValue;
    }
}
