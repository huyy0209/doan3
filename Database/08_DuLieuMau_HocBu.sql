/* ===================================================================
   QuanLyGiaSu - DỮ LIỆU MẪU: HỌC BÙ (chạy SAU 04_NangCap_v4.sql và 06_DuLieuMau_DiemDanh_HocPhi.sql)
   - Khoảng một nửa số buổi vắng trong 21 ngày gần nhất được xếp học bù
     2 ngày sau buổi vắng (có thể là ngày trong tương lai = lịch đã xếp, chưa học),
     vào ca đầu tiên trong ngày mà cả học viên lẫn gia sư của lớp đều rảnh.
   - Buổi bù đã qua có điểm danh "Có mặt", một số có điểm test.
   Chạy lại nhiều lần được (buổi vắng đã có lịch bù thì bỏ qua).
   =================================================================== */
SET QUOTED_IDENTIFIER ON;   -- bắt buộc khi ghi vào bảng có chỉ mục có điều kiện
SET ANSI_NULLS ON;
GO
USE QuanLyGiaSu;
GO

DECLARE @HomNay DATE = CAST(GETDATE() AS DATE);

DECLARE @Ca TABLE (ThuTu INT, CaHoc NVARCHAR(50));
INSERT INTO @Ca VALUES (1, N'Sáng (7h-9h)'), (2, N'Sáng muộn (9h-11h)'), (3, N'Chiều (14h-16h)'),
                       (4, N'Chiều muộn (16h-18h)'), (5, N'Tối (18h-20h)'), (6, N'Tối muộn (20h-22h)');

DECLARE @MaBuoi INT, @MaLop INT, @MaHV INT, @MaGS INT, @Ngay DATE, @CaBu NVARCHAR(50);

DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
    SELECT b.MaBuoi, b.MaLop, l.MaHV, l.MaGS, DATEADD(DAY, 2, b.NgayHoc)
    FROM dbo.BuoiHoc b
    JOIN dbo.LopHoc l ON l.MaLop = b.MaLop
    JOIN dbo.GiaSu g ON g.MaGS = l.MaGS
    WHERE b.MaBuoiGoc IS NULL
      AND b.DiemDanh IN (N'Vắng có phép', N'Vắng không phép')
      AND b.NgayHoc >= DATEADD(DAY, -21, @HomNay)
      AND b.MaBuoi % 2 = 0
      AND g.TrangThai <> N'Tạm nghỉ'
      AND NOT EXISTS (SELECT 1 FROM dbo.BuoiHoc x WHERE x.MaBuoiGoc = b.MaBuoi)
    ORDER BY b.NgayHoc;

OPEN cur;
FETCH NEXT FROM cur INTO @MaBuoi, @MaLop, @MaHV, @MaGS, @Ngay;
WHILE @@FETCH_STATUS = 0
BEGIN
    -- Ca đầu tiên trong ngày mà học viên và gia sư đều không có lớp / buổi bù khác
    SET @CaBu = NULL;
    SELECT TOP 1 @CaBu = c.CaHoc
    FROM @Ca c
    WHERE NOT EXISTS (SELECT 1 FROM dbo.LopHoc l
                      WHERE l.TrangThai <> N'Đã hủy' AND l.CaHoc = c.CaHoc
                        AND (l.MaHV = @MaHV OR l.MaGS = @MaGS)
                        AND CAST(l.ThoiGianBatDau AS DATE) <= @Ngay AND CAST(l.ThoiGianKetThuc AS DATE) >= @Ngay)
      AND NOT EXISTS (SELECT 1 FROM dbo.BuoiHoc x JOIN dbo.LopHoc l ON l.MaLop = x.MaLop
                      WHERE x.MaBuoiGoc IS NOT NULL AND x.NgayHoc = @Ngay AND x.CaHoc = c.CaHoc
                        AND (l.MaHV = @MaHV OR x.MaGSDay = @MaGS))
    ORDER BY c.ThuTu;

    IF @CaBu IS NOT NULL
        INSERT INTO dbo.BuoiHoc (MaLop, NgayHoc, DiemDanh, Diem, NhanXet, CaHoc, MaGSDay, MaBuoiGoc)
        VALUES (@MaLop, @Ngay, N'Có mặt',
                CASE WHEN @Ngay <= @HomNay AND @MaBuoi % 3 = 0 THEN 6.5 + (@MaBuoi % 7) * 0.5 END,
                CASE WHEN @Ngay <= @HomNay THEN N'Học bù, đã nắm lại bài' END,
                @CaBu, @MaGS, @MaBuoi);

    FETCH NEXT FROM cur INTO @MaBuoi, @MaLop, @MaHV, @MaGS, @Ngay;
END
CLOSE cur;
DEALLOCATE cur;
GO

PRINT N'Đã thêm dữ liệu mẫu học bù.';
GO
