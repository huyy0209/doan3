# Hệ thống Quản lý Gia sư (DoAnWD)

Ứng dụng desktop WinForms quản lý **gia sư, học viên và ghép lớp theo ca học** cho một trung tâm gia sư: ghép lớp không trùng ca, điểm danh + điểm bài test cuối buổi, phiếu báo kết quả cho phụ huynh, học phí, lịch học, thống kê và phân quyền Quản lý / Gia sư. Kiến trúc 3 lớp (UI → BLL → DAL).

> Chạy nhanh: bấm đúp `CaiDatCSDL.bat` (lần đầu) rồi `ChayNhanh.bat`.

## Công nghệ

| Thành phần | Phiên bản |
|---|---|
| Ngôn ngữ / Framework | C# · .NET Framework 4.8 · Windows Forms |
| ORM | Entity Framework 6.5.1 (Code First từ CSDL có sẵn) |
| CSDL | Microsoft SQL Server (2016 trở lên, Express/Developer đều được) |
| IDE | Visual Studio 2022 (17.13+ để mở file `DoAnWD.slnx`) |

## Chạy nhanh (Windows)

| Bước | Làm gì | Khi nào |
|---|---|---|
| 1 | Bấm đúp **`CaiDatCSDL.bat`**: tạo CSDL `QuanLyGiaSu`, nâng cấp cấu trúc, hỏi có thêm dữ liệu mẫu không | Lần đầu, và mỗi khi `git pull` có script mới trong `Database/` |
| 2 | Bấm đúp **`ChayNhanh.bat`**: tự build bản mới nhất bằng Visual Studio (MSBuild) rồi mở ứng dụng | Mỗi lần dùng |

- `CaiDatCSDL.bat` cần lệnh `sqlcmd` (cài cùng SQL Server / SSMS). Mặc định dùng SQL Server trên máy (`.`); instance khác thì chạy `CaiDatCSDL.bat .\SQLEXPRESS` và sửa `data source` trong `DoAnWD/App.config` cho khớp.
- `ChayNhanh.bat` chỉ build lại phần thay đổi nên lần sau mở rất nhanh. Máy không có Visual Studio thì nó mở bản đã build trước đó (`DoAnWD/bin/Release/DoAnWD.exe`).
- Có thể tạo shortcut của `ChayNhanh.bat` ra Desktop để mở như một ứng dụng bình thường.

**Đăng nhập lần đầu:** `admin` / `admin123` (ứng dụng tự tạo khi chưa có tài khoản nào). Vào **Đổi mật khẩu** ngay sau đó, rồi tạo tài khoản cho gia sư ở mục **Tài khoản**.

Nếu đã thêm dữ liệu mẫu, mỗi gia sư có sẵn tài khoản `gs<mã gia sư>` / `123456` (ví dụ `gs3`) để thử vai trò Gia sư. Đây là mật khẩu mẫu, đổi hoặc khóa trước khi dùng thật.

## Cấu trúc thư mục

```
doan3/
├── ChayNhanh.bat                # Build + mở ứng dụng
├── CaiDatCSDL.bat               # Tạo / nâng cấp CSDL, thêm dữ liệu mẫu
├── DoAnWD.slnx                  # Solution (Visual Studio 2022 17.13+)
├── Database/                    # Script SQL, đánh số theo thứ tự chạy
│   ├── 01_QuanLyGiaSu.sql       #   tạo CSDL + bảng gốc
│   ├── 02_NangCap_v2.sql        #   hồ sơ học viên, điểm danh, học phí, tài khoản
│   ├── 03_NangCap_v3.sql        #   hồ sơ gia sư, chỉ mục
│   └── 04_ … 06_DuLieuMau_*.sql #   dữ liệu mẫu (không bắt buộc)
└── DoAnWD/
    ├── Program.cs               # Điểm vào: kiểm tra CSDL, đăng nhập, mở trang chủ
    ├── App.config               # Connection string
    ├── Common/                  # Hằng số (ca học, trạng thái), tìm kiếm không dấu, đọc số tiền
    ├── Models/                  # Entity EF + DbContext
    ├── DAL/                     # Repository
    ├── BLL/
    │   ├── Services/            # Nghiệp vụ: gia sư, học viên, lớp, điểm danh, học phí, tài khoản, báo cáo
    │   ├── Validators/          # Kiểm tra dữ liệu nhập
    │   └── Dto/                 # Dữ liệu trả về cho màn hình
    └── UI/
        ├── TrangChu.cs          # Khung chính: menu trái, thanh ca học, điều hướng giữa các trang
        ├── Pages/               # Các trang trong khung chính (Tổng quan, Ghép lớp, Điểm danh, Lịch, Học phí, Gia sư, Học viên, Tài khoản)
        ├── Dialogs/             # Cửa sổ bật ra: đăng nhập, ghép lớp, thêm hồ sơ, thông báo
        ├── Controls/            # Khung nhập hồ sơ học viên / gia sư dùng chung
        └── Theme/               # Màu, font, nút, bảng, lịch... dùng chung cho mọi màn hình
```

## Cài đặt thủ công (không dùng file .bat)

1. **Clone repo**
   ```bash
   git clone https://github.com/huyy0209/doan3.git
   ```
2. **Tạo CSDL**: mở từng file trong thư mục `Database` bằng SQL Server Management Studio và bấm *Execute* theo **thứ tự số**:

   | Script | Bắt buộc | Nội dung |
   |---|---|---|
   | `01_QuanLyGiaSu.sql` | Có | Tạo CSDL + dữ liệu mẫu nhỏ |
   | `02_NangCap_v2.sql` | Có | Hồ sơ học viên, điểm danh, học phí, tài khoản |
   | `03_NangCap_v3.sql` | Có | Hồ sơ gia sư (email, học vấn, kinh nghiệm, ca nhận dạy, ghi chú) + chỉ mục tăng tốc |
   | `04_DuLieuMau_ThemNhieu.sql` | Không | Thêm 14 gia sư, 30 học viên, 46 lớp |
   | `05_DuLieuMau_DiemDanh_HocPhi.sql` | Không | Điểm danh, điểm test, học phí mẫu |
   | `06_DuLieuMau_v3.sql` | Không | Hồ sơ gia sư, ghi chú học viên, **tài khoản gia sư mẫu** |

   Mọi script chạy lại nhiều lần được, không xóa dữ liệu đang có. Dòng lệnh: `sqlcmd -S . -E -C -f 65001 -i Database\01_QuanLyGiaSu.sql` (tương tự cho các file khác).
3. **Kiểm tra connection string** trong `DoAnWD/App.config`. Mặc định là instance mặc định trên máy (`data source=.`). Nếu dùng SQL Express, đổi thành `data source=.\SQLEXPRESS;`.
4. Mở `DoAnWD.slnx` bằng Visual Studio 2022 → chuột phải solution → **Restore NuGet Packages** → **F5**.

Nếu không kết nối được CSDL hoặc CSDL chưa nâng cấp, ứng dụng báo rõ cần làm gì thay vì văng lỗi.

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

| Màn hình | Chức năng | Ai dùng |
|---|---|---|
| **Tổng quan** | Số gia sư/học viên/lớp, học phí đang thu, còn phải thu, trạng thái lớp, theo ca, lớp sắp bắt đầu/kết thúc | Quản lý |
| **Ghép lớp** | Danh sách gia sư và lớp; ghép lớp 3 bước (tìm học viên, chọn ca, chọn gia sư còn trống ca); hủy/xóa lớp | Quản lý |
| **Điểm danh & điểm** | Theo ngày + ca: có mặt / đi trễ / vắng, điểm bài test 15 phút (bỏ trống nếu không kiểm tra), nhận xét. Kết quả học viên: điểm TB, xếp loại, chuyên cần, biểu đồ điểm, **phiếu báo kết quả** cho phụ huynh | Quản lý, Gia sư (lớp của mình) |
| **Lịch học** | Lịch tháng; mỗi lớp hiện ở mọi ngày học | Quản lý, Gia sư (lịch của mình) |
| **Học phí** | Học phí / đã đóng / còn nợ từng lớp, lọc "còn nợ", ghi nhận các lần đóng, xuất CSV | Quản lý |
| **Gia sư** | Danh sách + hồ sơ (bấm **Thêm gia sư** để mở cửa sổ nhập riêng): email, học vấn/bằng cấp, kinh nghiệm, **ca nhận dạy**, ghi chú, Tạm nghỉ; lọc theo môn, trạng thái, ca nhận dạy; xem lịch dạy và tài khoản | Quản lý |
| **Học viên** | Danh sách + hồ sơ (bấm **Thêm học viên** để mở cửa sổ nhập riêng): phụ huynh + SĐT, trường/lớp, trình độ, ghi chú; lọc đang học / chưa có lớp / đã nghỉ, theo trình độ; phiếu báo; xuất CSV | Quản lý |
| **Tài khoản** | Tạo tài khoản gia sư/quản lý, đặt lại mật khẩu, khóa/mở khóa | Quản lý |

Khi ghép lớp, gia sư không nhận ca đã chọn bị ẩn (khi bật "chỉ hiện gia sư nhận và còn trống ca này") hoặc gắn nhãn "Không nhận ca này"; vẫn ghép được nếu đã thống nhất riêng.

### Hiệu năng
- Danh sách lớp luôn nạp kèm gia sư + học viên trong **một truy vấn** (không còn lỗi N+1).
- Lịch học chỉ đọc lớp của tháng đang xem; header đếm số lớp theo ca ngay trong SQL; công nợ tính tổng bằng SQL.
- Tự cập nhật trạng thái mỗi phút chỉ đọc các lớp còn hoạt động.
- **Chuyển trang tức thì**: mỗi màn hình chỉ dựng giao diện một lần rồi giữ lại; mở lại thì hiện ngay và chỉ nạp lại dữ liệu (`ILamMoi.LamMoi()`). Nút "Làm mới dữ liệu" cũng chỉ nạp lại dữ liệu, không dựng lại màn hình.
- Bảng Học viên, Gia sư, Học phí phân trang 50 dòng; Học phí mặc định chỉ hiện lớp còn phải thu; Ghép lớp mặc định chỉ hiện lớp đang học.

Tìm kiếm ở mọi nơi **không cần gõ dấu**: "ngoc 0981" vẫn tìm ra "Đỗ Bảo Ngọc - 0981111111".

### Điểm danh và điểm test
- Mỗi lớp có tối đa **một bản ghi mỗi ngày** (điểm danh + điểm + nhận xét); lưu lại cùng ngày thì cập nhật.
- Điểm thang 10, được nhập "8,5" hoặc "8.5". Vắng thì không có điểm. Không điểm danh cho ngày tương lai hoặc ngoài thời gian lớp.
- Xếp loại theo điểm TB các bài test: Giỏi ≥ 8, Khá ≥ 6,5, Trung bình ≥ 5, Yếu < 5.
- **Phiếu báo kết quả** là file HTML mở bằng trình duyệt (có biểu đồ điểm, bảng các buổi có điểm/nhận xét/vắng). Bấm Ctrl+P để in hoặc lưu PDF gửi phụ huynh.

### Học phí
- Còn nợ = học phí cả khóa − tổng các lần đã đóng. Không cho đóng vượt số còn nợ.
- Lớp đã có điểm danh hoặc đã đóng tiền thì chỉ **hủy** được (giữ lịch sử), không xóa hẳn.

### Tài khoản và bảo mật
- Mật khẩu lưu dạng băm **PBKDF2-SHA256** (salt ngẫu nhiên, 100.000 vòng), không lưu mật khẩu gốc.
- Đăng nhập sai không cho biết sai tên hay sai mật khẩu; sai từ lần thứ 3 phải chờ vài giây mới thử lại được.
- Luôn còn ít nhất một quản lý đang hoạt động; không tự khóa tài khoản đang dùng.

## Giao diện

Giao diện dựa trên hình ảnh **vở ô li**: nền giấy trắng xanh, màu chính là mực tím, có đường lề đỏ chạy dọc vùng nội dung. Mỗi ca học có một màu riêng theo thời điểm trong ngày, từ vàng nắng buổi sáng đến tím đêm, và màu này dùng thống nhất ở mọi nơi: dải ca, lịch, bảng, toast.

| Thành phần | Mô tả |
|---|---|
| Thanh tiêu đề | Ngày hôm nay, ca đang diễn ra, **dải 6 ca học** (ca hiện tại tô đặc, kèm số lớp học hôm nay), số lớp bắt đầu trong 24 giờ |
| Menu trái | Nền tím đậm; vạch đỏ chỉ mục đang mở và trượt theo khi chuyển màn hình (tắt nếu Windows tắt hiệu ứng) |
| Bảng | Mọi ô được tự vẽ nên hiển thị đồng nhất. Không kẻ dọc, dòng cao 36px, trạng thái hiện thành nhãn màu, ca học có chấm màu, cột tự co theo bề ngang. Dòng đang chọn tô **xanh dương nhạt** kèm vạch xanh đầu dòng, không còn khung chấm chấm khi click |
| Bảng nhập liệu | Ở bảng điểm danh, ô sửa được (điểm danh, điểm test, nhận xét) có khung như ô nhập; ô đang chọn viền xanh. Khung không mất khi click |
| Tiêu đề cột | Khai báo một lần bằng `ThemeManager.TieuDeCot(...)`, tự áp lại mỗi khi bảng nạp dữ liệu (kể cả bảng ở tab chưa mở) nên không còn tiêu đề kiểu `GiaSu`, `HocVien` |
| Sao chép | Ở mọi bảng: **Ctrl+C** chép dòng đang chọn; **chuột phải** để "Sao chép ô", "Sao chép dòng" hoặc "Sao chép cả bảng" (dán thẳng vào Excel) |
| Thêm mới | "Thêm học viên" / "Thêm gia sư" mở **cửa sổ nhập riêng**; lưu xong tự chọn người vừa thêm. Khung bên phải trang chỉ để xem / sửa người đang chọn |
| Lịch học | Control tự vẽ, co giãn theo cửa sổ; mỗi lớp hiện ở **mọi ngày** trong khoảng học (trước đây chỉ hiện ở ngày bắt đầu) |
| Nút bấm | Có icon (thêm, sửa, xóa, lưu, làm mới...) và 4 kiểu màu: chính (tím đặc), phụ (tím nhạt), xóa (đỏ), nhẹ (chữ tím); hành động chính của trang đặt ở góc phải tiêu đề |
| Ô số liệu | Mỗi loại một màu: gia sư tím, học viên xanh lá, lớp học cam; số trạng thái tô theo màu trạng thái |
| Ô nhập | Nền tím rất nhạt khi đang gõ để biết con trỏ ở đâu |
| Chuyển trang | Hiện ngay trang đã mở trước đó (bỏ hiệu ứng chụp ảnh màn hình gây khựng ~0,5 giây) |
| Chống nháy | Double buffering cho bảng/panel |

Mã nguồn giao diện nằm trong `DoAnWD/UI/Theme/`:

| File | Vai trò |
|---|---|
| `Theme.cs` | Token màu, font, màu theo ca và theo trạng thái |
| `ThemeManager.cs` | Áp giao diện lên mọi control của một form lúc chạy (`ThemeManager.Apply(form)`) |
| `ThemedButton.cs` | Nút bo góc: Primary / Secondary / Danger / Ghost |
| `SectionBox.cs` | Thay GroupBox: khung mảnh, tiêu đề nằm trong khung |
| `NavButton.cs`, `TabStrip.cs` | Mục menu trái, thanh tab gạch chân |
| `Icons.cs` | Bộ icon nét mảnh vẽ bằng GDI+, không cần file ảnh |
| `Ui.cs`, `ScoreChart.cs` | Hàm dựng control cho các màn hình mới (bố cục TableLayoutPanel tự co giãn), biểu đồ điểm |

| `ShiftRibbon.cs`, `MonthGrid.cs` | Dải ca học, lịch tháng |

`UI/Controls/HoSoEditors.cs` chứa khung nhập hồ sơ học viên / gia sư dùng chung cho trang và cho cửa sổ "Thêm ..." (`UI/Dialogs/dlgHoSo.cs`).

Các màn hình mới (`frmHocVien`, `frmGiaSu`, `frmHocTap`, `frmHocPhi`, `frmTaiKhoan`, `frmDangNhap`, hộp thoại ghép lớp) được dựng bằng code thay vì Designer để tự co giãn theo màn hình.

## Lịch sử nâng cấp

| Commit | Nội dung |
|---|---|
| `chore: add database script…` | Thêm script CSDL + dữ liệu mẫu, connection string dùng `.`, kiểm tra kết nối khi khởi động |
| `fix: correct class-matching rules…` | Sửa luật trùng lịch theo ca (trước đây chặn cả khi khác ca), thêm kiểm tra trùng lịch học viên, đồng bộ trạng thái gia sư, sửa lỗi xóa được học viên có lớp sắp học, chặn trùng SĐT, thêm hủy lớp, thống kê doanh thu |
| `feat(ui): upgrade…` | Nâng cấp hộp thoại ghép lớp, danh sách lớp, trang chủ, thống kê; dùng DbContext mới mỗi lần làm mới để tránh dữ liệu cũ |
| `docs: add README` | Tài liệu này |
| `feat(ui): redesign…` | Thiết kế lại toàn bộ giao diện theo chủ đề "vở ô li" |
| `feat(ui): readability…` | Chữ to hơn, nút có icon và màu rõ, ô số liệu theo màu, hiệu ứng chuyển trang |
| `chore(db): add larger sample data…` | Script dữ liệu mẫu lớn |
| `feat(db): student profile, attendance…` | CSDL v2: hồ sơ học viên, điểm danh + điểm test, học phí, tài khoản; các service + kiểm thử |
| `feat(ui): searchable 3-step class matching…` | Ghép lớp 3 bước có tìm kiếm/lọc; trang Học viên mới; lọc gia sư |
| `feat: attendance, fees, report card, login…` | Màn hình Điểm danh & điểm, Học phí, Tài khoản, đăng nhập + phân quyền |
| `perf: load less data…` | Tối ưu truy vấn, phân trang, giảm dữ liệu hiển thị |
| `feat: tutor profile…` | Trang Gia sư mới + hồ sơ, ca nhận dạy dùng khi ghép lớp, dữ liệu mẫu v3 |
| `perf(ui): instant page switch…` | Giữ màn hình đã mở để chuyển trang không khựng; dòng chọn màu xanh, sao chép trong bảng; thêm học viên / gia sư bằng cửa sổ riêng; gộp code nhập hồ sơ, xóa hàm không dùng |
| `fix(ui): grid borders, overview data; tidy project…` | Ô bảng điểm danh có khung ổn định; tab Lớp học ở Tổng quan đủ tiêu đề cột, đủ 6 ca, cột "Còn" dễ đọc; sắp xếp UI vào `Pages/`, `Dialogs/`, `Controls/`; đánh số script SQL; thêm `ChayNhanh.bat`, `CaiDatCSDL.bat` |

## Dữ liệu mẫu

| Script | Nội dung |
|---|---|
| `01_QuanLyGiaSu.sql` | Tạo CSDL + 6 gia sư (1 người Tạm nghỉ), 6 học viên, 6 lớp; gia sư *Nguyễn Văn An* dạy 2 lớp song song ở 2 ca khác nhau |
| `04_DuLieuMau_ThemNhieu.sql` | Thêm 14 gia sư (nhiều môn, có Cờ vua, Lập trình Scratch; 1 người Tạm nghỉ), 30 học viên (vài cặp anh chị em dùng chung SĐT phụ huynh) và 46 lớp: đã kết thúc, đang diễn ra, chưa bắt đầu, 2 lớp bắt đầu trong 24 giờ tới (để thấy thông báo) và 3 lớp đã hủy |
| `05_DuLieuMau_DiemDanh_HocPhi.sql` | Bổ sung phụ huynh / trường lớp / trình độ cho học viên còn trống; điểm danh các buổi đã học trong 21 ngày gần nhất, khoảng 1/3 số buổi có bài test 15 phút; các lần đóng học phí (đủ, một phần, chưa đóng) |
| `06_DuLieuMau_v3.sql` | Điền hồ sơ gia sư (email, học vấn, kinh nghiệm, ca nhận dạy), ghi chú học viên, tài khoản gia sư mẫu `gs<mã>` / `123456` |

Dữ liệu thêm được tính theo **ngày chạy script**, nên lịch, dải ca và thống kê luôn có số liệu "hôm nay". Script bỏ qua bản ghi đã có, không tạo lớp trùng ca của cùng gia sư/học viên, và cuối cùng đồng bộ lại trạng thái lớp + gia sư.
