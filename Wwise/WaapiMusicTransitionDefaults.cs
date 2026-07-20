using MgaWwiseIMImporter.UI;

namespace MgaWwiseIMImporter.Wwise;

/// <summary>
/// Music Switch Container のトランジション（WAAPI で設定可能な範囲のみ）。
/// <para>
/// 既定の Any → Any（名前 <c>Transition</c>）は必ず先頭に明示する。
/// <c>@TransitionRoot</c> を渡すと Wwise 側の既定ルールが消えることがあるため、
/// Audiokinetic の object.set 例と同様に空の Any→Any を children 先頭へ含める。
/// 続けて各 Playlist 向け Any → Object ルールを追加する（Exit Source At は遷移先の記憶値）。
/// Destination Sync To = Entry Cue / Source Fade-out 有効。
/// MusicFade の Time / Offset / Curve は WAAPI では新規作成できないため設定しない
/// （Work Unit XML 直編集は行わない）。
/// </para>
/// <para>
/// WAAPI 上のプロパティ名は UI 表示名と異なる。
/// Destination Sync To → DestinationJumpPositionPreset（Entry Cue = 0）。
/// </para>
/// </summary>
internal static class WaapiMusicTransitionDefaults
{
    // DestinationJumpPositionPreset: Entry Cue（異なる Playlist 間）
    private const int DestinationJumpPositionEntryCue = 0;
    // Context: Any / Object
    private const int ContextAny = 0;
    private const int ContextObject = 2;

    public const string DefaultAnyToAnyName = "Transition";

    /// <summary>
    /// 既定 Any→Any ＋ 各 Playlist 向け Any→Object を含む TransitionRoot。
    /// </summary>
    public static Dictionary<string, object?> BuildTransitionRoot(
        string containerPath,
        IReadOnlyList<WwisePlaylistPlan> playlists)
    {
        var children = new List<object>(playlists.Count + 1)
        {
            BuildDefaultAnyToAnyRule(),
        };
        foreach (var playlist in playlists)
        {
            children.Add(BuildAnyToPlaylistRule(containerPath, playlist));
        }

        return new Dictionary<string, object?>
        {
            ["type"] = "MusicTransition",
            ["name"] = string.Empty,
            ["@IsFolder"] = true,
            ["children"] = children,
        };
    }

    /// <summary>Wwise 既定と同じ Any → Any（名前 Transition）。</summary>
    private static Dictionary<string, object?> BuildDefaultAnyToAnyRule() =>
        new()
        {
            ["type"] = "MusicTransition",
            ["name"] = DefaultAnyToAnyName,
            ["@SourceContextType"] = ContextAny,
            ["@DestinationContextType"] = ContextAny,
        };

    private static Dictionary<string, object?> BuildAnyToPlaylistRule(
        string containerPath,
        WwisePlaylistPlan playlist) =>
        new()
        {
            ["type"] = "MusicTransition",
            // 再 EXPORT 時に merge できるよう、既定名 Transition と衝突しない安定名にする。
            ["name"] = playlist.Name,
            ["@SourceContextType"] = ContextAny,
            ["@DestinationContextType"] = ContextObject,
            ["@DestinationContextObject"] = $"{containerPath}\\{playlist.Name}",
            ["@ExitSourceAt"] = ToWaapiExitSourceAt(playlist.ExitSourceAt),
            ["@DestinationJumpPositionPreset"] = DestinationJumpPositionEntryCue,
            ["@EnableSourceFadeOut"] = 1,
        };

    /// <summary>Wwise ExitSourceAt 列挙値へ変換する。</summary>
    public static int ToWaapiExitSourceAt(PlaylistExitSourceMode mode) => mode switch
    {
        PlaylistExitSourceMode.Immediate => 0,
        PlaylistExitSourceMode.NextBar => 2,
        PlaylistExitSourceMode.NextBeat => 3,
        PlaylistExitSourceMode.NextCue => 4,
        PlaylistExitSourceMode.ExitCue => 7,
        _ => 2,
    };
}
