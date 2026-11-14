/* ===================================================================
   QuanLyGiaSu - DỮ LIỆU MẪU RIÊNG (seed_data.sql)
   Chạy SAU 01..06 (cấu trúc v6). Có thể chạy trước hoặc sau các script
   11..16 mà không đụng dữ liệu mẫu có sẵn.

   Nội dung: 10 gia sư, 18 học viên, 10 lớp học với tên / địa chỉ Việt Nam.
   - Gia sư dùng dải số 0977 100 001 .. 0977 100 010.
   - Học viên dùng dải số 0368 100 001 .. 0368 100 018.
   - Mọi thao tác nằm trong một transaction; chạy lại nhiều lần được
     (bản ghi đã tồn tại thì bỏ qua, không xoá dữ liệu hiện có).
   =================================================================== */
USE QuanLyGiaSu;
GO

SET QUOTED_IDENTIFIER ON;   -- cần cho bảng có chỉ mục lọc (04, 05, 06)
SET ANSI_NULLS ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @homNay DATETIME = CAST(CAST(GETDATE() AS DATE) AS DATETIME);
    DECLARE @soMon INT = 0, @soGS INT = 0, @soHV INT = 0, @soTK INT = 0, @soLop INT = 0;

    /* -----------------------------------------------------------------
       1. DANH MỤC MÔN HỌC
       Bảo đảm các môn dùng cho lớp mẫu có trong dbo.MonHoc. Nếu 05 đã
       thêm sẵn thì bỏ qua (không sửa môn đang có).
       ----------------------------------------------------------------- */
    DECLARE @mon TABLE
    (
        TenMon  NVARCHAR(50)  PRIMARY KEY,
        LoaiMon NVARCHAR(20)  NOT NULL,
        CapDo   NVARCHAR(500) NULL
    );

    INSERT INTO @mon (TenMon, LoaiMon, CapDo) VALUES
        (N'Toán',               N'Chính',      NULL),
        (N'Ngữ văn',            N'Chính',      NULL),
        (N'Tiếng Anh',          N'Chính',      NULL),
        (N'Vật lý',             N'Chính',      NULL),
        (N'Hóa học',            N'Chính',      NULL),
        (N'Sinh học',           N'Chính',      NULL),
        (N'Tin học',            N'Chính',      NULL),
        (N'Cờ vua',             N'Năng khiếu', N'Mầm non;Vỡ lòng;Căn bản;Sơ cấp;Trung cấp;Nâng cao'),
        (N'Lập trình Scratch',  N'Năng khiếu', N'Làm quen;Cơ bản;Nâng cao;Dự án'),
        (N'Vẽ',                 N'Năng khiếu', N'Cấp 1;Cấp 2;Cấp 3;Cấp 4;Cấp 5');

    INSERT INTO dbo.MonHoc (TenMon, LoaiMon, CapDo)
    SELECT m.TenMon, m.LoaiMon, m.CapDo
    FROM @mon m
    WHERE NOT EXISTS (SELECT 1 FROM dbo.MonHoc x WHERE x.TenMon = m.TenMon);
    SET @soMon = @@ROWCOUNT;

    /* -----------------------------------------------------------------
       2. GIA SƯ
       Hồ sơ đầy đủ: chuyên môn, email, học vấn, kinh nghiệm, ca nhận
       dạy (tên ngắn cách nhau dấu phẩy, đúng như Constants.CaHoc).
       ----------------------------------------------------------------- */
    DECLARE @gs TABLE
    (
        SDT        VARCHAR(20)   PRIMARY KEY,
        HoTen      NVARCHAR(100) NOT NULL,
        ChuyenMon  NVARCHAR(100) NOT NULL,
        Email      NVARCHAR(100) NULL,
        HocVan     NVARCHAR(150) NULL,
        KinhNghiem INT           NULL,
        CaNhanDay  NVARCHAR(200) NULL,
        GhiChu     NVARCHAR(500) NULL
    );

    INSERT INTO @gs (SDT, HoTen, ChuyenMon, Email, HocVan, KinhNghiem, CaNhanDay, GhiChu) VALUES
        ('0977100001', N'Nguyễn Thị Kim Ngân', N'Toán',
         'kimngan.toan@trungtam.edu.vn', N'Cử nhân Sư phạm Toán - ĐH Sư phạm TP.HCM', 6,
         N'Chiều,Chiều muộn,Tối,Tối muộn', N'Ưu tiên học viên cấp 2, có giáo án riêng'),
        ('0977100002', N'Trần Quốc Đạt', N'Tiếng Anh',
         'quocdat.anh@trungtam.edu.vn', N'Thạc sĩ Ngôn ngữ Anh - ĐH KHXH&NV', 8,
         N'Sáng,Sáng muộn,Chiều,Chiều muộn', N'Luyện thi chứng chỉ Cambridge cho thiếu nhi'),
        ('0977100003', N'Lê Hoàng Phúc', N'Cờ vua',
         'hoangphuc.co@trungtam.edu.vn', N'Kiện tướng FIDE, HLV cờ vua trẻ', 10,
         N'Chiều,Chiều muộn,Tối', N'Từng dẫn đội tuyển cờ vua cấp quận'),
        ('0977100004', N'Phạm Ngọc Mai', N'Ngữ văn',
         'ngocmai.van@trungtam.edu.vn', N'Cử nhân Sư phạm Ngữ văn - ĐH Sài Gòn', 4,
         N'Sáng,Sáng muộn,Chiều,Chiều muộn', N'Chuyên luyện kỹ năng viết đoạn văn'),
        ('0977100005', N'Võ Thành Luân', N'Vật lý',
         'thanhluan.ly@trungtam.edu.vn', N'Thạc sĩ Vật lý - ĐH Khoa học Tự nhiên', 7,
         N'Chiều muộn,Tối,Tối muộn', N'Dạy theo phương pháp thực nghiệm'),
        ('0977100006', N'Đặng Thị Hồng Nhung', N'Hóa học',
         'hongnhung.hoa@trungtam.edu.vn', N'Cử nhân Hóa học - ĐH Sư phạm TP.HCM', 9,
         N'Sáng,Sáng muộn,Chiều', N'Có kinh nghiệm ôn thi tuyển sinh lớp 10'),
        ('0977100007', N'Bùi Anh Tuấn', N'Tin học',
         'anhtuan.tin@trungtam.edu.vn', N'Kỹ sư Công nghệ Thông tin - ĐH Bách Khoa', 5,
         N'Chiều,Chiều muộn,Tối,Tối muộn', N'Hướng dẫn tin học văn phòng và lập trình cơ bản'),
        ('0977100008', N'Huỳnh Thanh Trúc', N'Sinh học',
         'thanhtruc.sinh@trungtam.edu.vn', N'Cử nhân Sư phạm Sinh học - ĐH Sài Gòn', 3,
         N'Tối,Tối muộn', N'Thích dạy bằng sơ đồ tư duy'),
        ('0977100009', N'Đỗ Minh Quân', N'Lập trình Scratch',
         'minhquan.scratch@trungtam.edu.vn', N'Sinh viên năm 4 CNTT - ĐH Công nghệ Thông tin', 2,
         N'Sáng,Sáng muộn,Chiều', N'Dạy Scratch qua các trò chơi nhỏ'),
        ('0977100010', N'Ngô Thị Bích Trâm', N'Vẽ',
         'bichtram.ve@trungtam.edu.vn', N'Giáo viên Mỹ thuật - Trường CĐ Văn hóa Nghệ thuật', 12,
         N'Chiều,Chiều muộn', N'Luyện vẽ màu nước và ký họa cho thiếu nhi');

    INSERT INTO dbo.GiaSu (HoTen, SDT, ChuyenMon, TrangThai, Email, HocVan, KinhNghiem, CaNhanDay, GhiChu)
    SELECT x.HoTen, x.SDT, x.ChuyenMon, N'Rảnh', x.Email, x.HocVan, x.KinhNghiem, x.CaNhanDay, x.GhiChu
    FROM @gs x
    WHERE NOT EXISTS (SELECT 1 FROM dbo.GiaSu g WHERE g.SDT = x.SDT);
    SET @soGS = @@ROWCOUNT;

    /* -----------------------------------------------------------------
       3. HỌC VIÊN
       Hồ sơ đầy đủ: phụ huynh, trường/lớp, trình độ, ghi chú.
       ----------------------------------------------------------------- */
    DECLARE @hv TABLE
    (
        SDT          VARCHAR(20)   PRIMARY KEY,
        HoTen        NVARCHAR(100) NOT NULL,
        DiaChi       NVARCHAR(200) NULL,
        TenPhuHuynh  NVARCHAR(100) NULL,
        SDTPhuHuynh  VARCHAR(20)   NULL,
        TruongLop    NVARCHAR(150) NULL,
        TrinhDo      NVARCHAR(50)  NULL,
        GhiChu       NVARCHAR(500) NULL
    );

    INSERT INTO @hv (SDT, HoTen, DiaChi, TenPhuHuynh, SDTPhuHuynh, TruongLop, TrinhDo, GhiChu) VALUES
        ('0368100001', N'Nguyễn Hoàng Bảo An', N'12 Nguyễn Thị Minh Khai, Quận 3, TP.HCM',
         N'Chị Nguyễn Thị Hạnh', '0909100001', N'TH Nguyễn Bỉnh Khiêm, lớp 3', N'Căn bản',
         N'Phụ huynh muốn nhận báo cáo mỗi tháng'),
        ('0368100002', N'Trần Gia Hân', N'45/7 Điện Biên Phủ, Bình Thạnh, TP.HCM',
         N'Anh Trần Văn Sơn', '0909100002', N'THCS Trần Văn Ơn, lớp 6', N'Sơ cấp',
         N'Cần luyện thêm kỹ năng nghe'),
        ('0368100003', N'Lê Minh Khang', N'88 Võ Văn Ngân, Thủ Đức, TP.HCM',
         N'Chị Lê Thị Thu', '0909100003', N'TH Nguyễn Hiền, lớp 4', N'Căn bản',
         N'Thích chơi cờ, tiếp thu nhanh'),
        ('0368100004', N'Phạm Ngọc Diệp', N'201 Nguyễn Văn Linh, Quận 7, TP.HCM',
         N'Anh Phạm Minh Tâm', '0909100004', N'THCS Nguyễn Hữu Thọ, lớp 8', N'Trung cấp',
         N'Viết văn còn lan man, cần luyện dàn ý'),
        ('0368100005', N'Huỳnh Đức Duy', N'9 Hoàng Văn Thụ, Tân Bình, TP.HCM',
         N'Chị Huỳnh Thị Lan', '0909100005', N'THPT Nguyễn Thượng Hiền, lớp 10', N'Căn bản',
         N'Mục tiêu ôn thi học kỳ'),
        ('0368100006', N'Võ Thị Thu Hà', N'76 Phan Văn Trị, Gò Vấp, TP.HCM',
         N'Anh Võ Quốc Bình', '0909100006', N'THCS Phạm Văn Chiêu, lớp 9', N'Trung cấp',
         N'Cần ôn thi tuyển sinh lớp 10'),
        ('0368100007', N'Đặng Nhật Nam', N'15 Hồ Văn Huê, Phú Nhuận, TP.HCM',
         N'Chị Đặng Thị Mai', '0909100007', N'THCS Độc Lập, lớp 7', N'Căn bản',
         N'Thích máy tính, cần thêm tin học văn phòng'),
        ('0368100008', N'Bùi Thanh Thảo', N'33 Thành Thái, Quận 10, TP.HCM',
         N'Anh Bùi Văn Hùng', '0909100008', N'THPT Nguyễn Du, lớp 11', N'Nâng cao',
         N'Học tốt, muốn ôn chuyên sâu'),
        ('0368100009', N'Ngô Quốc Huy', N'128 Tân Sơn Nhì, Tân Phú, TP.HCM',
         N'Chị Ngô Thị Hồng', '0909100009', N'TH Nguyễn Huệ, lớp 5', N'Cơ bản',
         N'Mê trò chơi, học Scratch rất hứng thú'),
        ('0368100010', N'Đỗ Khánh Linh', N'5 Lê Thánh Tôn, Quận 1, TP.HCM',
         N'Anh Đỗ Minh Tuấn', '0909100010', N'TH Lương Thế Vinh, lớp 4', N'Cấp 2',
         N'Có năng khiếu vẽ, phụ huynh muốn phát triển thêm'),
        ('0368100011', N'Hoàng Minh Triết', N'210 Tên Lửa, Bình Tân, TP.HCM',
         N'Chị Hoàng Thị Cúc', '0909100011', N'THCS Bình Trị Đông, lớp 6', N'Căn bản',
         N'Hay quên bài, cần nhắc ôn tập trước buổi học'),
        ('0368100012', N'Lý Minh Bảo', N'67 Nguyễn Trãi, Quận 5, TP.HCM',
         N'Anh Lý Quốc Khánh', '0909100012', N'THCS Kim Đồng, lớp 8', N'Sơ cấp',
         N'Cần cải thiện phát âm tiếng Anh'),
        ('0368100013', N'Mai Phương Nhi', N'18 Tôn Đản, Quận 4, TP.HCM',
         N'Chị Mai Thị Ngọc', '0909100013', N'TH Nguyễn Văn Trỗi, lớp 2', N'Vỡ lòng',
         N'Mới bắt đầu làm quen chữ và số'),
        ('0368100014', N'Trịnh Đức Anh', N'B12 KDC Vĩnh Lộc, Bình Chánh, TP.HCM',
         N'Anh Trịnh Văn Long', '0909100014', N'THPT Bình Chánh, lớp 12', N'Nâng cao',
         N'Mục tiêu thi tốt nghiệp THPT'),
        ('0368100015', N'Phan Thị Mỹ Duyên', N'25 Nguyễn Hữu Thọ, Nhà Bè, TP.HCM',
         N'Chị Phan Thị Huệ', '0909100015', N'THCS Nguyễn Văn Quỳ, lớp 7', N'Căn bản',
         N'Cần xây dựng lại nền tảng môn Toán'),
        ('0368100016', N'Tạ Hoàng Long', N'144 Lê Văn Khương, Quận 12, TP.HCM',
         N'Anh Tạ Văn Dũng', '0909100016', N'THCS Nguyễn An Ninh, lớp 8', N'Trung cấp',
         N'Thích học theo nhóm nhỏ'),
        ('0368100017', N'Nguyễn Bảo Trân', N'Thị trấn Hóc Môn, Hóc Môn, TP.HCM',
         N'Chị Nguyễn Thị Yến', '0909100017', N'TH Nguyễn Thị Nuôi, lớp 3', N'Căn bản',
         N'Nhút nhát, cần gia sư kiên nhẫn'),
        ('0368100018', N'Trương Tấn Phát', N'56 Phạm Thế Hiển, Quận 8, TP.HCM',
         N'Anh Trương Văn Lợi', '0909100018', N'THPT Nguyễn Văn Linh, lớp 11', N'Trung cấp',
         N'Cần ôn tập đều đặn mỗi tuần');

    INSERT INTO dbo.HocVien (HoTen, SDT, DiaChi, TenPhuHuynh, SDTPhuHuynh, TruongLop, TrinhDo, GhiChu)
    SELECT x.HoTen, x.SDT, x.DiaChi, x.TenPhuHuynh, x.SDTPhuHuynh, x.TruongLop, x.TrinhDo, x.GhiChu
    FROM @hv x
    WHERE NOT EXISTS (SELECT 1 FROM dbo.HocVien h WHERE h.SDT = x.SDT);
    SET @soHV = @@ROWCOUNT;

    /* -----------------------------------------------------------------
       4. TÀI KHOẢN GIA SƯ
       Cùng quy ước với 13_DuLieuMau_v3.sql: gs<Mã gia sư> / 123456.
       Hash PBKDF2-SHA256 bên dưới là của mật khẩu mẫu "123456".
       ----------------------------------------------------------------- */
    INSERT INTO dbo.TaiKhoan (TenDangNhap, MatKhauHash, VaiTro, MaGS, HoatDong)
    SELECT 'gs' + CAST(g.MaGS AS VARCHAR(10)),
           '100000.Whw+m30vSKawxOgfbZOidQ==.MJpoB1nj2Y4yM+uQ8U1X2agJ05GgD74aLPOR5PXBIGs=',
           N'GiaSu', g.MaGS,
           CASE WHEN g.TrangThai = N'Tạm nghỉ' THEN 0 ELSE 1 END
    FROM dbo.GiaSu g
    JOIN @gs x ON x.SDT = g.SDT
    WHERE NOT EXISTS (SELECT 1 FROM dbo.TaiKhoan t WHERE t.MaGS = g.MaGS)
      AND NOT EXISTS (SELECT 1 FROM dbo.TaiKhoan t
                      WHERE t.TenDangNhap = 'gs' + CAST(g.MaGS AS VARCHAR(10)));
    SET @soTK = @@ROWCOUNT;

    /* -----------------------------------------------------------------
       5. LỚP HỌC
       Mỗi lớp gắn một gia sư + một học viên, một ca cố định và các thứ
       học trong tuần. Ngày tính quanh ngày chạy script để có đủ trạng
       thái: đã kết thúc, đang diễn ra, chưa bắt đầu.
       Tên lớp theo đúng quy ước ứng dụng: "Môn - Học viên - Ca ngắn".
       ----------------------------------------------------------------- */
    DECLARE @lop TABLE
    (
        SdtGS        VARCHAR(20)   NOT NULL,
        SdtHV        VARCHAR(20)   NOT NULL,
        MonHoc       NVARCHAR(50)  NOT NULL,
        HocPhi       DECIMAL(18,2) NOT NULL,
        CaHoc        NVARCHAR(50)  NOT NULL,
        ThuHoc       VARCHAR(20)   NULL,
        BatDau       DATETIME      NOT NULL,
        KetThuc      DATETIME      NOT NULL,
        TenLop       NVARCHAR(150) NOT NULL,
        CapDoDauVao  NVARCHAR(50)  NULL
    );

    INSERT INTO @lop (SdtGS, SdtHV, MonHoc, HocPhi, CaHoc, ThuHoc, BatDau, KetThuc, TenLop, CapDoDauVao) VALUES
        -- Lớp đang diễn ra
        ('0977100001', '0368100001', N'Toán', 1500000, N'Tối (18h-20h)', '2,4,6',
         DATEADD(HOUR, 18, DATEADD(DAY, -35, @homNay)), DATEADD(HOUR, 20, DATEADD(DAY, 25, @homNay)),
         N'Toán - Nguyễn Hoàng Bảo An - Tối', NULL),
        ('0977100002', '0368100002', N'Tiếng Anh', 1800000, N'Sáng (7h-9h)', '3,5,7',
         DATEADD(HOUR, 7, DATEADD(DAY, -20, @homNay)), DATEADD(HOUR, 9, DATEADD(DAY, 40, @homNay)),
         N'Tiếng Anh - Trần Gia Hân - Sáng', NULL),
        ('0977100003', '0368100003', N'Cờ vua', 1200000, N'Chiều muộn (16h-18h)', '2,6',
         DATEADD(HOUR, 16, DATEADD(DAY, -60, @homNay)), DATEADD(HOUR, 18, DATEADD(DAY, 15, @homNay)),
         N'Cờ vua - Lê Minh Khang - Chiều muộn', N'Căn bản'),
        ('0977100004', '0368100004', N'Ngữ văn', 1400000, N'Chiều (14h-16h)', '2,4',
         DATEADD(HOUR, 14, DATEADD(DAY, -10, @homNay)), DATEADD(HOUR, 16, DATEADD(DAY, 50, @homNay)),
         N'Ngữ văn - Phạm Ngọc Diệp - Chiều', NULL),
        ('0977100005', '0368100005', N'Vật lý', 1600000, N'Tối muộn (20h-22h)', '3,5',
         DATEADD(HOUR, 20, DATEADD(DAY, -45, @homNay)), DATEADD(HOUR, 22, DATEADD(DAY, 15, @homNay)),
         N'Vật lý - Huỳnh Đức Duy - Tối muộn', NULL),
        ('0977100007', '0368100007', N'Tin học', 2000000, N'Chiều muộn (16h-18h)', '3,5,7',
         DATEADD(HOUR, 16, DATEADD(DAY, -25, @homNay)), DATEADD(HOUR, 18, DATEADD(DAY, 35, @homNay)),
         N'Tin học - Đặng Nhật Nam - Chiều muộn', NULL),
        ('0977100009', '0368100009', N'Lập trình Scratch', 1900000, N'Sáng (7h-9h)', '7,8',
         DATEADD(HOUR, 7, DATEADD(DAY, -15, @homNay)), DATEADD(HOUR, 9, DATEADD(DAY, 45, @homNay)),
         N'Lập trình Scratch - Ngô Quốc Huy - Sáng', N'Cơ bản'),
        -- Lớp đã kết thúc
        ('0977100008', '0368100008', N'Sinh học', 1300000, N'Tối (18h-20h)', '2,4,6',
         DATEADD(HOUR, 18, DATEADD(DAY, -70, @homNay)), DATEADD(HOUR, 20, DATEADD(DAY, -10, @homNay)),
         N'Sinh học - Bùi Thanh Thảo - Tối', NULL),
        -- Lớp chưa bắt đầu
        ('0977100006', '0368100006', N'Hóa học', 1700000, N'Sáng muộn (9h-11h)', '2,4,6',
         DATEADD(HOUR, 9, DATEADD(DAY, 5, @homNay)), DATEADD(HOUR, 11, DATEADD(DAY, 65, @homNay)),
         N'Hóa học - Võ Thị Thu Hà - Sáng muộn', NULL),
        ('0977100010', '0368100010', N'Vẽ', 1100000, N'Chiều (14h-16h)', '2,6',
         DATEADD(HOUR, 14, DATEADD(DAY, 10, @homNay)), DATEADD(HOUR, 16, DATEADD(DAY, 70, @homNay)),
         N'Vẽ - Đỗ Khánh Linh - Chiều', N'Cấp 2');

    INSERT INTO dbo.LopHoc
        (MaGS, MaHV, MonHoc, HocPhi, TrangThai, ThoiGianBatDau, ThoiGianKetThuc,
         CaHoc, TenLop, CapDoDauVao, ThuHoc)
    SELECT g.MaGS, h.MaHV, x.MonHoc, x.HocPhi,
           CASE WHEN GETDATE() < x.BatDau THEN N'Chưa bắt đầu'
                WHEN GETDATE() <= x.KetThuc THEN N'Đang diễn ra'
                ELSE N'Đã kết thúc' END,
           x.BatDau, x.KetThuc, x.CaHoc, x.TenLop, x.CapDoDauVao, x.ThuHoc
    FROM @lop x
    JOIN dbo.GiaSu g  ON g.SDT = x.SdtGS
    JOIN dbo.HocVien h ON h.SDT = x.SdtHV
    WHERE NOT EXISTS (SELECT 1 FROM dbo.LopHoc l WHERE l.TenLop = x.TenLop)
      AND NOT EXISTS (SELECT 1 FROM dbo.LopHoc l
                      WHERE l.MaGS = g.MaGS AND l.MaHV = h.MaHV
                        AND l.MonHoc = x.MonHoc AND l.CaHoc = x.CaHoc);
    SET @soLop = @@ROWCOUNT;

    /* -----------------------------------------------------------------
       6. ĐỒNG BỘ TRẠNG THÁI GIA SƯ THEO LỚP
       Chỉ cập nhật các gia sư thuộc bộ dữ liệu mẫu này, giữ nguyên
       trạng thái "Tạm nghỉ" nếu có.
       ----------------------------------------------------------------- */
    UPDATE g
    SET TrangThai = CASE
            WHEN EXISTS (SELECT 1 FROM dbo.LopHoc l
                         WHERE l.MaGS = g.MaGS AND l.TrangThai = N'Đang diễn ra') THEN N'Đang dạy'
            WHEN EXISTS (SELECT 1 FROM dbo.LopHoc l
                         WHERE l.MaGS = g.MaGS AND l.TrangThai = N'Chưa bắt đầu') THEN N'Bận'
            ELSE N'Rảnh' END
    FROM dbo.GiaSu g
    JOIN @gs x ON x.SDT = g.SDT
    WHERE g.TrangThai <> N'Tạm nghỉ';

    COMMIT TRANSACTION;

    PRINT N'seed_data: môn học mới = ' + CAST(@soMon AS NVARCHAR(10))
        + N', gia sư mới = ' + CAST(@soGS AS NVARCHAR(10))
        + N', học viên mới = ' + CAST(@soHV AS NVARCHAR(10))
        + N', tài khoản mới = ' + CAST(@soTK AS NVARCHAR(10))
        + N', lớp học mới = ' + CAST(@soLop AS NVARCHAR(10)) + N'.';

    /* Tổng số bản ghi của bộ dữ liệu mẫu này hiện có trong CSDL */
    SELECT
        (SELECT COUNT(*) FROM dbo.GiaSu g JOIN @gs x ON x.SDT = g.SDT)     AS GiaSuSeed,
        (SELECT COUNT(*) FROM dbo.HocVien h JOIN @hv x ON x.SDT = h.SDT)   AS HocVienSeed,
        (SELECT COUNT(*) FROM dbo.LopHoc l JOIN @lop x ON x.TenLop = l.TenLop) AS LopSeed;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    PRINT N'seed_data lỗi: ' + ERROR_MESSAGE();
    THROW;
END CATCH;
GO
