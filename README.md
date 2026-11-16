# Hệ thống Quản lý Gia sư – Bản web

[![Web CI](https://github.com/huyy0209/doan3/actions/workflows/web-ci.yml/badge.svg?branch=web)](https://github.com/huyy0209/doan3/actions/workflows/web-ci.yml)

Ứng dụng web quản lý **gia sư, học viên và ghép lớp theo ca + thứ học** cho trung tâm gia sư: ghép lớp không trùng lịch, sửa / gia hạn lớp, điểm danh + điểm test, học bù, đánh giá môn chính theo xếp loại và môn năng khiếu theo cấp độ, phiếu báo kết quả, học phí, lịch học, trang tổng quan và phân quyền Quản lý / Gia sư.

> Đây là nhánh **`web`**. Bản app WinForms nằm ở nhánh **`app`**; mỗi bản có CSDL riêng (app: `QuanLyGiaSu`, web: `QuanLyGiaSu_Web`).

## Bản web

Bản web chạy trên trình duyệt, dùng **CSDL riêng `QuanLyGiaSu_Web`** (cùng schema với CSDL `QuanLyGiaSu` của bản app WinForms ở nhánh `app`, hai bản chạy song song không đụng nhau) và giữ nguyên nghiệp vụ: ghép lớp theo ca + thứ không trùng lịch, sửa / hủy / xóa lớp trong transaction, điểm danh, điểm test, học bù, môn năng khiếu theo cấp độ, học phí, lịch học, tổng quan, thông báo lớp sắp bắt đầu, phân quyền Quản lý / Gia sư. Giao diện bám bộ "Vở ô li" của app (sidebar xanh bảng, font Be Vietnam Pro).

### Tính năng bản web

- **Quản lý:** hồ sơ gia sư, học viên và tài khoản; danh mục môn học; ghép/sửa/hủy lớp có kiểm tra lịch; tổng quan và lịch hôm nay; lịch tháng và thời khóa biểu tuần.
- **Học tập:** điểm danh theo ngày/ca, nhập điểm và nhận xét, xem kết quả học tập, xếp học bù; phiếu báo kết quả có thể in.
- **Tài chính:** công nợ và lịch sử học phí, ghi nhận khoản thu, in phiếu thu/bảng kê, xuất báo cáo; danh sách nhắc nợ có thể lọc, xuất Excel và sao chép tin nhắn.
- **Tiện ích đợt G:** nhật ký thao tác; biểu đồ tổng quan và khối Hôm nay; bảo mật bằng CSP/security headers, giới hạn thử đăng nhập, nén Brotli/Gzip và cache tệp tĩnh; nhập Excel, tìm kiếm nhanh bằng Ctrl+K, giao diện sáng/tối, PWA, sao lưu CSDL và nhắc nợ.
- **Phân quyền:** gia sư chỉ xem học viên/lớp được phân công và lịch của mình; các chức năng quản lý dành riêng cho tài khoản Quản lý.

Xem từng bước và ảnh minh họa trong **[Hướng dẫn sử dụng bản web](docs/huong-dan-su-dung-web.md)**.

| Thành phần | Công nghệ |
|---|---|
| Web | ASP.NET Core 8 MVC + Razor + Bootstrap 5 (`DoAnWeb/`, target `net8.0`, `RollForward=Major` nên chạy được trên runtime .NET 10) |
| ORM | Entity Framework Core 8, scaffold Database-First từ CSDL có sẵn (không migration) |
| Đăng nhập | Cookie authentication trên bảng `TaiKhoan`, **giữ nguyên thuật toán băm PBKDF2-SHA256 100.000 vòng** nên mọi tài khoản cũ đăng nhập được; khóa 5 phút sau 5 lần sai; bắt đổi mật khẩu mặc định |
| Kiểm thử | xUnit (`DoAnWeb.Tests/`, 487 test) chạy trên CSDL riêng mỗi tiến trình `QuanLyGiaSu_Web_Test_p{pid}` (tự RESTORE từ `QuanLyGiaSu_Web`, tự xóa khi xong), không ghi vào dữ liệu thật; kiểm thử giao diện tự động Playwright (`DoAnWeb.E2E/`) |

**Cách chạy** – bấm đúp **`QuanLyGiaSu.bat`** ở gốc repo, hoặc chạy thẳng một lệnh:

| Menu | Lệnh thẳng | Làm gì | Script con |
|---|---|---|---|
| 1 | `QuanLyGiaSu.bat web` | Build Release, chạy web tại `http://localhost:5080` (cửa sổ riêng) và mở trình duyệt | `scripts/start-web.bat` |
| 2 | `QuanLyGiaSu.bat stop` | Tắt tiến trình web | `scripts/stop-web.bat` |
| 3 | `QuanLyGiaSu.bat cai-csdl [.\SQLEXPRESS]` | Tạo / nâng cấp CSDL `QuanLyGiaSu_Web`, hỏi thêm dữ liệu mẫu (cần `sqlcmd`) | `scripts/CaiDatCSDL.bat` |
| 4 | `QuanLyGiaSu.bat sao-luu` | Sao lưu CSDL ra file `.bak` có ngày giờ | `scripts/SaoLuuCSDL.bat` |
| 5 | `QuanLyGiaSu.bat dong-bo` | Kiểm tra đồng bộ CSDL giữa nhánh `app` và `web` | `scripts/kiem-dong-bo-csdl.ps1` |
| 6 | `QuanLyGiaSu.bat shortcut` | Tạo shortcut "Quản lý gia sư" trên Desktop (icon `assets/QuanLyGiaSu.ico`) | – |
| 9 | `QuanLyGiaSu.bat dich-vu [-MoTuongLua\|-GoBo\|-WhatIf]` | Cài / gỡ bản web làm **dịch vụ Windows** tự chạy khi khởi động máy, tùy chọn mở cổng cho LAN (cần Administrator) | `scripts/cai-dich-vu-web.ps1` |

Lần đầu: chọn **3** (cài CSDL) rồi **1**. Cần **.NET SDK 8 trở lên** (`dotnet --version`), SQL Server (2016+) đang chạy với Windows Authentication; không cần Visual Studio.

Chạy tay: `dotnet run --project DoAnWeb` · chạy test: `dotnet test DoAnWeb.Tests`. Đổi SQL Server: sửa `ConnectionStrings:QuanLyGiaSu` trong `DoAnWeb/appsettings.json`. Các script được gọi bởi menu nằm trong thư mục `scripts/`.

### Kiểm thử giao diện tự động (Playwright)

`DoAnWeb.E2E/` mở Chromium headless và thao tác như người dùng thật trên web Kestrel tự khởi động (cổng trống, CSDL test `QuanLyGiaSu_Web_Test_p{pid}`, **không đụng `QuanLyGiaSu` / `QuanLyGiaSu_Web`**; CSDL test được tạo bằng cách của `DoAnWeb.Tests`). Dữ liệu thử có tiền tố `E2E_` và tự dọn. Bộ test có kịch bản đăng nhập, ghép lớp, điểm danh, học phí, tìm kiếm nhanh, thời khóa biểu, giao diện tối, nhật ký và điểm danh điện thoại. Test chụp ảnh tài liệu có `[Trait("Loai","ChupAnh")]`; trước khi chụp chỉ giữ dữ liệu E2E trong CSDL test.

```
dotnet build DoAnWeb.E2E
pwsh DoAnWeb.E2E/bin/Debug/net8.0/playwright.ps1 install chromium   # một lần: tải trình duyệt (không có pwsh thì dùng powershell)
dotnet test DoAnWeb.E2E
dotnet test DoAnWeb.E2E --filter "Loai=ChupAnh"                    # chụp lại ảnh vào docs/anh/
```

Ảnh chụp tài liệu được lưu ở `docs/anh/` (mỗi ảnh tối đa 200 KB); khi test fail, ảnh debug nằm ở `DoAnWeb.E2E/artifacts/` (đã ignore trong git). Cần SQL Server như khi chạy `DoAnWeb.Tests` (biến môi trường `DOANWEB_TEST_SQL` đổi server test, giống bộ test kia). Trên CI bộ E2E là job riêng `e2e` trong `.github/workflows/web-ci.yml`.

Khi triển khai qua HTTPS ngoài localhost, đặt `BaoMat:BatBuocHttps` thành `true` trong `DoAnWeb/appsettings.json` (hoặc cấu hình tương ứng trong môi trường triển khai) để cookie chỉ được gửi qua HTTPS. Mặc định là `false` để `start-web.bat` tiếp tục chạy HTTP tại `http://localhost:5080`.

**Chạy như dịch vụ Windows / mở cho máy khác trong LAN:** chạy `QuanLyGiaSu.bat dich-vu` bằng quyền Administrator (hoặc mục **9** trong menu) để publish và cài dịch vụ `QuanLyGiaSuWeb` tự khởi động cùng máy; thêm `-MoTuongLua` để mở cổng cho LAN, `-WhatIf` để xem trước các lệnh, `-GoBo` để gỡ. Hướng dẫn từng bước (tường lửa, HTTPS cho LAN, xem log ở `logs/`, quyền SQL Server, xử lý sự cố): **`docs/trien-khai.md`**.

**Kiểm tra đồng bộ CSDL:** Chạy `QuanLyGiaSu.bat dong-bo` hoặc trực tiếp `powershell -ExecutionPolicy Bypass -File scripts/kiem-dong-bo-csdl.ps1` để kiểm tra Database/ giữa nhánh app và web. Script trả về 0 nếu giống, 1 nếu khác.

**Tài khoản demo** (CSDL hiện tại của nhóm):

| Vai trò | Tên đăng nhập | Mật khẩu | Thấy gì |
|---|---|---|---|
| Quản lý | `admin` | `Admin@2026` | Toàn bộ: Tổng quan, Lớp học, Học viên & điểm, Lịch, Học phí, Gia sư, Tài khoản, Cài đặt |
| Gia sư | `gs1` | `123456` | Học viên & điểm, Điểm danh, Lịch – chỉ lớp của mình và buổi bù mình dạy |

CSDL mới tinh (chưa có tài khoản nào): web tự tạo `admin` / `admin123` và bắt đổi mật khẩu ở lần đăng nhập đầu. Đây là mật khẩu demo – đổi trước khi dùng thật.

Tài liệu chuyển đổi: `docs/web-plan.md` (màn hình, quyền, luật nghiệp vụ kèm nguồn), `docs/web-progress.md` (tiến độ, đổi agent), `docs/web-review.md` (review).

## Cấu trúc thư mục

```
doan3/ (nhánh web)
├── QuanLyGiaSu.bat     # Menu khởi động
├── scripts/            # start-web.bat, stop-web.bat, CaiDatCSDL.bat, SaoLuuCSDL.bat, cai-dich-vu-web.ps1
├── assets/             # QuanLyGiaSu.ico + tao-icon.ps1
├── DoAnWD.slnx         # Solution: DoAnWeb + DoAnWeb.Tests + DoAnWeb.E2E
├── Database/           # Script SQL đánh số theo thứ tự chạy (dùng chung với nhánh app)
├── DoAnWeb/            # ASP.NET Core 8 MVC: Controllers, Views, ViewModels, Services (nghiệp vụ), Data (EF Core), wwwroot
├── DoAnWeb.Tests/      # xUnit – luật nghiệp vụ, chạy trên CSDL QuanLyGiaSu_Web_Test_p{pid}
├── DoAnWeb.E2E/        # Playwright – kiểm thử giao diện trên Chromium headless, web Kestrel thật + QuanLyGiaSu_Web_Test_p{pid}
└── docs/               # web-plan.md, web-progress.md, web-review.md, progress/
```

## Nghiệp vụ chính

### Ca học và thứ học
Trung tâm có 6 ca cố định: Sáng (7h-9h), Sáng muộn (9h-11h), Chiều (14h-16h), Chiều muộn (16h-18h), Tối (18h-20h), Tối muộn (20h-22h). Một lớp học = **một ca cố định** + **các thứ học trong tuần** (ví dụ T2-T4-T6, hoặc hằng ngày) trong khoảng từ ngày bắt đầu đến ngày kết thúc. Giờ học lấy theo ca. Lớp tạo từ các bản trước (chưa có thứ học) được hiểu là học hằng ngày.

Mô hình hiện tại: **một lớp = một học viên + một gia sư** (kèm 1-1). Lớp nhóm nhiều học viên chưa hỗ trợ.

### Luật ghép lớp
| # | Luật |
|---|---|
| 1 | Gia sư đang **Tạm nghỉ** không nhận lớp mới |
| 2 | Gia sư không có 2 lớp **cùng ca, có chung thứ học** trong khoảng ngày giao nhau (khác ca, hoặc cùng ca nhưng khác thứ như 2-4-6 và 3-5-7, thì dạy được cả hai) |
| 3 | Học viên không có 2 lớp **cùng ca, có chung thứ học** trong khoảng ngày giao nhau |
| 4 | Học viên không học trùng môn với cùng gia sư khi lớp cũ còn hoạt động |
| 5 | Ngày bắt đầu không ở quá khứ, phải trước ngày kết thúc; học phí > 0 và ≤ 100 triệu |
| 6 | Phải chọn ít nhất một thứ và khoảng ngày phải có ít nhất một buổi rơi vào các thứ đó |

### Sửa lớp đã ghép
Trang Lớp học → **Sửa lớp**: đổi tên, học phí, ngày kết thúc (gia hạn / rút ngắn), thứ học, gia sư. Ca và ngày bắt đầu không đổi (đã gắn với điểm danh); muốn đổi thì hủy lớp rồi ghép lớp mới.

| # | Luật sửa lớp |
|---|---|
| 1 | Học phí mới không thấp hơn số đã đóng |
| 2 | Ngày kết thúc mới sau ngày bắt đầu và không sớm hơn buổi đã điểm danh gần nhất |
| 3 | Gia sư mới không Tạm nghỉ; lịch mới (thứ, ngày, gia sư) được kiểm tra trùng lại **từ hôm nay trở đi** |
| 4 | Tên mới không trùng lớp khác; có lỗi thì không thay đổi nào được lưu |
| 5 | Hủy lớp sẽ xóa luôn các buổi học bù **chưa tới ngày** của lớp đó |

### Trạng thái
- **Lớp học** tính theo thời gian: `Chưa bắt đầu` → `Đang diễn ra` → `Đã kết thúc`. Lớp có thể bị `Đã hủy` (giữ lại lịch sử, giải phóng lịch).
- **Gia sư** tự đồng bộ theo lịch dạy: có lớp đang diễn ra → `Đang dạy`; chỉ có lớp sắp học → `Bận`; không còn lớp → `Rảnh`. Quản lý có thể chọn `Tạm nghỉ` để ngừng nhận lớp mới.
- Trạng thái được cập nhật khi mở app, mỗi 60 giây, và khi bấm **Làm mới**.

### Điểm danh, điểm test và học bù
- Mỗi buổi học: có mặt / đi trễ / vắng có phép / vắng không phép, điểm bài test 15 phút cuối giờ (thang 10, bỏ trống nếu hôm đó không kiểm tra), nhận xét. Vắng thì không có điểm.
- **Học bù**: chọn một buổi vắng → chọn ngày, ca và gia sư dạy bù (mặc định gia sư của lớp, đổi được). Buổi bù vẫn tính cho lớp gốc; buổi vắng giữ nguyên trạng thái và ghi "Bù dd/MM, ca".

| # | Luật học bù |
|---|---|
| 1 | Chỉ bù cho buổi vắng; mỗi buổi vắng tối đa 1 lịch bù |
| 2 | Học viên không có lớp hoặc buổi bù khác **cùng ngày + cùng ca** (bị chặn) |
| 3 | Gia sư dạy bù không được Tạm nghỉ; nếu đang dạy ca đó thì **hỏi xác nhận** (cho học chung) |
| 4 | Ngày bù từ ngày lớp bắt đầu, tối đa 60 ngày tới; buổi bù chưa tới ngày chưa điểm danh được |
| 5 | Buổi vắng đã có lịch bù không đổi sang có mặt được (hủy lịch bù trước) |

- Chuyên cần: buổi vắng **đã học bù xong** tính là có mặt (theo điểm danh của buổi bù); buổi bù không cộng thêm vào số buổi của lớp. Điểm test của buổi bù tính vào điểm trung bình.
- Gia sư thấy và điểm danh được học viên đến học bù vào ca mình dạy, kể cả học viên của lớp khác.

### Môn học và đánh giá
- Quản lý quản lý **danh mục môn học** (trang Lớp học → nút *Môn học*): thêm, đổi tên, đổi loại, xóa.
  - **Môn chính** (Toán, Văn, Anh...): đánh giá bằng **xếp loại** theo điểm TB các bài test: Giỏi ≥ 8, Khá ≥ 6,5, Trung bình ≥ 5, Yếu < 5.
  - **Môn năng khiếu** (Cờ vua, Vẽ, Đàn...): đánh giá theo **cấp độ**; mỗi môn có danh sách cấp riêng (mặc định Cờ vua: Mầm non → Vỡ lòng → Căn bản → Sơ cấp → Trung cấp → Nâng cao).
- Ghép lớp năng khiếu chọn **cấp đầu vào** (mặc định theo trình độ của học viên nếu khớp).
- **Thi lên cấp**: đánh dấu "Thi lên cấp" ở buổi có bài test; app tự ghi cấp đang thi (cấp kế tiếp). Điểm **từ 8** là đạt và lên cấp đó; trượt thì giữ cấp cũ (không tụt cấp). Cấp hiện tại luôn tính lại từ cấp đầu vào + các bài thi, nên sửa điểm bài thi cũ cũng ra kết quả đúng.
- Đổi tên môn cập nhật luôn các lớp và chuyên môn gia sư đang dùng tên cũ. Môn đang có lớp không xóa được (chọn **Ngừng dạy** để ẩn khỏi ghép lớp mới). Không bỏ được cấp độ đang có lớp dùng.

### Tên lớp
- Khi ghép lớp, tên tự gợi ý **"Môn - Học viên - Ca"** (ví dụ *Cờ vua - Đỗ Bảo Ngọc - Tối*), sửa được; sửa rồi thì không tự đổi nữa, bấm *Gợi ý lại* để lấy lại gợi ý.
- Tên lớp **không trùng** (trùng thì gợi ý thêm "(2)"). Đổi tên ở trang Lớp học. Tên lớp hiện ở Lớp học, Học viên, điểm danh theo ca, Học phí, Lịch, Tổng quan và phiếu báo.

### Ràng buộc xóa
- Không xóa gia sư/học viên đang có lớp hoạt động.
- Không xóa gia sư/học viên đã có lịch sử lớp (để giữ số liệu thống kê) → dùng `Tạm nghỉ` cho gia sư.
- Số điện thoại gia sư không được trùng.


## Dữ liệu mẫu

| Script | Nội dung |
|---|---|
| `01_QuanLyGiaSu.sql` | Tạo CSDL + 6 gia sư (1 người Tạm nghỉ), 6 học viên, 6 lớp; gia sư *Nguyễn Văn An* dạy 2 lớp song song ở 2 ca khác nhau |
| `11_DuLieuMau_ThemNhieu.sql` | Thêm 14 gia sư (nhiều môn, có Cờ vua, Lập trình Scratch; 1 người Tạm nghỉ), 30 học viên (vài cặp anh chị em dùng chung SĐT phụ huynh) và 46 lớp: đã kết thúc, đang diễn ra, chưa bắt đầu, 2 lớp bắt đầu trong 24 giờ tới (để thấy thông báo) và 3 lớp đã hủy |
| `12_DuLieuMau_DiemDanh_HocPhi.sql` | Bổ sung phụ huynh / trường lớp / trình độ cho học viên còn trống; điểm danh các buổi đã học trong 21 ngày gần nhất, khoảng 1/3 số buổi có bài test 15 phút; các lần đóng học phí (đủ, một phần, chưa đóng) |
| `13_DuLieuMau_v3.sql` | Điền hồ sơ gia sư (email, học vấn, kinh nghiệm, ca nhận dạy), ghi chú học viên, tài khoản gia sư mẫu `gs<mã>` / `123456` |
| `14_DuLieuMau_HocBu.sql` | Khoảng một nửa số buổi vắng trong 21 ngày gần nhất được xếp học bù 2 ngày sau, vào ca đầu tiên trong ngày mà học viên và gia sư đều rảnh; buổi bù đã qua có điểm danh, một số có điểm test |
| `15_DuLieuMau_CapDo.sql` | Đặt tên + cấp đầu vào cho các lớp mẫu; ở mỗi lớp năng khiếu đang học, bài test gần nhất được đánh dấu là bài thi lên cấp kế tiếp |
| `16_DuLieuMau_v6.sql` | Khoảng 2/3 lớp mẫu chuyển sang học theo thứ (2-4-6 hoặc 3-5-7), xóa điểm danh mẫu rơi vào ngày không học, thêm thông tin trung tâm mẫu |

Dữ liệu thêm được tính theo **ngày chạy script**, nên lịch, dải ca và thống kê luôn có số liệu "hôm nay". Script bỏ qua bản ghi đã có, không tạo lớp trùng ca của cùng gia sư/học viên, và cuối cùng đồng bộ lại trạng thái lớp + gia sư.
