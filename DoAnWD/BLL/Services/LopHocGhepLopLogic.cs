using System;
using System.Collections.Generic;
using System.Linq;
using DoAnWD.Common;
using DoAnWD.Models;

namespace DoAnWD.BLL.Services
{
    internal static class LopHocGhepLopLogic
    {
        // LOW-01: gom cách tính khoảng giờ để form chỉ truyền dữ liệu giao diện.
        internal static void LayKhoang(string ca, DateTime batDau, DateTime ketThuc, out DateTime bd, out DateTime kt)
        {
            int gioBD, gioKT;
            if (Constants.CaHoc.TryGetGio(ca, out gioBD, out gioKT))
            {
                bd = batDau.Date.AddHours(gioBD);
                kt = ketThuc.Date.AddHours(gioKT);
            }
            else
            {
                bd = batDau.Date;
                kt = ketThuc.Date.AddDays(1).AddTicks(-1);
            }
        }

        // LOW-01: tách kiểm tra trùng lịch khỏi sự kiện và điều khiển của WinForms.
        internal static LopHoc TimLopTrung(IEnumerable<LopHoc> lopHoatDong, Func<LopHoc, bool> cuaAi,
            string ca, string thuHoc, DateTime batDau, DateTime ketThuc)
        {
            if (ca == null) return null;

            DateTime bd, kt;
            LayKhoang(ca, batDau, ketThuc, out bd, out kt);
            var moi = new LopHoc { CaHoc = ca, ThoiGianBatDau = bd, ThoiGianKetThuc = kt, ThuHoc = thuHoc };
            return lopHoatDong.FirstOrDefault(l => cuaAi(l) && l.TrungLich(moi));
        }

        // LOW-01: gom quy tắc gợi ý tên và hậu tố tránh trùng tên vào lớp nghiệp vụ.
        internal static string GoiYTenLop(string mon, string hocVien, string ca, ISet<string> tenDaCo)
        {
            string goiY = LopHocService.GoiYTenLop(mon, hocVien, ca);
            string ten = goiY;
            for (int i = 2; tenDaCo.Contains(ten) && i < 100; i++)
                ten = $"{goiY} ({i})";
            return ten;
        }
    }
}
