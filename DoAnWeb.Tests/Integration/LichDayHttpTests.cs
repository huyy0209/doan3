using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DoAnWeb.Common;
using Xunit;

namespace DoAnWeb.Tests.Integration;

public class LichDayHttpTests : HttpTestBase
{
    private static string TenTest() => "TEST_" + Guid.NewGuid().ToString("N").Substring(0, 10);

    [Fact]
    public async Task Lich_tuan_gioi_han_gia_su_va_an_lop_ngoai_khoang_ngay()
    {
        TestDb.DonDep(Db);
        try
        {
            var homNay = DateTime.Today;
            var thuHai = homNay.AddDays(-(((int)homNay.DayOfWeek + 6) % 7));
            var hocVienCuaToi = TaoHocVien(TenTest(), SdtTest());
            var hocVienNguoiKhac = TaoHocVien(TenTest(), SdtTest());
            string monCuaToi = TenTest();
            string monNguoiKhac = TenTest();
            string monCu = TenTest();
            string monSapToi = TenTest();
            var lopCuaToi = TaoLop(GS, hocVienCuaToi, monCuaToi,
                batDau: thuHai, ketThuc: thuHai.AddDays(6).AddHours(22));
            var lopNguoiKhac = TaoLop(GSKhac, hocVienNguoiKhac, monNguoiKhac,
                batDau: thuHai, ketThuc: thuHai.AddDays(6).AddHours(22));
            TaoLop(GS, TaoHocVien(TenTest(), SdtTest()), monCu,
                batDau: thuHai.AddDays(-10), ketThuc: thuHai.AddDays(-4).AddHours(22));
            TaoLop(GS, TaoHocVien(TenTest(), SdtTest()), monSapToi,
                batDau: thuHai.AddDays(7), ketThuc: thuHai.AddDays(14));

            var giaSu = await DangNhapGS();
            var lichGiaSu = await giaSu.Get("/LichDay?ngay=" + thuHai.ToString("yyyy-MM-dd") + "&maGS=" + GSKhac.MaGS);
            Assert.True(lichGiaSu.Status == HttpStatusCode.OK, $"Được {(int)lichGiaSu.Status}: {lichGiaSu.Chu}");
            Assert.Contains(monCuaToi, lichGiaSu.Chu);
            Assert.DoesNotContain(monNguoiKhac, lichGiaSu.Chu);
            Assert.DoesNotContain(monCu, lichGiaSu.Chu);
            Assert.DoesNotContain(monSapToi, lichGiaSu.Chu);
            Assert.Equal(HttpStatusCode.OK, (await giaSu.Get($"/LichDay/Details/{lopCuaToi.MaLop}")).Status);
            Assert.Equal(HttpStatusCode.NotFound, (await giaSu.Get($"/LichDay/Details/{lopNguoiKhac.MaLop}")).Status);

            var quanLy = await DangNhapQL();
            var lichQuanLy = await quanLy.Get("/LichDay?ngay=" + thuHai.ToString("yyyy-MM-dd"));
            Assert.Equal(HttpStatusCode.OK, lichQuanLy.Status);
            Assert.Contains(monCuaToi, lichQuanLy.Chu);
            Assert.Contains(monNguoiKhac, lichQuanLy.Chu);
            Assert.DoesNotContain(monCu, lichQuanLy.Chu);
            Assert.DoesNotContain(monSapToi, lichQuanLy.Chu);

            var lichDaLoc = await quanLy.Get("/LichDay?ngay=" + thuHai.ToString("yyyy-MM-dd") +
                "&maGS=" + GS.MaGS + "&monHoc=" + Uri.EscapeDataString(monCuaToi));
            Assert.Equal(HttpStatusCode.OK, lichDaLoc.Status);
            Assert.Contains(monCuaToi, lichDaLoc.Chu);
            Assert.DoesNotContain(monNguoiKhac, lichDaLoc.Chu);
        }
        finally
        {
            TestDb.DonDep(Db);
        }
    }
}
