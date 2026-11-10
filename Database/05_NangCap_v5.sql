/* ===================================================================
   QuanLyGiaSu - NÂNG CẤP v5 (chạy SAU 04_NangCap_v4.sql)
   - MonHoc: danh mục môn học do quản lý thêm / sửa / xóa.
       LoaiMon = N'Chính'      => đánh giá bằng xếp loại Yếu / Trung bình / Khá / Giỏi theo điểm TB
       LoaiMon = N'Năng khiếu' => đánh giá theo cấp độ (CapDo: danh sách cấp, cách nhau bằng dấu ;)
       NgungDay = 1            => ẩn khỏi danh sách khi ghép lớp mới (lớp cũ giữ nguyên)
   - LopHoc.TenLop     : tên lớp do người ghép lớp đặt (không trùng)
   - LopHoc.CapDoDauVao: cấp độ lúc vào lớp (môn năng khiếu)
   - BuoiHoc.ThiLenCap : buổi này là bài thi lên cấp; CapDoThi = cấp thi lên.
       Đạt khi điểm từ 8 trở lên => học viên lên cấp đó.
   - Thủ tục dbo.usp_DienThongTinLop: điền môn học còn thiếu vào danh mục,
     đặt tên cho lớp chưa có tên, cấp độ đầu vào cho lớp năng khiếu.
   Chạy lại nhiều lần được.
   =================================================================== */
SET QUOTED_IDENTIFIER ON;   -- bắt buộc với chỉ mục có điều kiện
SET ANSI_NULLS ON;
GO
USE QuanLyGiaSu;
GO

IF OBJECT_ID(N'dbo.MonHoc', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.MonHoc
    (
        MaMon    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_MonHoc PRIMARY KEY,
        TenMon   NVARCHAR(50)  NOT NULL CONSTRAINT UQ_MonHoc_TenMon UNIQUE,
        LoaiMon  NVARCHAR(20)  NOT NULL CONSTRAINT DF_MonHoc_LoaiMon DEFAULT N'Chính',
        CapDo    NVARCHAR(500) NULL,
        NgungDay BIT NOT NULL CONSTRAINT DF_MonHoc_NgungDay DEFAULT 0,
        CONSTRAINT CK_MonHoc_LoaiMon CHECK (LoaiMon IN (N'Chính', N'Năng khiếu'))
    );
END
GO

IF COL_LENGTH('dbo.LopHoc', 'TenLop') IS NULL
    ALTER TABLE dbo.LopHoc ADD TenLop NVARCHAR(150) NULL;
IF COL_LENGTH('dbo.LopHoc', 'CapDoDauVao') IS NULL
    ALTER TABLE dbo.LopHoc ADD CapDoDauVao NVARCHAR(50) NULL;
IF COL_LENGTH('dbo.BuoiHoc', 'ThiLenCap') IS NULL
    ALTER TABLE dbo.BuoiHoc ADD ThiLenCap BIT NOT NULL CONSTRAINT DF_BuoiHoc_ThiLenCap DEFAULT 0;
IF COL_LENGTH('dbo.BuoiHoc', 'CapDoThi') IS NULL
    ALTER TABLE dbo.BuoiHoc ADD CapDoThi NVARCHAR(50) NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_LopHoc_TenLop' AND object_id = OBJECT_ID('dbo.LopHoc'))
    CREATE UNIQUE INDEX UX_LopHoc_TenLop ON dbo.LopHoc(TenLop) WHERE TenLop IS NOT NULL;
GO

-- Danh mục mặc định
DECLARE @Mon TABLE (TenMon NVARCHAR(50), LoaiMon NVARCHAR(20), CapDo NVARCHAR(500));
INSERT INTO @Mon VALUES
    (N'Toán', N'Chính', NULL), (N'Ngữ văn', N'Chính', NULL), (N'Tiếng Anh', N'Chính', NULL),
    (N'Vật lý', N'Chính', NULL), (N'Hóa học', N'Chính', NULL), (N'Sinh học', N'Chính', NULL),
    (N'Tin học', N'Chính', NULL),
    (N'Cờ vua', N'Năng khiếu', N'Mầm non;Vỡ lòng;Căn bản;Sơ cấp;Trung cấp;Nâng cao'),
    (N'Lập trình Scratch', N'Năng khiếu', N'Làm quen;Cơ bản;Nâng cao;Dự án'),
    (N'Vẽ', N'Năng khiếu', N'Cấp 1;Cấp 2;Cấp 3;Cấp 4;Cấp 5'),
    (N'Đàn piano', N'Năng khiếu', N'Cấp 1;Cấp 2;Cấp 3;Cấp 4;Cấp 5');
INSERT INTO dbo.MonHoc (TenMon, LoaiMon, CapDo)
SELECT m.TenMon, m.LoaiMon, m.CapDo FROM @Mon m
WHERE NOT EXISTS (SELECT 1 FROM dbo.MonHoc x WHERE x.TenMon = m.TenMon);
GO

IF OBJECT_ID(N'dbo.usp_DienThongTinLop', N'P') IS NOT NULL DROP PROCEDURE dbo.usp_DienThongTinLop;
GO
CREATE PROCEDURE dbo.usp_DienThongTinLop
AS
BEGIN
    SET NOCOUNT ON;

    -- 1. Môn đang dùng (lớp, chuyên môn gia sư) mà chưa có trong danh mục => thêm, coi là môn chính
    INSERT INTO dbo.MonHoc (TenMon, LoaiMon)
    SELECT DISTINCT LTRIM(RTRIM(t.Ten)), N'Chính'
    FROM (SELECT MonHoc AS Ten FROM dbo.LopHoc UNION SELECT ChuyenMon FROM dbo.GiaSu) t
    WHERE t.Ten IS NOT NULL AND LTRIM(RTRIM(t.Ten)) <> N''
      AND NOT EXISTS (SELECT 1 FROM dbo.MonHoc m WHERE m.TenMon = LTRIM(RTRIM(t.Ten)));

    -- 2. Tên lớp: "Môn - Học viên - Ca"; trùng thì thêm mã lớp
    ;WITH x AS
    (
        SELECT l.MaLop,
               LEFT(CONCAT(l.MonHoc, N' - ', ISNULL(h.HoTen, N'?'), N' - ',
                    CASE WHEN CHARINDEX(N' (', l.CaHoc) > 0 THEN LEFT(l.CaHoc, CHARINDEX(N' (', l.CaHoc) - 1) ELSE l.CaHoc END), 130) AS Ten
        FROM dbo.LopHoc l
        LEFT JOIN dbo.HocVien h ON h.MaHV = l.MaHV
        WHERE l.TenLop IS NULL
    ),
    y AS (SELECT MaLop, Ten, ROW_NUMBER() OVER (PARTITION BY Ten ORDER BY MaLop) AS rn FROM x)
    UPDATE l
    SET TenLop = CASE WHEN y.rn = 1 AND NOT EXISTS (SELECT 1 FROM dbo.LopHoc z WHERE z.TenLop = y.Ten)
                      THEN y.Ten ELSE CONCAT(y.Ten, N' (', l.MaLop, N')') END
    FROM dbo.LopHoc l
    JOIN y ON y.MaLop = l.MaLop;

    -- 3. Cấp độ đầu vào cho lớp năng khiếu: lấy trình độ của học viên nếu khớp một cấp của môn, không thì cấp đầu tiên
    UPDATE l
    SET CapDoDauVao = CASE
            WHEN h.TrinhDo IS NOT NULL AND CHARINDEX(N';' + h.TrinhDo + N';', N';' + m.CapDo + N';') > 0 THEN h.TrinhDo
            WHEN CHARINDEX(N';', m.CapDo) > 0 THEN LEFT(m.CapDo, CHARINDEX(N';', m.CapDo) - 1)
            ELSE m.CapDo END
    FROM dbo.LopHoc l
    JOIN dbo.MonHoc m ON m.TenMon = l.MonHoc
    LEFT JOIN dbo.HocVien h ON h.MaHV = l.MaHV
    WHERE l.CapDoDauVao IS NULL AND m.LoaiMon = N'Năng khiếu' AND m.CapDo IS NOT NULL;
END
GO

EXEC dbo.usp_DienThongTinLop;
GO

PRINT N'Nâng cấp v5 (môn học, tên lớp, thi lên cấp) hoàn tất.';
GO
