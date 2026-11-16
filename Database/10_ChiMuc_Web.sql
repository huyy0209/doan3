/* Chỉ mục tối ưu truy vấn thường dùng của bản web; không thay đổi bảng hay cột. */
USE QuanLyGiaSu;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.BuoiHoc', N'U') IS NOT NULL
   AND NOT EXISTS
   (
       SELECT 1 FROM sys.indexes
       WHERE object_id = OBJECT_ID(N'dbo.BuoiHoc')
         AND name = N'IX_BuoiHoc_NgayHoc'
   )
    CREATE INDEX IX_BuoiHoc_NgayHoc
        ON dbo.BuoiHoc(NgayHoc)
        INCLUDE (MaLop, DiemDanh, MaGSDay, MaBuoiGoc, Diem, CaHoc, ThiLenCap);
GO

IF OBJECT_ID(N'dbo.ThanhToan', N'U') IS NOT NULL
   AND NOT EXISTS
   (
       SELECT 1 FROM sys.indexes
       WHERE object_id = OBJECT_ID(N'dbo.ThanhToan')
         AND name = N'IX_ThanhToan_NgayDong'
   )
    CREATE INDEX IX_ThanhToan_NgayDong
        ON dbo.ThanhToan(NgayDong)
        INCLUDE (MaLop, SoTien);
GO

IF OBJECT_ID(N'dbo.ThanhToan', N'U') IS NOT NULL
   AND NOT EXISTS
   (
       SELECT 1 FROM sys.indexes
       WHERE object_id = OBJECT_ID(N'dbo.ThanhToan')
         AND name = N'IX_ThanhToan_MaLop_NgayDong'
   )
    CREATE INDEX IX_ThanhToan_MaLop_NgayDong
        ON dbo.ThanhToan(MaLop, NgayDong DESC)
        INCLUDE (SoTien);
GO

IF OBJECT_ID(N'dbo.NhatKy', N'U') IS NOT NULL
   AND NOT EXISTS
   (
       SELECT 1 FROM sys.indexes
       WHERE object_id = OBJECT_ID(N'dbo.NhatKy')
         AND name = N'IX_NhatKy_ThoiGian_MaNK'
   )
    CREATE INDEX IX_NhatKy_ThoiGian_MaNK
        ON dbo.NhatKy(ThoiGian DESC, MaNK DESC)
        INCLUDE (TenDangNhap, HanhDong, Bang, KhoaChinh, DiaChiIP);
GO

PRINT N'Chỉ mục web đã sẵn sàng.';
GO
