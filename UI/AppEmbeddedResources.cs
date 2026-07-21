using System.Reflection;

namespace MgaWwiseIMImporter.UI;

/// <summary>exe に埋め込んだブランディング／フォント資産へのアクセス。</summary>
internal static class AppEmbeddedResources
{
    private const string LogoName = "MgaWwiseIMImporter.Branding.MiyabiGameAudio.png";
    private const string WindowIconName = "MgaWwiseIMImporter.Branding.MgaWwiseIMImporter.ico";
    private const string LogFontName = "MgaWwiseIMImporter.Fonts.UDEVGothic-Regular.ttf";

    public static Stream? OpenLogo() => Open(LogoName);

    public static Stream? OpenWindowIcon() => Open(WindowIconName);

    public static Stream? OpenLogFont() => Open(LogFontName);

    private static Stream? Open(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();
        return assembly.GetManifestResourceStream(name);
    }
}
