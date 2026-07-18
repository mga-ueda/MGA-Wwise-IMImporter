using System.Globalization;

namespace MgaWwiseIMImporter.Wave;

/// <summary>
/// UI で追加したマーカーのコメント（連番）生成ルール。
/// 接頭語・接尾語・連結文字は「無し」の場合に空文字で渡す。
/// </summary>
internal readonly record struct MarkerCommentRule(
    int Digits,
    bool ZeroPad,
    string Prefix,
    string Suffix,
    string Joiner,
    bool ResetPerPart)
{
    public static MarkerCommentRule Default { get; } = new(
        Digits: 3,
        ZeroPad: true,
        Prefix: string.Empty,
        Suffix: string.Empty,
        Joiner: string.Empty,
        ResetPerPart: false);

    public string Format(int number)
    {
        var parts = new List<string>(3);
        if (Prefix.Length > 0)
        {
            parts.Add(Prefix);
        }

        if (Digits > 0)
        {
            var numberText = number.ToString(CultureInfo.InvariantCulture);
            if (ZeroPad)
            {
                numberText = numberText.PadLeft(Digits, '0');
            }

            parts.Add(numberText);
        }

        if (Suffix.Length > 0)
        {
            parts.Add(Suffix);
        }

        return string.Join(Joiner, parts);
    }
}

/// <summary>
/// 読み込み元の不変プレビューと、現在の読み込み中だけ有効な追加マーカーを管理する。
/// </summary>
internal sealed class WaveformPreviewSession
{
    private readonly List<UserWaveformMarker> _userMarkers = [];
    private IReadOnlyList<WaveformMarkerMark> _effectiveMarkers;
    private IReadOnlyList<WaveformMarkerMark> _wwiseMarkers;
    private MarkerCommentRule _commentRule = MarkerCommentRule.Default;

    public WaveformPreviewSession(WaveformPreviewData preview)
    {
        Preview = preview;
        _effectiveMarkers = preview.Markers;
        _wwiseMarkers = preview.Markers;
    }

    /// <summary>コメント生成ルールを差し替え、既存の追加マーカーへ再適用する。</summary>
    public void SetCommentRule(MarkerCommentRule rule)
    {
        if (_commentRule == rule)
        {
            return;
        }

        _commentRule = rule;
        if (_userMarkers.Count > 0)
        {
            RebuildMarkerSnapshots();
        }
    }

    public WaveformPreviewData Preview { get; }

    public IReadOnlyList<WaveformMarkerMark> EffectiveMarkers => _effectiveMarkers;

    public IReadOnlyList<WaveformMarkerMark> WwiseMarkers => _wwiseMarkers;

    public bool AddMarkers(IEnumerable<long> sampleOffsets)
    {
        var existing = _userMarkers.Select(marker => marker.SampleOffset).ToHashSet();
        var changed = false;
        foreach (var sampleOffset in sampleOffsets.Distinct())
        {
            if (!IsEditableMarkerSample(sampleOffset)
                || !existing.Add(sampleOffset))
            {
                continue;
            }

            _userMarkers.Add(new UserWaveformMarker(Guid.NewGuid(), sampleOffset));
            changed = true;
        }

        if (changed)
        {
            RebuildMarkerSnapshots();
        }

        return changed;
    }

    public bool RemoveMarkers(IEnumerable<long> sampleOffsets)
    {
        var removals = sampleOffsets.ToHashSet();
        if (removals.Count == 0)
        {
            return false;
        }

        var removed = _userMarkers.RemoveAll(marker => removals.Contains(marker.SampleOffset));
        if (removed > 0)
        {
            RebuildMarkerSnapshots();
        }

        return removed > 0;
    }

    private bool IsEditableMarkerSample(long sampleOffset)
    {
        if (sampleOffset < 0 || sampleOffset >= Preview.WavInfo.FrameCount)
        {
            return false;
        }

        var insidePlaylist = Preview.OutputParts.Any(part =>
            sampleOffset >= part.StartSampleOffset
            && sampleOffset < part.EndSampleOffset);
        if (!insidePlaylist)
        {
            return false;
        }

        return !Preview.Regions.Any(region =>
            sampleOffset >= region.StartSampleOffset
            && sampleOffset < region.EndSampleOffset
            && (string.Equals(region.NameSuffix, "-A", StringComparison.OrdinalIgnoreCase)
                || string.Equals(region.NameSuffix, "-E", StringComparison.OrdinalIgnoreCase)));
    }

    private void RebuildMarkerSnapshots()
    {
        var orderedUserMarkers = _userMarkers
            .OrderBy(marker => marker.SampleOffset)
            .ToArray();
        var userMarkerMarks = new WaveformMarkerMark[orderedUserMarkers.Length];
        var globalNumber = 0;
        var partNumber = 0;
        var currentPartIndex = -1;
        for (var i = 0; i < orderedUserMarkers.Length; i++)
        {
            var marker = orderedUserMarkers[i];
            globalNumber++;
            var number = globalNumber;
            if (_commentRule.ResetPerPart)
            {
                var partIndex = FindOutputPartIndex(marker.SampleOffset);
                if (partIndex != currentPartIndex)
                {
                    currentPartIndex = partIndex;
                    partNumber = 0;
                }

                partNumber++;
                number = partNumber;
            }

            userMarkerMarks[i] = new WaveformMarkerMark(
                marker.SampleOffset,
                _commentRule.Format(number));
        }

        _effectiveMarkers = Preview.Markers
            .Concat(userMarkerMarks)
            .OrderBy(marker => marker.SampleOffset)
            .ToArray();

        _wwiseMarkers = _effectiveMarkers;
    }

    private int FindOutputPartIndex(long sampleOffset)
    {
        for (var i = 0; i < Preview.OutputParts.Count; i++)
        {
            var part = Preview.OutputParts[i];
            if (sampleOffset >= part.StartSampleOffset
                && sampleOffset < part.EndSampleOffset)
            {
                return i;
            }
        }

        return -1;
    }
}

internal readonly record struct UserWaveformMarker(Guid Id, long SampleOffset);
