namespace MgaWwiseIMImporter.Wwise;

/// <summary>
/// Music Switch Container の any→any トランジション既定値（WAAPI で設定可能な範囲のみ）。
/// <para>
/// Exit Source at = Immediate / Source Fade-out 有効。
/// MusicFade の Time / Offset / Curve は WAAPI では新規作成できないため設定しない
/// （Work Unit XML 直編集は行わない）。
/// </para>
/// </summary>
internal static class WaapiMusicTransitionDefaults
{
    // ExitSourceAt: Immediate
    private const int ExitSourceAtImmediate = 0;
    // Context: Any
    private const int ContextAny = 0;

    public static Dictionary<string, object?> BuildAnyToAnyTransitionRoot() =>
        new()
        {
            ["type"] = "MusicTransition",
            ["name"] = string.Empty,
            ["@IsFolder"] = true,
            ["children"] = new object[]
            {
                new Dictionary<string, object?>
                {
                    ["type"] = "MusicTransition",
                    ["name"] = string.Empty,
                    ["@SourceContextType"] = ContextAny,
                    ["@DestinationContextType"] = ContextAny,
                    ["@ExitSourceAt"] = ExitSourceAtImmediate,
                    ["@EnableSourceFadeOut"] = true,
                },
            },
        };
}
