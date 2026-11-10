using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DoAnWD.UI.Controls
{
    /// <summary>
    /// Ô nhập chữ viền mảnh màu theo giao diện (xám, đang gõ thì xanh) thay cho viền đen / viền lõm của Windows.
    /// </summary>
    public class ThemedTextBox : TextBox
    {
        private const int WM_PAINT = 0x000F, WM_NCPAINT = 0x0085, WM_SETFOCUS = 0x0007, WM_KILLFOCUS = 0x0008;
        private static readonly bool LaWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;

        [DllImport("user32.dll")] private static extern IntPtr GetWindowDC(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        public ThemedTextBox()
        {
            BorderStyle = BorderStyle.FixedSingle;
            Font = Theme.Body;
            ForeColor = Theme.Text;
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_NCPAINT || m.Msg == WM_PAINT || m.Msg == WM_SETFOCUS || m.Msg == WM_KILLFOCUS) VeVien();
        }

        private void VeVien()
        {
            if (!LaWindows || !IsHandleCreated || BorderStyle != BorderStyle.FixedSingle) return;
            IntPtr dc = GetWindowDC(Handle);
            if (dc == IntPtr.Zero) return;
            try
            {
                using (var g = Graphics.FromHdc(dc))
                using (var p = new Pen(!Enabled ? Theme.Rule : Focused ? Theme.Ink : Theme.RuleStrong))
                    g.DrawRectangle(p, 0, 0, Width - 1, Height - 1);
            }
            finally { ReleaseDC(Handle, dc); }
        }
    }
}
