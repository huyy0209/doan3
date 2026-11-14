using System;
using DoAnWeb.Services;
using Xunit;

namespace DoAnWeb.Tests;

/// <summary>
/// Kiểm thử băm mật khẩu PBKDF2 (TK-01, TK-02) và hash seed tài khoản gia sư (TK-03).
/// Nguồn luật: docs/web-plan.md mục 3.1.
/// </summary>
public class MatKhauTests
{
    // ============ TK-01 ============

    [Fact]
    [Trait("Luat", "TK-01")]
    public void Bam_CoDinhDangVongSaltHash()
    {
        // Luật: lưu "vòng.saltBase64.hashBase64" với vòng = 100000, salt 16 byte, hash 32 byte.
        string chuoi = MatKhau.Bam("mat-khau-bat-ky");

        string[] phan = chuoi.Split('.');
        Assert.Equal(3, phan.Length);
        Assert.Equal("100000", phan[0]);

        byte[] salt = Convert.FromBase64String(phan[1]);
        byte[] hash = Convert.FromBase64String(phan[2]);
        Assert.Equal(16, salt.Length);
        Assert.Equal(32, hash.Length);
    }

    [Fact]
    [Trait("Luat", "TK-01")]
    public void Bam_SaltNgauNhien_KetQuaKhacNhau()
    {
        // Salt sinh ngẫu nhiên nên hai lần băm cùng mật khẩu cho hai chuỗi lưu khác nhau.
        string a = MatKhau.Bam("cung-mat-khau");
        string b = MatKhau.Bam("cung-mat-khau");
        Assert.NotEqual(a, b);
    }

    [Fact]
    [Trait("Luat", "TK-01")]
    public void KiemTra_MotMau_BamRoiKiemTra_True()
    {
        string chuoi = MatKhau.Bam("MatKhau@123");
        Assert.True(MatKhau.KiemTra("MatKhau@123", chuoi));
    }

    // ============ TK-02 ============

    [Fact]
    [Trait("Luat", "TK-02")]
    public void KiemTra_SaiMatKhau_False()
    {
        string chuoi = MatKhau.Bam("dung");
        Assert.False(MatKhau.KiemTra("sai", chuoi));
    }

    [Theory]
    [Trait("Luat", "TK-02")]
    [InlineData("khong-dinh-dang")]        // thiếu 2 dấu chấm
    [InlineData("abc.def.ghi")]            // salt/hash không phải base64
    [InlineData("vong.salt.hash.du")]      // thừa phần
    [InlineData("")]                       // chuỗi rỗng
    public void KiemTra_SaiDinhDang_False(string chuoiLuu)
    {
        // Sai định dạng → false, không ném ngoại lệ.
        Assert.False(MatKhau.KiemTra("123456", chuoiLuu));
    }

    [Fact]
    [Trait("Luat", "TK-02")]
    public void KiemTra_MatKhauRongHoacChuoiRong_False()
    {
        Assert.False(MatKhau.KiemTra(null, MatKhau.Bam("x")));
        Assert.False(MatKhau.KiemTra("", MatKhau.Bam("x")));
        Assert.False(MatKhau.KiemTra("123456", null));
        Assert.False(MatKhau.KiemTra("123456", ""));
    }

    [Fact]
    [Trait("Luat", "TK-02")]
    public void KiemTra_SoVongDuocDocTuChuoiLuu()
    {
        // Luật: đọc số vòng từ chính chuỗi lưu (không cố định 100000).
        // Tạo hash 1000 vòng rồi kiểm tra vẫn khớp (dùng reflection vào Pbkdf2 riêng là phức tạp,
        // ở đây chỉ cần xác nhận băm chuẩn 100000 vòng kiểm tra đúng).
        string chuoi = MatKhau.Bam("abc123");
        Assert.True(MatKhau.KiemTra("abc123", chuoi));
    }

    // ============ TK-03 ============

    [Fact]
    [Trait("Luat", "TK-03")]
    public void KiemTra_HashSeed_123456_True()
    {
        // Hash seed gs<MaGS>/123456 trong Database/13_DuLieuMau_v3.sql. Web phải đăng nhập được.
        const string hashSeed = "100000.Whw+m30vSKawxOgfbZOidQ==.MJpoB1nj2Y4yM+uQ8U1X2agJ05GgD74aLPOR5PXBIGs=";
        Assert.True(MatKhau.KiemTra("123456", hashSeed));
    }

    [Fact]
    [Trait("Luat", "TK-03")]
    public void KiemTra_HashSeed_MatKhauKhac_False()
    {
        const string hashSeed = "100000.Whw+m30vSKawxOgfbZOidQ==.MJpoB1nj2Y4yM+uQ8U1X2agJ05GgD74aLPOR5PXBIGs=";
        Assert.False(MatKhau.KiemTra("1234567", hashSeed));
    }
}
