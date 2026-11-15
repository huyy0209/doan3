# Đề xuất hoàn thiện bản web

> Viết ngày 15/11/2026, sau khi bản web đã có đủ màn hình của app và đã qua review.
> Người dùng chọn mục nào thì mục đó được chia task và **push lên sau mỗi giai đoạn**.
> Ước lượng: S = ≤ 1 task nhỏ, M = 1 task, L = 2+ task.

## Đang làm (giai đoạn A)
- W11: sửa review #5–#9 (đổi namespace ViewModels, chặn CSV injection, cookie Secure khi bật HTTPS, gợi ý đăng nhập lần đầu, bỏ DI trùng).
- W12: test tích hợp HTTP cho mọi luồng ghi + phân quyền (review #2, #10); lỗi tìm ra sẽ sửa ở giai đoạn B.

## Đề xuất thêm (chưa làm – chờ chọn)

| # | Đề xuất | Lợi ích | Cỡ | Gợi ý agent |
|---|---|---|---|---|
| 1 | **Phân trang + sắp xếp** cho danh sách Gia sư, Học viên, Lớp học, Học phí (giữ bộ lọc khi chuyển trang) | Danh sách vẫn nhanh khi dữ liệu lên hàng nghìn dòng; giống `Pager` của app | M | copilot |
| 2 | **Nhật ký thao tác** (ai, lúc nào, sửa gì) cho ghép/sửa/hủy lớp, học phí, tài khoản – bảng mới `NhatKy` qua script `Database/09_NangCap_v9.sql` | Truy vết sai sót, tranh chấp học phí. *Cần thêm bảng → đổi schema có chủ đích, chạy lại được, không ảnh hưởng app* | M | sonnet |
| 3 | **Giao diện điện thoại**: sidebar thu gọn, bảng cuộn ngang, nút điểm danh to | Gia sư điểm danh ngay trên điện thoại tại lớp | M | copilot |
| 4 | **Xuất Excel (.xlsx)** cho công nợ, danh sách lớp, kết quả học tập (thêm ClosedXML) | Quản lý gửi báo cáo cho phụ huynh / kế toán | S | copilot |
| 5 | **Trang lỗi + nhật ký hệ thống**: ghi log ra file theo ngày (Serilog), trang 404/500 tiếng Việt đúng giao diện | Dễ điều tra khi người dùng báo lỗi | S | haiku |
| 6 | **Sao lưu / khôi phục CSDL từ web** (chỉ quản lý), lịch tự sao lưu hằng ngày | Không cần mở `QuanLyGiaSu.bat` trên máy chủ | M | sonnet |
| 7 | **Chạy như dịch vụ Windows / IIS** + hướng dẫn mở cho máy khác trong mạng LAN (kèm HTTPS) | Nhiều người dùng cùng lúc từ máy khác | M | sonnet |
| 8 | **Kiểm thử giao diện tự động (Playwright)**: đăng nhập, ghép lớp, điểm danh, đóng học phí | Bắt lỗi giao diện trước khi push | M | sonnet |
| 9 | **CI GitHub Actions** cho nhánh `web`: build + test mỗi lần push (SQL Server container) | Mỗi lần push đều biết ngay có vỡ gì không | S | copilot |
| 10 | **Đồng bộ app ↔ web**: script so sánh `Database/` hai nhánh, nhắc khi lệch | Hai nhánh dùng chung CSDL, tránh quên chép script | S | haiku |

## Thứ tự gợi ý
1. Giai đoạn B: sửa lỗi W12 tìm ra → Opus review → push.
2. Giai đoạn C: **9 (CI)** + **5 (log/lỗi)** + **1 (phân trang)** – rẻ, nâng chất lượng ngay → push.
3. Giai đoạn D: **3 (điện thoại)** + **4 (Excel)** → push.
4. Giai đoạn E (cần quyết định thêm vì đổi CSDL / triển khai): **2, 6, 7, 8**.
