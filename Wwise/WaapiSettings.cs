using System.Globalization;
using MgaWwiseIMImporter.UI;

namespace MgaWwiseIMImporter.Wwise;

/// <summary>
/// WAAPI 接続設定（アプリ内固定。INI には書かない）。
/// </summary>
internal sealed class WaapiSettings
{
    public const string Section = "Waapi";

    /// <summary>HTTP WAAPI の URL。</summary>
    public const string DefaultUrl = "http://127.0.0.1:8090/waapi";

    /// <summary>接続・RPC のタイムアウト（ミリ秒）。</summary>
    public const int DefaultTimeoutMs = 3000;

    /// <summary>起動時に接続確認するか。</summary>
    public bool ProbeOnStartup { get; init; } = true;

    /// <summary>HTTP WAAPI の URL。</summary>
    public string Url { get; init; } = DefaultUrl;

    /// <summary>接続・RPC のタイムアウト（ミリ秒）。</summary>
    public int TimeoutMs { get; init; } = DefaultTimeoutMs;

    public static WaapiSettings CreateDefault() => new();

    /// <summary>アプリ固定値を返す。旧 [Waapi] は除去する。</summary>
    public static WaapiSettings Load()
    {
        StripLegacySection();
        return CreateDefault();
    }

    /// <summary>旧 [Waapi] セクションを除去する。</summary>
    public static void StripLegacySection()
    {
        IniFile.RemoveSection(Section);
    }

    /// <summary>
    /// 旧 [Waapi] Keep Target 設定を読み取り、呼び出し側でプロジェクトへ移行するために返す。
    /// <see cref="Load"/> / <see cref="StripLegacySection"/> より前に呼ぶこと。
    /// </summary>
    public static bool TryReadLegacyKeepTarget(
        out bool keepTarget,
        out string keptTargetPath,
        out string keptTargetProjectFilePath)
    {
        keepTarget = false;
        keptTargetPath = string.Empty;
        keptTargetProjectFilePath = string.Empty;
        var values = IniFile.ReadSection(Section);
        if (!values.ContainsKey("KeepTarget")
            && !values.ContainsKey("KeptTargetPath"))
        {
            return false;
        }

        keepTarget = values.TryGetValue("KeepTarget", out var keep)
            && ParseBool(keep, defaultValue: false);
        keptTargetPath = values.TryGetValue("KeptTargetPath", out var path)
            ? path.Trim().Trim('"')
            : string.Empty;
        keptTargetProjectFilePath = values.TryGetValue("KeptTargetProjectFilePath", out var project)
            ? project.Trim().Trim('"')
            : string.Empty;
        return keepTarget || keptTargetPath.Length > 0;
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
