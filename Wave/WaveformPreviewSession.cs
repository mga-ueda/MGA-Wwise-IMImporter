namespace MgaWwiseIMImporter.Wave;

/// <summary>
/// 読み込み元の不変プレビューと、現在の読み込み中だけ有効な追加マーカーを管理する。
/// </summary>
internal sealed class WaveformPreviewSession
{
    private readonly List<UserWaveformMarker> _userMarkers = [];
    private IReadOnlyList<WaveformMarkerMark> _effectiveMarkers;
    private IReadOnlyList<WaveformMarkerMark> _wwiseMarkers;

    public WaveformPreviewSession(WaveformPreviewData preview)
    {
        Preview = preview;
        _effectiveMarkers = preview.Markers;
        _wwiseMarkers = preview.Markers;
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
        var userMarkerMarks = orderedUserMarkers
            .Select((marker, index) =>
                new WaveformMarkerMark(marker.SampleOffset, $"{index + 1:000}"))
            .ToArray();

        _effectiveMarkers = Preview.Markers
            .Concat(userMarkerMarks)
            .OrderBy(marker => marker.SampleOffset)
            .ToArray();

        _wwiseMarkers = _effectiveMarkers;
    }
}

internal readonly record struct UserWaveformMarker(Guid Id, long SampleOffset);
