using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using MgaWwiseIMImporter.UI;

namespace MgaWwiseIMImporter.Wwise;

/// <summary>
/// ロック中 Wwise プロジェクトを開く／既に開いていれば前面化する。
/// WAAPI が使えるときは RPC、だめなときは .wproj のシェル実行にフォールバック。
/// </summary>
internal static class WwiseProjectActivator
{
    [DllImport("user32.dll")]
    private static extern bool AllowSetForegroundWindow(int dwProcessId);

    public static async Task<(bool Ok, string Message)> OpenOrFocusAsync(
        WaapiSettings settings,
        string projectFilePath,
        CancellationToken cancellationToken = default)
    {
        var path = projectFilePath.Trim();
        if (path.Length == 0)
        {
            return (false, UiStrings.LogWwiseProjectPathMissing);
        }

        if (!File.Exists(path))
        {
            return (false, UiStrings.LogWwiseProjectFileMissing(path));
        }

        try
        {
            using var client = new WaapiHttpClient(
                settings.Url,
                TimeSpan.FromMilliseconds(Math.Max(settings.TimeoutMs, 10000)));

            var info = await client.CallAsync("ak.wwise.core.getInfo", cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            if (TryGetProcessId(info, out var processId))
            {
                _ = AllowSetForegroundWindow(processId);
            }

            var currentPath = string.Empty;
            try
            {
                var project = await client.CallAsync(
                        "ak.wwise.core.getProjectInfo",
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                currentPath = ReadProjectFilePath(project);
            }
            catch
            {
                // プロジェクト未ロード
            }

            if (PathsEqual(currentPath, path))
            {
                await client.CallAsync(
                        "ak.wwise.ui.bringToForeground",
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                return (true, UiStrings.LogWwiseProjectBroughtToFront(Path.GetFileNameWithoutExtension(path)));
            }

            await client.CallAsync(
                    "ak.wwise.ui.project.open",
                    new Dictionary<string, object?> { ["path"] = path },
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (TryGetProcessId(info, out processId))
            {
                _ = AllowSetForegroundWindow(processId);
            }

            try
            {
                await client.CallAsync(
                        "ak.wwise.ui.bringToForeground",
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
            catch
            {
                // open 直後は前面化に失敗することがある（ロード中など）。開ければ成功扱い。
            }

            return (true, UiStrings.LogWwiseProjectOpened(Path.GetFileNameWithoutExtension(path)));
        }
        catch (Exception)
        {
            return OpenViaShell(path);
        }
    }

    private static (bool Ok, string Message) OpenViaShell(string path)
    {
        try
        {
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            return (true, UiStrings.LogWwiseProjectShellOpen(Path.GetFileNameWithoutExtension(path)));
        }
        catch (Exception ex)
        {
            return (false, UiStrings.LogWwiseProjectOpenFailed(ex.Message));
        }
    }

    private static bool PathsEqual(string a, string b)
    {
        if (a.Length == 0 || b.Length == 0)
        {
            return false;
        }

        try
        {
            return string.Equals(
                Path.GetFullPath(a).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                Path.GetFullPath(b).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);
        }
    }

    private static string ReadProjectFilePath(JsonElement project)
    {
        if (TryGetString(project, "path", out var path))
        {
            return path;
        }

        if (TryGetString(project, "filePath", out var filePath))
        {
            return filePath;
        }

        return string.Empty;
    }

    private static bool TryGetProcessId(JsonElement info, out int processId)
    {
        processId = 0;
        if (info.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        if (info.TryGetProperty("processId", out var prop)
            && prop.ValueKind == JsonValueKind.Number
            && prop.TryGetInt32(out processId)
            && processId > 0)
        {
            return true;
        }

        if (info.TryGetProperty("pid", out prop)
            && prop.ValueKind == JsonValueKind.Number
            && prop.TryGetInt32(out processId)
            && processId > 0)
        {
            return true;
        }

        return false;
    }

    private static bool TryGetString(JsonElement element, string propertyName, out string value)
    {
        value = string.Empty;
        if (element.ValueKind != JsonValueKind.Object
            || !element.TryGetProperty(propertyName, out var property)
            || property.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        value = property.GetString() ?? string.Empty;
        return value.Length > 0;
    }
}
