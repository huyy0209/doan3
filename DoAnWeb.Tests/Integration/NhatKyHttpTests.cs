using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DoAnWeb.Data;
using DoAnWeb.Data.Entities;
using DoAnWeb.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DoAnWeb.Tests.Integration;

public sealed class NhatKyHttpTests : HttpTestBase
{
    [Fact]
    public void Interceptor_ghi_them_sua_xoa_va_bo_qua_hash_mat_khau()
    {
        string ten = TenItg();
        var nguoiDung = NguoiDungCoDinh.QuanLy(QL.MaTK, QL.TenDangNhap);
        var interceptor = new NhatKySaveChangesInterceptor(nguoiDung, new HttpContextAccessor());
        var options = new DbContextOptionsBuilder<QuanLyGiaSuContext>()
            .UseSqlServer(TestDb.ConnStr)
            .AddInterceptors(interceptor)
            .Options;

        int maTK;
        using (var db = new QuanLyGiaSuContext(options))
        {
            var tk = new TaiKhoan
            {
                TenDangNhap = ten,
                MatKhauHash = "never-log-this-hash",
                VaiTro = "QuanLy",
                HoatDong = true,
                NgayTao = DateTime.Now
            };
            db.TaiKhoans.Add(tk);
            db.SaveChanges();
            maTK = tk.MaTK;

            tk.TenDangNhap = ten + "_sua";
            tk.MatKhauHash = "another-secret-hash";
            db.SaveChanges();

            db.TaiKhoans.Remove(tk);
            db.SaveChanges();
        }

        using var kiemTra = Moi();
        var muc = kiemTra.NhatKys.Where(n => n.Bang == "TaiKhoan" && n.KhoaChinh.Contains($"MaTK={maTK}"))
            .OrderBy(n => n.MaNK).ToList();
        Assert.Equal(new[] { "Them", "Sua", "Xoa" }, muc.Select(n => n.HanhDong));
        Assert.All(muc, n => Assert.DoesNotContain("MatKhau", n.ChiTiet, StringComparison.OrdinalIgnoreCase));
        Assert.All(muc, n => Assert.DoesNotContain("secret-hash", n.ChiTiet, StringComparison.Ordinal));
        Assert.DoesNotContain("never-log-this-hash", muc[0].ChiTiet, StringComparison.Ordinal);
        Assert.Contains($"\"cu\":\"{ten}\"", muc[1].ChiTiet);
        Assert.Contains($"\"moi\":\"{ten}_sua\"", muc[1].ChiTiet);
    }

    [Fact]
    public async Task Quan_ly_loc_va_phan_trang_nhat_ky_con_gia_su_bi_chan()
    {
        var ngay = DateTime.Today;
        for (int i = 0; i < 25; i++)
        {
            Db.NhatKys.Add(new NhatKy
            {
                ThoiGian = ngay.AddHours(1).AddMinutes(i),
                MaTK = QL.MaTK,
                TenDangNhap = QL.TenDangNhap,
                HanhDong = "Sua",
                Bang = "BảngAuditTest",
                KhoaChinh = "Ma=ITG_" + i,
                ChiTiet = "{}"
            });
        }
        Db.SaveChanges();

        var ql = await DangNhapQL();
        var query = $"tuNgay={ngay:yyyy-MM-dd}&denNgay={ngay:yyyy-MM-dd}&nguoi={Uri.EscapeDataString(QL.TenDangNhap)}&hanhDong=Sua&bang={Uri.EscapeDataString("BảngAuditTest")}&trang=2&soDong=20";
        var trang = await ql.Get("/NhatKy?" + query);
        Assert.Equal(HttpStatusCode.OK, trang.Status);
        Assert.Contains("Hiện 21–25 / 25", trang.Chu);
        Assert.Contains("Ma=ITG_4", trang.Chu);
        Assert.DoesNotContain("Ma=ITG_24", trang.Chu);

        var gs = await DangNhapGS();
        Assert.True((await gs.Get("/NhatKy")).BiChan);
    }

    [Fact]
    public async Task Dang_nhap_thanh_cong_that_bai_va_xuat_ban_duoc_ghi()
    {
        string sai = TenItg();
        var client = Khach();
        await client.DangNhap(sai, "SaiSai999");
        await client.DangNhap(QL.TenDangNhap, MatKhauTest);
        await client.Post("/TaiKhoan/DangXuat", Array.Empty<System.Collections.Generic.KeyValuePair<string, string>>());

        using var db = Moi();
        Assert.Contains(db.NhatKys, n => n.HanhDong == "DangNhapLoi" && n.TenDangNhap == sai);
        Assert.Contains(db.NhatKys, n => n.HanhDong == "DangNhap" && n.MaTK == QL.MaTK);
        Assert.Contains(db.NhatKys, n => n.HanhDong == "DangXuat" && n.MaTK == QL.MaTK);
    }
}
