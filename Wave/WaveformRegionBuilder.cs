namespace MgaWwiseImporter.Wave;

/// <summary>
/// 出力計画用リージョンを、重なり・入れ子のない連続区画として構築する。
/// <para>
/// 境界はサンプル単位で厳密に隣接する（領域 i の終端 == 領域 i+1 の開始）。
/// </para>
/// <para>
/// 分割条件:
/// <list type="bullet">
/// <item>隣り合う小節頭の BPM が異なるとき、後ろの小節頭で分割（ランプ含む。小節途中イベントだけでは分割しない）。</item>
/// <item>拍子が変わる位置（拍子イベント）。</item>
/// <item>サイクルマーカーの In / Out。</item>
/// </list>
/// 例外:
/// <list type="bullet">
/// <item>波形冒頭が小節頭でないとき、次の小節線までを単独リージョンにする。</item>
/// <item>名前が -R で終わるサイクル範囲は Out で分割し、Out が小節途中なら次の小節頭までを 1 リージョンにする。範囲内は出力計画から除外（色分けのみ別扱い）。</item>
/// </list>
/// </para>
/// </summary>
internal static class WaveformRegionBuilder
{
    private const double PpqEpsilon = 1e-6;
    private const double BpmEpsilon = 1e-3;
    private const string ExcludeRangeSuffix = "-R";

    public static IReadOnlyList<WaveformRegionMark> Build(
        NuendoTracklistInfo tracklist,
        TempoMap tempoMap,
        uint sampleRate,
        long timelineOffset,
        long frameCount,
        double waveStartPpq,
        double waveEndPpq,
        IReadOnlyList<double> barBoundaries,
        IReadOnlyList<WaveformCycleMark> cycles)
    {
        if (frameCount <= 0 || sampleRate == 0)
        {
            return [];
        }

        var splitSamples = new SortedSet<long> { 0, frameCount };

        // 例外: 冒頭が小節頭でない → 次の小節線で区切り、冒頭半端小節だけを 1 リージョンにする
        if (!IsNearAny(barBoundaries, waveStartPpq))
        {
            var nextBar = BarGrid.FindNextBarPpq(barBoundaries, waveStartPpq);
            if (nextBar is double nextBarPpq
                && nextBarPpq <= waveEndPpq + PpqEpsilon)
            {
                TryAddSplitSample(
                    splitSamples,
                    tempoMap,
                    sampleRate,
                    timelineOffset,
                    frameCount,
                    nextBarPpq);
            }
        }

        // 小節頭 BPM が前の小節頭と違うとき分割（連続ランプで次小節頭の BPM が変わる場合を含む）
        AddSplitsWhereBarStartBpmChanges(
            splitSamples,
            tempoMap,
            sampleRate,
            timelineOffset,
            frameCount,
            waveStartPpq,
            waveEndPpq,
            barBoundaries);

        // 拍子変化
        foreach (var signature in tracklist.SignatureEvents)
        {
            var sigPpq = signature.Ppq;
            if (sigPpq < waveStartPpq - PpqEpsilon || sigPpq > waveEndPpq + PpqEpsilon)
            {
                continue;
            }

            TryAddSplitSample(
                splitSamples,
                tempoMap,
                sampleRate,
                timelineOffset,
                frameCount,
                sigPpq);
        }

        // サイクル In / Out。-R は Out が小節途中なら次小節頭までを 1 区画にするため分割を追加
        foreach (var cycle in cycles)
        {
            AddClampedSplit(splitSamples, cycle.StartSampleOffset, frameCount);
            AddClampedSplit(splitSamples, cycle.EndSampleOffset, frameCount);

            if (!IsExcludeRange(cycle))
            {
                continue;
            }

            var endAbsolute = timelineOffset + cycle.EndSampleOffset;
            var endPpq = tempoMap.FindPpqForSamples(endAbsolute, sampleRate);
            if (IsNearAny(barBoundaries, endPpq) || endPpq >= waveEndPpq - PpqEpsilon)
            {
                continue;
            }

            var nextBar = BarGrid.FindNextBarPpq(barBoundaries, endPpq);
            if (nextBar is double nextBarPpq
                && nextBarPpq <= waveEndPpq + PpqEpsilon)
            {
                TryAddSplitSample(
                    splitSamples,
                    tempoMap,
                    sampleRate,
                    timelineOffset,
                    frameCount,
                    nextBarPpq);
            }
        }

        var excludeRanges = cycles
            .Where(IsExcludeRange)
            .Select(c => (c.StartSampleOffset, c.EndSampleOffset))
            .ToList();

        var points = splitSamples.ToList();
        var regions = new List<WaveformRegionMark>();
        for (var i = 0; i + 1 < points.Count; i++)
        {
            var start = points[i];
            var end = points[i + 1];
            if (end <= start)
            {
                continue;
            }

            var mid = start + (end - start) / 2;
            var excluded = excludeRanges.Exists(r => mid >= r.StartSampleOffset && mid < r.EndSampleOffset);
            regions.Add(new WaveformRegionMark(start, end, excluded));
        }

        return regions;
    }

    /// <summary>
    /// 連続する着色（非除外）リージョンを 1 ファイル分にまとめる。
    /// -R などで色付けしない区間は区切りとなり、続く着色列は次の _n になる。
    /// </summary>
    public static IReadOnlyList<WaveformOutputPart> BuildOutputParts(
        IReadOnlyList<WaveformRegionMark> regions,
        string sourcePath)
    {
        var baseName = Path.GetFileNameWithoutExtension(sourcePath);
        if (string.IsNullOrEmpty(baseName))
        {
            baseName = "wave";
        }

        var parts = new List<WaveformOutputPart>();
        long? runStart = null;
        long runEnd = 0;
        var number = 1;

        void Flush()
        {
            if (runStart is not long start || runEnd <= start)
            {
                runStart = null;
                return;
            }

            parts.Add(new WaveformOutputPart(
                number,
                start,
                runEnd,
                $"{baseName}_{number}.wav"));
            number++;
            runStart = null;
        }

        foreach (var region in regions)
        {
            if (region.IsExcluded)
            {
                Flush();
                continue;
            }

            if (runStart is null)
            {
                runStart = region.StartSampleOffset;
            }

            runEnd = region.EndSampleOffset;
        }

        Flush();
        return parts;
    }

    private static void AddSplitsWhereBarStartBpmChanges(
        SortedSet<long> splitSamples,
        TempoMap tempoMap,
        uint sampleRate,
        long timelineOffset,
        long frameCount,
        double waveStartPpq,
        double waveEndPpq,
        IReadOnlyList<double> barBoundaries)
    {
        double? previousBarPpq = BarGrid.FindPreviousBarPpq(barBoundaries, waveStartPpq);
        // 波形先頭が小節頭そのもののとき、その線を「直前小節頭」扱いにする
        if (IsNearAny(barBoundaries, waveStartPpq))
        {
            previousBarPpq = waveStartPpq;
        }

        double? previousBpm = previousBarPpq is double prev
            ? tempoMap.GetBpmAt(prev)
            : null;

        foreach (var barPpq in barBoundaries)
        {
            if (barPpq < waveStartPpq - PpqEpsilon || barPpq > waveEndPpq + PpqEpsilon)
            {
                continue;
            }

            // 波形先頭ちょうどは既に 0 がある
            if (Math.Abs(barPpq - waveStartPpq) <= PpqEpsilon)
            {
                previousBarPpq = barPpq;
                previousBpm = tempoMap.GetBpmAt(barPpq);
                continue;
            }

            var bpm = tempoMap.GetBpmAt(barPpq);
            if (previousBpm is double pb && Math.Abs(bpm - pb) > BpmEpsilon)
            {
                TryAddSplitSample(
                    splitSamples,
                    tempoMap,
                    sampleRate,
                    timelineOffset,
                    frameCount,
                    barPpq);
            }

            previousBarPpq = barPpq;
            previousBpm = bpm;
        }
    }

    private static bool IsExcludeRange(WaveformCycleMark cycle)
    {
        return cycle.Comment.EndsWith(ExcludeRangeSuffix, StringComparison.OrdinalIgnoreCase);
    }

    private static void TryAddSplitSample(
        SortedSet<long> splits,
        TempoMap tempoMap,
        uint sampleRate,
        long timelineOffset,
        long frameCount,
        double ppq)
    {
        var local = tempoMap.PpqToSampleIndex(ppq, sampleRate) - timelineOffset;
        AddClampedSplit(splits, local, frameCount);
    }

    private static void AddClampedSplit(SortedSet<long> splits, long sample, long frameCount)
    {
        if (sample < 0 || sample > frameCount)
        {
            return;
        }

        splits.Add(sample);
    }

    private static bool IsNearAny(IReadOnlyList<double> values, double ppq)
    {
        foreach (var value in values)
        {
            if (Math.Abs(value - ppq) <= PpqEpsilon)
            {
                return true;
            }
        }

        return false;
    }
}
