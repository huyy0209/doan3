/* Nhật ký thao tác dành riêng cho bản web. Có thể chạy lại an toàn. */
USE QuanLyGiaSu;
GO

IF OBJECT_ID(N'dbo.TaiKhoan', N'U') IS NULL
BEGIN
    RAISERROR(N'Chưa có bảng dbo.TaiKhoan. Hãy chạy các script cấu trúc trước.', 16, 1);
END
ELSE IF OBJECT_ID(N'dbo.NhatKy', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.NhatKy
    (
        MaNK int IDENTITY(1,1) NOT NULL CONSTRAINT PK_NhatKy PRIMARY KEY,
        ThoiGian datetime2 NOT NULL CONSTRAINT DF_NhatKy_ThoiGian DEFAULT (SYSDATETIME()),
        MaTK int NULL,
        TenDangNhap nvarchar(50) NULL,
        HanhDong nvarchar(20) NOT NULL,
        Bang nvarchar(50) NULL,
        KhoaChinh nvarchar(100) NULL,
        ChiTiet nvarchar(max) NULL,
        DiaChiIP nvarchar(45) NULL,
        CONSTRAINT FK_NhatKy_TaiKhoan FOREIGN KEY (MaTK) REFERENCES dbo.TaiKhoan(MaTK)
    );
END
GO

IF OBJECT_ID(N'dbo.NhatKy', N'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.NhatKy') AND name = N'IX_NhatKy_ThoiGian')
        CREATE INDEX IX_NhatKy_ThoiGian ON dbo.NhatKy(ThoiGian DESC);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.NhatKy') AND name = N'IX_NhatKy_Bang_KhoaChinh')
        CREATE INDEX IX_NhatKy_Bang_KhoaChinh ON dbo.NhatKy(Bang, KhoaChinh);
END
GO
