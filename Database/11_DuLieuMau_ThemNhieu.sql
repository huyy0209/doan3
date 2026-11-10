/* ===================================================================
   QuanLyGiaSu - THÊM DỮ LIỆU MẪU (chạy SAU script 01_QuanLyGiaSu.sql)
   - 14 gia sư, 30 học viên, ~45 lớp học ở đủ trạng thái:
     đã kết thúc, đang diễn ra, chưa bắt đầu, sắp học trong 24 giờ, đã hủy
   - Ngày tính theo ngày chạy script => luôn có dữ liệu "hôm nay"
   - Không tạo lớp trùng ca của cùng gia sư / học viên
   - Chạy lại nhiều lần được: bản ghi đã có thì bỏ qua
   =================================================================== */
SET QUOTED_IDENTIFIER ON;   -- bảng có chỉ mục có điều kiện (04, 05)
GO
USE QuanLyGiaSu;
GO

/* ----------------------------- GIA SƯ ----------------------------- */
INSERT INTO dbo.GiaSu (HoTen, SDT, ChuyenMon, TrangThai) SELECT N'Đặng Quốc Bảo', '0961000000', N'Cờ vua', N'Rảnh' WHERE NOT EXISTS (SELECT 1 FROM dbo.GiaSu WHERE SDT = '0961000000');
INSERT INTO dbo.GiaSu (HoTen, SDT, ChuyenMon, TrangThai) SELECT N'Trương Mỹ Linh', '0961001137', N'Tiếng Anh', N'Rảnh' WHERE NOT EXISTS (SELECT 1 FROM dbo.GiaSu WHERE SDT = '0961001137');
INSERT INTO dbo.GiaSu (HoTen, SDT, ChuyenMon, TrangThai) SELECT N'Phan Thanh Tùng', '0961002274', N'Toán', N'Rảnh' WHERE NOT EXISTS (SELECT 1 FROM dbo.GiaSu WHERE SDT = '0961002274');
INSERT INTO dbo.GiaSu (HoTen, SDT, ChuyenMon, TrangThai) SELECT N'Huỳnh Ngọc Hân', '0961003411', N'Ngữ văn', N'Rảnh' WHERE NOT EXISTS (SELECT 1 FROM dbo.GiaSu WHERE SDT = '0961003411');
INSERT INTO dbo.GiaSu (HoTen, SDT, ChuyenMon, TrangThai) SELECT N'Vũ Đức Minh', '0961004548', N'Tin học', N'Rảnh' WHERE NOT EXISTS (SELECT 1 FROM dbo.GiaSu WHERE SDT = '0961004548');
INSERT INTO dbo.GiaSu (HoTen, SDT, ChuyenMon, TrangThai) SELECT N'Lâm Thảo Vy', '0961005685', N'Tiếng Anh', N'Rảnh' WHERE NOT EXISTS (SELECT 1 FROM dbo.GiaSu WHERE SDT = '0961005685');
INSERT INTO dbo.GiaSu (HoTen, SDT, ChuyenMon, TrangThai) SELECT N'Tạ Quang Hiếu', '0961006822', N'Cờ vua', N'Rảnh' WHERE NOT EXISTS (SELECT 1 FROM dbo.GiaSu WHERE SDT = '0961006822');
INSERT INTO dbo.GiaSu (HoTen, SDT, ChuyenMon, TrangThai) SELECT N'Đinh Thu Trang', '0961007959', N'Sinh học', N'Rảnh' WHERE NOT EXISTS (SELECT 1 FROM dbo.GiaSu WHERE SDT = '0961007959');
INSERT INTO dbo.GiaSu (HoTen, SDT, ChuyenMon, TrangThai) SELECT N'Châu Minh Khang', '0961009096', N'Lập trình Scratch', N'Rảnh' WHERE NOT EXISTS (SELECT 1 FROM dbo.GiaSu WHERE SDT = '0961009096');
INSERT INTO dbo.GiaSu (HoTen, SDT, ChuyenMon, TrangThai) SELECT N'Nguyễn Hoài Nam', '0961010233', N'Vật lý', N'Rảnh' WHERE NOT EXISTS (SELECT 1 FROM dbo.GiaSu WHERE SDT = '0961010233');
INSERT INTO dbo.GiaSu (HoTen, SDT, ChuyenMon, TrangThai) SELECT N'Lê Phương Anh', '0961011370', N'Toán', N'Rảnh' WHERE NOT EXISTS (SELECT 1 FROM dbo.GiaSu WHERE SDT = '0961011370');
INSERT INTO dbo.GiaSu (HoTen, SDT, ChuyenMon, TrangThai) SELECT N'Kiều Thanh Sơn', '0961012507', N'Hóa học', N'Rảnh' WHERE NOT EXISTS (SELECT 1 FROM dbo.GiaSu WHERE SDT = '0961012507');
INSERT INTO dbo.GiaSu (HoTen, SDT, ChuyenMon, TrangThai) SELECT N'Phạm Bảo Châu', '0961013644', N'Cờ vua', N'Rảnh' WHERE NOT EXISTS (SELECT 1 FROM dbo.GiaSu WHERE SDT = '0961013644');
INSERT INTO dbo.GiaSu (HoTen, SDT, ChuyenMon, TrangThai) SELECT N'Trần Gia Khiêm', '0961014781', N'Ngữ văn', N'Tạm nghỉ' WHERE NOT EXISTS (SELECT 1 FROM dbo.GiaSu WHERE SDT = '0961014781');
GO

/* ----------------------------- HỌC VIÊN --------------------------- */
-- Một số anh chị em dùng chung số điện thoại phụ huynh
INSERT INTO dbo.HocVien (HoTen, SDT, DiaChi) SELECT N'Trần Anh Hưng', '0352000000', N'Tân Bình, TP.HCM' WHERE NOT EXISTS (SELECT 1 FROM dbo.HocVien WHERE HoTen = N'Trần Anh Hưng' AND SDT = '0352000000');
INSERT INTO dbo.HocVien (HoTen, SDT, DiaChi) SELECT N'Hồ Gia My', '0352002741', N'Thủ Đức, TP.HCM' WHERE NOT EXISTS (SELECT 1 FROM dbo.HocVien WHERE HoTen = N'Hồ Gia My' AND SDT = '0352002741');
INSERT INTO dbo.HocVien (HoTen, SDT, DiaChi) SELECT N'Đỗ Hải Uyên', '0352005482', N'Thủ Đức, TP.HCM' WHERE NOT EXISTS (SELECT 1 FROM dbo.HocVien WHERE HoTen = N'Đỗ Hải Uyên' AND SDT = '0352005482');
INSERT INTO dbo.HocVien (HoTen, SDT, DiaChi) SELECT N'Bùi Nhật Đăng', '0352008223', N'Thủ Đức, TP.HCM' WHERE NOT EXISTS (SELECT 1 FROM dbo.HocVien WHERE HoTen = N'Bùi Nhật Đăng' AND SDT = '0352008223');
INSERT INTO dbo.HocVien (HoTen, SDT, DiaChi) SELECT N'Đặng Bảo An', '0352010964', N'Thủ Đức, TP.HCM' WHERE NOT EXISTS (SELECT 1 FROM dbo.HocVien WHERE HoTen = N'Đặng Bảo An' AND SDT = '0352010964');
INSERT INTO dbo.HocVien (HoTen, SDT, DiaChi) SELECT N'Trần Gia Phát', '0352013705', N'Quận 3, TP.HCM' WHERE NOT EXISTS (SELECT 1 FROM dbo.HocVien WHERE HoTen = N'Trần Gia Phát' AND SDT = '0352013705');
INSERT INTO dbo.HocVien (HoTen, SDT, DiaChi) SELECT N'Đặng Minh Đăng', '0352016446', N'Bình Tân, TP.HCM' WHERE NOT EXISTS (SELECT 1 FROM dbo.HocVien WHERE HoTen = N'Đặng Minh Đăng' AND SDT = '0352016446');
INSERT INTO dbo.HocVien (HoTen, SDT, DiaChi) SELECT N'Huỳnh Bảo Thư', '0352019187', N'Quận 10, TP.HCM' WHERE NOT EXISTS (SELECT 1 FROM dbo.HocVien WHERE HoTen = N'Huỳnh Bảo Thư' AND SDT = '0352019187');
INSERT INTO dbo.HocVien (HoTen, SDT, DiaChi) SELECT N'Huỳnh Anh Thư', '0352021928', N'Tân Phú, TP.HCM' WHERE NOT EXISTS (SELECT 1 FROM dbo.HocVien WHERE HoTen = N'Huỳnh Anh Thư' AND SDT = '0352021928');
INSERT INTO dbo.HocVien (HoTen, SDT, DiaChi) SELECT N'Bùi Phúc Châu', '0352024669', N'Tân Phú, TP.HCM' WHERE NOT EXISTS (SELECT 1 FROM dbo.HocVien WHERE HoTen = N'Bùi Phúc Châu' AND SDT = '0352024669');
INSERT INTO dbo.HocVien (HoTen, SDT, DiaChi) SELECT N'Huỳnh Gia Như', '0352027410', N'Tân Bình, TP.HCM' WHERE NOT EXISTS (SELECT 1 FROM dbo.HocVien WHERE HoTen = N'Huỳnh Gia Như' AND SDT = '0352027410');
INSERT INTO dbo.HocVien (HoTen, SDT, DiaChi) SELECT N'Hoàng Thanh Vy', '0352027410', N'Tân Bình, TP.HCM' WHERE NOT EXISTS (SELECT 1 FROM dbo.HocVien WHERE HoTen = N'Hoàng Thanh Vy' AND SDT = '0352027410');
INSERT INTO dbo.HocVien (HoTen, SDT, DiaChi) SELECT N'Hồ Nhật Long', '0352032892', N'Quận 10, TP.HCM' WHERE NOT EXISTS (SELECT 1 FROM dbo.HocVien WHERE HoTen = N'Hồ Nhật Long' AND SDT = '0352032892');
INSERT INTO dbo.HocVien (HoTen, SDT, DiaChi) SELECT N'Nguyễn Nhật Tâm', '0352035633', N'Bình Thạnh, TP.HCM' WHERE NOT EXISTS (SELECT 1 FROM dbo.HocVien WHERE HoTen = N'Nguyễn Nhật Tâm' AND SDT = '0352035633');
INSERT INTO dbo.HocVien (HoTen, SDT, DiaChi) SELECT N'Đặng Khánh Châu', '0352038374', N'Phú Nhuận, TP.HCM' WHERE NOT EXISTS (SELECT 1 FROM dbo.HocVien WHERE HoTen = N'Đặng Khánh Châu' AND SDT = '0352038374');
INSERT INTO dbo.HocVien (HoTen, SDT, DiaChi) SELECT N'Đỗ Hải Đăng', '0352041115', N'Quận 3, TP.HCM' WHERE NOT EXISTS (SELECT 1 FROM dbo.HocVien WHERE HoTen = N'Đỗ Hải Đăng' AND SDT = '0352041115');
INSERT INTO dbo.HocVien (HoTen, SDT, DiaChi) SELECT N'Võ Hải Trâm', '0352043856', N'Gò Vấp, TP.HCM' WHERE NOT EXISTS (SELECT 1 FROM dbo.HocVien WHERE HoTen = N'Võ Hải Trâm' AND SDT = '0352043856');
INSERT INTO dbo.HocVien (HoTen, SDT, DiaChi) SELECT N'Võ Hải Nam', '0352046597', N'Phú Nhuận, TP.HCM' WHERE NOT EXISTS (SELECT 1 FROM dbo.HocVien WHERE HoTen = N'Võ Hải Nam' AND SDT = '0352046597');
INSERT INTO dbo.HocVien (HoTen, SDT, DiaChi) SELECT N'Đỗ Quỳnh Hưng', '0352049338', N'Tân Bình, TP.HCM' WHERE NOT EXISTS (SELECT 1 FROM dbo.HocVien WHERE HoTen = N'Đỗ Quỳnh Hưng' AND SDT = '0352049338');
INSERT INTO dbo.HocVien (HoTen, SDT, DiaChi) SELECT N'Dương Minh Long', '0352052079', N'Quận 7, TP.HCM' WHERE NOT EXISTS (SELECT 1 FROM dbo.HocVien WHERE HoTen = N'Dương Minh Long' AND SDT = '0352052079');
INSERT INTO dbo.HocVien (HoTen, SDT, DiaChi) SELECT N'Dương Ngọc Bình', '0352054820', N'Tân Phú, TP.HCM' WHERE NOT EXISTS (SELECT 1 FROM dbo.HocVien WHERE HoTen = N'Dương Ngọc Bình' AND SDT = '0352054820');
INSERT INTO dbo.HocVien (HoTen, SDT, DiaChi) SELECT N'Bùi Phúc Duy', '0352054820', N'Tân Phú, TP.HCM' WHERE NOT EXISTS (SELECT 1 FROM dbo.HocVien WHERE HoTen = N'Bùi Phúc Duy' AND SDT = '0352054820');
INSERT INTO dbo.HocVien (HoTen, SDT, DiaChi) SELECT N'Đỗ Khánh Đăng', '0352060302', N'Quận 5, TP.HCM' WHERE NOT EXISTS (SELECT 1 FROM dbo.HocVien WHERE HoTen = N'Đỗ Khánh Đăng' AND SDT = '0352060302');
INSERT INTO dbo.HocVien (HoTen, SDT, DiaChi) SELECT N'Đặng Bảo Duy', '0352063043', N'Bình Tân, TP.HCM' WHERE NOT EXISTS (SELECT 1 FROM dbo.HocVien WHERE HoTen = N'Đặng Bảo Duy' AND SDT = '0352063043');
INSERT INTO dbo.HocVien (HoTen, SDT, DiaChi) SELECT N'Ngô Quỳnh Đăng', '0352065784', N'Tân Phú, TP.HCM' WHERE NOT EXISTS (SELECT 1 FROM dbo.HocVien WHERE HoTen = N'Ngô Quỳnh Đăng' AND SDT = '0352065784');
INSERT INTO dbo.HocVien (HoTen, SDT, DiaChi) SELECT N'Bùi Thanh Như', '0352068525', N'Phú Nhuận, TP.HCM' WHERE NOT EXISTS (SELECT 1 FROM dbo.HocVien WHERE HoTen = N'Bùi Thanh Như' AND SDT = '0352068525');
INSERT INTO dbo.HocVien (HoTen, SDT, DiaChi) SELECT N'Hồ Thanh Như', '0352071266', N'Bình Tân, TP.HCM' WHERE NOT EXISTS (SELECT 1 FROM dbo.HocVien WHERE HoTen = N'Hồ Thanh Như' AND SDT = '0352071266');
INSERT INTO dbo.HocVien (HoTen, SDT, DiaChi) SELECT N'Dương Khánh Uyên', '0352074007', N'Quận 7, TP.HCM' WHERE NOT EXISTS (SELECT 1 FROM dbo.HocVien WHERE HoTen = N'Dương Khánh Uyên' AND SDT = '0352074007');
INSERT INTO dbo.HocVien (HoTen, SDT, DiaChi) SELECT N'Hoàng Thanh An', '0352076748', N'Quận 1, TP.HCM' WHERE NOT EXISTS (SELECT 1 FROM dbo.HocVien WHERE HoTen = N'Hoàng Thanh An' AND SDT = '0352076748');
INSERT INTO dbo.HocVien (HoTen, SDT, DiaChi) SELECT N'Lê Phúc Trâm', '0352079489', N'Gò Vấp, TP.HCM' WHERE NOT EXISTS (SELECT 1 FROM dbo.HocVien WHERE HoTen = N'Lê Phúc Trâm' AND SDT = '0352079489');
GO

/* ----------------------------- LỚP HỌC ---------------------------- */
DECLARE @homNay DATETIME = CAST(CAST(GETDATE() AS DATE) AS DATETIME);
DECLARE @lop TABLE (SdtGS VARCHAR(20), SdtHV VARCHAR(20), MonHoc NVARCHAR(50), HocPhi DECIMAL(18,2),
                    CaHoc NVARCHAR(50), BatDau DATETIME, KetThuc DATETIME, DaHuy BIT);

INSERT INTO @lop VALUES
('0934567890', '0983333333', N'Vật lý', 1600000, N'Tối (18h-20h)', DATEADD(HOUR, 18, DATEADD(DAY, -84, @homNay)), DATEADD(HOUR, 20, DATEADD(DAY, -24, @homNay)), 0),
('0961006822', '0352008223', N'Cờ vua', 1250000, N'Sáng (7h-9h)', DATEADD(HOUR, 7, DATEADD(DAY, -88, @homNay)), DATEADD(HOUR, 9, DATEADD(DAY, -28, @homNay)), 0),
('0961014781', '0352074007', N'Ngữ văn', 1400000, N'Tối muộn (20h-22h)', DATEADD(HOUR, 20, DATEADD(DAY, -104, @homNay)), DATEADD(HOUR, 22, DATEADD(DAY, -59, @homNay)), 0),
('0961013644', '0352021928', N'Cờ vua', 1100000, N'Chiều muộn (16h-18h)', DATEADD(HOUR, 16, DATEADD(DAY, -125, @homNay)), DATEADD(HOUR, 18, DATEADD(DAY, -80, @homNay)), 0),
('0961012507', '0352035633', N'Hóa học', 1450000, N'Sáng (7h-9h)', DATEADD(HOUR, 7, DATEADD(DAY, -117, @homNay)), DATEADD(HOUR, 9, DATEADD(DAY, -27, @homNay)), 0),
('0961000000', '0352005482', N'Cờ vua', 1000000, N'Tối (18h-20h)', DATEADD(HOUR, 18, DATEADD(DAY, -62, @homNay)), DATEADD(HOUR, 20, DATEADD(DAY, -3, @homNay)), 0),
('0961012507', '0352060302', N'Hóa học', 1700000, N'Chiều muộn (16h-18h)', DATEADD(HOUR, 16, DATEADD(DAY, -88, @homNay)), DATEADD(HOUR, 18, DATEADD(DAY, -43, @homNay)), 0),
('0934567890', '0352052079', N'Vật lý', 1650000, N'Sáng (7h-9h)', DATEADD(HOUR, 7, DATEADD(DAY, -91, @homNay)), DATEADD(HOUR, 9, DATEADD(DAY, -61, @homNay)), 0),
('0961012507', '0352071266', N'Hóa học', 1250000, N'Chiều (14h-16h)', DATEADD(HOUR, 14, DATEADD(DAY, -85, @homNay)), DATEADD(HOUR, 16, DATEADD(DAY, -25, @homNay)), 0),
('0961010233', '0352041115', N'Vật lý', 1300000, N'Sáng (7h-9h)', DATEADD(HOUR, 7, DATEADD(DAY, -87, @homNay)), DATEADD(HOUR, 9, DATEADD(DAY, -3, @homNay)), 0),
('0961010233', '0352010964', N'Vật lý', 1600000, N'Sáng muộn (9h-11h)', DATEADD(HOUR, 9, DATEADD(DAY, -95, @homNay)), DATEADD(HOUR, 11, DATEADD(DAY, -50, @homNay)), 0),
('0961009096', '0352043856', N'Lập trình Scratch', 1500000, N'Chiều muộn (16h-18h)', DATEADD(HOUR, 16, DATEADD(DAY, -79, @homNay)), DATEADD(HOUR, 18, DATEADD(DAY, -34, @homNay)), 0),
('0923456789', '0352054820', N'Cờ vua', 1400000, N'Sáng (7h-9h)', DATEADD(HOUR, 7, DATEADD(DAY, -38, @homNay)), DATEADD(HOUR, 9, DATEADD(DAY, 7, @homNay)), 0),
('0961000000', '0983333333', N'Cờ vua', 1300000, N'Tối muộn (20h-22h)', DATEADD(HOUR, 20, DATEADD(DAY, -22, @homNay)), DATEADD(HOUR, 22, DATEADD(DAY, 8, @homNay)), 0),
('0961010233', '0352016446', N'Vật lý', 1400000, N'Chiều muộn (16h-18h)', DATEADD(HOUR, 16, DATEADD(DAY, -20, @homNay)), DATEADD(HOUR, 18, DATEADD(DAY, 10, @homNay)), 0),
('0945678901', '0352079489', N'Tin học', 2100000, N'Sáng muộn (9h-11h)', DATEADD(HOUR, 9, DATEADD(DAY, -11, @homNay)), DATEADD(HOUR, 11, DATEADD(DAY, 34, @homNay)), 0),
('0961000000', '0352013705', N'Cờ vua', 1400000, N'Chiều muộn (16h-18h)', DATEADD(HOUR, 16, DATEADD(DAY, -34, @homNay)), DATEADD(HOUR, 18, DATEADD(DAY, 26, @homNay)), 0),
('0961009096', '0352060302', N'Lập trình Scratch', 1900000, N'Sáng (7h-9h)', DATEADD(HOUR, 7, DATEADD(DAY, -12, @homNay)), DATEADD(HOUR, 9, DATEADD(DAY, 78, @homNay)), 0),
('0961004548', '0352013705', N'Tin học', 2100000, N'Chiều (14h-16h)', DATEADD(HOUR, 14, DATEADD(DAY, -8, @homNay)), DATEADD(HOUR, 16, DATEADD(DAY, 37, @homNay)), 0),
('0961003411', '0352068525', N'Ngữ văn', 1100000, N'Chiều muộn (16h-18h)', DATEADD(HOUR, 16, DATEADD(DAY, -24, @homNay)), DATEADD(HOUR, 18, DATEADD(DAY, 66, @homNay)), 0),
('0961004548', '0352068525', N'Tin học', 1600000, N'Sáng muộn (9h-11h)', DATEADD(HOUR, 9, DATEADD(DAY, -12, @homNay)), DATEADD(HOUR, 11, DATEADD(DAY, 48, @homNay)), 0),
('0961012507', '0352071266', N'Hóa học', 1700000, N'Chiều muộn (16h-18h)', DATEADD(HOUR, 16, DATEADD(DAY, -29, @homNay)), DATEADD(HOUR, 18, DATEADD(DAY, 1, @homNay)), 0),
('0961012507', '0352079489', N'Hóa học', 1350000, N'Chiều (14h-16h)', DATEADD(HOUR, 14, DATEADD(DAY, -13, @homNay)), DATEADD(HOUR, 16, DATEADD(DAY, 17, @homNay)), 0),
('0961001137', '0352021928', N'Tiếng Anh', 2250000, N'Sáng (7h-9h)', DATEADD(HOUR, 7, DATEADD(DAY, -10, @homNay)), DATEADD(HOUR, 9, DATEADD(DAY, 20, @homNay)), 0),
('0901234567', '0352046597', N'Toán', 1300000, N'Sáng muộn (9h-11h)', DATEADD(HOUR, 9, DATEADD(DAY, -36, @homNay)), DATEADD(HOUR, 11, DATEADD(DAY, 24, @homNay)), 0),
('0961013644', '0352019187', N'Cờ vua', 1050000, N'Sáng (7h-9h)', DATEADD(HOUR, 7, DATEADD(DAY, -23, @homNay)), DATEADD(HOUR, 9, DATEADD(DAY, 37, @homNay)), 0),
('0961013644', '0352019187', N'Cờ vua', 1400000, N'Tối (18h-20h)', DATEADD(HOUR, 18, DATEADD(DAY, -9, @homNay)), DATEADD(HOUR, 20, DATEADD(DAY, 51, @homNay)), 0),
('0961003411', '0352005482', N'Ngữ văn', 1500000, N'Sáng (7h-9h)', DATEADD(HOUR, 7, DATEADD(DAY, -17, @homNay)), DATEADD(HOUR, 9, DATEADD(DAY, 43, @homNay)), 0),
('0912345678', '0352046597', N'Tiếng Anh', 1700000, N'Tối muộn (20h-22h)', DATEADD(HOUR, 20, DATEADD(DAY, -34, @homNay)), DATEADD(HOUR, 22, DATEADD(DAY, 11, @homNay)), 0),
('0934567890', '0352076748', N'Vật lý', 1750000, N'Chiều (14h-16h)', DATEADD(HOUR, 14, DATEADD(DAY, -28, @homNay)), DATEADD(HOUR, 16, DATEADD(DAY, 32, @homNay)), 0),
('0923456789', '0352035633', N'Cờ vua', 1200000, N'Chiều (14h-16h)', DATEADD(HOUR, 14, DATEADD(DAY, -14, @homNay)), DATEADD(HOUR, 16, DATEADD(DAY, 31, @homNay)), 0),
('0961009096', '0352027410', N'Lập trình Scratch', 2050000, N'Chiều muộn (16h-18h)', DATEADD(HOUR, 16, DATEADD(DAY, -31, @homNay)), DATEADD(HOUR, 18, DATEADD(DAY, -1, @homNay)), 0),
('0912345678', '0352000000', N'Tiếng Anh', 2050000, N'Sáng muộn (9h-11h)', DATEADD(HOUR, 9, DATEADD(DAY, 16, @homNay)), DATEADD(HOUR, 11, DATEADD(DAY, 46, @homNay)), 0),
('0961004548', '0352002741', N'Tin học', 1950000, N'Sáng (7h-9h)', DATEADD(HOUR, 7, DATEADD(DAY, 2, @homNay)), DATEADD(HOUR, 9, DATEADD(DAY, 62, @homNay)), 0),
('0961005685', '0352068525', N'Tiếng Anh', 2350000, N'Tối (18h-20h)', DATEADD(HOUR, 18, DATEADD(DAY, 4, @homNay)), DATEADD(HOUR, 20, DATEADD(DAY, 34, @homNay)), 0),
('0961007959', '0352041115', N'Sinh học', 1300000, N'Tối (18h-20h)', DATEADD(HOUR, 18, DATEADD(DAY, 2, @homNay)), DATEADD(HOUR, 20, DATEADD(DAY, 47, @homNay)), 0),
('0961010233', '0352063043', N'Vật lý', 1450000, N'Tối muộn (20h-22h)', DATEADD(HOUR, 20, DATEADD(DAY, 16, @homNay)), DATEADD(HOUR, 22, DATEADD(DAY, 46, @homNay)), 0),
('0961006822', '0352076748', N'Cờ vua', 1050000, N'Tối muộn (20h-22h)', DATEADD(HOUR, 20, DATEADD(DAY, 20, @homNay)), DATEADD(HOUR, 22, DATEADD(DAY, 80, @homNay)), 0),
('0961009096', '0982222222', N'Lập trình Scratch', 1750000, N'Chiều (14h-16h)', DATEADD(HOUR, 14, DATEADD(DAY, 19, @homNay)), DATEADD(HOUR, 16, DATEADD(DAY, 79, @homNay)), 0),
('0961001137', '0984444444', N'Tiếng Anh', 2050000, N'Tối muộn (20h-22h)', DATEADD(HOUR, 20, DATEADD(DAY, 4, @homNay)), DATEADD(HOUR, 22, DATEADD(DAY, 34, @homNay)), 0),
('0945678901', '0352079489', N'Tin học', 2300000, N'Chiều (14h-16h)', DATEADD(HOUR, 14, DATEADD(DAY, 21, @homNay)), DATEADD(HOUR, 16, DATEADD(DAY, 81, @homNay)), 0),
('0961006822', '0352060302', N'Cờ vua', 1400000, N'Chiều muộn (16h-18h)', CASE WHEN DATEADD(HOUR, 16, @homNay) > GETDATE() THEN DATEADD(HOUR, 16, @homNay) ELSE DATEADD(HOUR, 16, DATEADD(DAY, 1, @homNay)) END, DATEADD(HOUR, 18, DATEADD(DAY, 91, @homNay)), 0),
('0961002274', '0352060302', N'Toán', 1650000, N'Tối muộn (20h-22h)', CASE WHEN DATEADD(HOUR, 20, @homNay) > GETDATE() THEN DATEADD(HOUR, 20, @homNay) ELSE DATEADD(HOUR, 20, DATEADD(DAY, 1, @homNay)) END, DATEADD(HOUR, 22, DATEADD(DAY, 46, @homNay)), 0),
('0945678901', '0352013705', N'Tin học', 2350000, N'Tối (18h-20h)', DATEADD(HOUR, 18, DATEADD(DAY, 5, @homNay)), DATEADD(HOUR, 20, DATEADD(DAY, 35, @homNay)), 1),
('0961013644', '0352054820', N'Cờ vua', 1500000, N'Chiều (14h-16h)', DATEADD(HOUR, 14, DATEADD(DAY, -14, @homNay)), DATEADD(HOUR, 16, DATEADD(DAY, 31, @homNay)), 1),
('0961002274', '0982222222', N'Toán', 1800000, N'Sáng (7h-9h)', DATEADD(HOUR, 7, DATEADD(DAY, -13, @homNay)), DATEADD(HOUR, 9, DATEADD(DAY, 47, @homNay)), 1);

-- Chỉ thêm lớp khi: chưa có lớp giống hệt, và gia sư / học viên không bận cùng ca trong khoảng ngày đó
-- (Lấy MaHV nhỏ nhất theo SĐT: anh chị em dùng chung SĐT thì lớp gán cho người đầu tiên)
INSERT INTO dbo.LopHoc (MaGS, MaHV, MonHoc, HocPhi, TrangThai, ThoiGianBatDau, ThoiGianKetThuc, CaHoc)
SELECT g.MaGS, h.MaHV, x.MonHoc, x.HocPhi,
       CASE WHEN x.DaHuy = 1 THEN N'Đã hủy' ELSE N'Chưa bắt đầu' END,
       x.BatDau, x.KetThuc, x.CaHoc
FROM @lop x
CROSS APPLY (SELECT TOP 1 MaGS FROM dbo.GiaSu WHERE SDT = x.SdtGS ORDER BY MaGS) g
CROSS APPLY (SELECT TOP 1 MaHV FROM dbo.HocVien WHERE SDT = x.SdtHV ORDER BY MaHV) h
WHERE NOT EXISTS (SELECT 1 FROM dbo.LopHoc l
                  WHERE l.MaGS = g.MaGS AND l.MaHV = h.MaHV AND l.MonHoc = x.MonHoc AND l.CaHoc = x.CaHoc)
  AND (x.DaHuy = 1 OR NOT EXISTS (SELECT 1 FROM dbo.LopHoc l
                  WHERE l.TrangThai <> N'Đã hủy' AND l.CaHoc = x.CaHoc
                    AND (l.MaGS = g.MaGS OR l.MaHV = h.MaHV)
                    AND l.ThoiGianBatDau <= x.KetThuc AND l.ThoiGianKetThuc >= x.BatDau));

PRINT N'Đã thêm ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + N' lớp học.';
GO

/* ------------------- ĐỒNG BỘ TRẠNG THÁI THEO THỜI GIAN ------------------- */
UPDATE dbo.LopHoc
SET TrangThai = CASE
        WHEN GETDATE() < ThoiGianBatDau THEN N'Chưa bắt đầu'
        WHEN GETDATE() <= ThoiGianKetThuc THEN N'Đang diễn ra'
        ELSE N'Đã kết thúc'
    END
WHERE TrangThai <> N'Đã hủy' AND ThoiGianBatDau IS NOT NULL AND ThoiGianKetThuc IS NOT NULL;

UPDATE g SET TrangThai =
    CASE
        WHEN EXISTS (SELECT 1 FROM dbo.LopHoc l WHERE l.MaGS = g.MaGS AND l.TrangThai = N'Đang diễn ra') THEN N'Đang dạy'
        WHEN EXISTS (SELECT 1 FROM dbo.LopHoc l WHERE l.MaGS = g.MaGS AND l.TrangThai = N'Chưa bắt đầu') THEN N'Bận'
        ELSE N'Rảnh'
    END
FROM dbo.GiaSu g
WHERE g.TrangThai IS NULL OR g.TrangThai <> N'Tạm nghỉ';
GO

SELECT
    (SELECT COUNT(*) FROM dbo.GiaSu)  AS TongGiaSu,
    (SELECT COUNT(*) FROM dbo.HocVien) AS TongHocVien,
    (SELECT COUNT(*) FROM dbo.LopHoc)  AS TongLopHoc;
GO
