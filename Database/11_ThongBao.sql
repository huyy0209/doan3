/* Thông báo dành riêng cho bản web. Có thể chạy lại an toàn. */
USE QuanLyGiaSu;
GO

IF OBJECT_ID(N'dbo.TaiKhoan', N'U') IS NULL
BEGIN
    RAISERROR(N'Chưa có bảng dbo.TaiKhoan. Hãy chạy các script cấu trúc trước.', 16, 1);
END
ELSE IF OBJECT_ID(N'dbo.ThongBao', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ThongBao
    (
        MaTB int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ThongBao PRIMARY KEY,
        MaTK int NOT NULL,
        TieuDe nvarchar(200) NOT NULL,
        NoiDung nvarchar(1000) NOT NULL,
        DuongDan nvarchar(300) NULL,
        DaDoc bit NOT NULL CONSTRAINT DF_ThongBao_DaDoc DEFAULT (0),
        ThoiGian datetime2 NOT NULL CONSTRAINT DF_ThongBao_ThoiGian DEFAULT (SYSDATETIME()),
        CONSTRAINT FK_ThongBao_TaiKhoan FOREIGN KEY (MaTK) REFERENCES dbo.TaiKhoan(MaTK)
    );
END
GO

IF OBJECT_ID(N'dbo.ThongBao', N'U') IS NOT NULL
    AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.ThongBao') AND name = N'IX_ThongBao_MaTK_DaDoc_ThoiGian')
BEGIN
    CREATE INDEX IX_ThongBao_MaTK_DaDoc_ThoiGian
        ON dbo.ThongBao(MaTK, DaDoc, ThoiGian DESC);
END
GO
