/* ===================================================================
   QuanLyGiaSu - DỮ LIỆU MẪU v6: THỨ HỌC TRONG TUẦN (chạy SAU 06_NangCap_v6.sql và 11 - 15)
   - 1/3 số lớp mẫu học thứ 2-4-6, 1/3 học thứ 3-5-7, còn lại học hằng ngày.
   - Xóa điểm danh mẫu rơi vào ngày lớp không học (giữ buổi đã có học bù).
   - Thông tin trung tâm mẫu.
   Chạy lại nhiều lần được.
   =================================================================== */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
USE QuanLyGiaSu;
GO

UPDATE dbo.LopHoc
SET ThuHoc = CASE MaLop % 3 WHEN 0 THEN '2,4,6' WHEN 1 THEN '3,5,7' END
WHERE ThuHoc IS NULL AND MaLop % 3 IN (0, 1) AND MaLop > 6;
GO

-- 01/01/1900 là thứ Hai => (số ngày tính từ đó) % 7 + 2 = thứ (2..8), không phụ thuộc SET DATEFIRST
DELETE b
FROM dbo.BuoiHoc b
JOIN dbo.LopHoc l ON l.MaLop = b.MaLop
WHERE l.ThuHoc IS NOT NULL
  AND b.MaBuoiGoc IS NULL
  AND CHARINDEX(CAST(DATEDIFF(DAY, '19000101', b.NgayHoc) % 7 + 2 AS VARCHAR(1)), l.ThuHoc) = 0
  AND NOT EXISTS (SELECT 1 FROM dbo.BuoiHoc x WHERE x.MaBuoiGoc = b.MaBuoi);
GO

INSERT INTO dbo.CaiDat (Khoa, GiaTri)
SELECT v.Khoa, v.GiaTri
FROM (VALUES ('TenTrungTam', N'Trung tâm Gia sư Ánh Dương'),
             ('DiaChi', N'12 Nguyễn Văn Bảo, Gò Vấp, TP.HCM'),
             ('SDT', N'028 1234 5678')) v(Khoa, GiaTri)
WHERE NOT EXISTS (SELECT 1 FROM dbo.CaiDat c WHERE c.Khoa = v.Khoa);
GO

PRINT N'Đã thêm dữ liệu mẫu v6.';
GO
