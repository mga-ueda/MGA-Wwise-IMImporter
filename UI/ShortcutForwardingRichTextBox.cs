using System.Runtime.InteropServices;

namespace MgaWwiseIMImporter.UI;

/// <summary>
/// 左にスクロールバー幅の半分の余白を付け、右端のバーとバランスを取る RichTextBox。
/// </summary>
internal sealed class ShortcutForwardingRichTextBox : RichTextBox
{
    private const int EmGetRect = 0x00B2;
    private const int EmSetRect = 0x00B3;

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        ApplyTextMargin();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        ApplyTextMargin();
    }

    /// <summary>テキスト描画領域の左をスクロールバー幅の半分だけ空ける。</summary>
    private void ApplyTextMargin()
    {
        if (!IsHandleCreated || ClientSize.Width <= 0 || ClientSize.Height <= 0)
        {
            return;
        }

        var margin = Math.Max(1, SystemInformation.VerticalScrollBarWidth / 2);
        var rect = new NativeRect();
        _ = SendMessage(Handle, EmGetRect, IntPtr.Zero, ref rect);

        // 取得に失敗／未初期化のときはクライアント全体を基準にする
        if (rect.Right <= rect.Left || rect.Bottom <= rect.Top)
        {
            rect.Left = 0;
            rect.Top = 0;
            rect.Right = ClientSize.Width;
            rect.Bottom = ClientSize.Height;
        }

        rect.Left = margin;
        if (rect.Right <= rect.Left)
        {
            rect.Right = rect.Left + 1;
        }

        _ = SendMessage(Handle, EmSetRect, IntPtr.Zero, ref rect);
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref NativeRect lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
