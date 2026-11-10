/* ===================================================================
   QuanLyGiaSu - DỮ LIỆU MẪU: TÊN LỚP, CẤP ĐỘ, THI LÊN CẤP
   (chạy SAU 05_NangCap_v5.sql và các script dữ liệu mẫu 11 - 14)
   - Đặt tên + cấp độ đầu vào cho các lớp mẫu vừa thêm (thủ tục usp_DienThongTinLop)
   - Lớp năng khiếu đang học: bài test gần nhất có điểm được đánh dấu là "thi lên cấp"
     (lên cấp kế tiếp sau cấp đầu vào). Điểm từ 8 => đạt.
   Chạy lại nhiều lần được.
   =================================================================== */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
USE QuanLyGiaSu;
GO

EXEC dbo.usp_DienThongTinLop;
GO

;WITH lop AS
(
    SELECT l.MaLop, l.CapDoDauVao, m.CapDo
    FROM dbo.LopHoc l
    JOIN dbo.MonHoc m ON m.TenMon = l.MonHoc
    WHERE m.LoaiMon = N'Năng khiếu' AND m.CapDo IS NOT NULL AND l.CapDoDauVao IS NOT NULL
      AND NOT EXISTS (SELECT 1 FROM dbo.BuoiHoc b WHERE b.MaLop = l.MaLop AND b.ThiLenCap = 1)
),
cap AS   -- cấp kế tiếp sau cấp đầu vào
(
    SELECT lop.MaLop,
           NULLIF(LEFT(phan_sau.Chuoi, CHARINDEX(N';', phan_sau.Chuoi + N';') - 1), N'') AS CapKe
    FROM lop
    CROSS APPLY (SELECT CHARINDEX(N';' + lop.CapDoDauVao + N';', N';' + lop.CapDo + N';') AS ViTri) v
    CROSS APPLY (SELECT CASE WHEN v.ViTri > 0
                             THEN SUBSTRING(lop.CapDo + N';', v.ViTri + LEN(lop.CapDoDauVao) + 1, 500)
                             ELSE N'' END AS Chuoi) phan_sau
),
bai AS   -- bài test gần nhất có điểm của mỗi lớp
(
    SELECT b.MaBuoi, b.MaLop, ROW_NUMBER() OVER (PARTITION BY b.MaLop ORDER BY b.NgayHoc DESC) AS rn
    FROM dbo.BuoiHoc b
    WHERE b.Diem IS NOT NULL AND b.MaLop IN (SELECT MaLop FROM cap WHERE CapKe IS NOT NULL)
)
UPDATE b
SET ThiLenCap = 1, CapDoThi = cap.CapKe
FROM dbo.BuoiHoc b
JOIN bai ON bai.MaBuoi = b.MaBuoi AND bai.rn = 1
JOIN cap ON cap.MaLop = b.MaLop;
GO

PRINT N'Đã thêm dữ liệu mẫu tên lớp, cấp độ, thi lên cấp.';
GO
