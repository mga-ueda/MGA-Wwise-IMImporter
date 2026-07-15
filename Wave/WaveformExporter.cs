using System.Globalization;
using System.Text;

namespace MgaWwiseImporter.Wave;

/// <summary>
/// 出力計画（<see cref="WaveformOutputPart"/>）に従い、リージョン付き WAV を分割書き出しする。
/// 除外区画（-R など）は書き出さず、その前後でファイルが分かれる。
/// </summary>
internal static class WaveformExporter
{
    public static string Export(
        string sourcePath,
        WavFileInfo wavInfo,
        IReadOnlyList<WaveformOutputPart> outputParts,
        IReadOnlyList<WaveformRegionMark> regions,
        IReadOnlyList<WaveformBarMark> bars,
        Action<WaveformOutputPart>? onPartBegin = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== Export ===");
        sb.AppendLine($"Source : {sourcePath}");
        sb.AppendLine($"Parts  : {outputParts.Count}");

        if (outputParts.Count == 0)
        {
            sb.AppendLine("Message : 出力パートが無いため書き出しをスキップします。");
            sb.AppendLine();
            return sb.ToString();
        }

        var directory = Path.GetDirectoryName(sourcePath) ?? string.Empty;
        sb.AppendLine($"OutputDir : {directory}");
        sb.AppendLine();

        var written = 0;
        foreach (var part in outputParts)
        {
            onPartBegin?.Invoke(part);

            var destPath = Path.Combine(directory, part.FileName);
            try
            {
                var cues = BuildCuesForPart(part, regions, bars);
                WavCueWriter.WriteSegment(
                    sourcePath,
                    destPath,
                    part.StartSampleOffset,
                    part.EndSampleOffset,
                    wavInfo.BlockAlign,
                    cues);

                var info = new FileInfo(destPath);
                var frames = part.EndSampleOffset - part.StartSampleOffset;
                var durationSec = wavInfo.SampleRate == 0
                    ? 0d
                    : frames / (double)wavInfo.SampleRate;

                sb.AppendLine($"--- Part #{part.Number} ---");
                sb.AppendLine($"File    : {destPath}");
                sb.AppendLine(
                    $"Samples : [{part.StartSampleOffset:N0} .. {part.EndSampleOffset:N0})"
                    + $"  frames={frames:N0}"
                    + $"  duration={durationSec:0.000}s");
                sb.AppendLine($"Size    : {info.Length:N0} bytes");
                sb.AppendLine($"Regions : {cues.Count}");
                foreach (var cue in cues)
                {
                    sb.AppendLine(
                        $"  - Region#{cue.Id}"
                        + $"  sample={cue.SampleOffset.ToString(CultureInfo.InvariantCulture)}"
                        + $"  length={cue.SampleLength.ToString(CultureInfo.InvariantCulture)}"
                        + $"  \"{cue.Comment}\"");
                }

                sb.AppendLine();
                written++;
            }
            catch (Exception ex)
            {
                sb.AppendLine("=== エラー ===");
                sb.AppendLine($"File    : {destPath}");
                sb.AppendLine($"Message : {ex.Message}");
                sb.AppendLine();
            }
        }

        sb.AppendLine("=== Export complete ===");
        sb.AppendLine($"Written : {written} / {outputParts.Count}");
        sb.AppendLine($"Time    : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();
        return sb.ToString();
    }

    private static IReadOnlyList<WavCueItem> BuildCuesForPart(
        WaveformOutputPart part,
        IReadOnlyList<WaveformRegionMark> regions,
        IReadOnlyList<WaveformBarMark> bars)
    {
        var cues = new List<WavCueItem>();
        uint nextId = 1;

        foreach (var region in regions)
        {
            if (region.IsExcluded)
            {
                continue;
            }

            // パート範囲と重なる着色リージョンのみ（パートは連続着色のまとまりなので通常は完全包含）
            var start = Math.Max(region.StartSampleOffset, part.StartSampleOffset);
            var end = Math.Min(region.EndSampleOffset, part.EndSampleOffset);
            if (end <= start)
            {
                continue;
            }

            var localStart = start - part.StartSampleOffset;
            var length = end - start;
            // NameSuffix は RegionBuilder が付与（例: " -L" / " -A"。-R は除外のためここには来ない）
            var comment = BuildRegionComment(start, bars) + region.NameSuffix;

            cues.Add(new WavCueItem
            {
                Id = nextId++,
                SampleOffset = localStart,
                SampleLength = length,
                Comment = comment,
            });
        }

        return cues;
    }

    private static string BuildRegionComment(long regionStartSample, IReadOnlyList<WaveformBarMark> bars)
    {
        WaveformBarMark? barHead = null;
        WaveformBarMark? any = null;
        foreach (var bar in bars.OrderBy(b => b.SampleOffset))
        {
            if (bar.SampleOffset > regionStartSample)
            {
                break;
            }

            any = bar;
            if (!bar.IsTempoChangeOnly)
            {
                barHead = bar;
            }
        }

        if ((barHead ?? any) is not WaveformBarMark mark)
        {
            return string.Empty;
        }

        var bpmText = Math.Round(mark.Bpm, MidpointRounding.AwayFromZero)
            .ToString(CultureInfo.InvariantCulture);
        return $"T{bpmText}-{mark.Numerator}/{mark.Denominator}";
    }
}
