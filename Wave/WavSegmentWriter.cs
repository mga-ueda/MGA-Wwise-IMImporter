using System.Text;

namespace MgaWwiseIMImporter.Wave;

/// <summary>
/// ソース WAV の指定サンプル範囲だけを切り出して書き出す（メタデータ埋め込みなし）。
/// </summary>
internal static class WavSegmentWriter
{
    /// <summary>
    /// ソース WAV の指定サンプル範囲を切り出して書き出す。
    /// </summary>
    public static void WriteSegment(
        string sourcePath,
        string destinationPath,
        long startSample,
        long endSample,
        ushort blockAlign)
    {
        if (blockAlign == 0)
        {
            throw new InvalidDataException("BlockAlign が不正です。");
        }

        if (endSample <= startSample)
        {
            throw new ArgumentOutOfRangeException(nameof(endSample), "書き出し範囲が空です。");
        }

        using var source = File.OpenRead(sourcePath);
        using var reader = new BinaryReader(source, Encoding.ASCII, leaveOpen: true);

        if (ReadFourCc(reader) != "RIFF")
        {
            throw new InvalidDataException("RIFF ヘッダーではありません。");
        }

        _ = reader.ReadUInt32();
        if (ReadFourCc(reader) != "WAVE")
        {
            throw new InvalidDataException("WAVE 形式ではありません。");
        }

        byte[]? fmtData = null;
        long dataStart = -1;
        uint dataSize = 0;

        while (source.Position + 8 <= source.Length)
        {
            var id = ReadFourCc(reader);
            var size = reader.ReadUInt32();
            var chunkDataStart = source.Position;
            if (chunkDataStart + size > source.Length)
            {
                throw new InvalidDataException($"チャンクサイズが不正です: {id}");
            }

            if (id == "fmt ")
            {
                fmtData = reader.ReadBytes((int)size);
            }
            else if (id == "data")
            {
                dataStart = chunkDataStart;
                dataSize = size;
            }

            var paddedSize = size + (size & 1);
            source.Position = chunkDataStart + paddedSize;
        }

        if (fmtData is null)
        {
            throw new InvalidDataException("fmt チャンクが見つかりません。");
        }

        if (dataStart < 0)
        {
            throw new InvalidDataException("data チャンクが見つかりません。");
        }

        var startByte = checked(startSample * (long)blockAlign);
        var endByte = checked(endSample * (long)blockAlign);
        if (startByte < 0 || endByte > dataSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(endSample),
                $"書き出し範囲が data 外です: samples=[{startSample}..{endSample})"
                + $" dataFrames={dataSize / blockAlign}");
        }

        var segmentByteLength = checked((int)(endByte - startByte));
        var contentSize = 4
            + 8 + fmtData.Length + (fmtData.Length & 1)
            + 8 + segmentByteLength + (segmentByteLength & 1);

        using var dest = File.Create(destinationPath);
        using var writer = new BinaryWriter(dest, Encoding.ASCII, leaveOpen: true);

        writer.Write(Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(contentSize);
        writer.Write(Encoding.ASCII.GetBytes("WAVE"));

        WriteChunk(writer, "fmt ", fmtData);

        writer.Write(Encoding.ASCII.GetBytes("data"));
        writer.Write(segmentByteLength);
        source.Position = dataStart + startByte;
        CopyExact(source, dest, segmentByteLength);
        if ((segmentByteLength & 1) == 1)
        {
            writer.Write((byte)0);
        }
    }

    private static void WriteChunk(BinaryWriter writer, string id, byte[] data)
    {
        writer.Write(Encoding.ASCII.GetBytes(id));
        writer.Write(data.Length);
        writer.Write(data);
        if ((data.Length & 1) == 1)
        {
            writer.Write((byte)0);
        }
    }

    private static void CopyExact(Stream source, Stream destination, int byteCount)
    {
        var buffer = new byte[Math.Min(byteCount, 1024 * 256)];
        var remaining = byteCount;
        while (remaining > 0)
        {
            var toRead = Math.Min(buffer.Length, remaining);
            var read = source.Read(buffer, 0, toRead);
            if (read <= 0)
            {
                throw new EndOfStreamException("data チャンクの読み取りが途中で終わりました。");
            }

            destination.Write(buffer, 0, read);
            remaining -= read;
        }
    }

    private static string ReadFourCc(BinaryReader reader)
    {
        return Encoding.ASCII.GetString(reader.ReadBytes(4));
    }
}
