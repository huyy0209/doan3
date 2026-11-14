# Review bản web DoAnWeb

> Người review: **coordinator** (W4 không chạy được: antigravity hết quota – reset sau 164h; muse lỗi 402 billing.
> Theo bảng dự phòng, coordinator tự review ngắn 10 mục). Nhánh `huyy0209/cập-nhật-lên-web` tại `9adcee8`.
> Cách làm: soát tĩnh controller/view/service + chạy app (GET 15 trang với `admin` và `gs1`) + `dotnet test` (240/240 pass).

## Đã kiểm, không có vấn đề
- Mọi `[HttpPost]` (31) đều có `[ValidateAntiForgeryToken]`; mọi trang mặc định cần đăng nhập (AuthorizeFilter toàn cục).
- Trang quản lý có `[Authorize(Roles = QuanLy)]` (LopHoc, MonHoc, GiaSu, QuanLyTaiKhoan, HocPhi, TongQuan, CaiDat, các action ghi của HocVien);
  gia sư xem học viên khác lớp mình → 403 (`HocTapController` dùng `HocVienService.DuocXem`), Lịch bỏ qua `maGS` gửi lên khi là gia sư.
- `FromSqlRaw` (GiaSuService.cs:174, HocVienService.cs:132) dùng `SqlParameter` + escape LIKE → không SQL injection.
- `Html.Raw(JsonSerializer.Serialize(...))` (Views/LopHoc/GhepLop.cshtml:257): System.Text.Json mặc định escape `<`, `>`, `&` → an toàn.
- Ghép / sửa / hủy / xóa lớp vẫn trong transaction (LopHocService.cs:190, 292 …) – CRIT-01 giữ đúng; băm mật khẩu PBKDF2 khớp hash seed (TK-03 có test).

## Phát hiện (xếp theo mức độ)

| # | Mức độ | Vị trí | Vấn đề | Hậu quả | Gợi ý sửa |
|---|---|---|---|---|---|
| 1 | ~~Cao~~ **ĐÃ SỬA (e93d606, bổ sung ea4f634)** | `DoAnWeb/Program.cs:16-25` | Cookie đăng nhập sống 8 giờ (trượt), không kiểm lại `TaiKhoan.HoatDong` / vai trò mỗi request. (TK-08, TK-17) | Quản lý khóa tài khoản gia sư nhưng người đó vẫn dùng tiếp tới 8 giờ; đổi vai trò không có hiệu lực ngay. | Thêm `CookieAuthenticationEvents.OnValidatePrincipal` đọc `HoatDong`/`VaiTro` (cache ngắn) và `RejectPrincipal()` khi đã khóa. |
| 2 | Trung bình | các controller có `[HttpPost]` | Luồng ghi (lưu gia sư/học viên, tạo tài khoản, sửa lớp, lưu môn học, thêm/xóa học phí, lưu điểm danh, xếp bù) **chưa được chạy thử end-to-end trên trình duyệt** – worker tránh ghi dữ liệu thật; test chỉ phủ service. | Lỗi binding form / ModelState / redirect chỉ lộ khi người dùng thao tác. | Chạy thử thủ công trên `QuanLyGiaSu_Test` (đổi connection string) hoặc thêm test WebApplicationFactory cho các POST chính. |
| 3 | ~~Trung bình~~ **ĐÃ SỬA (e93ae37, bổ sung d986318)** | `DoAnWeb/Services/LopHocService.cs:165` (trước `:190`) | Kiểm trùng lịch (`TimXungDot`) chạy **ngoài** transaction ghi lớp. | Hai quản lý ghép cùng lúc có thể tạo 2 lớp trùng ca cho cùng gia sư/học viên (WinForms 1 người dùng nên trước đây không lộ). LH-12, LH-13 | Mở transaction `Serializable` (hoặc `sp_getapplock` theo MaGS/MaHV) bao cả bước kiểm tra + ghi. |
| 4 | ~~Trung bình~~ **ĐÃ SỬA (a133712)** | `DoAnWeb/Controllers/LopHocController.cs:150, 317` | `catch (Exception)` đưa `ex.GetBaseException().Message` ra giao diện. | Lộ tên ràng buộc/bảng SQL khi lỗi CSDL; người dùng khó hiểu. | Ghi log lỗi, hiện câu chung "Không lưu được, thử lại" (giữ thông báo cụ thể cho ArgumentException/InvalidOperationException). |
| 5 | Trung bình | `DoAnWeb/ViewModels/*` (namespace `GiaSu`, `HocVien`, `LopHoc`…) | Namespace ViewModels trùng tên entity → 3 lần merge bị lỗi CS0118 (ad77cd0, 04331e9, cf4d878), đã sửa bằng tên đầy đủ. | Code mới viết `LopHoc` trong ViewModels sẽ lại lỗi build. | Đổi namespace thành `ViewModels.GiaSuVM`… hoặc thêm alias `using LopHocEntity = DoAnWeb.Data.Entities.LopHoc;`. |
| 6 | Thấp | `DoAnWeb/Services/BaoCaoService.cs:156-172` | Xuất CSV không chặn ô bắt đầu bằng `=`, `+`, `-`, `@`. | Tên/ghi chú bắt đầu bằng `=` thành công thức khi mở Excel (CSV injection). | Thêm `'` trước giá trị bắt đầu bằng các ký tự đó. |
| 7 | Thấp | `DoAnWeb/Program.cs:16-25, 82` | Cookie không đặt `SecurePolicy = Always`; web chạy HTTP (`start-web.bat` dùng `http://localhost:5080`). | Nếu mở cho máy khác trong mạng LAN, cookie và mật khẩu đi dạng rõ. | Khi triển khai ngoài localhost: bật HTTPS + `CookieSecurePolicy.Always`. |
| 8 | Thấp | `DoAnWeb/Controllers/TaiKhoanController.cs:33` | Gợi ý "lần đầu sử dụng admin/admin123" chỉ hiện đúng ở request vừa tạo tài khoản (TK-10). | Nếu Program.cs đã tạo admin lúc khởi động thì trang đăng nhập không bao giờ hiện gợi ý. | Hiện gợi ý khi chỉ có 1 tài khoản `admin` chưa đổi mật khẩu mặc định. |
| 9 | Thấp | `DoAnWeb/Services/DangKyDichVuPhu.cs` + `Program.cs:41` | `INguoiDungHienTai`, `AddMemoryCache`, `AddHttpContextAccessor` đăng ký 2 lần. | Vô hại nhưng dễ nhầm khi đổi cách cài đặt người dùng hiện tại. | Bỏ phần trùng trong `AddDichVuPhu`. |
| 10 | Thấp | `DoAnWeb.Tests/` | 240 test phủ luật service, chưa có test controller/phân quyền HTTP (403, redirect gia sư). | Hồi quy phân quyền trên web (thiếu `[Authorize]` ở action mới) không bị test bắt. | Thêm vài test WebApplicationFactory: gia sư gọi `/GiaSu`, `/HocPhi`, `/HocTap/KetQua/{hv khác}` phải bị chặn. |

## Tổng kết
Không thấy lỗi nghiêm trọng: phân quyền, antiforgery, SQL, băm mật khẩu và transaction đều đúng; 240 test luật nghiệp vụ pass.
Ưu tiên sửa #1 (khóa tài khoản chưa có hiệu lực ngay) và #2 (chạy thử các luồng ghi) trước khi dùng thật; #3 chỉ đáng lo khi nhiều quản lý thao tác cùng lúc.
