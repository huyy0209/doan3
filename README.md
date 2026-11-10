# Hệ thống Quản lý Gia sư (DoAnWD)

Ứng dụng desktop WinForms quản lý **gia sư, học viên và ghép lớp theo ca học** cho một trung tâm gia sư: ghép lớp không trùng ca, đặt tên lớp, điểm danh + điểm bài test cuối buổi, học bù cho buổi vắng, đánh giá môn chính theo xếp loại và môn năng khiếu theo cấp độ, phiếu báo kết quả cho phụ huynh, học phí, lịch học, thống kê và phân quyền Quản lý / Gia sư. Kiến trúc 3 lớp (UI → BLL → DAL).

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
│   ├── 04_NangCap_v4.sql        #   học bù
│   ├── 05_NangCap_v5.sql        #   danh mục môn học, tên lớp, thi lên cấp
│   └── 11_ … 15_DuLieuMau_*.sql #   dữ liệu mẫu (không bắt buộc; số 1x để script nâng cấp sau này không làm đổi số)
└── DoAnWD/
    ├── Program.cs               # Điểm vào: kiểm tra CSDL, đăng nhập, mở trang chủ
    ├── App.config               # Connection string
    ├── Common/                  # Hằng số (ca học, trạng thái), tìm kiếm không dấu, đọc số tiền
    ├── Models/                  # Entity EF + DbContext
    ├── DAL/                     # Repository
    ├── BLL/
    │   ├── Services/            # Nghiệp vụ: gia sư, học viên, lớp, môn học, điểm danh + học bù + cấp độ, học phí, tài khoản, báo cáo
    │   ├── Validators/          # Kiểm tra dữ liệu nhập
    │   └── Dto/                 # Dữ liệu trả về cho màn hình
    └── UI/
        ├── TrangChu.cs          # Khung chính: menu trái, thanh ca học, giữ các trang đã mở
        ├── DieuHuong.cs         # Trang này mở trang khác (vd. bấm ô Tổng quan => trang đã lọc sẵn)
        ├── Pages/               # namespace DoAnWD.UI.Pages: Tổng quan, Lớp học, Học viên & điểm, Lịch, Học phí, Gia sư, Tài khoản
        ├── Dialogs/             # namespace DoAnWD.UI.Dialogs: đăng nhập, ghép lớp, môn học, thêm hồ sơ, xếp học bù, thông báo
        ├── Controls/            # namespace DoAnWD.UI.Controls: khung nhập hồ sơ, cuộn tới đâu tải tới đó
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
   | `04_NangCap_v4.sql` | Có | Học bù: buổi học có ca, gia sư dạy bù, buổi vắng được bù |
   | `05_NangCap_v5.sql` | Có | Danh mục môn học (môn chính / năng khiếu + cấp độ), tên lớp, cấp đầu vào, bài thi lên cấp; tự đặt tên cho lớp cũ |
   | `11_DuLieuMau_ThemNhieu.sql` | Không | Thêm 14 gia sư, 30 học viên, 46 lớp |
   | `12_DuLieuMau_DiemDanh_HocPhi.sql` | Không | Điểm danh, điểm test, học phí mẫu |
   | `13_DuLieuMau_v3.sql` | Không | Hồ sơ gia sư, ghi chú học viên, **tài khoản gia sư mẫu** |
   | `14_DuLieuMau_HocBu.sql` | Không | Xếp học bù cho khoảng một nửa số buổi vắng gần đây |
   | `15_DuLieuMau_CapDo.sql` | Không | Tên lớp + cấp đầu vào cho lớp mẫu, bài thi lên cấp mẫu ở lớp năng khiếu |

   Mọi script chạy lại nhiều lần được, không xóa dữ liệu đang có. Dòng lệnh: `sqlcmd -S . -E -C -I -f 65001 -i Database\01_QuanLyGiaSu.sql` (tương tự cho các file khác).
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

## Chức năng

| Màn hình | Chức năng | Ai dùng |
|---|---|---|
| **Tổng quan** | Số gia sư/học viên/lớp, học phí đang thu, còn phải thu, trạng thái gia sư / lớp, học viên đang học, theo ca, lớp sắp bắt đầu/kết thúc. **Bấm vào ô số liệu để mở trang liên quan đã lọc sẵn** (vd. "Rảnh" → Gia sư đang rảnh, "Đang diễn ra" → Lớp học đang diễn ra, ô học phí → Học phí) | Quản lý |
| **Lớp học** | Mọi lớp: tìm theo tên lớp / học viên / gia sư / môn, lọc trạng thái, ca, môn; **cuộn tới đâu tải tới đó**. Lớp đang chọn: đổi tên, đánh giá (xếp loại hoặc cấp độ + lịch sử thi lên cấp), mở trang học viên, hủy / xóa lớp. **Ghép lớp mới** 3 bước (học viên → môn, cấp đầu vào, ca, ngày, học phí → gia sư dạy môn đó và còn trống ca) + tên lớp. **Môn học**: danh mục môn | Quản lý |
| **Lịch học** | Lịch tháng; mỗi lớp hiện ở mọi ngày học | Quản lý, Gia sư (lịch của mình) |
| **Học phí** | Học phí / đã đóng / còn nợ từng lớp, lọc "còn nợ", ghi nhận các lần đóng, xuất CSV | Quản lý |
| **Gia sư** | Danh sách + hồ sơ (bấm **Thêm gia sư** để mở cửa sổ nhập riêng): email, học vấn/bằng cấp, kinh nghiệm, **ca nhận dạy**, ghi chú, Tạm nghỉ; lọc theo môn, trạng thái, ca nhận dạy; xem lịch dạy và tài khoản | Quản lý |
| **Học viên & điểm** | Tab **Theo học viên**: danh sách (lọc đang học / chưa có lớp / đã nghỉ, theo trình độ; xuất CSV) + học viên đang chọn với 2 tab: *Buổi học & điểm* (chọn lớp, chuyên cần, điểm TB, xếp loại, biểu đồ điểm; sửa trực tiếp điểm danh / điểm / nhận xét từng buổi, thêm buổi còn thiếu, **xếp / hủy học bù**) và *Hồ sơ* (phụ huynh + SĐT, trường/lớp, trình độ, ghi chú). **Thêm học viên** mở cửa sổ nhập riêng; **phiếu báo kết quả** cho phụ huynh. Tab **Điểm danh theo ca**: chọn ngày + ca → điểm danh cả ca, gồm cả học viên đến học bù; bấm đúp tên để mở trang học viên | Quản lý; Gia sư (học viên của mình, không sửa hồ sơ; mở thẳng tab Điểm danh theo ca khi đăng nhập) |
| **Tài khoản** | Tạo tài khoản gia sư/quản lý, đặt lại mật khẩu, khóa/mở khóa | Quản lý |

Khi ghép lớp, gia sư không nhận ca đã chọn bị ẩn (khi bật "chỉ hiện gia sư nhận và còn trống ca này") hoặc gắn nhãn "Không nhận ca này"; vẫn ghép được nếu đã thống nhất riêng.

### Hiệu năng
- Danh sách lớp luôn nạp kèm gia sư + học viên trong **một truy vấn** (không còn lỗi N+1).
- Lịch học chỉ đọc lớp của tháng đang xem; header đếm số lớp theo ca ngay trong SQL; công nợ tính tổng bằng SQL.
- Tự cập nhật trạng thái mỗi phút chỉ đọc các lớp còn hoạt động.
- **Chuyển trang tức thì**: mỗi màn hình chỉ dựng giao diện một lần rồi giữ lại; mở lại thì hiện ngay và chỉ nạp lại dữ liệu (`ILamMoi.LamMoi()`). Nút "Làm mới dữ liệu" cũng chỉ nạp lại dữ liệu, không dựng lại màn hình.
- Bảng Học viên, Gia sư, Học phí phân trang 50 dòng; Học phí mặc định chỉ hiện lớp còn phải thu.
- Trang Lớp học và danh sách buổi học của học viên **cuộn tới đâu tải tới đó** (`CuonTaiDan<T>`): lúc đầu chỉ đưa 30–40 dòng vào bảng, cuộn gần cuối mới thêm đợt tiếp (dữ liệu đã có trong bộ nhớ, không gọi lại CSDL). Trang Lớp học mặc định chỉ hiện lớp còn hoạt động.
- Trang Lớp học đọc dữ liệu bằng 3 truy vấn (lớp kèm gia sư + học viên, buổi có điểm / bài thi, danh mục môn) rồi tìm / lọc trong bộ nhớ.
- Tổng quan: đếm lớp theo trạng thái bằng **một** câu GROUP BY (trước là 4 câu), danh sách lớp còn hoạt động đọc **một lần** dùng chung cho 3 bảng của tab Lớp học (trước là 3 lần).
- Tab "Điểm danh theo ca" chỉ nạp khi đang mở tab đó.

Tìm kiếm ở mọi nơi **không cần gõ dấu**: "ngoc 0981" vẫn tìm ra "Đỗ Bảo Ngọc - 0981111111".

### Điểm danh và điểm test
- Mỗi lớp có tối đa **một buổi thường mỗi ngày** (điểm danh + điểm + nhận xét); lưu lại cùng ngày thì cập nhật. Buổi học bù là bản ghi riêng.
- Điểm thang 10, được nhập "8,5" hoặc "8.5". Vắng thì không có điểm. Không điểm danh cho ngày tương lai hoặc ngoài thời gian lớp.
- Môn chính xếp loại theo điểm TB các bài test: Giỏi ≥ 8, Khá ≥ 6,5, Trung bình ≥ 5, Yếu < 5. Môn năng khiếu đánh giá theo cấp độ (xem *Môn học và đánh giá*).
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

Trang Học viên chia 3 file: `frmHocVien.cs` (khung trang, danh sách, hồ sơ), `frmHocVien.BuoiHoc.cs` (buổi học & điểm, học bù), `frmHocVien.DiemDanh.cs` (điểm danh theo ca). Cửa sổ xếp học bù: `UI/Dialogs/dlgHocBu.cs`.

`UI/Controls/HoSoEditors.cs` chứa khung nhập hồ sơ học viên / gia sư dùng chung cho trang và cho cửa sổ "Thêm ..." (`UI/Dialogs/dlgHoSo.cs`).

Các màn hình mới (`frmHocVien`, `frmGiaSu`, `frmHocPhi`, `frmTaiKhoan`, `frmDangNhap`, hộp thoại ghép lớp) được dựng bằng code thay vì Designer để tự co giãn theo màn hình.

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
| `feat: merge attendance into students, make-up classes…` | Gộp điểm danh + điểm vào trang Học viên (theo học viên và theo ca); học bù cho buổi vắng (ngày, ca, gia sư khác), kiểm tra trùng ca; CSDL v4, dữ liệu mẫu học bù |
| `feat: subject catalog, levels, class names; overview links…` | Danh mục môn học (môn chính / năng khiếu), đánh giá theo xếp loại hoặc cấp độ + thi lên cấp; tên lớp khi ghép lớp; trang Lớp học mới thay trang Ghép lớp cũ; bấm ô Tổng quan để mở trang đã lọc; cuộn tới đâu tải tới đó; gộp truy vấn Tổng quan, xóa hàm không dùng; UI chia namespace theo thư mục; CSDL v5 |

## Dữ liệu mẫu

| Script | Nội dung |
|---|---|
| `01_QuanLyGiaSu.sql` | Tạo CSDL + 6 gia sư (1 người Tạm nghỉ), 6 học viên, 6 lớp; gia sư *Nguyễn Văn An* dạy 2 lớp song song ở 2 ca khác nhau |
| `11_DuLieuMau_ThemNhieu.sql` | Thêm 14 gia sư (nhiều môn, có Cờ vua, Lập trình Scratch; 1 người Tạm nghỉ), 30 học viên (vài cặp anh chị em dùng chung SĐT phụ huynh) và 46 lớp: đã kết thúc, đang diễn ra, chưa bắt đầu, 2 lớp bắt đầu trong 24 giờ tới (để thấy thông báo) và 3 lớp đã hủy |
| `12_DuLieuMau_DiemDanh_HocPhi.sql` | Bổ sung phụ huynh / trường lớp / trình độ cho học viên còn trống; điểm danh các buổi đã học trong 21 ngày gần nhất, khoảng 1/3 số buổi có bài test 15 phút; các lần đóng học phí (đủ, một phần, chưa đóng) |
| `13_DuLieuMau_v3.sql` | Điền hồ sơ gia sư (email, học vấn, kinh nghiệm, ca nhận dạy), ghi chú học viên, tài khoản gia sư mẫu `gs<mã>` / `123456` |
| `14_DuLieuMau_HocBu.sql` | Khoảng một nửa số buổi vắng trong 21 ngày gần nhất được xếp học bù 2 ngày sau, vào ca đầu tiên trong ngày mà học viên và gia sư đều rảnh; buổi bù đã qua có điểm danh, một số có điểm test |
| `15_DuLieuMau_CapDo.sql` | Đặt tên + cấp đầu vào cho các lớp mẫu; ở mỗi lớp năng khiếu đang học, bài test gần nhất được đánh dấu là bài thi lên cấp kế tiếp |

Dữ liệu thêm được tính theo **ngày chạy script**, nên lịch, dải ca và thống kê luôn có số liệu "hôm nay". Script bỏ qua bản ghi đã có, không tạo lớp trùng ca của cùng gia sư/học viên, và cuối cùng đồng bộ lại trạng thái lớp + gia sư.
