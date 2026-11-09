using System.Drawing;
using System.Drawing.Drawing2D;

namespace DoAnWD.UI
{
    public enum Glyph
    {
        None, Plus, Edit, Trash, Check, Close, Refresh, ChevronLeft, ChevronRight,
        Calendar, Chart, User, Users, Link, Exit, Search
    }

    /// <summary>
    /// Icon nét mảnh vẽ bằng GDI+ (không cần file ảnh / font icon).
    /// Toạ độ thiết kế trên lưới 16x16 rồi co giãn theo ô vẽ.
    /// </summary>
    public static class Icons
    {
        public static void Draw(Graphics g, Glyph glyph, RectangleF box, Color color, float stroke = 1.7f)
        {
            if (glyph == Glyph.None) return;

            var state = g.Save();
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TranslateTransform(box.X, box.Y);
            float k = box.Width / 16f;
            g.ScaleTransform(k, k);

            using (var p = new Pen(color, stroke / k) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
            {
                switch (glyph)
                {
                    case Glyph.Plus:
                        g.DrawLine(p, 8, 3, 8, 13); g.DrawLine(p, 3, 8, 13, 8); break;
                    case Glyph.Check:
                        g.DrawLines(p, new[] { new PointF(3, 8.5f), new PointF(6.5f, 12), new PointF(13, 4.5f) }); break;
                    case Glyph.Close:
                        g.DrawLine(p, 4, 4, 12, 12); g.DrawLine(p, 12, 4, 4, 12); break;
                    case Glyph.Edit:
                        g.DrawPolygon(p, new[] { new PointF(3, 13), new PointF(3.5f, 10.5f), new PointF(10.5f, 3.5f), new PointF(12.5f, 5.5f), new PointF(5.5f, 12.5f) });
                        g.DrawLine(p, 9, 5, 11, 7); break;
                    case Glyph.Trash:
                        g.DrawLine(p, 2.5f, 4.5f, 13.5f, 4.5f);
                        g.DrawLines(p, new[] { new PointF(6, 4.5f), new PointF(6.5f, 2.5f), new PointF(9.5f, 2.5f), new PointF(10, 4.5f) });
                        g.DrawLines(p, new[] { new PointF(4, 4.5f), new PointF(4.8f, 13.5f), new PointF(11.2f, 13.5f), new PointF(12, 4.5f) });
                        g.DrawLine(p, 6.8f, 7, 6.8f, 11); g.DrawLine(p, 9.2f, 7, 9.2f, 11); break;
                    case Glyph.Refresh:
                        g.DrawArc(p, 3, 3, 10, 10, -40, 290);
                        g.DrawLines(p, new[] { new PointF(13.3f, 2.5f), new PointF(13.3f, 5.8f), new PointF(10, 5.8f) }); break;
                    case Glyph.ChevronLeft:
                        g.DrawLines(p, new[] { new PointF(10, 3), new PointF(5, 8), new PointF(10, 13) }); break;
                    case Glyph.ChevronRight:
                        g.DrawLines(p, new[] { new PointF(6, 3), new PointF(11, 8), new PointF(6, 13) }); break;
                    case Glyph.Calendar:
                        g.DrawRectangle(p, 2.5f, 3.5f, 11, 10);
                        g.DrawLine(p, 2.5f, 6.8f, 13.5f, 6.8f);
                        g.DrawLine(p, 5.5f, 2, 5.5f, 4.5f); g.DrawLine(p, 10.5f, 2, 10.5f, 4.5f); break;
                    case Glyph.Chart:
                        g.DrawLine(p, 2.5f, 13.5f, 13.5f, 13.5f);
                        g.DrawLine(p, 4.5f, 13.5f, 4.5f, 8); g.DrawLine(p, 8, 13.5f, 8, 3.5f); g.DrawLine(p, 11.5f, 13.5f, 11.5f, 6); break;
                    case Glyph.User:
                        g.DrawEllipse(p, 5.3f, 2.3f, 5.4f, 5.4f);
                        g.DrawBezier(p, 2.5f, 14, 2.5f, 9, 13.5f, 9, 13.5f, 14); break;
                    case Glyph.Users:
                        g.DrawEllipse(p, 2.8f, 3.3f, 4.6f, 4.6f);
                        g.DrawBezier(p, 1, 13.5f, 1, 9.3f, 9.2f, 9.3f, 9.2f, 13.5f);
                        g.DrawEllipse(p, 9, 3.8f, 4, 4);
                        g.DrawBezier(p, 10.5f, 9.6f, 13.5f, 9.4f, 15, 11, 15, 13.5f); break;
                    case Glyph.Link:
                        g.DrawEllipse(p, 1.5f, 5, 8, 6); g.DrawEllipse(p, 6.5f, 5, 8, 6); break;
                    case Glyph.Exit:
                        g.DrawLines(p, new[] { new PointF(9, 2.5f), new PointF(3, 2.5f), new PointF(3, 13.5f), new PointF(9, 13.5f) });
                        g.DrawLine(p, 6.5f, 8, 14, 8);
                        g.DrawLines(p, new[] { new PointF(11.5f, 5.5f), new PointF(14, 8), new PointF(11.5f, 10.5f) }); break;
                    case Glyph.Search:
                        g.DrawEllipse(p, 2.5f, 2.5f, 8, 8); g.DrawLine(p, 9.3f, 9.3f, 13.5f, 13.5f); break;
                }
            }
            g.Restore(state);
        }
    }
}
