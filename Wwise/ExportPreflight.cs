namespace MgaWwiseIMImporter.Wwise;

/// <summary>EXPORT / Wwise インポート前の書き出し先・接続・選択の検証結果。</summary>
internal sealed class ExportPreflightResult
{
    public required bool CanExport { get; init; }
    public required string Reason { get; init; }
    public string OutputDirectory { get; init; } = string.Empty;
    public string TargetPath { get; init; } = string.Empty;
    public string ProjectFilePath { get; init; } = string.Empty;
    public string OriginalsRoot { get; init; } = string.Empty;

    /// <summary>ログ用の要約（Reason と主要パス）。</summary>
    public string FormatLogMessage()
    {
        var lines = new List<string>
        {
            "=== Export Preflight ===",
            $"Status  : {(CanExport ? "OK" : "NG")}",
            $"Message : {Reason}",
        };

        if (OutputDirectory.Length > 0)
        {
            lines.Add($"Output  : {OutputDirectory}");
        }

        if (OriginalsRoot.Length > 0)
        {
            lines.Add($"Originals: {OriginalsRoot}");
        }

        if (ProjectFilePath.Length > 0)
        {
            lines.Add($"Project : {ProjectFilePath}");
        }

        if (TargetPath.Length > 0)
        {
            lines.Add($"Target  : {TargetPath}");
        }
        else if (!CanExport && Reason.Contains("未選択", StringComparison.Ordinal))
        {
            lines.Add("Target  : （未選択）");
        }

        lines.Add(string.Empty);
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }
}

/// <summary>
/// プロジェクト書き出し先と Wwise 接続／選択の整合を検証する。
/// 書き出し先は接続中プロジェクトの Originals 配下である必要がある。
/// </summary>
internal static class ExportPreflight
{
    public static ExportPreflightResult Evaluate(
        string? outputDirectory,
        WaapiProbeResult? waapi,
        bool hasEnabledParts)
    {
        if (!hasEnabledParts)
        {
            return Fail("有効な出力パートがありません。");
        }

        var directory = outputDirectory?.Trim() ?? string.Empty;
        if (directory.Length == 0)
        {
            return Fail("書き出し先が未指定です。プロジェクト設定でフォルダを選択してください。");
        }

        string fullDirectory;
        try
        {
            fullDirectory = Path.GetFullPath(directory);
        }
        catch (Exception ex)
        {
            return Fail($"書き出し先パスが不正です: {ex.Message}", directory);
        }

        if (!Directory.Exists(fullDirectory))
        {
            return Fail("書き出し先フォルダが存在しません。", fullDirectory);
        }

        if (waapi is null || !waapi.Ok)
        {
            return Fail(
                "Wwise に接続されていません。WAAPI 有効化と Wwise の起動を確認してください。",
                fullDirectory);
        }

        if (!waapi.HasSelection)
        {
            return Fail(
                "Wwise 上で作成先オブジェクトが選択されていません。",
                fullDirectory,
                projectFilePath: waapi.ProjectFilePath,
                targetPath: string.Empty);
        }

        var projectFilePath = waapi.ProjectFilePath.Trim();
        if (projectFilePath.Length == 0)
        {
            return Fail(
                "Wwise プロジェクトのパスを取得できません。プロジェクトを開いているか確認してください。",
                fullDirectory,
                targetPath: waapi.SelectedPath);
        }

        string originalsRoot;
        try
        {
            var projectRoot = Path.GetDirectoryName(Path.GetFullPath(projectFilePath));
            if (string.IsNullOrEmpty(projectRoot))
            {
                return Fail(
                    "Wwise プロジェクトのルートを解決できません。",
                    fullDirectory,
                    projectFilePath: projectFilePath,
                    targetPath: waapi.SelectedPath);
            }

            originalsRoot = Path.GetFullPath(Path.Combine(projectRoot, "Originals"));
        }
        catch (Exception ex)
        {
            return Fail(
                $"Originals パスの解決に失敗: {ex.Message}",
                fullDirectory,
                projectFilePath: projectFilePath,
                targetPath: waapi.SelectedPath);
        }

        if (!IsUnderDirectory(fullDirectory, originalsRoot))
        {
            return Fail(
                "書き出し先は接続中 Wwise プロジェクトの Originals 配下である必要があります。",
                fullDirectory,
                projectFilePath: projectFilePath,
                originalsRoot: originalsRoot,
                targetPath: waapi.SelectedPath);
        }

        return new ExportPreflightResult
        {
            CanExport = true,
            Reason = "書き出し可能です。",
            OutputDirectory = fullDirectory,
            TargetPath = waapi.SelectedPath,
            ProjectFilePath = Path.GetFullPath(projectFilePath),
            OriginalsRoot = originalsRoot,
        };
    }

    /// <summary>
    /// <paramref name="candidate"/> が <paramref name="root"/> 自身、またはその配下か。
    /// <c>Originals2</c> など境界名の誤判定を避けるため相対パスで判定する。
    /// </summary>
    public static bool IsUnderDirectory(string candidate, string root)
    {
        string fullCandidate;
        string fullRoot;
        try
        {
            fullCandidate = Path.GetFullPath(candidate)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            fullRoot = Path.GetFullPath(root)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
        catch
        {
            return false;
        }

        if (string.Equals(fullCandidate, fullRoot, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var relative = Path.GetRelativePath(fullRoot, fullCandidate);
        if (string.IsNullOrEmpty(relative)
            || relative == "."
            || Path.IsPathRooted(relative))
        {
            return false;
        }

        return !relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
            && !relative.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal)
            && relative != "..";
    }

    private static ExportPreflightResult Fail(
        string reason,
        string outputDirectory = "",
        string projectFilePath = "",
        string originalsRoot = "",
        string targetPath = "") =>
        new()
        {
            CanExport = false,
            Reason = reason,
            OutputDirectory = outputDirectory,
            ProjectFilePath = projectFilePath,
            OriginalsRoot = originalsRoot,
            TargetPath = targetPath,
        };
}
