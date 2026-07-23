using System.Drawing.Text;

namespace MgaWwiseIMImporter.UI;

/// <summary>
/// 小サイズでも滲まないよう、GDI+ のアンチエイリアス（グリッドフィット付き）で
/// 描画する LinkLabel。フッタの権利表記など、7〜8pt の英字表示に使う。
/// </summary>
internal sealed class SmoothLinkLabel : LinkLabel
{
    public SmoothLinkLabel()
    {
        // TextRenderingHint を効かせるため GDI+ 描画にする。
        UseCompatibleTextRendering = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        var state = e.Graphics.Save();
        e.Graphics.TranslateTransform(0f, 2f);
        base.OnPaint(e);
        e.Graphics.Restore(state);
    }
}
