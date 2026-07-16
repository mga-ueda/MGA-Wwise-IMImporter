using System.Globalization;
using System.Text;
using System.Xml.Linq;

namespace MgaWwiseIMImporter.Wave;

/// <summary>
/// WAV の iXML（BWFXML）チャンクから TimeReference を読む。
/// </summary>
internal static class WavIxmlReader
{
    public static bool TryReadTimeReference(byte[] chunkData, out ulong timeReferenceSamples)
    {
        timeReferenceSamples = 0;
        if (chunkData.Length == 0)
        {
            return false;
        }

        string text;
        try
        {
            var end = chunkData.Length;
            while (end > 0 && chunkData[end - 1] == 0)
            {
                end--;
            }

            text = Encoding.UTF8.GetString(chunkData, 0, end);
        }
        catch
        {
            return false;
        }

        XDocument doc;
        try
        {
            doc = XDocument.Parse(text);
        }
        catch
        {
            return false;
        }

        // iXML 内の BEXT 要素（RIFF bext チャンクとは別）
        var bext = doc.Root?.Element("BEXT");
        if (bext is null)
        {
            return false;
        }

        var lowText = bext.Element("BWF_TIME_REFERENCE_LOW")?.Value;
        var highText = bext.Element("BWF_TIME_REFERENCE_HIGH")?.Value;
        if (!ulong.TryParse(lowText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var low))
        {
            return false;
        }

        ulong high = 0;
        if (!string.IsNullOrWhiteSpace(highText)
            && !ulong.TryParse(highText, NumberStyles.Integer, CultureInfo.InvariantCulture, out high))
        {
            return false;
        }

        timeReferenceSamples = low + (high << 32);
        return true;
    }
}
