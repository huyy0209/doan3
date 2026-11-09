/* ===================================================================
   QuanLyGiaSu - NÂNG CẤP v3 (chạy SAU NangCap_v2.sql)
   Hồ sơ gia sư: email, học vấn / bằng cấp, số năm kinh nghiệm,
   các ca nhận dạy (dùng khi ghép lớp), ghi chú.
   Chạy lại nhiều lần được.
   =================================================================== */
USE QuanLyGiaSu;
GO

IF COL_LENGTH('dbo.GiaSu', 'Email') IS NULL
    ALTER TABLE dbo.GiaSu ADD Email NVARCHAR(100) NULL;
IF COL_LENGTH('dbo.GiaSu', 'HocVan') IS NULL
    ALTER TABLE dbo.GiaSu ADD HocVan NVARCHAR(150) NULL;
IF COL_LENGTH('dbo.GiaSu', 'KinhNghiem') IS NULL
    ALTER TABLE dbo.GiaSu ADD KinhNghiem INT NULL;
IF COL_LENGTH('dbo.GiaSu', 'CaNhanDay') IS NULL
    ALTER TABLE dbo.GiaSu ADD CaNhanDay NVARCHAR(200) NULL;   -- "Sáng,Tối"; NULL = mọi ca
IF COL_LENGTH('dbo.GiaSu', 'GhiChu') IS NULL
    ALTER TABLE dbo.GiaSu ADD GhiChu NVARCHAR(500) NULL;
GO

IF OBJECT_ID(N'dbo.CK_GiaSu_KinhNghiem', N'C') IS NULL
    ALTER TABLE dbo.GiaSu ADD CONSTRAINT CK_GiaSu_KinhNghiem CHECK (KinhNghiem IS NULL OR (KinhNghiem >= 0 AND KinhNghiem <= 60));
GO

-- Tăng tốc các truy vấn lọc theo ngày / trạng thái (lịch, điểm danh, tự cập nhật trạng thái)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_LopHoc_TrangThai_ThoiGian')
    CREATE INDEX IX_LopHoc_TrangThai_ThoiGian ON dbo.LopHoc(TrangThai, ThoiGianBatDau, ThoiGianKetThuc);
GO

PRINT N'Nâng cấp v3 hoàn tất.';
GO
