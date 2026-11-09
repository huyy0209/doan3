/* ===================================================================
   QuanLyGiaSu - Script tạo CSDL + dữ liệu mẫu
   Chạy trong SQL Server Management Studio (SSMS) hoặc:
     sqlcmd -S . -E -i Database\QuanLyGiaSu.sql
   Script chạy lại nhiều lần được (bảng đã có thì bỏ qua).
   =================================================================== */
IF DB_ID(N'QuanLyGiaSu') IS NULL
    CREATE DATABASE QuanLyGiaSu;
GO

USE QuanLyGiaSu;
GO

/* ----------------------------- GIA SƯ ----------------------------- */
IF OBJECT_ID(N'dbo.GiaSu', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.GiaSu
    (
        MaGS      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_GiaSu PRIMARY KEY,
        HoTen     NVARCHAR(100) NULL,
        SDT       VARCHAR(20)   NULL,
        ChuyenMon NVARCHAR(100) NULL,
        TrangThai NVARCHAR(50)  NULL CONSTRAINT DF_GiaSu_TrangThai DEFAULT N'Rảnh'
    );
    -- Không cho 2 gia sư trùng số điện thoại
    CREATE UNIQUE INDEX UX_GiaSu_SDT ON dbo.GiaSu(SDT) WHERE SDT IS NOT NULL;
END
GO

/* ----------------------------- HỌC VIÊN --------------------------- */
IF OBJECT_ID(N'dbo.HocVien', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HocVien
    (
        MaHV   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_HocVien PRIMARY KEY,
        HoTen  NVARCHAR(100) NULL,
        SDT    VARCHAR(20)   NULL,
        DiaChi NVARCHAR(200) NULL
    );
END
GO

/* ----------------------------- LỚP HỌC ---------------------------- */
IF OBJECT_ID(N'dbo.LopHoc', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LopHoc
    (
        MaLop           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_LopHoc PRIMARY KEY,
        MaGS            INT NULL CONSTRAINT FK_LopHoc_GiaSu  REFERENCES dbo.GiaSu(MaGS),
        MaHV            INT NULL CONSTRAINT FK_LopHoc_HocVien REFERENCES dbo.HocVien(MaHV),
        MonHoc          NVARCHAR(50)   NULL,
        HocPhi          DECIMAL(18, 2) NULL,
        TrangThai       NVARCHAR(50)   NULL,
        ThoiGianBatDau  DATETIME       NULL,
        ThoiGianKetThuc DATETIME       NULL,
        CaHoc           NVARCHAR(50)   NULL,
        CONSTRAINT CK_LopHoc_HocPhi   CHECK (HocPhi IS NULL OR HocPhi > 0),
        CONSTRAINT CK_LopHoc_ThoiGian CHECK (ThoiGianBatDau IS NULL OR ThoiGianKetThuc IS NULL
                                             OR ThoiGianBatDau < ThoiGianKetThuc)
    );
    CREATE INDEX IX_LopHoc_MaGS ON dbo.LopHoc(MaGS);
    CREATE INDEX IX_LopHoc_MaHV ON dbo.LopHoc(MaHV);
END
GO

/* ------------------------- DỮ LIỆU MẪU ---------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.GiaSu)
BEGIN
    INSERT INTO dbo.GiaSu (HoTen, SDT, ChuyenMon, TrangThai) VALUES
    (N'Nguyễn Văn An',     '0901234567', N'Toán',      N'Rảnh'),
    (N'Trần Thị Bình',     '0912345678', N'Tiếng Anh', N'Rảnh'),
    (N'Lê Minh Cường',     '0923456789', N'Cờ vua',    N'Rảnh'),
    (N'Phạm Thu Dung',     '0934567890', N'Vật lý',    N'Rảnh'),
    (N'Hoàng Gia Huy',     '0945678901', N'Tin học',   N'Rảnh'),
    (N'Võ Ngọc Lan',       '0956789012', N'Hóa học',   N'Tạm nghỉ');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.HocVien)
BEGIN
    INSERT INTO dbo.HocVien (HoTen, SDT, DiaChi) VALUES
    (N'Đỗ Bảo Ngọc',   '0981111111', N'Quận 1, TP.HCM'),
    (N'Bùi Minh Khoa', '0982222222', N'Bình Thạnh, TP.HCM'),
    (N'Ngô Thanh Tâm', '0983333333', N'Thủ Đức, TP.HCM'),
    (N'Lý Gia Bảo',    '0984444444', N'Quận 7, TP.HCM'),
    (N'Mai Anh Thư',   '0985555555', N'Gò Vấp, TP.HCM'),
    (N'Trịnh Quốc Việt','0986666666', N'Tân Bình, TP.HCM');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.LopHoc)
BEGIN
    DECLARE @homNay DATETIME = CAST(CAST(GETDATE() AS DATE) AS DATETIME);

    INSERT INTO dbo.LopHoc (MaGS, MaHV, MonHoc, HocPhi, TrangThai, ThoiGianBatDau, ThoiGianKetThuc, CaHoc) VALUES
    -- Đang diễn ra
    (1, 1, N'Toán',      1500000, N'Đang diễn ra', DATEADD(DAY, -10, @homNay), DATEADD(DAY, 20, @homNay), N'Tối (18h-20h)'),
    (3, 2, N'Cờ vua',    1200000, N'Đang diễn ra', DATEADD(DAY, -5,  @homNay), DATEADD(DAY, 55, @homNay), N'Chiều muộn (16h-18h)'),
    -- Cùng gia sư (1) nhưng khác ca => hợp lệ
    (1, 3, N'Toán',      1500000, N'Đang diễn ra', DATEADD(DAY, -3,  @homNay), DATEADD(DAY, 27, @homNay), N'Chiều (14h-16h)'),
    -- Chưa bắt đầu (sẽ có thông báo nếu bắt đầu trong 24h)
    (2, 4, N'Tiếng Anh', 1800000, N'Chưa bắt đầu', DATEADD(HOUR, 30, @homNay), DATEADD(DAY, 31, @homNay), N'Sáng (7h-9h)'),
    (5, 5, N'Tin học',   2000000, N'Chưa bắt đầu', DATEADD(DAY, 5,   @homNay), DATEADD(DAY, 65, @homNay), N'Tối muộn (20h-22h)'),
    -- Đã kết thúc
    (4, 6, N'Vật lý',    1300000, N'Đã kết thúc',  DATEADD(DAY, -60, @homNay), DATEADD(DAY, -2, @homNay), N'Sáng muộn (9h-11h)');

    -- Đồng bộ trạng thái gia sư theo lớp
    UPDATE g SET TrangThai =
        CASE
            WHEN EXISTS (SELECT 1 FROM dbo.LopHoc l WHERE l.MaGS = g.MaGS AND l.TrangThai = N'Đang diễn ra') THEN N'Đang dạy'
            WHEN EXISTS (SELECT 1 FROM dbo.LopHoc l WHERE l.MaGS = g.MaGS AND l.TrangThai = N'Chưa bắt đầu') THEN N'Bận'
            ELSE g.TrangThai
        END
    FROM dbo.GiaSu g
    WHERE g.TrangThai <> N'Tạm nghỉ';
END
GO

PRINT N'CSDL QuanLyGiaSu đã sẵn sàng.';
GO
