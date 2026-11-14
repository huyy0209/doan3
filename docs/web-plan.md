# Kế hoạch chuyển DoAnWD (WinForms) lên Web

> Giai đoạn 0 – chốt nghiệp vụ. Chưa có dòng code web nào. Chờ duyệt.
> Nguồn: `DoAnWD/` (C# WinForms .NET 4.8 + EF6), `Database/*.sql`. Mọi tham chiếu `file:dòng` tính từ gốc repo,
> tại commit `6f6a35f`.

---

## 1. Màn hình WinForms → trang web

| # | WinForms (file) | Chức năng | Trang web (route) | Vai trò | Task |
|---|---|---|---|---|---|
| 1 | `UI/Dialogs/frmDangNhap.cs` (đăng nhập) | Đăng nhập, tạm khóa khi sai nhiều | `/TaiKhoan/DangNhap` | Ẩn danh | Khung (GĐ1) |
| 2 | `UI/Dialogs/frmDangNhap.cs:165` `frmDoiMatKhau` | Đổi mật khẩu; bắt đổi khi dùng mật khẩu mặc định | `/TaiKhoan/DoiMatKhau` | Mọi người | Khung (GĐ1) |
| 3 | `UI/TrangChu.cs` (menu, đăng xuất) | Menu theo vai trò, đăng xuất | `_Layout.cshtml` + `/TaiKhoan/DangXuat` (POST) | Mọi người | Khung (GĐ1) |
| 4 | `UI/Pages/frmGiaSu.cs` + `UI/Dialogs/dlgHoSo.cs` | DS gia sư, tìm, lọc môn/ca/trạng thái, thêm/sửa hồ sơ, xóa | `/GiaSu` (Index, Create, Edit, Delete) | Quản lý | **W1** |
| 5 | `UI/Pages/frmHocVien.cs` (tab hồ sơ) | DS học viên, tìm, thêm/sửa hồ sơ, xóa | `/HocVien` (Index, Create, Edit, Delete) | QL: tất cả; GS: chỉ xem HV của mình | **W1** |
| 6 | `UI/Pages/frmTaiKhoan.cs` | DS tài khoản, tạo, đặt lại mật khẩu, khóa/mở | `/QuanLyTaiKhoan` (Index, Create, DatLaiMatKhau, DoiTrangThai) | Quản lý | **W1** |
| 7 | `UI/Pages/frmLopHoc.cs` | DS lớp + đánh giá, lọc, hủy lớp, xóa lớp | `/LopHoc` (Index, Details, Huy, Delete) | Quản lý | **W2** |
| 8 | `UI/Dialogs/frmChucNangGhepLop.cs` | Ghép lớp: chọn HV, GS, môn, ca, thứ, ngày, học phí, tên lớp, cấp độ; kiểm tra trùng lịch real-time | `/LopHoc/GhepLop` (GET form + POST; AJAX `/LopHoc/KiemTraXungDot`, `/LopHoc/GoiYTenLop`) | Quản lý | **W2** |
| 9 | `UI/Dialogs/dlgSuaLop.cs` | Sửa lớp: tên, học phí, ngày kết thúc, thứ, đổi gia sư | `/LopHoc/Edit/{id}` | Quản lý | **W2** |
| 10 | `UI/Dialogs/dlgMonHoc.cs` | Danh mục môn học (thêm/sửa/xóa, cấp độ, ngừng dạy) | `/MonHoc` | Quản lý | Chưa giao (xem câu hỏi 3) |
| 11 | `UI/Pages/frmHocVien.DiemDanh.cs` | Điểm danh + điểm test theo ngày/ca | `/DiemDanh` | QL + GS (lớp của mình) | Chưa giao |
| 12 | `UI/Pages/frmHocVien.BuoiHoc.cs` + `UI/Dialogs/dlgHocBu.cs` | Kết quả học tập theo lớp, sửa buổi, xếp/hủy học bù, phiếu báo | `/HocVien/KetQua/{id}`, `/HocBu` | QL + GS | Chưa giao |
| 13 | `UI/Pages/frmHocPhi.cs` | Công nợ, lịch sử đóng, thêm/xóa lần đóng, xuất CSV | `/HocPhi` | Quản lý | Chưa giao |
| 14 | `UI/Pages/frmTongQuan.cs` | Bảng tổng quan, việc tồn, thu theo tháng | `/` (Home) | Quản lý | GĐ1: trang chủ tối giản; đầy đủ = chưa giao |
| 15 | `UI/Pages/frmCalendarView.cs` | Lịch tháng theo gia sư | `/Lich` | QL (chọn GS) / GS (chỉ mình) | Chưa giao |
| 16 | `UI/Dialogs/dlgCaiDat.cs` | Tên/địa chỉ/SĐT trung tâm | `/CaiDat` | Quản lý | Chưa giao |
| 17 | `UI/Dialogs/frmNotificationToast.cs` + `NotificationService` | Báo lớp sắp bắt đầu (24h) | Badge trên `_Layout` | QL + GS | Chưa giao |

Ghi chú: toàn bộ **service nghiệp vụ** (kể cả của các màn "chưa giao") được chuyển ở GĐ1 để W3 test được và để làm tiếp màn hình sau.

---

## 2. Vai trò và quyền

Vai trò lưu ở `TaiKhoan.VaiTro`: `QuanLy` | `GiaSu` (`DoAnWD/Common/Constants.cs:207-216`, CHECK `Database/02_NangCap_v2.sql:68`).
Tài khoản `GiaSu` bắt buộc gắn `MaGS` (`Database/02_NangCap_v2.sql:72`, `DoAnWD/BLL/Services/TaiKhoanService.cs:252-253`).

| Chức năng | Quản lý | Gia sư | Nguồn |
|---|---|---|---|
| Tổng quan, Lớp học, Học phí, Gia sư, Tài khoản, Cài đặt, Môn học | ✔ | ✘ (menu ẩn) | `DoAnWD/UI/TrangChu.cs:173-176,179`, `DoAnWD/UI/Pages/frmLopHoc.cs:83` |
| Học viên & điểm (xem) | ✔ tất cả | ✔ chỉ HV có lớp của mình hoặc buổi bù mình dạy | `DoAnWD/BLL/Services/HocTapService.cs:455-468` |
| Thêm/sửa/xóa hồ sơ học viên | ✔ | ✘ (chỉ xem) | `DoAnWD/UI/Pages/frmHocVien.cs:150,343`, `DoAnWD/BLL/Services/HocVienService.cs:40,71` |
| Điểm danh / nhập điểm | ✔ | ✔ lớp mình + buổi bù mình dạy | `DoAnWD/BLL/Services/HocTapService.cs:246,270-274` |
| Xếp học bù | ✔ | ✔ chỉ lớp mình | `DoAnWD/BLL/Services/HocTapService.cs:545,557-558` |
| Hủy học bù | ✔ | ✔ lớp mình hoặc buổi mình dạy | `DoAnWD/BLL/Services/HocTapService.cs:602,608-613` |
| Lịch | ✔ chọn gia sư bất kỳ | ✔ chỉ lịch mình | `DoAnWD/UI/Pages/frmCalendarView.cs:72,183` |
| Thông báo lớp sắp bắt đầu | ✔ tất cả lớp | ✔ lớp mình | `DoAnWD/BLL/Services/NotificationService.cs:46` |
| Đổi mật khẩu của mình | ✔ | ✔ | `DoAnWD/UI/TrangChu.cs:234` |
| Màn hình đầu sau đăng nhập | Tổng quan | Điểm danh | `DoAnWD/UI/TrangChu.cs:375-381` |

Web: cookie claims `MaTK`, `TenDangNhap`, `VaiTro` (ClaimTypes.Role), `MaGS`. Controller dùng `[Authorize(Roles="QuanLy")]`
cho trang quản lý. **Service vẫn tự kiểm tra quyền** (thay `PhienDangNhap` tĩnh bằng `INguoiDungHienTai` scoped đọc từ
`HttpContext.User`) – giữ đúng tinh thần HIGH-02: không tin tham số từ tầng giao diện.

---

## 3. Luật nghiệp vụ (mỗi luật kèm nguồn)

Mã luật dùng cho test W3 (`[Trait("Luat","LH-03")]`).

### 3.1 Tài khoản & đăng nhập (TK)
| Mã | Luật | Nguồn |
|---|---|---|
| TK-01 | Băm PBKDF2-HMAC-SHA256, salt ngẫu nhiên 16 byte, 100.000 vòng, hash 32 byte; lưu `"vòng.saltBase64.hashBase64"` | `DoAnWD/BLL/Services/TaiKhoanService.cs:15-27,48-52` |
| TK-02 | Kiểm tra: tách 3 phần theo `.`, đọc số vòng từ chuỗi lưu, so sánh thời gian hằng; sai định dạng → false | `TaiKhoanService.cs:29-46,55-61` |
| TK-03 | Seed `gs<MaGS>` / `123456` dùng hash `100000.Whw+m30vSKawxOgfbZOidQ==.MJpo…` → web phải đăng nhập được | `Database/13_DuLieuMau_v3.sql:48-56`, `Database/seed_data.sql:190-200` |
| TK-04 | Tên đăng nhập trim + `ToLowerInvariant` trước khi tìm/lưu | `TaiKhoanService.cs:157,255` |
| TK-05 | Thiếu tên/mật khẩu → "Nhập tên đăng nhập và mật khẩu." | `TaiKhoanService.cs:154-155` |
| TK-06 | Sai tên, sai mật khẩu, hoặc đang tạm khóa → **cùng một câu** "Sai tên đăng nhập hoặc mật khẩu." | `TaiKhoanService.cs:153,161-166,177` |
| TK-07 | Sai liên tiếp 5 lần → `KhoaDen = now + 5 phút`, reset `SoLanSai = 0` | `TaiKhoanService.cs:125-126,169-176` |
| TK-08 | Đúng mật khẩu nhưng `HoatDong = false` → "Tài khoản đã bị khóa. Liên hệ quản lý." (kiểm sau mật khẩu) | `TaiKhoanService.cs:179-180` |
| TK-09 | Đăng nhập đúng → xóa `SoLanSai`, `KhoaDen` | `TaiKhoanService.cs:181-186` |
| TK-10 | Bảng `TaiKhoan` rỗng → tạo `admin` / `admin123` (QuanLy) | `TaiKhoanService.cs:121-122,132-148`, `DoAnWD/Program.cs:33` |
| TK-11 | Đăng nhập bằng mật khẩu mặc định `admin123` → bắt đổi mật khẩu trước khi vào | `DoAnWD/UI/Dialogs/frmDangNhap.cs:124-132` |
| TK-12 | Mật khẩu mới: 6–100 ký tự | `TaiKhoanService.cs:123,191-197` |
| TK-13 | Tên đăng nhập: 3–50 ký tự, chỉ `[A-Za-z0-9_.]` | `TaiKhoanService.cs:199-207` |
| TK-14 | Đổi mật khẩu: mới ≠ cũ, mới ≠ `admin123`, phải đúng mật khẩu hiện tại | `TaiKhoanService.cs:209-225` |
| TK-15 | Tạo TK (chỉ QL): vai trò ∈ {QuanLy, GiaSu}; GiaSu phải có MaGS; tên không trùng; 1 gia sư tối đa 1 TK; QuanLy thì MaGS = null | `TaiKhoanService.cs:245-274`, `Database/08_NangCap_v8.sql:37` |
| TK-16 | Đặt lại mật khẩu (chỉ QL) → gỡ luôn tạm khóa | `TaiKhoanService.cs:277-291` |
| TK-17 | Khóa/mở (chỉ QL): không khóa chính mình; phải còn ≥1 quản lý hoạt động | `TaiKhoanService.cs:294-310` |
| TK-18 | Danh sách gia sư chưa có tài khoản (để tạo mới) | `TaiKhoanService.cs:313-320` |
| TK-19 | Thao tác quản lý khi chưa đăng nhập hoặc không phải QL → từ chối (HIGH-02) | `TaiKhoanService.cs:95-99` |

### 3.2 Gia sư (GS)
| Mã | Luật | Nguồn |
|---|---|---|
| GS-01 | Họ tên bắt buộc, 2–100 ký tự (sau trim), không chứa chữ số | `DoAnWD/BLL/Validators/GiaSuValidator.cs:35-55`, `Constants.cs:285-286` |
| GS-02 | SĐT bắt buộc, regex `^0\d{9,10}$` | `GiaSuValidator.cs:60-77`, `Constants.cs:283` |
| GS-03 | Chuyên môn bắt buộc, ≤100 ký tự | `GiaSuValidator.cs:82-99` |
| GS-04 | Email (tùy chọn) ≤100, dạng `x@y.z`; kinh nghiệm 0–60; học vấn ≤150; ghi chú ≤500; ca nhận dạy ∈ 6 ca | `GiaSuValidator.cs:102-125`, CHECK `Database/03_NangCap_v3.sql:23` |
| GS-05 | SĐT không trùng gia sư khác (bỏ qua chính mình khi sửa) | `DoAnWD/BLL/Services/GiaSuService.cs:65-66`, `DoAnWD/DAL/Repositories/GiaSuRepository.cs:126-134`, `Database/01_QuanLyGiaSu.sql:26` |
| GS-06 | Ca nhận dạy: chọn 0 hoặc đủ 6 ca → lưu NULL (= mọi ca); khác → "Sáng,Tối" (tên ngắn) | `GiaSuService.cs:87-89`, `DoAnWD/Models/GiaSu.cs:52-58` |
| GS-07 | Trạng thái: chỉ "Tạm nghỉ" do người dùng chọn; còn lại tính theo lịch | `GiaSuService.cs:91-96` |
| GS-08 | Đồng bộ trạng thái: có lớp đang diễn ra → "Đang dạy"; chỉ có lớp chưa bắt đầu → "Bận"; không còn lớp → "Rảnh"; "Tạm nghỉ" giữ nguyên | `GiaSuRepository.cs:101-121` |
| GS-09 | Xóa: chặn nếu còn lớp hoạt động; chặn nếu có lớp trong lịch sử (gợi ý Tạm nghỉ); chặn nếu từng dạy buổi bù; xóa kèm tài khoản | `GiaSuService.cs:102-145` |
| GS-10 | Gia sư nhận lớp mới = mọi gia sư trừ "Tạm nghỉ" | `GiaSuRepository.cs:26-32` |
| GS-11 | Tìm kiếm không dấu, nhiều từ (AND), "đ"→"d", trên HoTen/SDT/ChuyenMon/TrangThai | `GiaSuRepository.cs:139-165`, `DoAnWD/Common/TimKiem.cs:14-33` |

### 3.3 Học viên (HV)
| Mã | Luật | Nguồn |
|---|---|---|
| HV-01 | Họ tên như GS-01 | `DoAnWD/BLL/Validators/HocVienValidator.cs:34-54` |
| HV-02 | Cần ít nhất 1 SĐT (học viên hoặc phụ huynh); SĐT nào có nhập thì phải đúng regex | `HocVienValidator.cs:83-87` |
| HV-03 | Địa chỉ ≤200, tên PH ≤100, trường/lớp ≤150, trình độ ≤50, ghi chú ≤500 | `HocVienValidator.cs:89-93` |
| HV-04 | Trường rỗng/khoảng trắng lưu NULL, có giá trị thì trim | `DoAnWD/BLL/Services/HocVienService.cs:33,56-63` |
| HV-05 | Xóa: chặn nếu còn lớp hoạt động; chặn nếu có lớp trong lịch sử | `HocVienService.cs:69-94`, `DoAnWD/DAL/Repositories/HocVienRepository.cs:59-79` |
| HV-06 | Tìm kiếm không dấu trên HoTen/SDT/DiaChi/TenPhuHuynh/SDTPhuHuynh/TruongLop/TrinhDo | `HocVienRepository.cs:19-47` |
| HV-07 | SĐT liên hệ = SĐT phụ huynh nếu có, không thì SĐT học viên | `DoAnWD/Models/HocVien.cs:49-50` |

### 3.4 Lớp học & ghép lớp (LH)
| Mã | Luật | Nguồn |
|---|---|---|
| LH-01 | Ghép lớp chỉ quản lý | `DoAnWD/BLL/Services/LopHocService.cs:107` |
| LH-02 | Bắt buộc HV, GS, môn (≤50), học phí >0 và ≤100.000.000 | `DoAnWD/BLL/Validators/LopHocValidator.cs:15-56`, `Constants.cs:289-290`, CHECK `Database/01_QuanLyGiaSu.sql:57` |
| LH-03 | Ca ∈ 6 ca cố định; bắt đầu < kết thúc; bắt đầu không trước hôm nay | `LopHocValidator.cs:61-90`, `Constants.cs:62-79` |
| LH-04 | Môn phải có trong danh mục và chưa "Ngừng dạy" | `LopHocService.cs:115-118` |
| LH-05 | Môn năng khiếu: cấp đầu vào phải thuộc danh sách cấp của môn; bỏ trống = cấp thấp nhất | `LopHocService.cs:120-126` |
| LH-06 | Gia sư phải tồn tại và không "Tạm nghỉ"; học viên phải tồn tại | `LopHocService.cs:129-137` |
| LH-07 | Tên lớp: trống → gợi ý "Môn - Học viên - Ca ngắn" (cắt 140 ký tự), thêm " (2)", " (3)"… nếu trùng; ≤150; không trùng tên lớp khác | `LopHocService.cs:75-97,139-144`, `Database/05_NangCap_v5.sql:46` |
| LH-08 | Thứ học lưu "2,4,6" (2=T2…8=CN); chọn 0 hoặc đủ 7 → NULL = hằng ngày | `Constants.cs:136-177` |
| LH-09 | Khoảng giờ lớp = ngày bắt đầu + giờ đầu ca, ngày kết thúc + giờ cuối ca | `DoAnWD/BLL/Services/LopHocGhepLopLogic.cs:12-25`, `Constants.cs:93-104` |
| LH-10 | Số buổi = số ngày trong khoảng rơi vào thứ học; = 0 → lỗi | `DoAnWD/Models/LopHoc.cs:71-83`, `LopHocService.cs:166-167` |
| LH-11 | Trùng lịch = cùng ca + khoảng ngày giao nhau + có ít nhất 1 ngày cả hai cùng học (theo thứ); thiếu ngày mà cùng ca → coi là trùng | `LopHoc.cs:220-265` |
| LH-12 | Xung đột gia sư: GS đã có lớp hoạt động trùng lịch | `LopHocService.cs:378-385` |
| LH-13 | Xung đột học viên: HV đã có lớp hoạt động trùng lịch | `LopHocService.cs:387-391` |
| LH-14 | Xung đột buổi học bù (HIGH-01): GS/HV có buổi bù cùng ca, ngày nằm trong khoảng và là ngày học của lớp mới | `LopHocService.cs:393-415` |
| LH-15 | HV không học trùng môn với cùng GS khi lớp cũ còn hoạt động và giao thời gian (kể cả khác ca) | `LopHocService.cs:417-427` |
| LH-16 | Lớp "hoạt động" = trạng thái ∈ {Chưa bắt đầu, Đang diễn ra, Đang học} và chưa quá ngày kết thúc; "Đã hủy" không chiếm lịch | `Constants.cs:51-56`, `LopHoc.cs:133-145`, `DoAnWD/DAL/Repositories/LopHocRepository.cs:61-72` |
| LH-17 | Trạng thái lớp tính theo giờ: now < bắt đầu → Chưa bắt đầu; trong khoảng → Đang diễn ra; sau → Đã kết thúc; "Đã hủy" giữ nguyên | `LopHoc.cs:107-127` |
| LH-18 | **Ghép lớp ghi lớp + đồng bộ trạng thái gia sư trong 1 transaction** (CRIT-01) | `LopHocService.cs:170-190` |
| LH-19 | Sửa lớp: không sửa lớp đã hủy/thiếu dữ liệu; học phí mới ≥ số đã đóng; ngày kết thúc giữ giờ cuối ca, > bắt đầu, không trước buổi đã điểm danh gần nhất; đổi tên kiểm trùng; đổi GS thì GS mới không tạm nghỉ | `LopHocService.cs:216-257` |
| LH-20 | Sửa lớp: nếu đổi GS/thứ/ngày kết thúc và lớp chưa hết → kiểm trùng lịch từ max(bắt đầu, hôm nay), bỏ qua chính lớp đó; khoảng mới phải có ≥1 buổi | `LopHocService.cs:259-271` |
| LH-21 | Sửa lớp: cập nhật + tính lại trạng thái (gia hạn lớp đã kết thúc → học tiếp) + đồng bộ GS cũ và mới, **1 transaction** | `LopHocService.cs:273-298` |
| LH-22 | Hủy lớp: chặn nếu đã hủy hoặc đã kết thúc; đặt "Đã hủy", xóa buổi bù tương lai, đồng bộ GS, **1 transaction** | `LopHocService.cs:502-541` |
| LH-23 | Xóa hẳn lớp: chặn nếu đã có điểm danh hoặc đóng tiền (bảo dùng Hủy); xóa + đồng bộ GS **1 transaction** | `LopHocService.cs:546-582` |
| LH-24 | Tự cập nhật trạng thái các lớp hoạt động theo giờ, đồng bộ GS bị ảnh hưởng (WinForms chạy mỗi 60 s) | `LopHocService.cs:441-469`, `Constants.cs:298`, `DoAnWD/UI/TrangChu.cs:291` |
| LH-25 | DS lớp: đánh giá = xếp loại theo điểm TB (môn chính) hoặc "Cấp X" (năng khiếu); sắp: chưa hủy, chưa kết thúc trước, rồi theo tên | `LopHocService.cs:306-357` |
| LH-26 | (UI, cảnh báo không chặn) GS không nhận ca đã chọn → vẫn ghép được | `DoAnWD/UI/Dialogs/frmChucNangGhepLop.cs:459,478-482` |
| LH-27 | (UI) Lọc GS theo chuyên môn = môn; "chỉ người trống" ẩn GS không nhận ca/trùng lịch | `frmChucNangGhepLop.cs:392-395` |

### 3.5 Học tập: điểm danh, điểm, học bù (HT) – service chuyển ở GĐ1, màn hình chưa giao
| Mã | Luật | Nguồn |
|---|---|---|
| HT-01 | Điểm danh ∈ {Có mặt, Đi trễ, Vắng có phép, Vắng không phép}; "có học" = Có mặt/Đi trễ | `Constants.cs:190-204`, CHECK `Database/02_NangCap_v2.sql:39` |
| HT-02 | Không điểm danh ngày tương lai, trước ngày bắt đầu, sau ngày kết thúc lớp | `DoAnWD/BLL/Services/HocTapService.cs:47-57` |
| HT-03 | Điểm 0–10 (làm tròn 2 số; nhận "8,5"/"8.5"); vắng thì không có điểm; nhận xét ≤300 | `HocTapService.cs:58-82` |
| HT-04 | Mỗi lớp tối đa 1 buổi thường / ngày | `Database/04_NangCap_v4.sql:31` |
| HT-05 | Buổi vắng đã xếp bù thì không đổi sang có mặt | `HocTapService.cs:282-291` |
| HT-06 | Thi lên cấp: chỉ môn năng khiếu, phải có điểm; cấp thi = cấp kế tiếp (giữ cấp cũ nếu sửa); đã cấp cao nhất → lỗi | `HocTapService.cs:293-313` |
| HT-07 | Cấp hiện tại = cao nhất trong (cấp đầu vào, các cấp thi đạt ≥ 8 điểm); không tụt cấp | `DoAnWD/BLL/Services/MonHocService.cs:139,158-176`, `Constants.cs:187` |
| HT-08 | Xếp loại: ≥8 Giỏi, ≥6.5 Khá, ≥5 Trung bình, còn lại Yếu | `HocTapService.cs:35-42` |
| HT-09 | Tổng hợp lớp: buổi vắng đã học bù xong tính theo điểm danh buổi bù; buổi bù không cộng số buổi | `HocTapService.cs:373-452` |
| HT-10 | Xếp bù: chỉ buổi vắng, chưa có lịch bù; ngày ≥ ngày bắt đầu lớp, ≤ hôm nay + 60; ca hợp lệ; GS dạy bù không tạm nghỉ; HV bận ca đó → lỗi; GS bận → cảnh báo cần xác nhận | `HocTapService.cs:541-596` |
| HT-11 | Mỗi buổi vắng tối đa 1 buổi bù; buổi bù phải có ca + GS dạy | `Database/04_NangCap_v4.sql:35,44-45` |
| HT-12 | Hủy học bù: chỉ xóa buổi bù, buổi gốc giữ nguyên | `HocTapService.cs:599-617` |

### 3.6 Học phí (HP)
| Mã | Luật | Nguồn |
|---|---|---|
| HP-01 | Tình trạng: chưa đóng (≤0) / đã đủ (≥ học phí) / còn nợ | `DoAnWD/BLL/Services/HocPhiService.cs:22-26` |
| HP-02 | Công nợ bỏ lớp đã hủy chưa đóng đồng nào; lớp đã hủy: còn nợ = 0, tình trạng "Đã hủy" | `HocPhiService.cs:29-73` |
| HP-03 | Thêm lần đóng (QL): tiền >0, ngày không tương lai, ghi chú ≤200, lớp không hủy, còn nợ >0, tiền ≤ còn nợ | `HocPhiService.cs:84-121`, CHECK `Database/02_NangCap_v2.sql:51` |
| HP-04 | Xóa lần đóng (QL) | `HocPhiService.cs:123-133` |

### 3.7 Môn học (MH), Cài đặt (CD), Tổng quan (TQ)
| Mã | Luật | Nguồn |
|---|---|---|
| MH-01 | Tên môn 1–50, không trùng; loại ∈ {Chính, Năng khiếu}; năng khiếu cần ≥2 cấp, mỗi cấp ≤50, không chứa `;`, không trùng, tổng ≤500 | `MonHocService.cs:42-58,73-74`, `Database/05_NangCap_v5.sql:26-30` |
| MH-02 | Sửa: không bỏ cấp đang được lớp dùng; đổi tên → cập nhật `LopHoc.MonHoc` và `GiaSu.ChuyenMon` dùng tên cũ | `MonHocService.cs:82-100` |
| MH-03 | Xóa: chặn nếu môn có lớp (gợi ý Ngừng dạy) | `MonHocService.cs:117-130` |
| CD-01 | Tên trung tâm 1–100; địa chỉ ≤300; SĐT ≤50; trống → mặc định "Trung tâm gia sư" | `DoAnWD/BLL/Services/CaiDatService.cs:21,49-66` |
| TQ-01 | Việc tồn: hôm qua chưa điểm danh, buổi vắng chưa bù (30 ngày), lớp còn nợ, lớp kết thúc/bắt đầu trong 7 ngày | `DoAnWD/BLL/Services/TongQuanService.cs:61-69` |
| TQ-02 | Thông báo lớp "Chưa bắt đầu" bắt đầu trong 24 giờ tới; gia sư chỉ thấy lớp mình | `DoAnWD/BLL/Services/NotificationService.cs:27-60` |

---

## 4. Kiến trúc web

```
DoAnWeb/                          ASP.NET Core 8 MVC + Razor + Bootstrap 5
  DoAnWeb.csproj                  net8.0 (xem câu hỏi 2)
  Program.cs                      DI, cookie auth, DbContext, HostedService cập nhật trạng thái (LH-24)
  appsettings.json                ConnectionStrings:QuanLyGiaSu (giống App.config: Server=.;Database=QuanLyGiaSu;Trusted_Connection=True;TrustServerCertificate=True)
  Data/                           EF Core 8 scaffold (Database-First) – KHÔNG migration, KHÔNG đổi schema
    QuanLyGiaSuContext.cs, Entities/*.cs  (+ partial class giữ các thuộc tính tính toán: TrangThaiTheoThoiGian, TrungLich, DemSoBuoi…)
  Common/                         Constants, TimKiem, SoTien (chép từ WinForms, bỏ phần System.Drawing)
  Services/                       chuyển từ DoAnWD/BLL: MatKhau, TaiKhoanService, GiaSuService, HocVienService,
                                  LopHocService (+GhepLopLogic), HocTapService, HocPhiService, MonHocService,
                                  CaiDatService, TongQuanService, NotificationService, Validators/, Dto/
    INguoiDungHienTai.cs          thay PhienDangNhap tĩnh (đọc claims)
  Controllers/  Views/  ViewModels/  wwwroot/
DoAnWeb.Tests/                    xUnit, tham chiếu DoAnWeb
```

Quyết định kỹ thuật:
1. **Giữ nguyên schema**: `dotnet ef dbcontext scaffold` từ DB `QuanLyGiaSu` hiện có (đã kiểm: có đủ bảng, cột `MaMon` v7). Không chạy migration nào.
2. **Băm mật khẩu**: `Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(mk), salt, vong, HashAlgorithmName.SHA256, 32)` – cho cùng kết quả với `new Rfc2898DeriveBytes(string, salt, vong, SHA256).GetBytes(32)` của .NET 4.8 (constructor string dùng UTF-8). Giữ định dạng `vòng.salt.hash`. Test bắt buộc: hash seed `123456` (TK-03) phải verify = true.
3. **Cookie auth**: scheme mặc định, `LoginPath=/TaiKhoan/DangNhap`, `AccessDeniedPath=/TaiKhoan/KhongCoQuyen`, HttpOnly, SameSite=Lax, hết hạn trượt 8 giờ. Mọi form POST có `[ValidateAntiForgeryToken]`.
4. **Transaction**: `LopHocService` dùng 1 DbContext scoped; GhepLop/SuaLop/HuyLop/Delete bọc `BeginTransaction()` như bản gốc (LH-18, 21, 22, 23).
5. **Service** đăng ký `Scoped`; không còn `new GiaSuContextDB()` trong service – dùng DbContext tiêm vào (HocTapService/HocPhiService/MonHocService từng tạo context riêng mỗi hàm → web dùng context scoped, `AsNoTracking` cho truy vấn đọc).
6. **Ngoại lệ → thông báo**: `ArgumentException`/`InvalidOperationException` → `ModelState`/TempData hiện cho người dùng; `UnauthorizedAccessException` → 403.
7. **Giao diện**: Bootstrap 5 (libman/CDN cục bộ trong `wwwroot/lib`), font Be Vietnam Pro (chép từ `DoAnWD/Fonts`), tiếng Việt, định dạng `dd/MM/yyyy`, tiền `N0` vi-VN.
8. **Không đụng WinForms**: không sửa/xóa `DoAnWD/`, `start.bat`, `stop.bat`. WinForms vẫn chạy song song trên cùng DB.

---

## 5. Câu hỏi cần anh/chị trả lời

1. **Tên agent Orca.** `orca orchestration worker-start --help` chỉ liệt kê ví dụ: `claude, codex, cursor, antigravity, muse, zcode, opencode, opencode2`. Không thấy `copilot` hay `deepseek/dsh`. Trên máy có CLI `copilot` (shim VS Code) và `dsh` (npm). Xác nhận agent id cho **W1 (Copilot)** và **W3 (DeepSeek Harness)** – hay để tôi thử `--agent copilot` / `--agent dsh`, nếu Orca từ chối thì dùng dự phòng theo bảng mục C?
2. **.NET 8 nhưng máy chỉ có .NET 10** (SDK 10.0.301, runtime 10.0.9, không có runtime 8). Chọn:
   (a) *[đề xuất]* target `net8.0` + `<RollForward>Major</RollForward>` → build bằng SDK 10, chạy trên runtime 10, mã vẫn là .NET 8;
   (b) cài thêm runtime ASP.NET Core 8;
   (c) đổi sang `net10.0`.
3. **Phạm vi**: phân công chỉ có Gia sư, Học viên, Tài khoản (W1) và Lớp/Ghép lớp (W2). Các màn **Điểm danh, Kết quả/Học bù, Học phí, Môn học, Lịch, Tổng quan đầy đủ, Cài đặt** chưa ai làm. Đề xuất: GĐ1 vẫn chuyển toàn bộ service (để test), màn hình các phần này để **ngoài phạm vi đợt này**, ghi vào báo cáo "chưa làm". Đồng ý, hay muốn thêm task W5?
4. **Build gate**: `dotnet build DoAnWD.slnx` hiện **đã lỗi** sẵn (WinForms dùng packages.config, thiếu `packages/EntityFramework.6.5.1`). Đề xuất: vẫn thêm DoAnWeb + DoAnWeb.Tests vào `DoAnWD.slnx`, nhưng tiêu chí "build pass" = `dotnet build DoAnWeb/DoAnWeb.csproj` và `dotnet test DoAnWeb.Tests/DoAnWeb.Tests.csproj`. Đồng ý?
5. **Tài khoản admin lần đầu** (TK-10, TK-11): web cũng tự tạo `admin/admin123` khi bảng rỗng và bắt đổi mật khẩu mặc định? (DB hiện có 33 tài khoản nên không ảnh hưởng dữ liệu đang có.) Đề xuất: có.
6. **Cột `LopHoc.MaMon` (v7)**: WinForms khi ghép lớp **không** ghi `MaMon` (chỉ ghi `MonHoc` chuỗi, `LopHocService.cs:153-165`). Web có ghi thêm `MaMon` không? Đề xuất: có (đúng ý v7, không đổi schema) – nhưng đây là thay đổi hành vi nhỏ nên hỏi.
7. **Test W3 cần CSDL**: đề xuất test luật thuần (validator, MatKhau, TrungLich, DemSoBuoi, TienTrinhCapDo, XepLoai…) không cần DB, còn test service (xung đột lịch, transaction, khóa đăng nhập) dùng **SQLite in-memory** qua EF Core. Không chạm DB thật. Đồng ý?
8. **Tài khoản demo trong README**: ghi `gs1 / 123456` (gia sư) và tài khoản quản lý nào? DB hiện có `admin` (QuanLy) nhưng tôi không biết mật khẩu hiện tại – cung cấp, hoặc tôi ghi "admin / admin123 (DB mới)".

---

## 6. Chia task

Quy ước chung: chú thích tiếng Việt; 1 chức năng = 1 commit (`feat(web): …` / `test(web): …`); cập nhật `docs/progress/<mã>.md`
sau mỗi commit; `dotnet build DoAnWeb/DoAnWeb.csproj` pass trước khi báo xong. **Không sửa file ngoài danh sách được phép**;
cần sửa file chung (Program.cs, _Layout, Services) thì `ask` coordinator.

| Task | Agent (dự phòng) | Worktree / nhánh | Được sửa | Không được sửa | Nội dung |
|---|---|---|---|---|---|
| GĐ1 | Coordinator | `huyy0209/cập-nhật-lên-web` | `DoAnWeb/**` (khung), `DoAnWD.slnx`, `docs/web-*.md` | `DoAnWD/**`, `Database/**`, `start.bat`, `stop.bat` | csproj, Program.cs, appsettings, Data/ (scaffold), Common/, Services/ (toàn bộ BLL), `INguoiDungHienTai`, `TaiKhoanController` (DangNhap/DangXuat/DoiMatKhau/KhongCoQuyen), `HomeController`, `Views/Shared/_Layout` + menu theo vai trò, `wwwroot/` (Bootstrap, font, site.css) |
| W1 | copilot → opencode → deepseek | `new-child` tên `web-w1` | `DoAnWeb/Controllers/GiaSuController.cs`, `HocVienController.cs`, `QuanLyTaiKhoanController.cs`; `DoAnWeb/Views/GiaSu/**`, `Views/HocVien/**`, `Views/QuanLyTaiKhoan/**`; `DoAnWeb/ViewModels/GiaSu/**`, `ViewModels/HocVien/**`, `ViewModels/QuanLyTaiKhoan/**`; `docs/progress/W1.md` | Services, Data, Program.cs, _Layout | DS + tìm + lọc, thêm/sửa/xóa, validate (dùng validator có sẵn + DataAnnotations) cho Gia sư, Học viên, Tài khoản (tạo, đặt lại MK, khóa/mở). Luật: GS-*, HV-*, TK-12..18 |
| W2 | opencode → copilot → deepseek | `new-child` tên `web-w2` | `DoAnWeb/Controllers/LopHocController.cs`; `DoAnWeb/Views/LopHoc/**`; `DoAnWeb/ViewModels/LopHoc/**`; `DoAnWeb/wwwroot/js/lophoc.js`; `docs/progress/W2.md` | Services (chỉ gọi, không viết lại nghiệp vụ), Data, Program.cs, _Layout | DS lớp + lọc, chi tiết, ghép lớp (form + kiểm tra xung đột AJAX gọi `LopHocService.TimXungDot`), sửa lớp, hủy lớp, xóa lớp. Luật: LH-* |
| W3 | deepseek → copilot → opencode | `new-child` tên `web-w3` | `DoAnWeb.Tests/**`, `docs/progress/W3.md` | mọi thứ khác (kể cả `DoAnWD.slnx` – coordinator thêm khi merge) | xUnit test luật ở mục 3 (mã luật trong `[Trait("Luat", …)]`), bắt buộc có TK-01..03 (hash seed 123456), LH-11..15, LH-18 (rollback), TK-06..09. Lỗi thật trong service → báo coordinator, không tự sửa service |
| W4 | antigravity → (bỏ, coordinator review 10 mục) | `new-child` tên `web-w4`, **chỉ chạy sau khi W1–W3 merge** | `docs/web-review.md` | mọi file code | Review bản web, tối đa 15 mục, xếp theo mức độ |

Thứ tự: GĐ1 (commit "feat(web): dựng khung ASP.NET Core") → W1, W2, W3 song song → merge + build + test → W4 → GĐ3 (start-web.bat, stop-web.bat, README, push).
