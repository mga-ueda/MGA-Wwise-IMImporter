using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace MgaWwiseIMImporter.UI;

/// <summary>埋め込みフォントを、このアプリのプロセス内だけで利用可能にする。</summary>
internal static class AppFonts
{
    private static PrivateFontCollection? _privateFonts;
    private static GCHandle _fontHandle;
    private static bool _registered;

    public static void EnsureRegistered()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;
        try
        {
            using var stream = AppEmbeddedResources.OpenLogFont();
            if (stream is null)
            {
                return;
            }

            var fontData = new byte[stream.Length];
            stream.ReadExactly(fontData);

            // AddMemoryFont はピン留めメモリをフォント破棄まで解放してはならない。
            _fontHandle = GCHandle.Alloc(fontData, GCHandleType.Pinned);
            var collection = new PrivateFontCollection();
            collection.AddMemoryFont(_fontHandle.AddrOfPinnedObject(), fontData.Length);
            _privateFonts = collection;
        }
        catch (Exception)
        {
            ReleaseFontHandle();
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
        _privateFonts?.Dispose();
        _privateFonts = null;
        ReleaseFontHandle();
    }

    private static void ReleaseFontHandle()
    {
        if (_fontHandle.IsAllocated)
        {
            _fontHandle.Free();
        }
    }
}
