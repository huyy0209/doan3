/* ===================================================================
   QuanLyGiaSu - DỮ LIỆU MẪU: HỒ SƠ HỌC VIÊN, ĐIỂM DANH, ĐIỂM TEST, HỌC PHÍ
   Chạy SAU 04_NangCap_v4.sql (và sau 11_DuLieuMau_ThemNhieu.sql nếu có).
   - Bổ sung phụ huynh / trường lớp / trình độ cho học viên còn trống
   - Tạo điểm danh cho các buổi đã học trong 21 ngày gần nhất,
     khoảng 1/3 số buổi có bài test 15 phút
   - Tạo các lần đóng học phí: đóng đủ, đóng một phần, chưa đóng
   Chạy lại nhiều lần được (chỉ thêm phần còn thiếu).
   =================================================================== */
SET QUOTED_IDENTIFIER ON;   -- bảng BuoiHoc có chỉ mục có điều kiện (04_NangCap_v4.sql)
SET ANSI_NULLS ON;
GO
USE QuanLyGiaSu;
GO

/* ----------------------- HỒ SƠ HỌC VIÊN ----------------------- */
;WITH hv AS (
    SELECT MaHV, HoTen,
           ABS(CHECKSUM(HoTen)) AS h
    FROM dbo.HocVien
)
UPDATE x SET
    TenPhuHuynh = COALESCE(x.TenPhuHuynh,
        CASE hv.h % 2 WHEN 0 THEN N'Anh ' ELSE N'Chị ' END +
        (CASE hv.h % 6 WHEN 0 THEN N'Tuấn' WHEN 1 THEN N'Hương' WHEN 2 THEN N'Dũng'
                       WHEN 3 THEN N'Mai' WHEN 4 THEN N'Phong' ELSE N'Thảo' END)),
    SDTPhuHuynh = COALESCE(x.SDTPhuHuynh, x.SDT),
    TruongLop = COALESCE(x.TruongLop,
        (CASE hv.h % 4 WHEN 0 THEN N'TH Lê Văn Tám' WHEN 1 THEN N'TH Nguyễn Bỉnh Khiêm'
                       WHEN 2 THEN N'THCS Trần Văn Ơn' ELSE N'TH Kim Đồng' END) +
        N', lớp ' + CAST(1 + hv.h % 6 AS NVARCHAR(2))),
    TrinhDo = COALESCE(x.TrinhDo,
        CASE hv.h % 4 WHEN 0 THEN N'Vỡ lòng' WHEN 1 THEN N'Căn bản' WHEN 2 THEN N'Sơ cấp' ELSE N'Trung cấp' END)
FROM dbo.HocVien x
JOIN hv ON hv.MaHV = x.MaHV;
GO

/* ----------------------- ĐIỂM DANH + ĐIỂM TEST ----------------------- */
-- Mỗi lớp có buổi học mỗi ngày trong khoảng [bắt đầu, kết thúc].
-- Tạo bản ghi cho các ngày đã qua (tối đa 21 ngày gần nhất), bỏ lớp đã hủy.
;WITH so AS (
    SELECT TOP (21) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n
    FROM sys.all_objects
),
ngay AS (
    SELECT l.MaLop, CAST(DATEADD(DAY, -so.n, CAST(GETDATE() AS DATE)) AS DATE) AS NgayHoc
    FROM dbo.LopHoc l
    CROSS JOIN so
    WHERE l.TrangThai <> N'Đã hủy'
      AND l.ThoiGianBatDau IS NOT NULL AND l.ThoiGianKetThuc IS NOT NULL
      AND DATEADD(DAY, -so.n, CAST(GETDATE() AS DATE)) >= CAST(l.ThoiGianBatDau AS DATE)
      AND DATEADD(DAY, -so.n, CAST(GETDATE() AS DATE)) <= CAST(l.ThoiGianKetThuc AS DATE)
),
ngau_nhien AS (
    SELECT MaLop, NgayHoc,
           ABS(CHECKSUM(MaLop * 7919 + DATEDIFF(DAY, '2000-01-01', NgayHoc))) AS r
    FROM ngay
)
INSERT INTO dbo.BuoiHoc (MaLop, NgayHoc, DiemDanh, Diem, NhanXet)
SELECT MaLop, NgayHoc,
       CASE WHEN r % 20 = 0 THEN N'Vắng không phép'
            WHEN r % 20 = 1 THEN N'Vắng có phép'
            WHEN r % 20 = 2 THEN N'Đi trễ'
            ELSE N'Có mặt' END,
       -- khoảng 1/3 số buổi có mặt có bài test; điểm 4.0 - 10.0, bước 0.25
       CASE WHEN r % 20 >= 2 AND r % 3 = 0
            THEN CAST(4 + (r % 25) * 0.25 AS DECIMAL(4,2)) END,
       CASE WHEN r % 20 >= 2 AND r % 3 = 0 THEN
            CASE r % 5 WHEN 0 THEN N'Nắm bài tốt' WHEN 1 THEN N'Cần ôn lại phần cuối'
                       WHEN 2 THEN N'Tiến bộ so với tuần trước' WHEN 3 THEN N'Làm bài nhanh, còn sai sót nhỏ'
                       ELSE N'Tập trung tốt' END END
FROM ngau_nhien
WHERE NOT EXISTS (SELECT 1 FROM dbo.BuoiHoc b WHERE b.MaLop = ngau_nhien.MaLop AND b.NgayHoc = ngau_nhien.NgayHoc AND b.MaBuoiGoc IS NULL);

PRINT N'Đã thêm ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N' buổi học.';
GO

/* ----------------------- HỌC PHÍ ----------------------- */
-- Lớp đã kết thúc: đóng đủ. Lớp đang học: đóng đủ / đóng 50% / chưa đóng. Lớp chưa học: một số đặt cọc 30%.
;WITH l AS (
    SELECT MaLop, HocPhi, TrangThai, ThoiGianBatDau, ABS(CHECKSUM(MaLop * 104729)) % 3 AS k
    FROM dbo.LopHoc
    WHERE HocPhi IS NOT NULL AND TrangThai <> N'Đã hủy'
      AND NOT EXISTS (SELECT 1 FROM dbo.ThanhToan t WHERE t.MaLop = LopHoc.MaLop)
)
INSERT INTO dbo.ThanhToan (MaLop, SoTien, NgayDong, HinhThuc, GhiChu)
SELECT MaLop,
       CASE WHEN TrangThai = N'Đã kết thúc' OR k = 0 THEN HocPhi
            WHEN TrangThai = N'Chưa bắt đầu' THEN ROUND(HocPhi * 0.3, -3)
            ELSE ROUND(HocPhi * 0.5, -3) END,
       CASE WHEN ThoiGianBatDau < GETDATE() THEN ThoiGianBatDau ELSE GETDATE() END,
       CASE k WHEN 1 THEN N'Tiền mặt' ELSE N'Chuyển khoản' END,
       CASE WHEN TrangThai = N'Chưa bắt đầu' THEN N'Đặt cọc'
            WHEN TrangThai = N'Đã kết thúc' OR k = 0 THEN N'Đóng đủ khóa'
            ELSE N'Đóng đợt 1' END
FROM l
WHERE NOT (TrangThai = N'Đang diễn ra' AND k = 2)       -- 1/3 lớp đang học chưa đóng
  AND NOT (TrangThai = N'Chưa bắt đầu' AND k <> 1);    -- 1/3 lớp sắp học đã đặt cọc

PRINT N'Đã thêm ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N' lần đóng học phí.';
GO
