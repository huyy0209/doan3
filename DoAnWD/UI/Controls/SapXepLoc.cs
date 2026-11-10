using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace DoAnWD.UI.Controls
{
    /// <summary>
    /// Tiện ích cho bảng danh sách:
    ///  1. Bấm tiêu đề cột => sắp xếp theo cột đó (bấm lần nữa đảo chiều, có mũi tên ở tiêu đề).
    ///     Cột tên người xếp theo TÊN (chữ cái của tên gọi, kiểu Việt Nam), rồi tới họ đệm.
    ///  2. Bấm vào ô trạng thái / môn / ca... => lọc danh sách theo đúng giá trị đó
    ///     (đặt ô lọc tương ứng; bấm lại lần nữa thì bỏ lọc).
    /// Trang có lọc + phân trang thì truyền hàm nạp lại (lamMoi) và dùng Ap(...) để sắp toàn bộ danh sách
    /// trước khi cắt trang; bảng đơn giản thì bỏ trống lamMoi, tiện ích tự sắp dữ liệu đang gắn vào bảng.
    /// </summary>
    public class SapXepLoc
    {
        private static readonly CompareInfo SoSanhVi = CultureInfo.GetCultureInfo("vi-VN").CompareInfo;
        private static readonly HashSet<string> CotTenNguoi = new HashSet<string>
        {
            "HoTen", "HocVien", "GiaSu", "PhuHuynh", "TenPhuHuynh"
        };

        private readonly DataGridView _dgv;
        private readonly Action _lamMoi;
        private readonly Dictionary<string, ComboBox> _locKhiBam = new Dictionary<string, ComboBox>();
        private readonly ToolTip _goiY = new ToolTip();

        /// <summary>Thuộc tính đang sắp xếp (null = giữ thứ tự gốc)</summary>
        public string Cot { get; private set; }
        public bool Giam { get; private set; }

        /// <param name="cotMacDinh">Cột sắp xếp lúc đầu (ví dụ "HoTen" => A-Z theo tên)</param>
        public SapXepLoc(DataGridView dgv, Action lamMoi = null, string cotMacDinh = null)
        {
            _dgv = dgv;
            _lamMoi = lamMoi;
            Cot = cotMacDinh;
            foreach (DataGridViewColumn c in dgv.Columns) c.SortMode = DataGridViewColumnSortMode.Programmatic;
            dgv.ColumnAdded += (s, e) => e.Column.SortMode = DataGridViewColumnSortMode.Programmatic;
            dgv.ColumnHeaderMouseClick += BamTieuDe;
            dgv.DataBindingComplete += (s, e) => VeMuiTen();
            dgv.CellClick += BamO;
            dgv.CellMouseEnter += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && LayLoc(e.ColumnIndex) != null)
                    _dgv.Cursor = Cursors.Hand;
            };
            dgv.CellMouseLeave += (s, e) => _dgv.Cursor = Cursors.Default;
        }

        /// <summary>Bấm ô ở cột (theo tên thuộc tính) => đặt ô lọc này theo giá trị của ô</summary>
        public SapXepLoc LocKhiBam(string cot, ComboBox oLoc)
        {
            _locKhiBam[cot] = oLoc;
            return this;
        }

        /// <summary>Sắp xếp một danh sách theo cột đang chọn (dùng trong hàm lọc của trang)</summary>
        public IEnumerable<T> Ap<T>(IEnumerable<T> ds)
        {
            if (Cot == null) return ds;
            var p = typeof(T).GetProperty(Cot);
            if (p == null) return ds;
            var so = new SoSanh(CotTenNguoi.Contains(Cot));
            return Giam ? ds.OrderByDescending(x => p.GetValue(x, null), so) : ds.OrderBy(x => p.GetValue(x, null), so);
        }

        // ---------------------------------------------------------------

        private string TenCot(int i)
        {
            var c = _dgv.Columns[i];
            return string.IsNullOrEmpty(c.DataPropertyName) ? c.Name : c.DataPropertyName;
        }

        private ComboBox LayLoc(int cot)
        {
            ComboBox o;
            return _locKhiBam.TryGetValue(TenCot(cot), out o) ? o : null;
        }

        private void BamTieuDe(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || e.ColumnIndex < 0) return;
            string cot = TenCot(e.ColumnIndex);
            if (Cot == cot) Giam = !Giam;
            else { Cot = cot; Giam = false; }

            if (_lamMoi != null) _lamMoi();
            else TuSapXep();
            VeMuiTen();
        }

        /// <summary>Bảng không có hàm nạp lại: sắp chính danh sách đang gắn vào bảng</summary>
        private void TuSapXep()
        {
            var ds = _dgv.DataSource as IEnumerable;
            if (ds == null) return;
            var phanTu = ds.Cast<object>().ToList();
            if (phanTu.Count < 2) return;
            var kieu = phanTu[0].GetType();
            var p = kieu.GetProperty(Cot);
            if (p == null) return;
            var so = new SoSanh(CotTenNguoi.Contains(Cot));
            var sapXep = Giam ? phanTu.OrderByDescending(x => p.GetValue(x, null), so) : phanTu.OrderBy(x => p.GetValue(x, null), so);

            // Gắn lại bằng danh sách đúng kiểu để bảng vẫn nhận ra các cột
            var moi = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(kieu));
            foreach (var x in sapXep) moi.Add(x);
            _dgv.DataSource = moi;
        }

        private void VeMuiTen()
        {
            foreach (DataGridViewColumn c in _dgv.Columns)
            {
                if (c.SortMode != DataGridViewColumnSortMode.Programmatic) continue;
                c.HeaderCell.SortGlyphDirection = TenCot(c.Index) == Cot
                    ? (Giam ? SortOrder.Descending : SortOrder.Ascending)
                    : SortOrder.None;
            }
        }

        private void BamO(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            var oLoc = LayLoc(e.ColumnIndex);
            if (oLoc == null) return;
            string giaTri = _dgv[e.ColumnIndex, e.RowIndex].FormattedValue?.ToString();
            if (string.IsNullOrWhiteSpace(giaTri)) return;

            int chiSo = -1;
            for (int i = 0; i < oLoc.Items.Count; i++)
                if (string.Equals(oLoc.GetItemText(oLoc.Items[i]), giaTri, StringComparison.OrdinalIgnoreCase)) { chiSo = i; break; }
            if (chiSo < 0) return;

            // Bấm lại đúng giá trị đang lọc => bỏ lọc (về mặc định)
            var dep = oLoc as ThemedComboBox;
            int macDinh = dep != null ? dep.MucMacDinh : 0;
            oLoc.SelectedIndex = oLoc.SelectedIndex == chiSo && chiSo != macDinh ? macDinh : chiSo;
        }

        /// <summary>So sánh giá trị ô: chữ theo tiếng Việt (tên người theo tên gọi), số / ngày theo giá trị, ô trống xếp cuối</summary>
        private class SoSanh : IComparer<object>
        {
            private readonly bool _tenNguoi;
            public SoSanh(bool tenNguoi) { _tenNguoi = tenNguoi; }

            public int Compare(object a, object b)
            {
                bool rongA = a == null || (a is string && string.IsNullOrWhiteSpace((string)a));
                bool rongB = b == null || (b is string && string.IsNullOrWhiteSpace((string)b));
                if (rongA || rongB) return rongA == rongB ? 0 : rongA ? 1 : -1;

                var sa = a as string;
                var sb = b as string;
                if (sa != null && sb != null)
                {
                    if (_tenNguoi) { sa = KhoaTen(sa); sb = KhoaTen(sb); }
                    return SoSanhVi.Compare(sa, sb, CompareOptions.IgnoreCase);
                }
                var ca = a as IComparable;
                if (ca != null && a.GetType() == b.GetType()) return ca.CompareTo(b);
                return SoSanhVi.Compare(a.ToString(), b.ToString(), CompareOptions.IgnoreCase);
            }

            /// <summary>"Đỗ Bảo Ngọc" => "Ngọc Đỗ Bảo": xếp theo tên gọi trước, rồi họ đệm</summary>
            private static string KhoaTen(string hoTen)
            {
                var tu = hoTen.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                return tu.Length < 2 ? hoTen : tu[tu.Length - 1] + " " + string.Join(" ", tu, 0, tu.Length - 1);
            }
        }
    }
}
