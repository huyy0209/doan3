using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using DoAnWeb.Common;
using DoAnWeb.Data.Entities;
using DoAnWeb.Services;
using Xunit;

namespace DoAnWeb.Tests.Integration;

/// <summary>
/// Test tích hợp action JSON <c>/TongQuan/BieuDo</c>: trả đúng số liệu vừa thêm (tiền tố TEST_)
/// và chỉ quản lý mới gọi được.
/// </summary>
public class TongQuanBieuDoHttpTests : HttpTestBase
{
    private static string TenTest() => "TEST_" + Guid.NewGuid().ToString("N").Substring(0, 10);

    [Fact]
    [Trait("Luat", "TQ-BD")]
    public async Task Bieu_do_tra_dung_so_lieu_du_lieu_TEST_va_tu_don()
    {
        var ql = await DangNhapQL();
        var truoc = await DocSoLieu(ql);

        var dauThang = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var khuVuc = TenTest();
        var monA = TaoMon(khuVuc + "A");
        var monB = TaoMon(khuVuc + "B");
        var hv1 = TaoHocVien(khuVuc + "1", SdtTest());
        var hv2 = TaoHocVien(khuVuc + "2", SdtTest());
        var hv3 = TaoHocVien(khuVuc + "3", SdtTest());

        try
        {
            var lop1 = ThemLop(monA, hv1, dauThang.AddHours(8));
            var lop2 = ThemLop(monA, hv2, dauThang.AddHours(9));
            var lop3 = ThemLop(monB, hv3, dauThang.AddHours(10), Constants.LopHocStatus.DangHoc);

            Db.ThanhToans.Add(new ThanhToan { MaLop = lop1.MaLop, SoTien = 300000, NgayDong = dauThang.AddDays(1), HinhThuc = "Tiền mặt" });
            Db.ThanhToans.Add(new ThanhToan { MaLop = lop2.MaLop, SoTien = 200000, NgayDong = dauThang.AddDays(2), HinhThuc = "Tiền mặt" });
            Db.BuoiHocs.Add(new BuoiHoc { MaLop = lop1.MaLop, NgayHoc = dauThang.AddDays(1), CaHoc = lop1.CaHoc, DiemDanh = Constants.DiemDanh.CoMat });
            Db.BuoiHocs.Add(new BuoiHoc { MaLop = lop1.MaLop, NgayHoc = dauThang.AddDays(2), CaHoc = lop1.CaHoc, DiemDanh = Constants.DiemDanh.DiTre });
            Db.BuoiHocs.Add(new BuoiHoc { MaLop = lop1.MaLop, NgayHoc = dauThang.AddDays(3), CaHoc = lop1.CaHoc, DiemDanh = Constants.DiemDanh.VangKhongPhep });
            Db.BuoiHocs.Add(new BuoiHoc { MaLop = lop2.MaLop, NgayHoc = dauThang.AddDays(4), CaHoc = lop2.CaHoc, DiemDanh = Constants.DiemDanh.CoMat });
            Db.SaveChanges();

            var sau = await DocSoLieu(ql);

            Assert.Equal(ThongKeBieuDoService.SoThang, sau.SoThangDoanhThu);
            Assert.Equal(ThongKeBieuDoService.SoThang, sau.SoThangHocVienMoi);
            Assert.Equal(truoc.DoanhThuThangNay + 500000m, sau.DoanhThuThangNay);
            Assert.Equal(truoc.HocVienMoiThangNay + 3, sau.HocVienMoiThangNay);
            Assert.Equal(truoc.SoLop(monA.TenMon) + 2, sau.SoLop(monA.TenMon));
            Assert.Equal(truoc.SoLop(monB.TenMon) + 1, sau.SoLop(monB.TenMon));
            Assert.Equal(truoc.CoMat + 3, sau.CoMat);
            Assert.Equal(truoc.TongBuoi + 4, sau.TongBuoi);
            Assert.Equal(Math.Round((decimal)sau.CoMat * 100 / sau.TongBuoi, 1), sau.TyLe);
        }
        finally
        {
            TestDb.DonDep(Db);
        }
    }

    [Fact]
    [Trait("Luat", "TK-19")]
    public async Task Gia_su_khong_goi_duoc_bieu_do()
    {
        var gs = await DangNhapGS();
        var t = await gs.Get("/TongQuan/BieuDo");
        Assert.True(t.BiChan, $"Gia sư gọi /TongQuan/BieuDo phải bị chặn, nhưng được {(int)t.Status} {t.Location}");
    }

    [Fact]
    public async Task Chua_dang_nhap_khong_goi_duoc_bieu_do()
    {
        var t = await Khach().Get("/TongQuan/BieuDo");
        Assert.True(t.VeDangNhap, $"Chưa đăng nhập phải về trang đăng nhập, nhưng được {(int)t.Status} {t.Location}");
    }

    private LopHoc ThemLop(MonHocDanhMuc mon, HocVien hv, DateTime batDau, string trangThai = null)
    {
        var lop = new LopHoc
        {
            MaGS = GS.MaGS,
            MaHV = hv.MaHV,
            MonHoc = mon.TenMon,
            MaMon = mon.MaMon,
            TenLop = TenTest(),
            HocPhi = 100000,
            TrangThai = trangThai ?? Constants.LopHocStatus.DangDienRa,
            ThoiGianBatDau = batDau,
            ThoiGianKetThuc = batDau.AddMonths(2),
            CaHoc = Constants.CaHoc.Sang
        };
        Db.LopHocs.Add(lop);
        Db.SaveChanges();
        return lop;
    }

    /// <summary>Các con số cần so sánh, đọc từ JSON của action.</summary>
    private sealed class SoLieu
    {
        public int SoThangDoanhThu { get; set; }
        public int SoThangHocVienMoi { get; set; }
        public decimal DoanhThuThangNay { get; set; }
        public int HocVienMoiThangNay { get; set; }
        public int CoMat { get; set; }
        public int TongBuoi { get; set; }
        public decimal TyLe { get; set; }

        private readonly Dictionary<string, int> _theoMon = new();

        public int SoLop(string tenMon) => _theoMon.TryGetValue(tenMon, out int n) ? n : 0;

        public void NapTheoMon(JsonElement mang)
        {
            foreach (var e in mang.EnumerateArray())
            {
                _theoMon[e.GetProperty("tenMon").GetString()] = e.GetProperty("soLop").GetInt32();
            }
        }
    }

    private static async Task<SoLieu> DocSoLieu(TrinhDuyet ql)
    {
        var t = await ql.Get("/TongQuan/BieuDo");
        Assert.Equal(HttpStatusCode.OK, t.Status);
        using var doc = JsonDocument.Parse(t.Html);
        var root = doc.RootElement;
        var dsThu = root.GetProperty("doanhThu");
        var dsHocVien = root.GetProperty("hocVienMoi");
        var cc = root.GetProperty("chuyenCan");
        var kq = new SoLieu
        {
            SoThangDoanhThu = dsThu.GetArrayLength(),
            SoThangHocVienMoi = dsHocVien.GetArrayLength(),
            DoanhThuThangNay = dsThu[dsThu.GetArrayLength() - 1].GetProperty("soTien").GetDecimal(),
            HocVienMoiThangNay = dsHocVien[dsHocVien.GetArrayLength() - 1].GetProperty("soLuong").GetInt32(),
            CoMat = cc.GetProperty("coMat").GetInt32(),
            TongBuoi = cc.GetProperty("tongBuoi").GetInt32(),
            TyLe = cc.GetProperty("tyLe").GetDecimal()
        };
        kq.NapTheoMon(root.GetProperty("lopTheoMon"));
        return kq;
    }
}
