using System.Text;

namespace MgaWwiseImporter.Wave;

/// <summary>
/// WAV の data チャンクから表示用ピーク（min/max）を読み取る。
/// </summary>
internal static class WavPeakReader
{
    public static WavPeakData Read(WavFileInfo info, int peakCount)
    {
        if (peakCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(peakCount));
        }

        if (info.FrameCount <= 0 || info.BlockAlign == 0 || info.Channels == 0)
        {
            return WavPeakData.Empty;
        }

        using var stream = File.OpenRead(info.Path);
        using var reader = new BinaryReader(stream, Encoding.ASCII, leaveOpen: false);

        if (!TryFindDataChunk(stream, reader, out var dataStart, out var dataSize))
        {
            throw new InvalidDataException("data チャンクが見つかりません。");
        }

        var frameCount = (int)Math.Min(info.FrameCount, dataSize / info.BlockAlign);
        if (frameCount <= 0)
        {
            return WavPeakData.Empty;
        }

        var buckets = Math.Min(peakCount, frameCount);
        var mins = new float[buckets];
        var maxs = new float[buckets];

        var channels = info.Channels;
        var bytesPerSample = info.BitsPerSample / 8;
        if (bytesPerSample <= 0)
        {
            throw new InvalidDataException("BitsPerSample が不正です。");
        }

        var sampleReader = CreateSampleReader(info.AudioFormat, info.BitsPerSample);
        var frameBuffer = new byte[info.BlockAlign];

        for (var bucket = 0; bucket < buckets; bucket++)
        {
            var startFrame = (int)((long)bucket * frameCount / buckets);
            var endFrame = (int)((long)(bucket + 1) * frameCount / buckets);
            if (endFrame <= startFrame)
            {
                endFrame = startFrame + 1;
            }

            var framesInBucket = endFrame - startFrame;
            // バケット内を間引きして読む（精度と速度のバランス）
            var step = Math.Max(1, framesInBucket / 48);
            var min = float.MaxValue;
            var max = float.MinValue;

            for (var frame = startFrame; frame < endFrame; frame += step)
            {
                stream.Position = dataStart + (long)frame * info.BlockAlign;
                var read = stream.Read(frameBuffer, 0, frameBuffer.Length);
                if (read < frameBuffer.Length)
                {
                    break;
                }

                float mono = 0;
                for (var ch = 0; ch < channels; ch++)
                {
                    mono += sampleReader(frameBuffer, ch * bytesPerSample);
                }

                mono /= channels;
                if (mono < min)
                {
                    min = mono;
                }

                if (mono > max)
                {
                    max = mono;
                }
            }

            if (min > max)
            {
                min = 0;
                max = 0;
            }

            mins[bucket] = min;
            maxs[bucket] = max;
        }

        return new WavPeakData(mins, maxs, info.FrameCount, info.SampleRate);
    }

    private static Func<byte[], int, float> CreateSampleReader(ushort audioFormat, ushort bitsPerSample)
    {
        if (audioFormat == 3 && bitsPerSample == 32)
        {
            return (buffer, offset) => BitConverter.ToSingle(buffer, offset);
        }

        if (audioFormat is 1 or 65534)
        {
            return bitsPerSample switch
            {
                8 => (buffer, offset) => (buffer[offset] - 128) / 128f,
                16 => (buffer, offset) => BitConverter.ToInt16(buffer, offset) / 32768f,
                24 => (buffer, offset) =>
                {
                    var value = buffer[offset]
                        | (buffer[offset + 1] << 8)
                        | (buffer[offset + 2] << 16);
                    if ((value & 0x800000) != 0)
                    {
                        value |= unchecked((int)0xFF000000);
                    }

                    return value / 8388608f;
                },
                32 => (buffer, offset) => BitConverter.ToInt32(buffer, offset) / 2147483648f,
                _ => throw new NotSupportedException($"{bitsPerSample} bit PCM は未対応です。"),
            };
        }

        throw new NotSupportedException($"AudioFormat={audioFormat} は波形表示未対応です。");
    }

    private static bool TryFindDataChunk(
        Stream stream,
        BinaryReader reader,
        out long dataStart,
        out uint dataSize)
    {
        dataStart = 0;
        dataSize = 0;

        stream.Position = 0;
        if (ReadFourCc(reader) != "RIFF")
        {
            return false;
        }

        _ = reader.ReadUInt32();
        if (ReadFourCc(reader) != "WAVE")
        {
            return false;
        }

        while (stream.Position + 8 <= stream.Length)
        {
            var chunkId = ReadFourCc(reader);
            var chunkSize = reader.ReadUInt32();
            var chunkDataStart = stream.Position;

            if (chunkId == "data")
            {
                dataStart = chunkDataStart;
                dataSize = chunkSize;
                return true;
            }

            var paddedSize = chunkSize + (chunkSize & 1);
            stream.Position = chunkDataStart + paddedSize;
        }

        return false;
    }

    private static string ReadFourCc(BinaryReader reader)
    {
        return Encoding.ASCII.GetString(reader.ReadBytes(4));
    }
}

internal sealed class WavPeakData
{
    public static WavPeakData Empty { get; } = new([], [], 0, 0);

    public WavPeakData(float[] mins, float[] maxs, long frameCount, uint sampleRate = 0)
    {
        Mins = mins;
        Maxs = maxs;
        FrameCount = frameCount;
        SampleRate = sampleRate;
    }

    public float[] Mins { get; }
    public float[] Maxs { get; }
    public long FrameCount { get; }
    public uint SampleRate { get; }
    public double DurationSeconds => SampleRate == 0 ? 0 : FrameCount / (double)SampleRate;
    public bool IsEmpty => Mins.Length == 0 || FrameCount <= 0;
}
