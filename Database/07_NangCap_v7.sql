/* ===================================================================
   QuanLyGiaSu - NÂNG CẤP v7 (chạy SAU 06_NangCap_v6.sql)
   - LOW-02: Chuẩn hóa MonHoc sang khóa ngoại MaMon trong bảng LopHoc.
     * Thêm cột LopHoc.MaMon INT NULL
     * Backfill dữ liệu MaMon từ bảng MonHoc theo tên môn (LopHoc.MonHoc = MonHoc.TenMon)
     * Thêm ràng buộc khóa ngoại FK_LopHoc_MonHoc (LopHoc.MaMon -> MonHoc.MaMon)
   Chạy lại nhiều lần được (idempotent). Tên không khớp thì để NULL.
   =================================================================== */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
USE QuanLyGiaSu;
GO

-- 1. Thêm cột MaMon vào LopHoc nếu chưa tồn tại
IF COL_LENGTH('dbo.LopHoc', 'MaMon') IS NULL
BEGIN
    ALTER TABLE dbo.LopHoc ADD MaMon INT NULL;
    PRINT N'Đã thêm cột MaMon vào bảng LopHoc.';
END
GO

-- 2. Backfill MaMon từ bảng MonHoc theo tên môn (tên không khớp thì để NULL)
IF COL_LENGTH('dbo.LopHoc', 'MaMon') IS NOT NULL AND OBJECT_ID(N'dbo.MonHoc', N'U') IS NOT NULL
BEGIN
    UPDATE l
    SET l.MaMon = m.MaMon
    FROM dbo.LopHoc l
    INNER JOIN dbo.MonHoc m ON l.MonHoc = m.TenMon
    WHERE l.MaMon IS NULL;
    PRINT N'Đã backfill MaMon cho bảng LopHoc theo tên MonHoc.';
END
GO

-- 3. Tạo khóa ngoại FK_LopHoc_MonHoc nếu chưa tồn tại
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_LopHoc_MonHoc' AND parent_object_id = OBJECT_ID('dbo.LopHoc'))
BEGIN
    ALTER TABLE dbo.LopHoc
    ADD CONSTRAINT FK_LopHoc_MonHoc FOREIGN KEY (MaMon) REFERENCES dbo.MonHoc(MaMon);
    PRINT N'Đã tạo khóa ngoại FK_LopHoc_MonHoc.';
END
GO

PRINT N'Nâng cấp v7 (chuẩn hóa khóa ngoại MaMon trong LopHoc) hoàn tất.';
GO
