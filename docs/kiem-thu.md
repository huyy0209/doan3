# KẾ HOẠCH KIỂM THỬ SAU ĐỢT SỬA LỖI

Nhánh: `huyy0209/coordinator` (đã gồm 17 bản sửa trong `docs/code-review.md`).
Cách ghi kết quả: điền **Đạt / Lỗi** vào cột KQ; nếu Lỗi, ghi lại thao tác và thông báo hiện ra.

---

## 0. Chuẩn bị (làm 1 lần)

| # | Việc | Kết quả mong đợi | KQ |
|---|---|---|---|
| 0.1 | Đóng app cũ (`stop.bat`). Sao lưu CSDL bằng `SaoLuuCSDL.bat` | Có file `.bak` mới | |
| 0.2 | Chạy `CaiDatCSDL.bat`, chọn **C** để nạp dữ liệu mẫu | Chạy qua `07_NangCap_v7`, `08_NangCap_v8` không báo `[X]`; cuối cùng hiện `[OK] CSDL da san sang` | |
| 0.3 | Chạy lại `CaiDatCSDL.bat` lần 2 | Vẫn chạy hết, không lỗi (script chạy lại được) | |
| 0.4 | Chạy lệnh `sqlcmd -S . -E -C -i Database\seed_data.sql` (nếu muốn thêm 10 gia sư/18 học viên) | Không lỗi, chạy lần 2 không sinh bản ghi trùng | |
| 0.5 | Bấm đúp `start.bat` | Build xong, mở màn hình đăng nhập | |

Tài khoản: quản lý `admin` / `admin123`; gia sư `gs<Mã gia sư>` / `123456` (ví dụ `gs1`).

---

## 1. Đăng nhập và phân quyền — HIGH-02, HIGH-03

| # | Thao tác | Kết quả mong đợi | KQ |
|---|---|---|---|
| 1.1 | Đăng nhập `admin` / `admin123` | Vào trang chủ, thấy đủ menu quản lý | |
| 1.2 | Đăng nhập `admin` / sai mật khẩu | Thông báo đúng câu **"Sai tên đăng nhập hoặc mật khẩu."** | |
| 1.3 | Đăng nhập `khongtontai` / bất kỳ | **Cùng câu** như 1.2 (không lộ tài khoản có tồn tại hay không) | |
| 1.4 | Đăng nhập tài khoản gia sư đang bị khóa (gia sư "Tạm nghỉ") | Cùng câu như 1.2 | |
| 1.5 | Đăng nhập `gs1` / `123456` | Chỉ thấy lớp/buổi học của chính gia sư đó; không vào được chức năng quản lý (tài khoản, xóa gia sư…) | |
| 1.6 | Với `gs1`, xem lịch / điểm danh | Không thấy dữ liệu của gia sư khác | |

## 2. Gia sư — CRIT-02, HIGH-04, LOW-05

| # | Thao tác | Kết quả mong đợi | KQ |
|---|---|---|---|
| 2.1 | Thêm / sửa một gia sư mới | Lưu thành công, hiện trong danh sách | |
| 2.2 | Tìm kiếm gia sư gõ **có dấu** ("Nguyễn") và **không dấu** ("nguyen") | Cả hai đều ra kết quả đúng | |
| 2.3 | Xóa gia sư vừa tạo (chưa dạy lớp nào) | Xóa được | |
| 2.4 | Xóa gia sư **từng dạy buổi học bù** (xem mục 6 để tạo) | **Không crash**; thông báo chặn xóa và gợi ý chuyển "Tạm nghỉ" | |
| 2.5 | Xóa gia sư đang có lớp | Bị chặn, có thông báo rõ | |

## 3. Học viên — HIGH-04, LOW-05, LOW-01, MED-05

| # | Thao tác | Kết quả mong đợi | KQ |
|---|---|---|---|
| 3.1 | Thêm / sửa / xóa học viên | Hoạt động như trước | |
| 3.2 | Tìm kiếm học viên có dấu / không dấu / theo SĐT | Ra kết quả đúng | |
| 3.3 | Mở trang Học viên, chuyển qua các tab Buổi học, Điểm danh | Hiển thị bình thường, không lỗi (form đã được tách bớt code) | |

## 4. Ghép lớp — CRIT-01, HIGH-01, MED-01, MED-06, LOW-01, LOW-02

| # | Thao tác | Kết quả mong đợi | KQ |
|---|---|---|---|
| 4.1 | Ghép lớp mới: chọn gia sư, học viên, môn, ca, thứ, khoảng ngày | Tạo lớp thành công; tên lớp gợi ý hợp lý; trạng thái gia sư cập nhật (Bận/Đang dạy) | |
| 4.2 | Ghép lớp thứ hai **trùng gia sư + ca + thứ** với lớp 4.1 | Bị chặn, báo trùng lịch | |
| 4.3 | Ghép lớp trùng **học viên** + ca + thứ | Bị chặn, báo trùng lịch | |
| 4.4 | Tạo 1 buổi học bù cho lớp 4.1 vào ngày X ca Y (mục 6.1), rồi ghép lớp mới cho **cùng gia sư** rơi vào ngày X ca Y | **Bị chặn** (trước đây lọt — HIGH-01) | |
| 4.5 | Chọn khoảng ngày không có buổi nào rơi vào thứ đã chọn | Báo lỗi, không tạo lớp | |
| 4.6 | Đóng/mở form ghép lớp nhiều lần (10 lần) | Không chậm dần, không lỗi | |
| 4.7 | SSMS: `SELECT TOP 5 MaLop, MonHoc, MaMon FROM LopHoc ORDER BY MaLop DESC` | Lớp mới có `MaMon` khớp môn đã chọn (LOW-02) | |

## 5. Sửa / hủy lớp — CRIT-01, MED-04

| # | Thao tác | Kết quả mong đợi | KQ |
|---|---|---|---|
| 5.1 | Sửa lớp (đổi tên, học phí, ca) | Lưu đúng; trạng thái gia sư cập nhật theo | |
| 5.2 | Hủy một lớp đang diễn ra | Lớp chuyển "Đã hủy"; gia sư về Rảnh nếu không còn lớp khác | |
| 5.3 | Xóa một lớp | Xóa sạch, không còn buổi/điểm danh mồ côi | |

## 6. Học bù và lịch dạy — HIGH-01, MED-02, MED-06

| # | Thao tác | Kết quả mong đợi | KQ |
|---|---|---|---|
| 6.1 | Tạo buổi học bù cho một lớp | Lưu được, hiện trên lịch | |
| 6.2 | Tạo buổi học bù trùng ca với lớp khác của cùng gia sư | Bị chặn, báo trùng | |
| 6.3 | Mở Lịch (Calendar), chuyển tuần/tháng qua lại nhiều lần | Hiện đúng, không chậm dần | |
| 6.4 | Trang chủ / "Lớp trong ngày" có buổi học bù | Ghi đúng ngày buổi gốc; nếu mất buổi gốc thì ghi "Học bù buổi không rõ", **không** hiện "01/01" | |

## 7. Điểm danh — LOW-01

| # | Thao tác | Kết quả mong đợi | KQ |
|---|---|---|---|
| 7.1 | Điểm danh có mặt / vắng cho một buổi | Lưu đúng, mở lại vẫn còn | |
| 7.2 | Đăng nhập gia sư, điểm danh lớp của mình | Được | |

## 8. Học phí — MED-04 (CRIT-04 không sửa)

| # | Thao tác | Kết quả mong đợi | KQ |
|---|---|---|---|
| 8.1 | Ghi nhận thanh toán một phần cho lớp đang học | Còn nợ giảm đúng | |
| 8.2 | Ghi nhận số tiền lớn hơn số còn nợ | Bị chặn | |
| 8.3 | Xem học phí của lớp **đã hủy** | Còn nợ = 0, tình trạng "Đã hủy" | |
| 8.4 | Ghi nhận thanh toán cho lớp đã hủy | Bị chặn, có thông báo | |

## 9. Thống kê / Tổng quan — MED-03

| # | Thao tác | Kết quả mong đợi | KQ |
|---|---|---|---|
| 9.1 | Xem doanh thu tháng hiện tại | Bằng **tổng tiền đã thu** trong tháng (đối chiếu: `SELECT SUM(SoTien) FROM ThanhToan WHERE MONTH(NgayThanhToan)=... AND YEAR(NgayThanhToan)=...`) | |
| 9.2 | Ghi thêm 1 thanh toán (mục 8.1) rồi xem lại | Doanh thu tăng đúng số vừa thu | |
| 9.3 | Trang Tổng quan: tab Lớp học, 6 ca, cột "Còn" | Hiển thị như trước | |

## 10. Xuất phiếu báo — LOW-03

| # | Thao tác | Kết quả mong đợi | KQ |
|---|---|---|---|
| 10.1 | Xuất phiếu báo học phí / học tập | Mở được file HTML trong trình duyệt | |
| 10.2 | Xuất nhiều lần | Không lỗi; file HTML tạm cũ hơn 1 ngày bị dọn | |

## 11. Tài khoản — LOW-04

| # | Thao tác | Kết quả mong đợi | KQ |
|---|---|---|---|
| 11.1 | Quản lý tạo tài khoản cho gia sư chưa có tài khoản | Tạo được | |
| 11.2 | Tạo tài khoản **thứ hai** cho cùng gia sư đó | Bị chặn, có thông báo (không crash) | |
| 11.3 | Đổi mật khẩu, đăng nhập lại bằng mật khẩu mới | Được | |

## 12. Chạy nền, thông báo, cài đặt — MED-05

| # | Thao tác | Kết quả mong đợi | KQ |
|---|---|---|---|
| 12.1 | Để app mở 5–10 phút ở trang chủ | Thông báo nhắc lịch (toast) vẫn hiện, không crash | |
| 12.2 | Mở Cài đặt, đổi một tùy chọn, đóng/mở lại app | Tùy chọn được giữ | |
| 12.3 | Đang chạy app, tắt SQL Server (services.msc) 1 phút rồi bật lại | App không crash vì Timer nền; thao tác lại được sau khi bật | |

## 13. Khởi động / dừng

| # | Thao tác | Kết quả mong đợi | KQ |
|---|---|---|---|
| 13.1 | `stop.bat` khi app đang chạy | `[OK] Da dung DoAnWD.exe` | |
| 13.2 | `stop.bat` khi app không chạy | `[!] Ung dung DoAnWD.exe hien khong chay.` | |

---

## Phân chia gợi ý (nếu test theo nhóm)

| Người | Phần | Ước lượng |
|---|---|---|
| Người 1 | 0, 1, 11, 13 (cài đặt, đăng nhập, tài khoản) | 30 phút |
| Người 2 | 2, 3, 7 (gia sư, học viên, điểm danh) | 30 phút |
| Người 3 | 4, 5, 6 (ghép lớp, sửa/hủy, học bù — **quan trọng nhất**) | 45 phút |
| Người 4 | 8, 9, 10, 12 (học phí, thống kê, phiếu báo, chạy nền) | 40 phút |

Làm một mình: theo thứ tự 0 → 1 → 4 → 5 → 6 → 8 → 9 → 2 → 3 → 7 → 10 → 11 → 12 → 13 (phần dễ hỏng dữ liệu trước).
