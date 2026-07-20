using System.Globalization;
using MgaWwiseIMImporter.UI;

namespace MgaWwiseIMImporter.Wwise;

/// <summary>
/// Wwise へのインポート設定（アプリ内固定。INI には書かない）。
/// LookAhead／Prefetch はプロジェクト設定（[Project.*]）。旧 [WwiseImport] は移行後に除去する。
/// </summary>
internal sealed class WwiseImportSettings
{
    public const string Section = "WwiseImport";
    public const string DefaultStateGroupParentPath = @"\States\Default Work Unit";
    public const int DefaultLookAheadMs = 500;
    public const int DefaultPrefetchLengthMs = 500;

    /// <summary>Music Track のストリーミング有効（既定オン）。</summary>
    public bool StreamEnabled { get; init; } = true;

    /// <summary>2 番目以降のセグメントの Look-ahead time（ms）。</summary>
    public int LookAheadMs { get; init; } = DefaultLookAheadMs;

    /// <summary>Playlist 先頭セグメント先頭トラックの Prefetch Length（ms）。ストリーミング時に有効。</summary>
    public int PrefetchLengthMs { get; init; } = DefaultPrefetchLengthMs;

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

    /// <summary>アプリ固定値を返す。</summary>
    public static WwiseImportSettings Load() => new();

    /// <summary>
    /// 旧 [WwiseImport] の LookAhead／Prefetch を読む（プロジェクト未設定時の移行用）。
    /// <see cref="StripLegacySection"/> より前に呼ぶこと。
    /// </summary>
    public static void ReadLegacyStreaming(out int lookAheadMs, out int prefetchLengthMs)
    {
        var values = IniFile.ReadSection(Section);
        lookAheadMs = values.TryGetValue("LookAheadMs", out var lookAhead)
            && int.TryParse(lookAhead, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedLookAhead)
            ? Math.Clamp(parsedLookAhead, 0, 9999)
            : DefaultLookAheadMs;
        prefetchLengthMs = values.TryGetValue("PrefetchLengthMs", out var prefetch)
            && int.TryParse(prefetch, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedPrefetch)
            ? Math.Clamp(parsedPrefetch, 0, 9999)
            : DefaultPrefetchLengthMs;
    }

    /// <summary>旧 [WwiseImport] セクションを除去する。</summary>
    public static void StripLegacySection()
    {
        IniFile.RemoveSection(Section);
    }

    /// <summary>旧 API 名。セクション全体を除去する。</summary>
    public static void StripStreamingKeys() => StripLegacySection();
}
