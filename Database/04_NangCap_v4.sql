/* ===================================================================
   QuanLyGiaSu - NÂNG CẤP v4: HỌC BÙ (chạy SAU 03_NangCap_v3.sql)
   - Học viên vắng một buổi có thể học bù vào NGÀY và CA khác,
     với gia sư của lớp hoặc gia sư khác.
   - Buổi học bù là một dòng BuoiHoc mới, vẫn thuộc lớp gốc (MaLop),
     trỏ về buổi vắng qua MaBuoiGoc; ghi ca học bù (CaHoc) và gia sư dạy bù (MaGSDay).
   - Buổi thường: MaBuoiGoc, CaHoc, MaGSDay đều NULL (ca + gia sư lấy theo lớp).
   Chạy lại nhiều lần được.
   =================================================================== */
SET QUOTED_IDENTIFIER ON;   -- bắt buộc để tạo chỉ mục có điều kiện (filtered index)
SET ANSI_NULLS ON;
GO
USE QuanLyGiaSu;
GO

IF COL_LENGTH('dbo.BuoiHoc', 'CaHoc') IS NULL
    ALTER TABLE dbo.BuoiHoc ADD CaHoc NVARCHAR(50) NULL;
IF COL_LENGTH('dbo.BuoiHoc', 'MaGSDay') IS NULL
    ALTER TABLE dbo.BuoiHoc ADD MaGSDay INT NULL CONSTRAINT FK_BuoiHoc_GiaSuDay REFERENCES dbo.GiaSu(MaGS);
IF COL_LENGTH('dbo.BuoiHoc', 'MaBuoiGoc') IS NULL
    ALTER TABLE dbo.BuoiHoc ADD MaBuoiGoc INT NULL CONSTRAINT FK_BuoiHoc_BuoiGoc REFERENCES dbo.BuoiHoc(MaBuoi);
GO

-- Buổi học bù có thể rơi vào ngày lớp đó cũng có buổi thường (lớp học mỗi ngày),
-- nên bỏ ràng buộc "1 lớp / 1 ngày" cũ và chỉ áp cho buổi thường.
IF OBJECT_ID(N'dbo.UQ_BuoiHoc_Lop_Ngay', N'UQ') IS NOT NULL
    ALTER TABLE dbo.BuoiHoc DROP CONSTRAINT UQ_BuoiHoc_Lop_Ngay;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_BuoiHoc_Lop_Ngay_Thuong' AND object_id = OBJECT_ID('dbo.BuoiHoc'))
    CREATE UNIQUE INDEX UX_BuoiHoc_Lop_Ngay_Thuong ON dbo.BuoiHoc(MaLop, NgayHoc) WHERE MaBuoiGoc IS NULL;

-- Mỗi buổi vắng chỉ có tối đa 1 buổi học bù
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_BuoiHoc_BuoiGoc' AND object_id = OBJECT_ID('dbo.BuoiHoc'))
    CREATE UNIQUE INDEX UX_BuoiHoc_BuoiGoc ON dbo.BuoiHoc(MaBuoiGoc) WHERE MaBuoiGoc IS NOT NULL;

-- Tìm nhanh các buổi học bù theo ngày + ca (danh sách điểm danh theo ca, kiểm tra trùng lịch)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_BuoiHoc_HocBu_Ngay' AND object_id = OBJECT_ID('dbo.BuoiHoc'))
    CREATE INDEX IX_BuoiHoc_HocBu_Ngay ON dbo.BuoiHoc(NgayHoc, CaHoc) INCLUDE (MaLop, MaGSDay) WHERE MaBuoiGoc IS NOT NULL;
GO

-- Buổi học bù bắt buộc có ca và gia sư dạy
IF OBJECT_ID(N'dbo.CK_BuoiHoc_HocBu', N'C') IS NULL
    ALTER TABLE dbo.BuoiHoc ADD CONSTRAINT CK_BuoiHoc_HocBu
        CHECK (MaBuoiGoc IS NULL OR (CaHoc IS NOT NULL AND MaGSDay IS NOT NULL));
GO

PRINT N'Nâng cấp v4 (học bù) hoàn tất.';
GO
