namespace MgaWwiseImporter.Wave;

/// <summary>書き出し WAV に埋め込む cue／リージョン 1 件。</summary>
internal sealed class WavCueItem
{
    public required uint Id { get; init; }
    public required long SampleOffset { get; init; }
    public required long SampleLength { get; init; }
    /// <summary>labl / ltxt に出す表示名（例: <c>T120-4/4-A</c>）。</summary>
    public required string Comment { get; init; }
}
