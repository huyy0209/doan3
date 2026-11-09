# Hệ thống Quản lý Gia sư (DoAnWD)

Ứng dụng desktop WinForms quản lý **gia sư, học viên và ghép lớp theo ca học** cho một trung tâm gia sư. Kiến trúc 3 lớp (UI → BLL → DAL), có tự động cập nhật trạng thái lớp theo thời gian, nhắc lớp sắp bắt đầu, lịch học theo tháng và thống kê.

## Công nghệ

| Thành phần | Phiên bản |
|---|---|
| Ngôn ngữ / Framework | C# · .NET Framework 4.8 · Windows Forms |
| ORM | Entity Framework 6.5.1 (Code First từ CSDL có sẵn) |
| CSDL | Microsoft SQL Server (2016 trở lên, Express/Developer đều được) |
| IDE | Visual Studio 2022 (17.13+ để mở file `DoAnWD.slnx`) |

## Cấu trúc thư mục

```
doan3/
├── Database/
│   └── QuanLyGiaSu.sql          # Script tạo CSDL + dữ liệu mẫu
├── DoAnWD.slnx                  # Solution
└── DoAnWD/
    ├── Program.cs               # Điểm vào, kiểm tra kết nối CSDL
    ├── App.config               # Connection string
    ├── Common/Constants.cs      # Trạng thái, ca học, màu, thông báo
    ├── Models/                  # Entity EF: GiaSu, HocVien, LopHoc, DbContext
    ├── DAL/                     # Repository generic + repository từng bảng
    ├── BLL/
    │   ├── Services/            # Nghiệp vụ: GiaSu, HocVien, LopHoc, ThongKe, Notification
    │   └── Validators/          # Kiểm tra dữ liệu nhập
    └── UI/                      # Các form WinForms
```

## Cài đặt và chạy

1. **Clone repo**
   ```bash
   git clone https://github.com/huyy0209/doan3.git
   ```
2. **Tạo CSDL**: mở `Database/QuanLyGiaSu.sql` trong SQL Server Management Studio và bấm *Execute*, hoặc chạy:
   ```bash
   sqlcmd -S . -E -i Database\QuanLyGiaSu.sql
   ```
   Script chạy lại nhiều lần được; chỉ chèn dữ liệu mẫu khi bảng còn trống.
3. **Kiểm tra connection string** trong `DoAnWD/App.config`. Mặc định là instance mặc định trên máy (`data source=.`). Nếu bạn dùng SQL Express, đổi thành:
   ```xml
   data source=.\SQLEXPRESS;
   ```
4. Mở `DoAnWD.slnx` bằng Visual Studio 2022 → chuột phải solution → **Restore NuGet Packages** → **F5**.

Nếu không kết nối được CSDL, ứng dụng sẽ báo rõ lỗi và hướng dẫn sửa thay vì văng lỗi.

## Nghiệp vụ chính

### Ca học
Trung tâm có 6 ca cố định: Sáng (7h-9h), Sáng muộn (9h-11h), Chiều (14h-16h), Chiều muộn (16h-18h), Tối (18h-20h), Tối muộn (20h-22h). Một lớp học = **một ca cố định mỗi ngày** trong khoảng từ ngày bắt đầu đến ngày kết thúc. Giờ học lấy theo ca.

### Luật ghép lớp
| # | Luật |
|---|---|
| 1 | Gia sư đang **Tạm nghỉ** không nhận lớp mới |
| 2 | Gia sư không có 2 lớp **cùng ca** trong khoảng ngày giao nhau (khác ca thì dạy song song được) |
| 3 | Học viên không có 2 lớp **cùng ca** trong khoảng ngày giao nhau |
| 4 | Học viên không học trùng môn với cùng gia sư khi lớp cũ còn hoạt động |
| 5 | Ngày bắt đầu không ở quá khứ, phải trước ngày kết thúc; học phí > 0 và ≤ 100 triệu |

### Trạng thái
- **Lớp học** tính theo thời gian: `Chưa bắt đầu` → `Đang diễn ra` → `Đã kết thúc`. Lớp có thể bị `Đã hủy` (giữ lại lịch sử, giải phóng lịch).
- **Gia sư** tự đồng bộ theo lịch dạy: có lớp đang diễn ra → `Đang dạy`; chỉ có lớp sắp học → `Bận`; không còn lớp → `Rảnh`. Quản lý có thể chọn `Tạm nghỉ` để ngừng nhận lớp mới.
- Trạng thái được cập nhật khi mở app, mỗi 60 giây, và khi bấm **Làm mới**.

### Ràng buộc xóa
- Không xóa gia sư/học viên đang có lớp hoạt động.
- Không xóa gia sư/học viên đã có lịch sử lớp (để giữ số liệu thống kê) → dùng `Tạm nghỉ` cho gia sư.
- Số điện thoại gia sư không được trùng.

## Chức năng

| Màn hình | Chức năng |
|---|---|
| **Thống kê** (mở mặc định) | Tổng gia sư/học viên/lớp, học phí lớp đang hoạt động, học phí khai giảng trong tháng, số lớp theo trạng thái, theo ca, lớp sắp bắt đầu/sắp kết thúc |
| **Gia sư** | Thêm/sửa/xóa, tìm kiếm, tô màu theo trạng thái, đặt Tạm nghỉ |
| **Học viên** | Thêm/sửa/xóa, tìm kiếm |
| **Ghép lớp** | Danh sách gia sư → lớp của gia sư (ca, ngày, học phí, trạng thái); ghép lớp mới; hủy hoặc xóa lớp |
| **Lịch học** | Lịch theo tháng, lọc theo gia sư |
| Thông báo | Toast + âm thanh cho lớp bắt đầu trong 24 giờ tới; dòng trạng thái góc phải |

Trong hộp thoại **Ghép lớp**, danh sách ca tô **đỏ gạch ngang** những ca gia sư hoặc học viên đã bận trong khoảng ngày đã chọn, và nút Ghép lớp chỉ bật khi không có xung đột.

## Giao diện

Giao diện dựa trên hình ảnh **vở ô li**: nền giấy trắng xanh, màu chính là mực tím, có đường lề đỏ chạy dọc vùng nội dung. Mỗi ca học có một màu riêng theo thời điểm trong ngày, từ vàng nắng buổi sáng đến tím đêm, và màu này dùng thống nhất ở mọi nơi: dải ca, lịch, bảng, toast.

| Thành phần | Mô tả |
|---|---|
| Thanh tiêu đề | Ngày hôm nay, ca đang diễn ra, **dải 6 ca học** (ca hiện tại tô đặc, kèm số lớp học hôm nay), số lớp bắt đầu trong 24 giờ |
| Menu trái | Nền tím đậm; vạch đỏ chỉ mục đang mở và trượt theo khi chuyển màn hình (tắt nếu Windows tắt hiệu ứng) |
| Bảng | Không kẻ dọc, dòng cao 36px, trạng thái hiện thành nhãn màu, ca học có chấm màu, cột tự co theo bề ngang |
| Lịch học | Control tự vẽ, co giãn theo cửa sổ; mỗi lớp hiện ở **mọi ngày** trong khoảng học (trước đây chỉ hiện ở ngày bắt đầu) |
| Chống nháy | Double buffering cho bảng/panel, `WS_EX_COMPOSITED` cho cửa sổ chính |

Mã nguồn giao diện nằm trong `DoAnWD/UI/Theme/`:

| File | Vai trò |
|---|---|
| `Theme.cs` | Token màu, font, màu theo ca và theo trạng thái |
| `ThemeManager.cs` | Áp giao diện lên mọi control của một form lúc chạy (`ThemeManager.Apply(form)`) |
| `ThemedButton.cs` | Nút bo góc: Primary / Secondary / Danger / Ghost |
| `SectionBox.cs` | Thay GroupBox: khung mảnh, tiêu đề nằm trong khung |
| `NavButton.cs`, `TabStrip.cs` | Mục menu trái, thanh tab gạch chân |
| `ShiftRibbon.cs`, `MonthGrid.cs` | Dải ca học, lịch tháng |

## Lịch sử nâng cấp

| Commit | Nội dung |
|---|---|
| `chore: add database script…` | Thêm script CSDL + dữ liệu mẫu, connection string dùng `.`, kiểm tra kết nối khi khởi động |
| `fix: correct class-matching rules…` | Sửa luật trùng lịch theo ca (trước đây chặn cả khi khác ca), thêm kiểm tra trùng lịch học viên, đồng bộ trạng thái gia sư, sửa lỗi xóa được học viên có lớp sắp học, chặn trùng SĐT, thêm hủy lớp, thống kê doanh thu |
| `feat(ui): upgrade…` | Nâng cấp hộp thoại ghép lớp, danh sách lớp, trang chủ, thống kê; dùng DbContext mới mỗi lần làm mới để tránh dữ liệu cũ |
| `docs: add README` | Tài liệu này |
| `feat(ui): redesign…` | Thiết kế lại toàn bộ giao diện theo chủ đề "vở ô li" |

## Dữ liệu mẫu

Script tạo sẵn 6 gia sư (1 người Tạm nghỉ), 6 học viên và 6 lớp ở các trạng thái khác nhau, trong đó gia sư *Nguyễn Văn An* dạy 2 lớp song song ở 2 ca khác nhau, để thử ngay các luật ghép lớp.
