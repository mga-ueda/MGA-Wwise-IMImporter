namespace MgaWwiseImporter.Wave;

internal sealed class WaveformPreviewData
{
    public WaveformPreviewData(
        WavPeakData peaks,
        string sourcePath,
        IReadOnlyList<WaveformBarMark>? bars = null,
        IReadOnlyList<WaveformMarkerMark>? markers = null,
        IReadOnlyList<WaveformCycleMark>? cycles = null,
        IReadOnlyList<WaveformRegionMark>? regions = null,
        IReadOnlyList<WaveformOutputPart>? outputParts = null)
    {
        Peaks = peaks;
        SourcePath = sourcePath;
        Bars = bars ?? [];
        Markers = markers ?? [];
        Cycles = cycles ?? [];
        Regions = regions ?? [];
        OutputParts = outputParts ?? [];
    }

    public WavPeakData Peaks { get; }
    public string SourcePath { get; }
    public IReadOnlyList<WaveformBarMark> Bars { get; }
    public IReadOnlyList<WaveformMarkerMark> Markers { get; }
    public IReadOnlyList<WaveformCycleMark> Cycles { get; }
    public IReadOnlyList<WaveformRegionMark> Regions { get; }
    public IReadOnlyList<WaveformOutputPart> OutputParts { get; }
}
