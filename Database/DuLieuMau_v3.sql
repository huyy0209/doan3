/* ===================================================================
   QuanLyGiaSu - DỮ LIỆU MẪU v3 (chạy SAU NangCap_v3.sql)
   - Hồ sơ gia sư còn trống: email, học vấn, kinh nghiệm, ca nhận dạy, ghi chú
   - Ghi chú cho học viên còn trống
   - Tài khoản đăng nhập cho mọi gia sư chưa có: gs<Mã gia sư> / 123456
     (VD: gs1, gs3...). Đây là MẬT KHẨU MẪU, đổi ngay khi dùng thật.
   Chạy lại nhiều lần được (chỉ điền phần còn trống).
   =================================================================== */
USE QuanLyGiaSu;
GO

/* ----------------------- HỒ SƠ GIA SƯ ----------------------- */
;WITH g AS (SELECT MaGS, ChuyenMon, ABS(CHECKSUM(HoTen)) AS h FROM dbo.GiaSu)
UPDATE x SET
    Email = COALESCE(x.Email, N'giasu' + CAST(x.MaGS AS NVARCHAR(10)) + N'@trungtam.edu.vn'),
    HocVan = COALESCE(x.HocVan,
        CASE WHEN g.ChuyenMon = N'Cờ vua' THEN
                 CASE g.h % 3 WHEN 0 THEN N'Kiện tướng quốc gia' WHEN 1 THEN N'Vận động viên cờ vua cấp 1' ELSE N'HLV cờ vua, chứng chỉ FIDE Arbiter' END
             ELSE
                 CASE g.h % 4 WHEN 0 THEN N'Cử nhân Sư phạm' WHEN 1 THEN N'Sinh viên năm 4'
                              WHEN 2 THEN N'Thạc sĩ' ELSE N'Kỹ sư' END + N' ' + g.ChuyenMon
        END),
    KinhNghiem = COALESCE(x.KinhNghiem, 1 + g.h % 8),
    CaNhanDay = CASE WHEN x.CaNhanDay IS NOT NULL THEN x.CaNhanDay
                     WHEN g.h % 4 = 0 THEN NULL                                         -- nhận mọi ca
                     WHEN g.h % 4 = 1 THEN N'Chiều,Chiều muộn,Tối,Tối muộn'             -- đi học buổi sáng
                     WHEN g.h % 4 = 2 THEN N'Sáng,Sáng muộn,Chiều,Chiều muộn'           -- không dạy tối
                     ELSE N'Tối,Tối muộn' END,                                           -- chỉ dạy tối
    GhiChu = COALESCE(x.GhiChu,
        CASE g.h % 3 WHEN 0 THEN N'Ưu tiên học viên tiểu học'
                     WHEN 1 THEN N'Có thể dạy online khi cần'
                     ELSE N'Nhận thêm lớp cuối tuần' END)
FROM dbo.GiaSu x
JOIN g ON g.MaGS = x.MaGS;
GO

/* ----------------------- GHI CHÚ HỌC VIÊN ----------------------- */
UPDATE dbo.HocVien
SET GhiChu = CASE ABS(CHECKSUM(HoTen)) % 4
                 WHEN 0 THEN N'Hiếu động, cần bài tập ngắn'
                 WHEN 1 THEN N'Phụ huynh muốn nhận báo cáo mỗi tháng'
                 WHEN 2 THEN N'Học tốt, có thể lên trình độ sau khóa này'
                 ELSE N'Hay quên bài, nhắc ôn tập trước buổi học' END
WHERE GhiChu IS NULL;
GO

/* ----------------------- TÀI KHOẢN GIA SƯ ----------------------- */
-- Mật khẩu "123456" đã băm sẵn bằng PBKDF2-SHA256 (giống cách ứng dụng băm)
INSERT INTO dbo.TaiKhoan (TenDangNhap, MatKhauHash, VaiTro, MaGS, HoatDong)
SELECT 'gs' + CAST(g.MaGS AS VARCHAR(10)),
       '100000.Whw+m30vSKawxOgfbZOidQ==.MJpoB1nj2Y4yM+uQ8U1X2agJ05GgD74aLPOR5PXBIGs=',
       N'GiaSu', g.MaGS,
       CASE WHEN g.TrangThai = N'Tạm nghỉ' THEN 0 ELSE 1 END
FROM dbo.GiaSu g
WHERE NOT EXISTS (SELECT 1 FROM dbo.TaiKhoan t WHERE t.MaGS = g.MaGS)
  AND NOT EXISTS (SELECT 1 FROM dbo.TaiKhoan t WHERE t.TenDangNhap = 'gs' + CAST(g.MaGS AS VARCHAR(10)));

PRINT N'Đã tạo ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N' tài khoản gia sư (mật khẩu mẫu 123456).';
GO
