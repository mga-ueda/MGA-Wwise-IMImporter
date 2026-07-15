using System.Globalization;
using MgaWwiseImporter.UI;

namespace MgaWwiseImporter.Wwise;

/// <summary>
/// WAAPI 接続設定（exe 横の MgaWwiseImporter.ini [Waapi]）。
/// </summary>
internal sealed class WaapiSettings
{
    public const string Section = "Waapi";

    /// <summary>起動時に接続確認するか。既定はオン。</summary>
    public bool ProbeOnStartup { get; init; } = true;

    /// <summary>HTTP WAAPI の URL。既定は localhost:8090。</summary>
    public string Url { get; init; } = DefaultUrl;

    /// <summary>接続・RPC のタイムアウト（ミリ秒）。</summary>
    public int TimeoutMs { get; init; } = 3000;

    public const string DefaultUrl = "http://127.0.0.1:8090/waapi";

    public static WaapiSettings Load()
    {
        EnsureDefaultsWritten();

        var values = IniFile.ReadSection(Section);
        return new WaapiSettings
        {
            ProbeOnStartup = values.TryGetValue("ProbeOnStartup", out var probe)
                ? ParseBool(probe, defaultValue: true)
                : true,
            Url = values.TryGetValue("Url", out var url) && !string.IsNullOrWhiteSpace(url)
                ? url.Trim()
                : DefaultUrl,
            TimeoutMs = values.TryGetValue("TimeoutMs", out var timeout)
                && int.TryParse(timeout, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ms)
                && ms > 0
                ? ms
                : 3000,
        };
    }

    public static void EnsureDefaultsWritten()
    {
        var values = IniFile.ReadSection(Section);
        var changed = false;

        if (!values.ContainsKey("ProbeOnStartup"))
        {
            values["ProbeOnStartup"] = "1";
            changed = true;
        }

        if (!values.ContainsKey("Url"))
        {
            values["Url"] = DefaultUrl;
            changed = true;
        }

        if (!values.ContainsKey("TimeoutMs"))
        {
            values["TimeoutMs"] = "3000";
            changed = true;
        }

        if (changed)
        {
            IniFile.WriteSection(Section, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["ProbeOnStartup"] = values["ProbeOnStartup"],
                ["Url"] = values["Url"],
                ["TimeoutMs"] = values["TimeoutMs"],
            });
        }
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
