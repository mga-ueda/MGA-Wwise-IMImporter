using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace MgaWwiseIMImporter.UI;

/// <summary>
/// 埋め込みフォントをプロセス内で利用可能にする。
/// ログは RichTextBox（RichEdit）のため AddMemoryFont だけでは実描画に使われず、
/// プロポーショナルへ落ちることがある。一時ファイルへ展開して AddFontResourceEx する。
/// </summary>
internal static class AppFonts
{
    private const uint FrPrivate = 0x10;
    private static string? _registeredPath;

    // AddFontResourceEx(FR_PRIVATE) だけでは GDI+ の new Font(名前, ...) から
    // フォントが見つからず既定フォントへ化けるため、GDI+ 用に別途保持する。
    private static PrivateFontCollection? _privateFonts;

    public static void EnsureRegistered()
    {
        if (_registeredPath is not null)
        {
            return;
        }

        try
        {
            using var stream = AppEmbeddedResources.OpenLogFont();
            if (stream is null)
            {
                return;
            }

            var fontData = new byte[stream.Length];
            stream.ReadExactly(fontData);

            var path = EnsureExtractedFontFile(fontData);
            if (AddFontResourceEx(path, FrPrivate, IntPtr.Zero) <= 0)
            {
                return;
            }

            _registeredPath = path;
            var collection = new PrivateFontCollection();
            collection.AddFontFile(path);
            _privateFonts = collection;
        }
        catch (Exception)
        {
            _privateFonts = null;
            _registeredPath = null;
        }

        Application.ApplicationExit += (_, _) => Unregister();
    }

    /// <summary>ログ表示用の等幅フォントを生成する。同梱フォントが使えない場合は Consolas。</summary>
    public static Font CreateLogFont(float sizePt)
    {
        var family = _privateFonts?.Families is { Length: > 0 } families ? families[0] : null;
        return family is not null
            ? new Font(family, sizePt, FontStyle.Regular, GraphicsUnit.Point)
            : new Font("Consolas", sizePt, FontStyle.Regular, GraphicsUnit.Point);
    }

    private static string EnsureExtractedFontFile(byte[] fontData)
    {
        var dir = Path.Combine(Path.GetTempPath(), "MgaWwiseIMImporter");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, "UDEVGothic-Regular.ttf");

        if (File.Exists(path))
        {
            var existing = File.ReadAllBytes(path);
            if (existing.AsSpan().SequenceEqual(fontData))
            {
                return path;
            }
        }

        File.WriteAllBytes(path, fontData);
        return path;
    }

    private static void Unregister()
    {
        if (_registeredPath is not { } path)
        {
            return;
        }

        _privateFonts?.Dispose();
        _privateFonts = null;
        RemoveFontResourceEx(path, FrPrivate, IntPtr.Zero);
        _registeredPath = null;
    }

    [DllImport("gdi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int AddFontResourceEx(string fileName, uint flags, IntPtr reserved);

    [DllImport("gdi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RemoveFontResourceEx(string fileName, uint flags, IntPtr reserved);
}
