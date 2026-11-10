# Hệ thống Quản lý Gia sư (DoAnWD)

Ứng dụng desktop WinForms quản lý **gia sư, học viên và ghép lớp theo ca + thứ học** cho một trung tâm gia sư: ghép lớp không trùng lịch, sửa / gia hạn lớp, điểm danh + điểm bài test cuối buổi, học bù cho buổi vắng, đánh giá môn chính theo xếp loại và môn năng khiếu theo cấp độ, phiếu báo kết quả cho phụ huynh, học phí, lịch học, trang tổng quan "hôm nay làm gì" và phân quyền Quản lý / Gia sư. Kiến trúc 3 lớp (UI → BLL → DAL).

> Chạy nhanh: bấm đúp `CaiDatCSDL.bat` (lần đầu **và sau mỗi lần `git pull`**) rồi `ChayNhanh.bat`.
>
> Bản này cần **CSDL v6**. Nếu mở app thấy báo thiếu cột / bảng, chạy lại `CaiDatCSDL.bat` (không mất dữ liệu).

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

- **`SaoLuuCSDL.bat`**: sao lưu CSDL ra file `.bak` có ngày giờ trong tên. Nên chạy mỗi tuần một lần và trước khi nâng cấp.

**Đăng nhập lần đầu:** `admin` / `admin123` (ứng dụng tự tạo khi chưa có tài khoản nào). Đăng nhập bằng mật khẩu mặc định thì ứng dụng **bắt đặt mật khẩu mới ngay** rồi mới cho vào. Sau đó vào **Cài đặt trung tâm** (cuối menu trái) nhập tên, địa chỉ, số điện thoại trung tâm, rồi tạo tài khoản cho gia sư ở mục **Tài khoản**.

Nếu đã thêm dữ liệu mẫu, mỗi gia sư có sẵn tài khoản `gs<mã gia sư>` / `123456` (ví dụ `gs3`) để thử vai trò Gia sư. Đây là mật khẩu mẫu, đổi hoặc khóa trước khi dùng thật.

## Cấu trúc thư mục

```
doan3/
├── ChayNhanh.bat                # Build + mở ứng dụng
├── CaiDatCSDL.bat               # Tạo / nâng cấp CSDL, thêm dữ liệu mẫu
├── SaoLuuCSDL.bat               # Sao lưu CSDL ra file .bak
├── DoAnWD.slnx                  # Solution (Visual Studio 2022 17.13+)
├── Database/                    # Script SQL, đánh số theo thứ tự chạy
│   ├── 01_QuanLyGiaSu.sql       #   tạo CSDL + bảng gốc
│   ├── 02_NangCap_v2.sql        #   hồ sơ học viên, điểm danh, học phí, tài khoản
│   ├── 03_NangCap_v3.sql        #   hồ sơ gia sư, chỉ mục
│   ├── 04_NangCap_v4.sql        #   học bù
│   ├── 05_NangCap_v5.sql        #   danh mục môn học, tên lớp, thi lên cấp
│   ├── 06_NangCap_v6.sql        #   thứ học của lớp, khóa đăng nhập sai, cài đặt trung tâm
│   └── 11_ … 16_DuLieuMau_*.sql #   dữ liệu mẫu (không bắt buộc; số 1x để script nâng cấp sau này không làm đổi số)
└── DoAnWD/
    ├── Program.cs               # Điểm vào: kiểm tra CSDL, đăng nhập, mở trang chủ
    ├── App.config               # Connection string
    ├── Fonts/                   # Font Be Vietnam Pro (giấy phép OFL), chép kèm bản build
    ├── Common/                  # Hằng số (ca học, thứ học, trạng thái), tìm kiếm không dấu, đọc số tiền
    ├── Models/                  # Entity EF + DbContext
    ├── DAL/                     # Repository
    ├── BLL/
    │   ├── Services/            # Nghiệp vụ: gia sư, học viên, lớp, môn học, điểm danh + học bù + cấp độ, học phí, tài khoản, báo cáo, tổng quan, cài đặt
    │   ├── Validators/          # Kiểm tra dữ liệu nhập
    │   └── Dto/                 # Dữ liệu trả về cho màn hình
    └── UI/
        ├── TrangChu.cs          # Khung chính: menu trái (tên trung tâm, người đăng nhập), thanh ca học, giữ các trang đã mở
        ├── DieuHuong.cs         # Trang này mở trang khác (vd. bấm ô Tổng quan => trang đã lọc sẵn)
        ├── Pages/               # namespace DoAnWD.UI.Pages: Tổng quan, Lớp học, Học viên & điểm, Lịch, Học phí, Gia sư, Tài khoản
        ├── Dialogs/             # namespace DoAnWD.UI.Dialogs: đăng nhập, ghép lớp, sửa lớp, môn học, thêm hồ sơ, xếp học bù, cài đặt trung tâm
        ├── Controls/            # namespace DoAnWD.UI.Controls: khung nhập hồ sơ, chọn ca / chọn thứ, bảng "Hôm nay", ô số, biểu đồ thu, thông báo tự tắt, cuộn tải dần
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
   | `06_NangCap_v6.sql` | Có | Thứ học trong tuần của lớp, đếm lần đăng nhập sai + khóa tạm, bảng cài đặt (thông tin trung tâm) |
   | `11_DuLieuMau_ThemNhieu.sql` | Không | Thêm 14 gia sư, 30 học viên, 46 lớp |
   | `12_DuLieuMau_DiemDanh_HocPhi.sql` | Không | Điểm danh, điểm test, học phí mẫu |
   | `13_DuLieuMau_v3.sql` | Không | Hồ sơ gia sư, ghi chú học viên, **tài khoản gia sư mẫu** |
   | `14_DuLieuMau_HocBu.sql` | Không | Xếp học bù cho khoảng một nửa số buổi vắng gần đây |
   | `15_DuLieuMau_CapDo.sql` | Không | Tên lớp + cấp đầu vào cho lớp mẫu, bài thi lên cấp mẫu ở lớp năng khiếu |
   | `16_DuLieuMau_v6.sql` | Không | Một phần lớp mẫu học theo thứ (2-4-6, 3-5-7), dọn điểm danh mẫu rơi vào ngày không học, thông tin trung tâm mẫu |

   Mọi script chạy lại nhiều lần được, không xóa dữ liệu đang có. Dòng lệnh: `sqlcmd -S . -E -C -I -f 65001 -i Database\01_QuanLyGiaSu.sql` (tương tự cho các file khác).
3. **Kiểm tra connection string** trong `DoAnWD/App.config`. Mặc định là instance mặc định trên máy (`data source=.`). Nếu dùng SQL Express, đổi thành `data source=.\SQLEXPRESS;`.
4. Mở `DoAnWD.slnx` bằng Visual Studio 2022 → chuột phải solution → **Restore NuGet Packages** → **F5**.

Nếu không kết nối được CSDL hoặc CSDL chưa nâng cấp, ứng dụng báo rõ cần làm gì thay vì văng lỗi.

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

## Chức năng

| Màn hình | Chức năng | Ai dùng |
|---|---|---|
| **Tổng quan** | Trả lời ba câu khi mở máy: **hôm nay dạy gì** (tấm bảng 6 ca, mỗi dòng một lớp, chấm vàng = đã điểm danh; bấm một ca để mở điểm danh ca đó), **việc nào còn tồn** (hôm qua chưa điểm danh, buổi vắng 30 ngày gần đây chưa xếp học bù, lớp còn nợ học phí, lớp kết thúc / bắt đầu trong 7 ngày; bấm vào là mở đúng trang, đúng học viên), **tiền thu ra sao** (4 ô số + biểu đồ 6 tháng). Nút nhanh *Điểm danh hôm nay*, *Ghép lớp mới* | Quản lý |
| **Lớp học** | Mọi lớp: tìm theo tên lớp / học viên / gia sư / môn, lọc trạng thái, ca, môn; **cuộn tới đâu tải tới đó**. Lớp đang chọn: lịch học (thứ, ca, số buổi), đánh giá (xếp loại hoặc cấp độ + lịch sử thi lên cấp), **sửa lớp** (tên, học phí, gia hạn, thứ học, đổi gia sư), mở trang học viên, hủy / xóa lớp. **Ghép lớp mới** 3 bước (học viên → môn, cấp đầu vào, ca, **thứ học**, ngày, học phí → gia sư dạy môn đó và còn trống lịch) + tên lớp; ca học viên đang bận hiện chữ "Bận". **Môn học**: danh mục môn | Quản lý |
| **Lịch học** | Lịch tháng; mỗi lớp hiện ở mọi ngày học | Quản lý, Gia sư (lịch của mình) |
| **Học phí** | Học phí / đã đóng / còn nợ từng lớp, lọc "còn nợ", ghi nhận các lần đóng, xuất CSV | Quản lý |
| **Gia sư** | Danh sách + hồ sơ (bấm **Thêm gia sư** để mở cửa sổ nhập riêng): email, học vấn/bằng cấp, kinh nghiệm, **ca nhận dạy**, ghi chú, Tạm nghỉ; lọc theo môn, trạng thái, ca nhận dạy; xem lịch dạy và tài khoản | Quản lý |
| **Học viên & điểm** | Tab **Theo học viên**: danh sách (lọc đang học / chưa có lớp / đã nghỉ, theo trình độ; xuất CSV) + học viên đang chọn với 2 tab: *Buổi học & điểm* (chọn lớp, chuyên cần, điểm TB, xếp loại, biểu đồ điểm; sửa trực tiếp điểm danh / điểm / nhận xét từng buổi, thêm buổi còn thiếu, **xếp / hủy học bù**) và *Hồ sơ* (phụ huynh + SĐT, trường/lớp, trình độ, ghi chú). **Thêm học viên** mở cửa sổ nhập riêng; **phiếu báo kết quả** cho phụ huynh. Tab **Điểm danh theo ca**: chọn ngày + ca → điểm danh cả ca, gồm cả học viên đến học bù; bấm đúp tên để mở trang học viên | Quản lý; Gia sư (học viên của mình, không sửa hồ sơ; mở thẳng tab Điểm danh theo ca khi đăng nhập) |
| **Tài khoản** | Tạo tài khoản gia sư/quản lý, đặt lại mật khẩu, khóa/mở khóa | Quản lý |
| **Cài đặt trung tâm** | Tên, địa chỉ, số điện thoại trung tâm: hiện ở màn hình đăng nhập, menu trái, tiêu đề cửa sổ và đầu phiếu báo kết quả | Quản lý |

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
- Đăng nhập sai không cho biết sai tên hay sai mật khẩu; sai từ lần thứ 3 phải chờ vài giây mới thử lại được. Sai **5 lần liên tiếp thì tài khoản bị khóa 5 phút** (lưu trong CSDL nên đóng mở lại app không lách được); quản lý đặt lại mật khẩu thì mở khóa luôn.
- Đăng nhập bằng mật khẩu mặc định `admin123` bị buộc đặt mật khẩu mới; không đặt lại được về mật khẩu mặc định.
- **Phân quyền kiểm tra ở tầng nghiệp vụ**, không chỉ ẩn nút: ghép / sửa / hủy / xóa lớp, lưu / xóa hồ sơ, học phí, danh mục môn, tài khoản, cài đặt đều từ chối khi người đăng nhập không phải quản lý.
- Luôn còn ít nhất một quản lý đang hoạt động; không tự khóa tài khoản đang dùng.
- Sao lưu: `SaoLuuCSDL.bat`. Khôi phục bằng SSMS (Restore Database → chọn file `.bak`).

## Giao diện

Chủ đề **"Bảng xanh"**: khung ứng dụng màu xanh bảng viết, điểm nhấn vàng phấn, đỏ bút chấm chỉ dành cho lỗi và việc gấp, nội dung nằm trên các thẻ trắng nền giấy xanh rất nhạt. Chữ dùng **Be Vietnam Pro** (font thiết kế cho tiếng Việt, đi kèm trong thư mục `Fonts/`, không cần cài vào máy; không nạp được thì tự dùng Segoe UI). Mỗi ca học có một màu chấm riêng dùng thống nhất ở lịch, bảng và ô chọn ca.

| Thành phần | Mô tả |
|---|---|
| Đăng nhập | Nửa trái là tấm bảng xanh mang tên, địa chỉ, số điện thoại trung tâm; nửa phải là ô nhập. Lỗi hiện ngay dưới ô nhập |
| Menu trái | Tên trung tâm ở trên cùng; mục đang mở có nền sáng hơn + vạch vàng phấn; cuối menu là *Làm mới dữ liệu*, *Cài đặt trung tâm* và ô người đang đăng nhập (bấm để đổi mật khẩu / đăng xuất) |
| Thanh trên | Ngày hôm nay, **dải 6 ca** (ca đang diễn ra tô xanh đặc, ca có lớp hiện số lớp), số lớp bắt đầu trong 24 giờ |
| Tổng quan | Tấm bảng "Hôm nay" là phần chính của trang; bên cạnh là danh sách việc cần xử lý, bên dưới là 4 ô số và biểu đồ thu 6 tháng |
| Thông báo | Lưu / ghép / xóa thành công hiện **bảng báo nhỏ tự tắt** ở cuối cửa sổ, không phải bấm OK. Lỗi và câu hỏi xác nhận vẫn là hộp thoại; hộp thoại Sửa lớp và Đăng nhập báo lỗi ngay trong khung |
| Danh sách trống | Nói rõ vì sao trống và làm gì tiếp (vd. ghép lớp không có gia sư trống lịch: gợi ý bỏ ô lọc hoặc đổi ca / thứ) |
| Bảng | Mọi ô tự vẽ: chữ luôn một dòng, dài thì "…"; số tiền không bao giờ bị cắt mất chữ số đầu. Cột chia theo tỉ lệ khai báo (`ThemeManager.ChiaCot`), bảng không cuộn ngang. Không kẻ dọc, trạng thái là nhãn màu, ca học có chấm màu. Dòng đang chọn tô xanh dương nhạt |
| Sắp xếp + lọc nhanh | **Bấm tiêu đề cột để sắp xếp** (bấm lần nữa đảo chiều). Cột tên người xếp theo **tên gọi** kiểu Việt Nam. **Bấm ô trạng thái / chuyên môn / tình trạng để lọc ngay theo giá trị đó**, bấm lại để bỏ lọc |
| Ô chọn, ô nhập | `ThemedComboBox`, `ThemedTextBox`: viền xám mảnh, đang gõ / đang chọn thì viền xanh. Bộ lọc đang khác mặc định thì tô nền để biết danh sách đang bị lọc |
| Chọn ca, chọn thứ | Ô bấm thay cho danh sách: 6 ca (ca học viên bận ghi "Bận"), 7 thứ T2…CN |
| Nút bấm | Có icon, 4 kiểu: chính (xanh đặc), phụ (trắng viền), xóa (đỏ nhạt), nhẹ (chỉ chữ). Mỗi trang một nút chính ở góc phải tiêu đề |
| Sao chép | Ở mọi bảng: **Ctrl+C** chép dòng đang chọn; **chuột phải** để chép ô, dòng hoặc cả bảng (dán thẳng vào Excel) |
| Co giãn | Các form dùng tỉ lệ theo DPI (không theo font) nên bố cục giống nhau trên mọi máy; chuyển trang hiện ngay trang đã mở trước đó |

Mã nguồn giao diện nằm trong `DoAnWD/UI/Theme/`:

| File | Vai trò |
|---|---|
| `Theme.cs` | Token màu, font, màu theo ca và theo trạng thái |
| `ThemeManager.cs` | Áp giao diện lên mọi control của một form lúc chạy (`ThemeManager.Apply(form)`) |
| `ThemedButton.cs` | Nút bo góc: Primary / Secondary / Danger / Ghost |
| `SectionBox.cs` | Thẻ trắng bo góc có tiêu đề (thay GroupBox) |
| `NavButton.cs`, `TabStrip.cs` | Mục menu trái, thanh tab gạch chân |
| `Icons.cs` | Bộ icon nét mảnh vẽ bằng GDI+, không cần file ảnh |
| `Ui.cs`, `ScoreChart.cs` | Hàm dựng control dùng chung (`DauTrang`, `Section`, `HangLoc`, `LocHaiHang`, `Grid`...), biểu đồ điểm |

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
| `style(ui): clearer dropdowns…` | Ô chọn / bộ lọc tự vẽ cho rõ (viền, mũi tên, tô màu khi đang lọc) |
| `feat(ui): sort by column, click a value to filter…` | Sắp xếp theo cột (tên theo tên gọi), bấm ô trạng thái để lọc; gom code: tiêu đề cột khai báo một chỗ (`TieuDeCot`), hàng lọc dùng chung (`Ui.HangLoc`), Tổng quan bỏ ~170 dòng cấu hình cột lặp lại |
| `feat: weekday schedule, edit class, today dashboard, redesign…` | CSDL v6: thứ học của lớp (trùng lịch xét cả thứ), sửa lớp, hủy lớp dọn buổi bù, khóa đăng nhập sai + buộc đổi mật khẩu mặc định, phân quyền ở tầng nghiệp vụ, cài đặt trung tâm, sao lưu. Giao diện mới "Bảng xanh" + font Be Vietnam Pro, trang Tổng quan mới, đăng nhập mới, thông báo tự tắt |

## Kiểm thử và giới hạn đã biết

- Luật nghiệp vụ (ghép lớp theo thứ, sửa lớp, học bù, cấp độ, học phí, khóa đăng nhập, phân quyền, số liệu tổng quan) có **116 phép kiểm tự động** chạy trên kho dữ liệu giả lập trong bộ nhớ; giao diện được chụp kiểm tra bằng Mono trên Linux.
- Chưa chạy kiểm trên **Windows + SQL Server thật** trong quá trình phát triển bản này. Các điểm chỉ có trên Windows nên xem lại khi chạy lần đầu: nạp font Be Vietnam Pro, viền ô nhập `ThemedTextBox`, script `06` / `16` và `SaoLuuCSDL.bat`.
- Một lớp chỉ có một học viên (kèm 1-1). Chưa có: lớp nhóm, nhắc học phí tự động, xuất Excel (hiện xuất CSV), nhiều chi nhánh.

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
