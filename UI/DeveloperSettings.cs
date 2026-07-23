using System.Globalization;

namespace MgaWwiseIMImporter.UI;

/// <summary>
/// 開発者向け設定（exe 横の MgaWwiseIMImporter.ini [Developer]）。
/// </summary>
internal sealed class DeveloperSettings
{
    public const string Section = "Developer";

    /// <summary>Playlist／再生エンジンの詳細診断ログを出すか。既定はオン。</summary>
    public bool DetailedPlaybackLog { get; init; } = true;

    public static DeveloperSettings Load()
    {
        EnsureDefaultsWritten();

        var values = IniFile.ReadSection(Section);
        return new DeveloperSettings
        {
            DetailedPlaybackLog = values.TryGetValue("DetailedPlaybackLog", out var detailedLog)
                ? ParseBool(detailedLog, defaultValue: true)
                : true,
        };
    }

    /// <summary>
    /// 不足キーがあれば現状の既定値で書き足す（既存値は維持）。
    /// </summary>
    public static void EnsureDefaultsWritten()
    {
        var values = IniFile.ReadSection(Section);
        if (values.ContainsKey("DetailedPlaybackLog"))
        {
            return;
        }

        values["DetailedPlaybackLog"] = "1";
        WriteSection(values);
    }

    /// <summary>[Developer] DetailedPlaybackLog だけ更新する（他キーは維持）。</summary>
    public static void SaveDetailedPlaybackLog(bool enabled)
    {
        EnsureDefaultsWritten();
        var values = IniFile.ReadSection(Section);
        values["DetailedPlaybackLog"] = enabled ? "1" : "0";
        WriteSection(values);
    }

    private static void WriteSection(Dictionary<string, string> values)
    {
        IniFile.WriteSection(Section, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["DetailedPlaybackLog"] = values.TryGetValue("DetailedPlaybackLog", out var detailedLog)
                ? detailedLog
                : "1",
        });
    }

    private static bool ParseBool(string text, bool defaultValue)
    {
        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
        {
            return number != 0;
        }

        if (bool.TryParse(text, out var flag))
        {
            return flag;
        }

        if (string.Equals(text, "on", StringComparison.OrdinalIgnoreCase)
            || string.Equals(text, "yes", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(text, "off", StringComparison.OrdinalIgnoreCase)
            || string.Equals(text, "no", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return defaultValue;
    }
}
