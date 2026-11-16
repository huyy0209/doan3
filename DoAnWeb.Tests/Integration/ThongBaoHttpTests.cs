using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DoAnWeb.Data.Entities;
using DoAnWeb.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DoAnWeb.Tests.Integration;

public class ThongBaoHttpTests : HttpTestBase
{
    private ThongBao TaoThongBao(int maTK, string tieuDe, string duongDan, bool daDoc = false)
    {
        var thongBao = new ThongBao
        {
            MaTK = maTK,
            TieuDe = tieuDe,
            NoiDung = "Nội dung kiểm thử.",
            DuongDan = duongDan,
            DaDoc = daDoc
        };
        Db.ThongBaos.Add(thongBao);
        Db.SaveChanges();
        return thongBao;
    }

    [Fact]
    public async Task GiaSu_chi_xem_va_danh_dau_thong_bao_cua_minh()
    {
        var cuaMinh = TaoThongBao(TKGS.MaTK, "ITG_Thông báo của tôi", "/LopHoc");
        var cuaQuanLy = TaoThongBao(QL.MaTK, "ITG_Thông báo riêng quản lý", "/ThongBao");
        var gs = await DangNhapGS();

        var trang = await gs.Get("/ThongBao");
        Assert.Equal(HttpStatusCode.OK, trang.Status);
        Assert.Contains(cuaMinh.TieuDe, trang.Chu);
        Assert.DoesNotContain(cuaQuanLy.TieuDe, trang.Chu);

        var ketQua = await gs.Post($"/ThongBao/Doc/{cuaQuanLy.MaTB}", []);
        Assert.True(ketQua.RedirectToi("/ThongBao"));
        using var db = Moi();
        Assert.False(db.ThongBaos.Single(x => x.MaTB == cuaQuanLy.MaTB).DaDoc);
    }

    [Fact]
    public async Task Duong_dan_ngoai_khong_duoc_dung_lam_dich_chuyen_huong()
    {
        var thongBao = TaoThongBao(TKGS.MaTK, "ITG_Đường dẫn lạ", "https://example.invalid/");
        var gs = await DangNhapGS();

        var ketQua = await gs.Post($"/ThongBao/Doc/{thongBao.MaTB}", []);

        Assert.True(ketQua.RedirectToi("/ThongBao"));
        Assert.DoesNotContain("example.invalid", ketQua.Location);
        using var db = Moi();
        Assert.True(db.ThongBaos.Single(x => x.MaTB == thongBao.MaTB).DaDoc);
    }

    [Fact]
    public async Task Danh_dau_tat_ca_chi_cap_nhat_thong_bao_cua_tai_khoan_hien_tai()
    {
        var cuaMinh = TaoThongBao(TKGS.MaTK, "ITG_Thông báo chưa đọc", "/ThongBao");
        var cuaQuanLy = TaoThongBao(QL.MaTK, "ITG_Thông báo QL chưa đọc", "/ThongBao");
        var gs = await DangNhapGS();

        var ketQua = await gs.Post("/ThongBao/DanhDauTatCaDaDoc", []);

        Assert.True(ketQua.RedirectToi("/ThongBao"));
        using var db = Moi();
        Assert.True(db.ThongBaos.Single(x => x.MaTB == cuaMinh.MaTB).DaDoc);
        Assert.False(db.ThongBaos.Single(x => x.MaTB == cuaQuanLy.MaTB).DaDoc);
    }

    [Fact]
    public async Task Trang_thong_bao_phan_trang_va_gioi_han_theo_nguoi_dung()
    {
        for (int i = 0; i < 21; i++)
            TaoThongBao(TKGS.MaTK, $"ITG_Phân trang {i:D2}", "/ThongBao");
        TaoThongBao(QL.MaTK, "ITG_Không thuộc gia sư", "/ThongBao");
        var gs = await DangNhapGS();

        var trangMot = await gs.Get("/ThongBao?trang=1");
        var trangHai = await gs.Get("/ThongBao?trang=2");

        Assert.Equal(HttpStatusCode.OK, trangMot.Status);
        Assert.Equal(HttpStatusCode.OK, trangHai.Status);
        Assert.Equal(20, trangMot.Doc.QuerySelectorAll(".thong-bao-dong").Length);
        Assert.Single(trangHai.Doc.QuerySelectorAll(".thong-bao-dong"));
        Assert.DoesNotContain("ITG_Không thuộc gia sư", trangMot.Chu);
    }

    [Fact]
    public async Task Nhac_no_qua_han_tao_mot_thong_bao_moi_nguoi_moi_ngay()
    {
        var hv = TaoHocVien(TenItg());
        var lop = TaoLop(GS, hv, "ITG_MonNoQuaHan", batDau: System.DateTime.Today.AddDays(-40));
        using (var scope = Factory.Services.CreateScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<ThongBaoService>();
            await service.TaoNhacNoQuaHanAsync();
            await service.TaoNhacNoQuaHanAsync();
        }

        using var db = Moi();
        var thongBao = db.ThongBaos.Where(x => x.MaTK == QL.MaTK &&
                                               x.TieuDe == "Học viên còn nợ học phí quá 30 ngày")
            .ToList();
        Assert.Single(thongBao);
        Assert.Contains(hv.HoTen, thongBao[0].NoiDung);
        Assert.Equal("/NhacNo?quaNgay=31", thongBao[0].DuongDan);
    }
}
