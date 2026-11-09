/* ===================================================================
   QuanLyGiaSu - NÂNG CẤP v2 (chạy SAU QuanLyGiaSu.sql)
   - Hồ sơ học viên: phụ huynh, SĐT phụ huynh, trường/lớp, trình độ, ghi chú
   - BuoiHoc  : điểm danh từng buổi + điểm bài test 15 phút (không bắt buộc)
   - ThanhToan: các lần đóng học phí của từng lớp
   - TaiKhoan : đăng nhập, phân quyền Quản lý / Gia sư
     (tài khoản admin mặc định do ứng dụng tự tạo ở lần chạy đầu)
   Chạy lại nhiều lần được.
   =================================================================== */
USE QuanLyGiaSu;
GO

/* ----------------------- HỒ SƠ HỌC VIÊN ----------------------- */
IF COL_LENGTH('dbo.HocVien', 'TenPhuHuynh') IS NULL
    ALTER TABLE dbo.HocVien ADD TenPhuHuynh NVARCHAR(100) NULL;
IF COL_LENGTH('dbo.HocVien', 'SDTPhuHuynh') IS NULL
    ALTER TABLE dbo.HocVien ADD SDTPhuHuynh VARCHAR(20) NULL;
IF COL_LENGTH('dbo.HocVien', 'TruongLop') IS NULL
    ALTER TABLE dbo.HocVien ADD TruongLop NVARCHAR(150) NULL;
IF COL_LENGTH('dbo.HocVien', 'TrinhDo') IS NULL
    ALTER TABLE dbo.HocVien ADD TrinhDo NVARCHAR(50) NULL;
IF COL_LENGTH('dbo.HocVien', 'GhiChu') IS NULL
    ALTER TABLE dbo.HocVien ADD GhiChu NVARCHAR(500) NULL;
GO

/* ----------------------- BUỔI HỌC ----------------------- */
IF OBJECT_ID(N'dbo.BuoiHoc', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BuoiHoc
    (
        MaBuoi    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_BuoiHoc PRIMARY KEY,
        MaLop     INT NOT NULL CONSTRAINT FK_BuoiHoc_LopHoc REFERENCES dbo.LopHoc(MaLop),
        NgayHoc   DATE NOT NULL,
        DiemDanh  NVARCHAR(30) NOT NULL CONSTRAINT DF_BuoiHoc_DiemDanh DEFAULT N'Có mặt',
        Diem      DECIMAL(4, 2) NULL,          -- NULL = hôm đó không kiểm tra
        NhanXet   NVARCHAR(300) NULL,
        CONSTRAINT UQ_BuoiHoc_Lop_Ngay UNIQUE (MaLop, NgayHoc),   -- mỗi lớp 1 bản ghi / ngày
        CONSTRAINT CK_BuoiHoc_Diem CHECK (Diem IS NULL OR (Diem >= 0 AND Diem <= 10)),
        CONSTRAINT CK_BuoiHoc_DiemDanh CHECK (DiemDanh IN (N'Có mặt', N'Đi trễ', N'Vắng có phép', N'Vắng không phép'))
    );
END
GO

/* ----------------------- THANH TOÁN HỌC PHÍ ----------------------- */
IF OBJECT_ID(N'dbo.ThanhToan', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ThanhToan
    (
        MaTT      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ThanhToan PRIMARY KEY,
        MaLop     INT NOT NULL CONSTRAINT FK_ThanhToan_LopHoc REFERENCES dbo.LopHoc(MaLop),
        SoTien    DECIMAL(18, 2) NOT NULL CONSTRAINT CK_ThanhToan_SoTien CHECK (SoTien > 0),
        NgayDong  DATETIME NOT NULL CONSTRAINT DF_ThanhToan_NgayDong DEFAULT GETDATE(),
        HinhThuc  NVARCHAR(30) NULL,
        GhiChu    NVARCHAR(200) NULL
    );
    CREATE INDEX IX_ThanhToan_MaLop ON dbo.ThanhToan(MaLop);
END
GO

/* ----------------------- TÀI KHOẢN ----------------------- */
IF OBJECT_ID(N'dbo.TaiKhoan', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TaiKhoan
    (
        MaTK         INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_TaiKhoan PRIMARY KEY,
        TenDangNhap  VARCHAR(50) NOT NULL CONSTRAINT UQ_TaiKhoan_TenDangNhap UNIQUE,
        MatKhauHash  VARCHAR(200) NOT NULL,     -- PBKDF2: "vòng lặp.salt.hash" (Base64)
        VaiTro       NVARCHAR(20) NOT NULL CONSTRAINT CK_TaiKhoan_VaiTro CHECK (VaiTro IN (N'QuanLy', N'GiaSu')),
        MaGS         INT NULL CONSTRAINT FK_TaiKhoan_GiaSu REFERENCES dbo.GiaSu(MaGS),
        HoatDong     BIT NOT NULL CONSTRAINT DF_TaiKhoan_HoatDong DEFAULT 1,
        NgayTao      DATETIME NOT NULL CONSTRAINT DF_TaiKhoan_NgayTao DEFAULT GETDATE(),
        CONSTRAINT CK_TaiKhoan_GiaSu CHECK (VaiTro = N'QuanLy' OR MaGS IS NOT NULL)
    );
END
GO

PRINT N'Nâng cấp v2 hoàn tất.';
GO
