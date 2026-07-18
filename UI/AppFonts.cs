using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace MgaWwiseIMImporter.UI;

/// <summary>同梱フォントを、このアプリのプロセス内だけで利用可能にする。</summary>
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

        var path = Path.Combine(AppContext.BaseDirectory, "Fonts", "UDEVGothic-Regular.ttf");
        if (!File.Exists(path) || AddFontResourceEx(path, FrPrivate, IntPtr.Zero) <= 0)
        {
            return;
        }

        _registeredPath = path;
        try
        {
            var collection = new PrivateFontCollection();
            collection.AddFontFile(path);
            _privateFonts = collection;
        }
        catch (Exception)
        {
            _privateFonts = null;
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

    private static void Unregister()
    {
        if (_registeredPath is not { } path)
        {
            return;
        }

        RemoveFontResourceEx(path, FrPrivate, IntPtr.Zero);
        _registeredPath = null;
    }

    [DllImport("gdi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int AddFontResourceEx(string fileName, uint flags, IntPtr reserved);

    [DllImport("gdi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RemoveFontResourceEx(string fileName, uint flags, IntPtr reserved);
}
