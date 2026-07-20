using System.Globalization;
using MgaWwiseIMImporter.UI;

namespace MgaWwiseIMImporter.Wwise;

/// <summary>
/// Wwise へのインポート設定（exe 横の MgaWwiseIMImporter.ini [WwiseImport]）。
/// LookAhead／Prefetch はプロジェクト設定（[Project.*]）へ移行済み。
/// 旧 [WwiseImport] キーは読み取り互換・移行用に残す。
/// </summary>
internal sealed class WwiseImportSettings
{
    public const string Section = "WwiseImport";
    public const string DefaultStateGroupParentPath = @"\States\Default Work Unit";

    /// <summary>Music Track のストリーミング有効（既定オン）。</summary>
    public bool StreamEnabled { get; init; } = true;

    /// <summary>2 番目以降のセグメントの Look-ahead time（ms）。</summary>
    public int LookAheadMs { get; init; } = 500;

    /// <summary>Playlist 先頭セグメント先頭トラックの Prefetch Length（ms）。ストリーミング時に有効。</summary>
    public int PrefetchLengthMs { get; init; } = 500;

    /// <summary>
    /// 複数パート時に作る State Group の親パス。
    /// 既定は <c>\States\Default Work Unit</c>。
    /// </summary>
    public string StateGroupParentPath { get; init; } = DefaultStateGroupParentPath;

    public string ResolveStateGroupPath(string groupName)
    {
        var parent = StateGroupParentPath.Trim().TrimEnd('\\');
        if (parent.Length == 0)
        {
            parent = DefaultStateGroupParentPath;
        }

        return $"{parent}\\{groupName}";
    }

    public WwiseImportSettings WithStreaming(
        bool streamEnabled,
        int lookAheadMs,
        int prefetchLengthMs) =>
        new()
        {
            StreamEnabled = streamEnabled,
            LookAheadMs = Math.Clamp(lookAheadMs, 0, 9999),
            PrefetchLengthMs = Math.Clamp(prefetchLengthMs, 0, 9999),
            StateGroupParentPath = StateGroupParentPath,
        };

    public static WwiseImportSettings Load()
    {
        EnsureDefaultsWritten();

        var values = IniFile.ReadSection(Section);
        return new WwiseImportSettings
        {
            LookAheadMs = values.TryGetValue("LookAheadMs", out var lookAhead)
                && int.TryParse(lookAhead, NumberStyles.Integer, CultureInfo.InvariantCulture, out var lookAheadMs)
                ? Math.Clamp(lookAheadMs, 0, 9999)
                : 500,
            PrefetchLengthMs = values.TryGetValue("PrefetchLengthMs", out var prefetch)
                && int.TryParse(prefetch, NumberStyles.Integer, CultureInfo.InvariantCulture, out var prefetchMs)
                ? Math.Clamp(prefetchMs, 0, 9999)
                : 500,
            StateGroupParentPath = values.TryGetValue("StateGroupParentPath", out var sgParent)
                && !string.IsNullOrWhiteSpace(sgParent)
                ? sgParent.Trim().Trim('"')
                : DefaultStateGroupParentPath,
        };
    }

    public static void EnsureDefaultsWritten()
    {
        var values = IniFile.ReadSection(Section);
        var changed = false;

        // 旧・XML フェード用キー／WaveCopyDir は除去
        if (values.Remove("SourceFadeOutTimeSec")
            | values.Remove("SourceFadeOutOffsetSec")
            | values.Remove("WaveCopyDir"))
        {
            changed = true;
        }

        // LookAhead／Prefetch は [Project.*] へ移行済みのため、既定追記しない。
        if (!values.ContainsKey("StateGroupParentPath"))
        {
            values["StateGroupParentPath"] = DefaultStateGroupParentPath;
            changed = true;
        }

        if (changed)
        {
            WriteValues(values);
        }
    }

    /// <summary>
    /// 旧 [WwiseImport] の LookAhead／Prefetch を除去する（プロジェクトへ移行後）。
    /// </summary>
    public static void StripStreamingKeys()
    {
        var values = IniFile.ReadSection(Section);
        var changed = values.Remove("LookAheadMs") | values.Remove("PrefetchLengthMs");
        if (changed)
        {
            WriteValues(values);
        }
    }

    private static void WriteValues(Dictionary<string, string> values)
    {
        var written = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["StateGroupParentPath"] = values.TryGetValue("StateGroupParentPath", out var parent)
                ? parent
                : DefaultStateGroupParentPath,
        };

        // 移行完了前は旧キーを残す（StripStreamingKeys で除去）。
        if (values.TryGetValue("LookAheadMs", out var lookAhead))
        {
            written["LookAheadMs"] = lookAhead;
        }

        if (values.TryGetValue("PrefetchLengthMs", out var prefetch))
        {
            written["PrefetchLengthMs"] = prefetch;
        }

        IniFile.WriteSection(Section, written);
    }
}
