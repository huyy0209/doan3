using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using DoAnWD.UI;
using DoAnWD.UI.Dialogs;
using DoAnWD.UI.Pages;

namespace DoAnWD.UI.Controls
{
    /// <summary>
    /// Cuộn tới đâu tải tới đó cho DataGridView: lúc đầu chỉ đưa một đợt dòng vào lưới,
    /// cuộn gần cuối thì thêm đợt tiếp theo (dữ liệu đã có sẵn trong bộ nhớ, không gọi lại CSDL).
    /// Lưới nhẹ hơn khi danh sách dài, mở trang nhanh hơn.
    /// </summary>
    public class CuonTaiDan<T>
    {
        private readonly DataGridView _dgv;
        private readonly int _moiDot;
        private List<T> _tatCa = new List<T>();
        private bool _dangThem;

        /// <summary>Danh sách đang gắn vào lưới (chỉ phần đã tải)</summary>
        public BindingList<T> HienThi { get; private set; } = new BindingList<T>();

        public int TongSo => _tatCa.Count;
        public int ConLai => _tatCa.Count - HienThi.Count;

        /// <summary>Báo khi số dòng đã tải thay đổi (để cập nhật dòng "đang hiện x / y")</summary>
        public event EventHandler DaTaiThem;

        public CuonTaiDan(DataGridView dgv, int moiDot = 40)
        {
            _dgv = dgv;
            _moiDot = Math.Max(10, moiDot);
            _dgv.Scroll += (s, e) =>
            {
                if (e.ScrollOrientation == ScrollOrientation.VerticalScroll) TaiNeuGanCuoi();
            };
            _dgv.SizeChanged += (s, e) => LapDayKhung();
        }

        /// <summary>Gắn danh sách mới: hiện đợt đầu, phần còn lại tải khi cuộn</summary>
        public void Nap(IEnumerable<T> tatCa)
        {
            _tatCa = tatCa.ToList();
            HienThi = new BindingList<T>(_tatCa.Take(_moiDot).ToList());
            _dgv.DataSource = HienThi;
            DaTaiThem?.Invoke(this, EventArgs.Empty);
            LapDayKhung();
        }

        /// <summary>Thêm một đợt dòng vào cuối lưới</summary>
        public void TaiThem()
        {
            if (_dangThem || ConLai <= 0) return;
            _dangThem = true;
            try
            {
                int den = Math.Min(_tatCa.Count, HienThi.Count + _moiDot);
                for (int i = HienThi.Count; i < den; i++) HienThi.Add(_tatCa[i]);
            }
            finally { _dangThem = false; }
            DaTaiThem?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Tải tới khi phần tử thỏa điều kiện có trong lưới; trả về chỉ số dòng (-1 nếu không có)</summary>
        public int TaiDen(Func<T, bool> dieuKien)
        {
            int i = _tatCa.FindIndex(x => dieuKien(x));
            if (i < 0) return -1;
            while (HienThi.Count <= i && ConLai > 0) TaiThem();
            return i;
        }

        private void TaiNeuGanCuoi()
        {
            if (ConLai <= 0 || _dgv.RowCount == 0) return;
            int cuoiDangThay = _dgv.FirstDisplayedScrollingRowIndex + _dgv.DisplayedRowCount(true);
            if (cuoiDangThay >= _dgv.RowCount - 3) TaiThem();
        }

        /// <summary>Lưới cao hơn số dòng đã tải (không có thanh cuộn để cuộn) => tải thêm cho đầy khung</summary>
        private void LapDayKhung()
        {
            if (!_dgv.IsHandleCreated || ConLai <= 0) return;
            _dgv.BeginInvoke((MethodInvoker)(() =>
            {
                int an = 0;
                while (ConLai > 0 && _dgv.RowCount > 0 && _dgv.DisplayedRowCount(false) >= _dgv.RowCount && an++ < 20)
                    TaiThem();
            }));
        }
    }
}
