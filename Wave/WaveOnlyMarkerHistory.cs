namespace MgaWwiseIMImporter.Wave;

/// <summary>
/// Wave 単体モードのマーカー操作履歴（Undo / Redo）。
/// スナップショットはマーカー一覧のコピー。
/// </summary>
internal sealed class WaveOnlyMarkerHistory
{
    private readonly Stack<IReadOnlyList<WaveformMarkerMark>> _undo = new();
    private readonly Stack<IReadOnlyList<WaveformMarkerMark>> _redo = new();

    public void Clear()
    {
        _undo.Clear();
        _redo.Clear();
    }

    /// <summary>変更前の状態を Undo スタックへ積む（Redo は破棄）。</summary>
    public void PushBeforeChange(IReadOnlyList<WaveformMarkerMark> before)
    {
        _undo.Push(Clone(before));
        _redo.Clear();
    }

    public bool TryUndo(
        IReadOnlyList<WaveformMarkerMark> current,
        out IReadOnlyList<WaveformMarkerMark> restored)
    {
        if (_undo.Count == 0)
        {
            restored = [];
            return false;
        }

        _redo.Push(Clone(current));
        restored = _undo.Pop();
        return true;
    }

    public bool TryRedo(
        IReadOnlyList<WaveformMarkerMark> current,
        out IReadOnlyList<WaveformMarkerMark> restored)
    {
        if (_redo.Count == 0)
        {
            restored = [];
            return false;
        }

        _undo.Push(Clone(current));
        restored = _redo.Pop();
        return true;
    }

    private static IReadOnlyList<WaveformMarkerMark> Clone(IReadOnlyList<WaveformMarkerMark> source) =>
        source.Select(marker => new WaveformMarkerMark(
            marker.SampleOffset,
            marker.Comment,
            marker.IsSharedProjection,
            marker.IsFromWaveEmbedded)).ToArray();
}
