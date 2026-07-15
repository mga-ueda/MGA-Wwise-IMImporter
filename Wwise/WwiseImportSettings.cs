using System.Globalization;
using MgaWwiseImporter.UI;

namespace MgaWwiseImporter.Wwise;

/// <summary>
/// Wwise へのインポート設定（exe 横の MgaWwiseImporter.ini [WwiseImport]）。
/// </summary>
internal sealed class WwiseImportSettings
{
    public const string Section = "WwiseImport";
    public const string DefaultStateGroupParentPath = @"\States\Default Work Unit";

    /// <summary>2 番目以降のセグメントの Look-ahead time（ms）。</summary>
    public int LookAheadMs { get; init; } = 500;

    /// <summary>全 Music Track の Prefetch Length（ms）。ストリーミング時に有効。</summary>
    public int PrefetchLengthMs { get; init; } = 500;

    /// <summary>
    /// エクスポート WAV のコピー先ディレクトリ。空ならコピーせず、書き出し場所から直接インポートする。
    /// </summary>
    public string WaveCopyDir { get; init; } = string.Empty;

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

    public static WwiseImportSettings Load()
    {
        EnsureDefaultsWritten();

        var values = IniFile.ReadSection(Section);
        return new WwiseImportSettings
        {
            LookAheadMs = values.TryGetValue("LookAheadMs", out var lookAhead)
                && int.TryParse(lookAhead, NumberStyles.Integer, CultureInfo.InvariantCulture, out var lookAheadMs)
                ? Math.Clamp(lookAheadMs, 0, 10000)
                : 500,
            PrefetchLengthMs = values.TryGetValue("PrefetchLengthMs", out var prefetch)
                && int.TryParse(prefetch, NumberStyles.Integer, CultureInfo.InvariantCulture, out var prefetchMs)
                ? Math.Clamp(prefetchMs, 0, 10000)
                : 500,
            WaveCopyDir = values.TryGetValue("WaveCopyDir", out var dir)
                ? dir.Trim().Trim('"')
                : string.Empty,
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

        // 旧・XML フェード用キーは除去
        if (values.Remove("SourceFadeOutTimeSec") | values.Remove("SourceFadeOutOffsetSec"))
        {
            changed = true;
        }

        if (!values.ContainsKey("LookAheadMs"))
        {
            values["LookAheadMs"] = "500";
            changed = true;
        }

        if (!values.ContainsKey("PrefetchLengthMs"))
        {
            values["PrefetchLengthMs"] = "500";
            changed = true;
        }

        if (!values.ContainsKey("WaveCopyDir"))
        {
            values["WaveCopyDir"] = string.Empty;
            changed = true;
        }

        if (!values.ContainsKey("StateGroupParentPath"))
        {
            values["StateGroupParentPath"] = DefaultStateGroupParentPath;
            changed = true;
        }

        if (changed)
        {
            IniFile.WriteSection(Section, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["LookAheadMs"] = values["LookAheadMs"],
                ["PrefetchLengthMs"] = values["PrefetchLengthMs"],
                ["WaveCopyDir"] = values["WaveCopyDir"],
                ["StateGroupParentPath"] = values["StateGroupParentPath"],
            });
        }
    }
}
