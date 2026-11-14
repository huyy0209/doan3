/* ===================================================================
   QuanLyGiaSu - NÂNG CẤP v8: UNIQUE INDEX cho TaiKhoan.MaGS
   Chạy SAU 06_NangCap_v6.sql (hoặc sau 02_NangCap_v2.sql).
   Chạy lại nhiều lần được: chỉ tạo index nếu chưa có.

   Nghiệp vụ: mỗi gia sư chỉ được cấp tối đa MỘT tài khoản đăng nhập.
   Index có điều kiện WHERE MaGS IS NOT NULL (tài khoản quản lý có MaGS = NULL
   vẫn được phép có nhiều bản ghi).
   =================================================================== */
USE QuanLyGiaSu;
GO

IF OBJECT_ID(N'dbo.TaiKhoan', N'U') IS NULL
BEGIN
    RAISERROR(N'Chưa có bảng dbo.TaiKhoan. Hãy chạy 02_NangCap_v2.sql trước.', 16, 1);
END
ELSE IF EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_TaiKhoan_MaGS' AND object_id = OBJECT_ID(N'dbo.TaiKhoan')
)
BEGIN
    PRINT N'Index UX_TaiKhoan_MaGS đã tồn tại, bỏ qua.';
END
ELSE IF EXISTS
(
    SELECT MaGS FROM dbo.TaiKhoan
    WHERE MaGS IS NOT NULL
    GROUP BY MaGS
    HAVING COUNT(*) > 1
)
BEGIN
    RAISERROR(N'Không tạo được UX_TaiKhoan_MaGS: đang có nhiều tài khoản trùng MaGS. Hãy xử lý dữ liệu trùng trước (xóa/đổi MaGS) rồi chạy lại script.', 16, 1);
END
ELSE
BEGIN
    CREATE UNIQUE INDEX UX_TaiKhoan_MaGS ON dbo.TaiKhoan(MaGS) WHERE MaGS IS NOT NULL;
    PRINT N'Đã tạo index UX_TaiKhoan_MaGS.';
END
GO
