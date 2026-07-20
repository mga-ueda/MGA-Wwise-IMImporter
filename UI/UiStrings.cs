namespace MgaWwiseIMImporter.UI;

/// <summary>
/// ツールチップ・ダイアログ・ユーザー向けログの日英文言を一箇所に集約する。
/// 画面の固定ラベル（EXPORT 等）は対象外。編集はこのファイルを優先する。
/// </summary>
internal static class UiStrings
{
    public static UiLanguage Language { get; private set; } = UiLanguage.Japanese;

    public static event EventHandler? LanguageChanged;

    public static bool IsJapanese => Language == UiLanguage.Japanese;

    public static void SetLanguage(UiLanguage language)
    {
        if (Language == language)
        {
            return;
        }

        Language = language;
        LanguageChanged?.Invoke(null, EventArgs.Empty);
    }

    public static void SetLanguageFromIni(string? value)
    {
        SetLanguage(ParseLanguage(value));
    }

    public static UiLanguage ParseLanguage(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return UiLanguage.Japanese;
        }

        var trimmed = value.Trim();
        if (trimmed.Equals("en", StringComparison.OrdinalIgnoreCase)
            || trimmed.Equals("english", StringComparison.OrdinalIgnoreCase)
            || trimmed.Equals(nameof(UiLanguage.English), StringComparison.OrdinalIgnoreCase))
        {
            return UiLanguage.English;
        }

        return UiLanguage.Japanese;
    }

    public static string ToIniValue(UiLanguage language) =>
        language == UiLanguage.English ? "en" : "ja";

    public static string Get(string japanese, string english) =>
        IsJapanese ? japanese : english;

    public static string Format(string japaneseFormat, string englishFormat, params object[] args) =>
        string.Format(
            System.Globalization.CultureInfo.CurrentCulture,
            Get(japaneseFormat, englishFormat),
            args);

    // --- Dialog common ---
    public static string DialogOk => Get("OK", "OK");
    public static string DialogCancel => Get("キャンセル", "Cancel");
    public static string DialogYes => Get("はい", "Yes");
    public static string DialogNo => Get("いいえ", "No");

    // --- Language toggle ---
    public static string TipLanguageToggle => Get(
        "表示言語を切り替えます（日本語 / English）。アプリ設定に保存されます。",
        "Switch display language (Japanese / English). Saved in app settings.");

    public static string TipLanguageJapanese => Get(
        "現在: 日本語。クリックで English に切り替えます。",
        "Current: Japanese. Click to switch to English.");

    public static string TipLanguageEnglish => Get(
        "現在: English。クリックで日本語に切り替えます。",
        "Current: English. Click to switch to Japanese.");

    // --- Action bar tooltips ---
    public static string TipDebugLog => Get(
        "再生・操作の詳細な診断情報を画面ログへ出力します（開発用）。",
        "Write detailed playback/diagnostics to the on-screen log (for development).");

    public static string TipCompactFileNumbers => Get(
        "ON: 無効化した Playlist があっても、書き出す WAV の番号を 1 から詰めます。"
        + Environment.NewLine
        + "OFF: 元の番号を維持します（欠番が残ります）。",
        "ON: Renumber exported WAV files from 1, skipping disabled playlists."
        + Environment.NewLine
        + "OFF: Keep original numbers (gaps remain).");

    public static string TipKeepLastSession => Get(
        "起動時およびこのプロジェクトへ戻ったときに、最後の作業セッション（波形・グループ／無効化／追加マーカー／Fade・Exit Source At）を復元します（プロジェクト設定）。",
        "On startup and when returning to this project, restore the last session (wave, groups, disables, markers, Fade / Exit Source At). Project setting.");

    public static string TipAlwaysOnTop => Get(
        "ウィンドウを常に最前面へ表示します（アプリ設定）。",
        "Keep the window always on top (app setting).");

    public static string TipClear => Get(
        "波形・セッション・ログをクリアし、選択中プロジェクトの設定をアプリ既定へ戻します。"
        + Environment.NewLine
        + "Always on Top／Keep Target はアプリ設定のため変わりません。"
        + Environment.NewLine
        + "プロジェクト自体は削除しません。",
        "Clear wave, session, and log, and reset the active project settings to app defaults."
        + Environment.NewLine
        + "Always on Top / Keep Target (app settings) are unchanged."
        + Environment.NewLine
        + "The project itself is not deleted.");

    public static string TipReload => Get(
        "最後にドロップまたは自動読み込みした WAV／XML を、元のファイルから再読み込みします。"
        + Environment.NewLine
        + "ログ・Playlist のグループ化・無効化・追加マーカーはリセットされます。",
        "Reload the last dropped or auto-loaded WAV/XML from the original files."
        + Environment.NewLine
        + "Log, playlist grouping, disables, and added markers are reset.");

    public static string TipExport => Get(
        "分割 WAV を書き出し、続けて Wwise へインポートします。"
        + Environment.NewLine
        + "無効化した Playlist は書き出し対象外です。",
        "Export split WAVs and import them into Wwise."
        + Environment.NewLine
        + "Disabled playlists are excluded.");

    public static string TipProjectFolder => Get(
        "波形の書き出し先フォルダを選択します（接続中 Wwise プロジェクトの Originals 配下）。",
        "Choose the export folder (must be under the connected Wwise project's Originals).");

    public static string TipProjectDelete => Get(
        "選択中のプロジェクトを削除します（DEL）。",
        "Delete the selected project (DEL).");

    public static string TipProjectName => Get(
        "プロジェクト名の選択と編集。末尾の「+ New Project」で新規作成します。",
        "Select or edit the project name. Use “+ New Project” at the end to create one.");

    public static string TipProjectOutputPath => Get(
        "分割 WAV の書き出し先フォルダです。横のフォルダボタンで変更できます。",
        "Folder for exported split WAVs. Change it with the folder button.");

    public static string TipSpectrum => Get(
        "再生出力の簡易スペクトラム表示です。",
        "Simple spectrum meter for playback output.");

    public static string TipLogEditor => Get(
        "操作・EXPORT・接続などのログです。右下のアイコンで消去・コピー・保存できます。",
        "Log for operations, EXPORT, and connection. Use the icons to clear, copy, or save.");

    public static string TipLogClear => Get(
        "ログ表示だけを消去します（ファイルは消しません）。",
        "Clear the log display only (does not delete files).");

    public static string TipLogCopy => Get(
        "ログ全文をクリップボードへコピーします。",
        "Copy the full log to the clipboard.");

    public static string TipLogDownload => Get(
        "ログをファイルへ保存します。",
        "Save the log to a file.");

    public static string TipCopyright => Get(
        "著作権・ライセンス情報（GitHub）を開きます。",
        "Open copyright / license information on GitHub.");

    public static string TipPlaylistHeader => Get(
        "遷移先として選ぶ Music Playlist の一覧です。クリックで Fade／Exit Source At を反映し、再生中は遷移を予約します。",
        "List of Music Playlists to jump to. Click to apply Fade / Exit Source At; while playing, schedules a transition.");

    public static string TipPlaylistItem(string playlistName) => Format(
        "{0}{1}"
        + "Shift + クリック／ドラッグ: グループ化（既存グループも新しい ID で上書き可）{1}"
        + "Ctrl + クリック／ドラッグ: グループ解除{1}"
        + "Ctrl + Shift + クリック／ドラッグ: 無効化／再有効化",
        "{0}{1}"
        + "Shift + click/drag: group (can overwrite an existing group with a new ID){1}"
        + "Ctrl + click/drag: ungroup{1}"
        + "Ctrl + Shift + click/drag: disable / re-enable",
        playlistName,
        Environment.NewLine);

    public static string TipWaveformEditSourceName => Get(
        "ダブルクリックでファイル名を編集",
        "Double-click to edit the file name");

    public static string TipWaveformMarkerLane => Get(
        "Shift + クリック／ドラッグ: マーカーを連続付与"
        + Environment.NewLine
        + "Ctrl + クリック／ドラッグ: マーカーを連続削除",
        "Shift + click/drag: add markers continuously"
        + Environment.NewLine
        + "Ctrl + click/drag: remove markers continuously");

    public static string TipWaveformZoomFitAll => Get(
        "ダブルクリックでタイムライン全体を表示",
        "Double-click to show the full timeline");

    public static string TipWaveformZoomPlaylist => Get(
        "ダブルクリックで Music Playlist を拡大表示",
        "Double-click to zoom the Music Playlist");

    // --- Fade / Exit Source tooltips ---
    public static string TipFadeInHeader => Get(
        "いま再生しているソース側のフェードイン時間です（次ソースの Destination Fade-in ではありません）。",
        "Fade-in time for the currently playing source (not Wwise Destination Fade-in).");

    public static string TipFadeOutHeader => Get(
        "いま再生しているソース側のフェードアウト時間です。",
        "Fade-out time for the currently playing source.");

    public static string TipExitSourceHeader => Get(
        "再生中に別 Playlist へ移るとき、いまのソースをどのタイミングで退出するかです。",
        "When jumping to another playlist while playing, when the current source should exit.");

    public static string TipFadeNone => Get(
        "フェードなし（即時）。",
        "No fade (immediate).");

    public static string TipFadeSeconds(string seconds) => Format(
        "{0} 秒のフェードです。Playlist を選んでから変更するとそのパート（グループ）に記憶されます。",
        "{0} second fade. Select a playlist first to store it per part (group).",
        seconds);

    public static string TipGroupFadeHeader => Get(
        "同一グループ内の遷移だけで使う Group Fade です。通常の Fade はグループ内では無効になります。",
        "Group Fade used only for transitions inside the same group. Normal Fade is disabled within a group.");

    public static string TipExitImmediate => Get(
        "即座に退出して遷移します。",
        "Exit immediately and transition.");

    public static string TipExitNextBar => Get(
        "次の小節境界で退出します。",
        "Exit at the next bar boundary.");

    public static string TipExitNextBeat => Get(
        "次の拍境界で退出します。",
        "Exit at the next beat boundary.");

    public static string TipExitNextCue => Get(
        "次の Custom Cue（単発マーカー）で退出します。",
        "Exit at the next Custom Cue (single marker).");

    public static string TipExitExitCue => Get(
        "Exit Cue で退出します。",
        "Exit at the Exit Cue.");

    // --- Marker options (existing) ---
    public static string TipStreamHeader => Get(
        "Wwise Music Track のストリーミング関連設定です。",
        "Streaming settings for Wwise Music Tracks.");

    public static string TipStreamEnabled => Get(
        "オンの場合、Music Track をストリーミング有効で作成します（既定オン）。"
        + " オフのときは Look-ahead Time／Prefetch Length は適用されません。",
        "When on, create Music Tracks with streaming enabled (default on)."
        + " When off, Look-ahead Time / Prefetch Length are not applied.");

    public static string TipLookAheadLabel => Get(
        "2 番目以降のセグメントの Look-ahead Time（ms、0〜9999。既定 500）。"
        + " Stream オン時のみ有効。先頭セグメント内の全トラック（グループ化レイヤー含む）は Zero latency のため 0 固定です。",
        "Look-ahead Time for the 2nd and later segments (ms, 0–9999, default 500)."
        + " Only when Stream is on. All tracks in the first segment (including layered groups) use Zero latency (0).");

    public static string TipLookAheadBox => Get(
        "Look-ahead Time（ms）。0〜9999。既定は 500 です。Stream オン時のみ有効。",
        "Look-ahead Time (ms). 0–9999. Default 500. Only when Stream is on.");

    public static string TipPrefetchLabel => Get(
        "Playlist 先頭セグメント先頭トラックの Prefetch Length（ms、0〜9999。既定 500）。Stream オン時のみ有効。"
        + " 先頭セグメントの 2 番目以降トラック（グループ化レイヤー）には Zero latency のみ適用します。",
        "Prefetch Length for the first track of the first playlist segment (ms, 0–9999, default 500). Only when Stream is on."
        + " Later tracks in the first segment (layered groups) get Zero latency only.");

    public static string TipPrefetchBox => Get(
        "Prefetch Length（ms）。0〜9999。既定は 500 です。"
        + " Playlist 先頭セグメント先頭トラックにだけ反映されます。Stream オン時のみ有効。",
        "Prefetch Length (ms). 0–9999. Default 500."
        + " Applied only to the first track of the first playlist segment. Only when Stream is on.");

    public static string TipLoudnessHeader => Get(
        "このアプリ独自のラウドネス正規化です（Wwise の非破壊 Loudness Normalize とは無関係）。"
        + " EXPORT 時に分割 WAV へ破壊編集でゲインを焼き込みます。",
        "App-specific loudness normalization (unrelated to Wwise’s non-destructive Loudness Normalize)."
        + " On EXPORT, gain is baked into split WAVs.");

    public static string TipLoudnessEnabled => Get(
        "オンの場合、EXPORT で分割した各 WAV の音量を Target LKFS へ破壊的に正規化します"
        + "（既定オフ。Wwise 標準機能ではなく、このアプリ独自の処理です）。"
        + " 元の連続波形は変更せず、書き出すセパレート WAV のみを書き換えます。",
        "When on, destructively normalize each split WAV to Target LKFS on EXPORT"
        + " (default off; app-specific, not a Wwise feature)."
        + " The original continuous wave is unchanged; only exported separate WAVs are rewritten.");

    public static string TipLoudnessTarget => Get(
        "正規化の目標ラウドネス（LKFS、−70〜0。既定 −24）。Normalize オン時のみ有効。",
        "Target loudness (LKFS, −70 to 0, default −24). Only when Normalize is on.");

    public static string TipLoudnessTargetBox => Get(
        "目標ラウドネス（LKFS）。−70〜0。既定は −24 です。Normalize オン時のみ有効。",
        "Target loudness (LKFS). −70 to 0. Default −24. Only when Normalize is on.");

    public static string TipLoudnessUnit => Get(
        "単位は LKFS（ITU-R BS.1770 / LUFS と同値）です。",
        "Unit is LKFS (same scale as ITU-R BS.1770 / LUFS).");

    public static string TipLoudnessGroupBalance => Get(
        "オンの場合、グループ内で最も大きい音量のファイルを Target に合わせ、"
        + "他メンバーは相対バランスを保ったまま同じゲインを破壊編集で適用します（既定オン）。"
        + " オフでは各ファイルを個別に Target へ正規化します。",
        "When on, match the loudest file in a group to Target and apply the same gain to members"
        + " to keep relative balance (default on)."
        + " When off, normalize each file to Target individually.");

    public static string TipAutoVolume => Get(
        "オンの場合、Loudness Normalize で変化した音量の逆を Music Playlist の"
        + " Make-Up Gain または Voice Volume へ書き戻します（既定オン）。Normalize オン時のみ有効。",
        "When on, write the inverse of Loudness Normalize gain back to the Music Playlist"
        + " Make-Up Gain or Voice Volume (default on). Only when Normalize is on.");

    public static string TipAutoVolumeMakeUpGain => Get(
        "Auto Volume の補償を Music Playlist の Make-Up Gain へ設定します（既定）。"
        + " Voice Volume は 0 にします。",
        "Apply Auto Volume compensation to Music Playlist Make-Up Gain (default)."
        + " Voice Volume is set to 0.");

    public static string TipAutoVolumeVoiceVolume => Get(
        "Auto Volume の補償を Music Playlist の Voice Volume へ設定します。"
        + " Make-Up Gain は 0 にします。",
        "Apply Auto Volume compensation to Music Playlist Voice Volume."
        + " Make-Up Gain is set to 0.");

    public static string TipAutoVolumeHeader => Get(
        "Loudness Normalize のゲイン変化を Music Playlist の音量プロパティで打ち消します。",
        "Compensate Loudness Normalize gain changes via Music Playlist volume properties.");

    public static string TipMoreOptionsHeader => Get(
        "Stream／Loudness Normalize／Auto Volume／Marker Grid／Marker Comment を開閉します（既定は開いた状態）。"
        + " 開閉状態はプロジェクト設定へ自動保存されます。"
        + " 開閉しても Music Playlist の高さは変わりません。",
        "Expand/collapse Stream / Loudness Normalize / Auto Volume / Marker Grid / Marker Comment (default open)."
        + " Expansion is saved per project."
        + " Playlist height is unchanged when toggling.");

    public static string TipMarkerGridHeader => Get(
        "マーカーをドラッグで付与するときのスナップ間隔を指定します。縦線の描画には影響しません。",
        "Snap interval when dragging markers. Does not affect grid line drawing.");

    public static string TipMarkerGridTimeline => Get(
        "現在タイムラインに表示されているグリッドへスナップします。従来と同じ動作です。",
        "Snap to the grid currently shown on the timeline (legacy behavior).");

    public static string TipMarkerGridBar => Get(
        "タイムラインの表示倍率に関係なく、必ず小節単位でマーカーを付与します。",
        "Always snap markers to bars, regardless of zoom.");

    public static string TipMarkerGridBeat => Get(
        "タイムラインの表示倍率に関係なく、必ず拍単位でマーカーを付与します。",
        "Always snap markers to beats, regardless of zoom.");

    public static string TipMarkerCommentHeader => Get(
        "追加マーカーから生成する Wwise Custom Cue 名の規則を設定します。",
        "Rules for Wwise Custom Cue names generated from added markers.");

    public static string TipCommentDigits => Get(
        "連番の桁数を 1～6 で指定します。空欄または 0 の場合は連番自体を付けません。"
        + " 1 以上のときは、その桁で表せる最大値までしかマーカーを追加できません（例: 3 → 999 件）。",
        "Digit count 1–6. Empty or 0 disables numbering."
        + " When 1+, you can only add as many markers as that digit width allows (e.g. 3 → 999).");

    public static string TipCommentDigitsBox => Get(
        "連番の桁数です。空欄または 0 で連番なし、1～6 で連番ありになります。"
        + " 桁数を超える連番は追加できません。",
        "Digit count. Empty or 0 = no number; 1–6 enables numbering."
        + " Numbers beyond the digit width cannot be added.");

    public static string TipCommentZeroPad => Get(
        "オンの場合、Digits の桁数まで常に 0 で埋めます"
        + "（例: Digits=2 → 01、Digits=3 → 001、Digits=4 → 0001）。"
        + "オフのときは桁埋めせず 1, 2, 3… と表示します。",
        "When on, zero-pad to Digits (e.g. Digits=2 → 01, Digits=3 → 001)."
        + " When off, show 1, 2, 3… without padding.");

    public static string TipCommentResetPerPart => Get(
        "オンの場合、Music Playlist の各パート（書き出しファイル）ごとに連番を 1 へ戻します。",
        "When on, reset the serial number to 1 for each Music Playlist part (export file).");

    public static string TipCommentPrefix => Get(
        "入力がある場合、連番の前に接頭語を追加します。Digits が空欄または 0 のときは必須です。",
        "Optional prefix before the number. Required when Digits is empty or 0.");

    public static string TipCommentPrefixBox => Get(
        "Custom Cue 名の先頭に付ける文字列を入力します。空欄なら接頭語なし。"
        + " Digits が空欄または 0 のときは必須です。",
        "Text prepended to the Custom Cue name. Empty = no prefix."
        + " Required when Digits is empty or 0.");

    public static string TipCommentSuffix => Get(
        "入力がある場合、連番の後ろに接尾語を追加します。",
        "Optional suffix after the number.");

    public static string TipCommentSuffixBox => Get(
        "Custom Cue 名の連番より後ろに付ける文字列を入力します。空欄なら接尾語なし。Unicode 文字を使用できます。",
        "Text after the number in the Custom Cue name. Empty = no suffix. Unicode allowed.");

    public static string TipCommentSeparator => Get(
        "入力がある場合、接頭語／接尾語と連番の間に区切り文字を追加します。",
        "Optional separator between prefix/suffix and the number.");

    public static string TipCommentSeparatorBox => Get(
        "接頭語／接尾語と連番を繋ぐ文字列を入力します（例: _ または -）。空欄なら区切りなし。",
        "Separator between prefix/suffix and number (e.g. _ or -). Empty = none.");

    public static string TipCommentPreview => Get(
        "生成される Wwise Custom Cue 名の例と、名前が有効かどうかを表示します。",
        "Shows an example Wwise Custom Cue name and whether it is valid.");

    // --- Keep Target / status ---
    public static string TipKeepTargetUnlock => Get(
        "いまの作成先パスをアプリ側で固定します。"
        + " その後 Wwise 上で選択を変えても、表示と EXPORT 先はこの固定パスのままです。"
        + " 起動時／EXPORT 前には可能なら Wwise 上でも同じパスを再選択します。",
        "Lock the current destination path in the app."
        + " Later Wwise selection changes will not change the display or EXPORT target."
        + " On startup / before EXPORT, the same path is re-selected in Wwise when possible.");

    public static string TipKeepTargetLock => Get(
        "作成先の固定を解除します。",
        "Unlock the destination path.");

    public static string KeepTargetOnLabel => Get("- Keep Target -", "- Keep Target -");
    public static string KeepTargetOffLabel => Get("- Not Keep Target -", "- Not Keep Target -");

    // --- Transport ---
    private static string WithKeyRepeat(string japanese, string english) =>
        Get(
            japanese + Environment.NewLine + "長押しでキーリピート",
            english + Environment.NewLine + "Hold for key repeat");

    public static string TipTransportPlayPause => Get(
        "再生 / 一時停止  [Space]",
        "Play / Pause  [Space]");

    public static string TipTransportJumpToBar => Get(
        "小節番号を指定して移動  [G]",
        "Jump to bar number  [G]");

    public static string TipTransportGoToStart => WithKeyRepeat(
        "先頭へ移動  [Ctrl+Home]",
        "Go to start  [Ctrl+Home]");

    public static string TipTransportPreviousPage => WithKeyRepeat(
        "前の表示ページ  [Page Up]",
        "Previous view page  [Page Up]");

    public static string TipTransportPreviousPlaylist => WithKeyRepeat(
        "前の Music Playlist へ移動  [Ctrl+←]",
        "Previous Music Playlist  [Ctrl+←]");

    public static string TipTransportPreviousBar => WithKeyRepeat(
        "前の小節  [Home]",
        "Previous bar  [Home]");

    public static string TipTransportNextBar => WithKeyRepeat(
        "次の小節  [End]",
        "Next bar  [End]");

    public static string TipTransportNextPlaylist => WithKeyRepeat(
        "次の Music Playlist へ移動  [Ctrl+→]",
        "Next Music Playlist  [Ctrl+→]");

    public static string TipTransportNextPage => WithKeyRepeat(
        "次の表示ページ  [Page Down]",
        "Next view page  [Page Down]");

    public static string TipTransportGoToEnd => WithKeyRepeat(
        "末尾へ移動  [Ctrl+End]",
        "Go to end  [Ctrl+End]");

    public static string TipTransportTimeZoomIn => WithKeyRepeat(
        "時間軸を拡大  [↑]",
        "Zoom in time  [↑]");

    public static string TipTransportTimeZoomOut => WithKeyRepeat(
        "時間軸を縮小  [↓]",
        "Zoom out time  [↓]");

    public static string TipTransportTimeZoomMax => WithKeyRepeat(
        "時間軸を最大拡大  [Ctrl+↑]",
        "Max time zoom  [Ctrl+↑]");

    public static string TipTransportTimeZoomReset => WithKeyRepeat(
        "時間軸を全体表示  [Ctrl+↓]",
        "Fit time to view  [Ctrl+↓]");

    public static string TipTransportAmpZoomIn => WithKeyRepeat(
        "振幅を拡大  [Shift+↑]",
        "Zoom in amplitude  [Shift+↑]");

    public static string TipTransportAmpZoomOut => WithKeyRepeat(
        "振幅を縮小  [Shift+↓]",
        "Zoom out amplitude  [Shift+↓]");

    public static string TipTransportAmpZoomMax => WithKeyRepeat(
        "振幅を最大拡大  [Ctrl+Shift+↑]",
        "Max amplitude zoom  [Ctrl+Shift+↑]");

    public static string TipTransportAmpZoomReset => WithKeyRepeat(
        "振幅を既定に戻す  [Ctrl+Shift+↓]",
        "Reset amplitude zoom  [Ctrl+Shift+↓]");

    public static string TipForTransportCommand(TransportCommand command) => command switch
    {
        TransportCommand.TogglePlayback => TipTransportPlayPause,
        TransportCommand.JumpToBar => TipTransportJumpToBar,
        TransportCommand.GoToStart => TipTransportGoToStart,
        TransportCommand.PreviousPage => TipTransportPreviousPage,
        TransportCommand.PreviousPlaylist => TipTransportPreviousPlaylist,
        TransportCommand.PreviousBar => TipTransportPreviousBar,
        TransportCommand.NextBar => TipTransportNextBar,
        TransportCommand.NextPlaylist => TipTransportNextPlaylist,
        TransportCommand.NextPage => TipTransportNextPage,
        TransportCommand.GoToEnd => TipTransportGoToEnd,
        TransportCommand.TimeZoomIn => TipTransportTimeZoomIn,
        TransportCommand.TimeZoomOut => TipTransportTimeZoomOut,
        TransportCommand.TimeZoomMax => TipTransportTimeZoomMax,
        TransportCommand.TimeZoomReset => TipTransportTimeZoomReset,
        TransportCommand.AmpZoomIn => TipTransportAmpZoomIn,
        TransportCommand.AmpZoomOut => TipTransportAmpZoomOut,
        TransportCommand.AmpZoomMax => TipTransportAmpZoomMax,
        TransportCommand.AmpZoomReset => TipTransportAmpZoomReset,
        _ => string.Empty,
    };

    // --- Dialogs ---
    public static string DialogExitTitle => Get("終了確認", "Confirm exit");
    public static string DialogExitBody => Get(
        "アプリケーションを終了しますか？",
        "Do you want to exit the application?");

    public static string DialogDeleteProjectTitle => Get("プロジェクト削除", "Delete project");
    public static string DialogDeleteProjectBody(string name) => Format(
        "プロジェクト「{0}」を削除しますか？",
        "Delete project “{0}”?",
        name);

    public static string DialogCreateProjectFailedTitle => Get(
        "プロジェクトの作成に失敗",
        "Failed to create project");

    public static string DialogRenameFailedTitle => Get(
        "名前を変更できません",
        "Cannot rename");

    public static string DialogRenameFailedBody => Get(
        "ファイル名として使用できる、拡張子なしの名前を入力してください。",
        "Enter a valid file name without extension.");

    public static string DialogClearFailedTitle => Get(
        "クリアに失敗",
        "Clear failed");

    public static string DialogClearProjectFailedTitle => Get(
        "プロジェクトのクリアに失敗",
        "Failed to clear project");

    public static string DialogSaveFailedTitle => Get(
        "保存に失敗",
        "Save failed");

    public static string DialogSaveProjectFailedTitle => Get(
        "プロジェクトの保存に失敗",
        "Failed to save project");

    public static string DialogDeleteFailedTitle => Get(
        "削除に失敗",
        "Delete failed");

    public static string DialogLogCopyFailedTitle => Get(
        "ログのコピーに失敗",
        "Failed to copy log");

    public static string DialogLogSaveFailedTitle => Get(
        "ログの保存に失敗",
        "Failed to save log");

    public static string DialogLogSaveTitle => Get("ログを保存", "Save log");
    public static string DialogFolderBrowseDescription => Get(
        "波形の書き出し先フォルダを選択",
        "Select the folder for exported audio");

    public static string DialogExportTitle => Get("EXPORT", "EXPORT");
    public static string DialogOpenGithubFailed => Get(
        "GitHub を開けませんでした。",
        "Unable to open GitHub.");

    // --- Logs (user-facing) ---
    public static string LogKeepTargetNeedSelection => Get(
        "Keep Target : 作成先が表示されていないためオンにできません。"
        + " Wwise で作成先を選んでから再度オンにしてください。",
        "Keep Target : cannot enable because no target is shown."
        + " Select a target in Wwise, then enable again.");

    public static string LogKeepTargetOff => Get(
        "Keep Target : OFF（Wwise の選択に追従します）",
        "Keep Target : OFF (follows Wwise selection)");

    public static string LogKeepTargetOn(string path) => Format(
        "Keep Target : ON（このパスへ書き出します → {0}）",
        "Keep Target : ON (export to → {0})",
        path);

    public static string LogProjectCreated(string name) => Format(
        "=== Project ==={0}Message : プロジェクト「{1}」を作成しました（アプリ既定）。{0}{0}",
        "=== Project ==={0}Message : Created project “{1}” (app defaults).{0}{0}",
        Environment.NewLine,
        name);

    public static string LogProjectDeleted(string name) => Format(
        "=== Project ==={0}Message : プロジェクト「{1}」を削除しました。{0}{0}",
        "=== Project ==={0}Message : Deleted project “{1}”.{0}{0}",
        Environment.NewLine,
        name);

    public static string LogProjectCleared(string name) => Format(
        "=== Project ==={0}Message : プロジェクト「{1}」をクリアしました（アプリ既定）。{0}{0}",
        "=== Project ==={0}Message : Cleared project “{1}” (app defaults).{0}{0}",
        Environment.NewLine,
        name);

    public static string DialogDeleteProjectFailedTitle => Get(
        "プロジェクトの削除に失敗",
        "Failed to delete project");

    public static string LogExportPreflightHeader => "=== Export Preflight ===";
    public static string LogStatusOk => "OK";
    public static string LogStatusNg => "NG";
    public static string LogTargetUnselected => Get("（未選択）", "(none selected)");

    public static string PreflightNoParts => Get(
        "有効な出力パートがありません。",
        "No enabled output parts.");

    public static string PreflightNoOutputDir => Get(
        "書き出し先が未指定です。プロジェクト設定でフォルダを選択してください。",
        "Export folder is not set. Choose a folder in project settings.");

    public static string PreflightBadOutputPath(string message) => Format(
        "書き出し先パスが不正です: {0}",
        "Invalid export path: {0}",
        message);

    public static string PreflightOutputMissing => Get(
        "書き出し先フォルダが存在しません。",
        "Export folder does not exist.");

    public static string PreflightWaapiDisconnected => Get(
        "Wwise に接続されていません。WAAPI 有効化と Wwise の起動を確認してください。",
        "Not connected to Wwise. Enable WAAPI and ensure Wwise is running.");

    public static string PreflightKeepTargetNoPath => Get(
        "Keep Target がオンですが作成先パスが未設定です。"
        + " Wwise で作成先を選んでから Keep Target をオンにしてください。",
        "Keep Target is on but no target path is set."
        + " Select a target in Wwise, then enable Keep Target.");

    public static string PreflightNoSelection => Get(
        "Wwise 上で作成先オブジェクトが選択されていません。",
        "No destination object is selected in Wwise.");

    public static string PreflightNoProjectPath => Get(
        "Wwise プロジェクトのパスを取得できません。プロジェクトを開いているか確認してください。",
        "Cannot get the Wwise project path. Ensure a project is open.");

    public static string PreflightNoProjectRoot => Get(
        "Wwise プロジェクトのルートを解決できません。",
        "Cannot resolve the Wwise project root.");

    public static string PreflightOriginalsResolveFailed(string message) => Format(
        "Originals パスの解決に失敗: {0}",
        "Failed to resolve Originals path: {0}",
        message);

    public static string PreflightNotUnderOriginals => Get(
        "書き出し先は接続中 Wwise プロジェクトの Originals 配下である必要があります。",
        "Export folder must be under the connected Wwise project’s Originals.");

    public static string PreflightOkKeepTarget(string path) => Format(
        "書き出し可能です（Keep Target → {0} へ作成します）。",
        "Ready to export (Keep Target → create under {0}).",
        path);

    public static string PreflightOk => Get(
        "書き出し可能です。",
        "Ready to export.");

    // --- Wwise import progress (common lines) ---
    public static string LogWwiseImportHeader => "=== Wwise Import ===";
    public static string LogWwiseImportComplete => Get(
        "=== Wwise Import complete ===",
        "=== Wwise Import complete ===");

    public static string LogWwiseObjectsCreated => Get(
        "Wwise objects created.",
        "Wwise objects created.");

    public static string LogStateGroupUpdateExisting => Get(
        "StateGrp : 既存オブジェクトを変更",
        "StateGrp : updating existing object");

    public static string LogStateGroupCreateNew => Get(
        "StateGrp : 新規作成",
        "StateGrp : creating new");

    public static string LogCreatingStateGroup => Get(
        "Creating State Group...",
        "Creating State Group...");

    public static string LogCreatingMusicSwitch => Get(
        "Creating Music Switch Container...",
        "Creating Music Switch Container...");

    public static string LogCreatingPlaylist(int index, int total, string name) => Format(
        "Creating playlist {0}/{1}: {2}...",
        "Creating playlist {0}/{1}: {2}...",
        index,
        total,
        name);

    public static string LogBindingStates => Get(
        "Binding States to Playlists...",
        "Binding States to Playlists...");

    public static string LogConfiguringTransitions => Get(
        "Configuring transitions...",
        "Configuring transitions...");

    public static string LogCreatingWwiseObjects => Get(
        "Creating Wwise objects...",
        "Creating Wwise objects...");

    public static string LogTransitionAnyToPlaylist(
        string name,
        string exitSourceAt) => Format(
        "Transition : Any → {0} / Exit Source at={1} / Destination Sync To=Entry Cue / Fade-out ON",
        "Transition : Any → {0} / Exit Source at={1} / Destination Sync To=Entry Cue / Fade-out ON",
        name,
        exitSourceAt);

    public static string LogTransitionDestinationSet(string name) => Format(
        "Transition : Any → {0} の Destination を設定",
        "Transition : set Destination for Any → {0}",
        name);

    public static string LogCueTrimmed(string segmentPath, int deleted, string cueLabel) => Format(
        "Cue : {0} の余剰 {1} Cue を {2} 件削除",
        "Cue : removed {2} extra {1} cue(s) on {0}",
        segmentPath,
        cueLabel,
        deleted);
}
