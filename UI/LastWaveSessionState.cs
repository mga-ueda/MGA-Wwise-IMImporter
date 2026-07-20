using System.Text.Json;
using System.Text.Json.Serialization;
using MgaWwiseIMImporter.Wave;

namespace MgaWwiseIMImporter.UI;

/// <summary>
/// プロジェクト単位の Last Session 作業状態（グループ／無効化／トランジション設定／アプリ追加マーカー）。
/// exe 横 JSON サイドカーへ保存する。
/// </summary>
internal sealed class LastWaveSessionState
{
    public string WavePath { get; set; } = string.Empty;

    public List<LastWavePartSignature> Parts { get; set; } = [];

    /// <summary>パート番号 → グループ ID。</summary>
    public Dictionary<string, int> PartGroupIds { get; set; } = new(StringComparer.Ordinal);

    /// <summary>グループ ID → 色パレット index。</summary>
    public Dictionary<string, int> GroupColorIndexes { get; set; } = new(StringComparer.Ordinal);

    public int NextGroupId { get; set; } = 1;

    public int NextColorIndex { get; set; }

    public List<long> UserMarkerSampleOffsets { get; set; } = [];

    public List<int> DisabledPartNumbers { get; set; } = [];

    /// <summary>パート番号 → Exit Source At（列挙名）。</summary>
    public Dictionary<string, string> PartExitSourceModes { get; set; } = new(StringComparer.Ordinal);

    /// <summary>パート番号 → Fade In 秒数。</summary>
    public Dictionary<string, double> PartFadeInSeconds { get; set; } = new(StringComparer.Ordinal);

    /// <summary>パート番号 → Fade Out 秒数。</summary>
    public Dictionary<string, double> PartFadeOutSeconds { get; set; } = new(StringComparer.Ordinal);

    /// <summary>パート番号 → 同一グループ内遷移用 Fade In 秒数。</summary>
    public Dictionary<string, double> PartGroupFadeInSeconds { get; set; } = new(StringComparer.Ordinal);

    /// <summary>パート番号 → 同一グループ内遷移用 Fade Out 秒数。</summary>
    public Dictionary<string, double> PartGroupFadeOutSeconds { get; set; } = new(StringComparer.Ordinal);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static string SidecarPath(string projectName)
    {
        var safe = SanitizeFileName(projectName);
        return Path.Combine(
            Path.GetDirectoryName(IniFile.Path) ?? AppContext.BaseDirectory,
            $"MgaWwiseIMImporter.lastwave.{safe}.json");
    }

    public static string SanitizeFileName(string projectName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sb = new System.Text.StringBuilder(projectName.Length);
        foreach (var ch in projectName.Trim())
        {
            if (ch is '[' or ']' or '=' || invalid.Contains(ch))
            {
                sb.Append('_');
            }
            else
            {
                sb.Append(ch);
            }
        }

        var safe = sb.ToString().Trim();
        return safe.Length == 0 ? "Unnamed" : safe;
    }

    public static LastWaveSessionState Capture(
        string wavePath,
        IReadOnlyList<WaveformOutputPart> parts,
        IReadOnlyDictionary<int, int> partGroupIds,
        IReadOnlyDictionary<int, int> groupColorIndexes,
        int nextGroupId,
        int nextColorIndex,
        IEnumerable<long> userMarkerSampleOffsets,
        IEnumerable<int> disabledPartNumbers,
        IReadOnlyDictionary<int, PlaylistExitSourceMode> partExitSourceModes,
        IReadOnlyDictionary<int, double> partFadeInSeconds,
        IReadOnlyDictionary<int, double> partFadeOutSeconds,
        IReadOnlyDictionary<int, double> partGroupFadeInSeconds,
        IReadOnlyDictionary<int, double> partGroupFadeOutSeconds)
    {
        return new LastWaveSessionState
        {
            WavePath = NormalizePath(wavePath),
            Parts = parts
                .Select(part => new LastWavePartSignature
                {
                    Number = part.Number,
                    StartSampleOffset = part.StartSampleOffset,
                    EndSampleOffset = part.EndSampleOffset,
                })
                .ToList(),
            PartGroupIds = partGroupIds.ToDictionary(
                pair => pair.Key.ToString(),
                pair => pair.Value,
                StringComparer.Ordinal),
            GroupColorIndexes = groupColorIndexes.ToDictionary(
                pair => pair.Key.ToString(),
                pair => pair.Value,
                StringComparer.Ordinal),
            NextGroupId = Math.Max(1, nextGroupId),
            NextColorIndex = Math.Max(0, nextColorIndex),
            UserMarkerSampleOffsets = userMarkerSampleOffsets.Distinct().OrderBy(x => x).ToList(),
            DisabledPartNumbers = disabledPartNumbers.Distinct().OrderBy(x => x).ToList(),
            PartExitSourceModes = partExitSourceModes.ToDictionary(
                pair => pair.Key.ToString(),
                pair => pair.Value.ToString(),
                StringComparer.Ordinal),
            PartFadeInSeconds = partFadeInSeconds.ToDictionary(
                pair => pair.Key.ToString(),
                pair => pair.Value,
                StringComparer.Ordinal),
            PartFadeOutSeconds = partFadeOutSeconds.ToDictionary(
                pair => pair.Key.ToString(),
                pair => pair.Value,
                StringComparer.Ordinal),
            PartGroupFadeInSeconds = partGroupFadeInSeconds.ToDictionary(
                pair => pair.Key.ToString(),
                pair => pair.Value,
                StringComparer.Ordinal),
            PartGroupFadeOutSeconds = partGroupFadeOutSeconds.ToDictionary(
                pair => pair.Key.ToString(),
                pair => pair.Value,
                StringComparer.Ordinal),
        };
    }

    public static string NormalizePath(string path)
    {
        try
        {
            return Path.GetFullPath(path);
        }
        catch
        {
            return path.Trim();
        }
    }

    public string ToJson() => JsonSerializer.Serialize(this, JsonOptions);

    public static bool TryParse(string json, out LastWaveSessionState? state)
    {
        state = null;
        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            state = JsonSerializer.Deserialize<LastWaveSessionState>(json, JsonOptions);
            return state is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    public bool TryGetPartGroupIds(out Dictionary<int, int> partGroupIds)
    {
        partGroupIds = new Dictionary<int, int>();
        foreach (var pair in PartGroupIds)
        {
            if (!int.TryParse(pair.Key, out var partNumber))
            {
                return false;
            }

            partGroupIds[partNumber] = pair.Value;
        }

        return true;
    }

    public bool TryGetGroupColorIndexes(out Dictionary<int, int> groupColorIndexes)
    {
        groupColorIndexes = new Dictionary<int, int>();
        foreach (var pair in GroupColorIndexes)
        {
            if (!int.TryParse(pair.Key, out var groupId))
            {
                return false;
            }

            groupColorIndexes[groupId] = pair.Value;
        }

        return true;
    }

    public bool TryGetPartExitSourceModes(out Dictionary<int, PlaylistExitSourceMode> partExitSourceModes)
    {
        partExitSourceModes = new Dictionary<int, PlaylistExitSourceMode>();
        if (PartExitSourceModes is null || PartExitSourceModes.Count == 0)
        {
            return true;
        }

        foreach (var pair in PartExitSourceModes)
        {
            if (!int.TryParse(pair.Key, out var partNumber)
                || !Enum.TryParse<PlaylistExitSourceMode>(pair.Value, ignoreCase: true, out var mode))
            {
                return false;
            }

            partExitSourceModes[partNumber] = mode;
        }

        return true;
    }

    public bool TryGetPartFadeSeconds(
        out Dictionary<int, double> partFadeInSeconds,
        out Dictionary<int, double> partFadeOutSeconds,
        out Dictionary<int, double> partGroupFadeInSeconds,
        out Dictionary<int, double> partGroupFadeOutSeconds)
    {
        partFadeInSeconds = new Dictionary<int, double>();
        partFadeOutSeconds = new Dictionary<int, double>();
        partGroupFadeInSeconds = new Dictionary<int, double>();
        partGroupFadeOutSeconds = new Dictionary<int, double>();
        if (!TryParseFadeMap(PartFadeInSeconds, partFadeInSeconds)
            || !TryParseFadeMap(PartFadeOutSeconds, partFadeOutSeconds)
            || !TryParseFadeMap(PartGroupFadeInSeconds, partGroupFadeInSeconds)
            || !TryParseFadeMap(PartGroupFadeOutSeconds, partGroupFadeOutSeconds))
        {
            return false;
        }

        return true;
    }

    private static bool TryParseFadeMap(
        Dictionary<string, double>? source,
        Dictionary<int, double> destination)
    {
        if (source is null || source.Count == 0)
        {
            return true;
        }

        foreach (var pair in source)
        {
            if (!int.TryParse(pair.Key, out var partNumber))
            {
                return false;
            }

            destination[partNumber] = pair.Value;
        }

        return true;
    }
}

internal sealed class LastWavePartSignature
{
    public int Number { get; set; }

    public long StartSampleOffset { get; set; }

    public long EndSampleOffset { get; set; }

    public bool Matches(WaveformOutputPart part) =>
        Number == part.Number
        && StartSampleOffset == part.StartSampleOffset
        && EndSampleOffset == part.EndSampleOffset;
}
