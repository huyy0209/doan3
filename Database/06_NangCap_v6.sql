/* ===================================================================
   QuanLyGiaSu - NÂNG CẤP v6 (chạy SAU 05_NangCap_v5.sql)
   - LopHoc.ThuHoc : các thứ học trong tuần, ví dụ "2,4,6" (2 = thứ Hai ... 8 = Chủ nhật).
                     NULL = học hằng ngày (lớp cũ giữ nguyên cách tính).
   - TaiKhoan.SoLanSai / KhoaDen : đếm số lần nhập sai mật khẩu, tạm khóa đăng nhập.
   - CaiDat : thông tin trung tâm (tên, địa chỉ, số điện thoại) dùng trên phiếu báo, màn hình đăng nhập.
   Chạy lại nhiều lần được.
   =================================================================== */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
USE QuanLyGiaSu;
GO

IF COL_LENGTH('dbo.LopHoc', 'ThuHoc') IS NULL
    ALTER TABLE dbo.LopHoc ADD ThuHoc VARCHAR(20) NULL;
IF COL_LENGTH('dbo.TaiKhoan', 'SoLanSai') IS NULL
    ALTER TABLE dbo.TaiKhoan ADD SoLanSai INT NOT NULL CONSTRAINT DF_TaiKhoan_SoLanSai DEFAULT 0;
IF COL_LENGTH('dbo.TaiKhoan', 'KhoaDen') IS NULL
    ALTER TABLE dbo.TaiKhoan ADD KhoaDen DATETIME NULL;
GO

IF OBJECT_ID(N'dbo.CaiDat', N'U') IS NULL
    CREATE TABLE dbo.CaiDat
    (
        Khoa   VARCHAR(50)   NOT NULL CONSTRAINT PK_CaiDat PRIMARY KEY,
        GiaTri NVARCHAR(500) NULL
    );
GO

PRINT N'Nâng cấp v6 (thứ học trong tuần, khóa đăng nhập, thông tin trung tâm) hoàn tất.';
GO
