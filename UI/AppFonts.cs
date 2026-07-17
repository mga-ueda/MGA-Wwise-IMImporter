using System.Runtime.InteropServices;

namespace MgaWwiseIMImporter.UI;

/// <summary>同梱フォントを、このアプリのプロセス内だけで利用可能にする。</summary>
internal static class AppFonts
{
    private const uint FrPrivate = 0x10;
    private const string BundledFamilyName = "UDEV Gothic";
    private static string? _registeredPath;

    public static string LogFamilyName { get; private set; } = "MS Gothic";

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
        LogFamilyName = BundledFamilyName;
        Application.ApplicationExit += (_, _) => Unregister();
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
