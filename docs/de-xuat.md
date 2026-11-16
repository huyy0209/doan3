# Đề xuất hoàn thiện bản web – trạng thái triển khai

> Viết ngày 15/11/2026, sau khi bản web đã có đủ màn hình của app và đã qua review.
> Các đề xuất 1–10 bên dưới đã được triển khai. Chi tiết tính năng mới đợt G xem `docs/web-progress.md` và tài liệu `docs/huong-dan-su-dung-web.md`.
> Ước lượng: S = ≤ 1 task nhỏ, M = 1 task, L = 2+ task.

## Đang làm (giai đoạn A)
- W11: sửa review #5–#9 (đổi namespace ViewModels, chặn CSV injection, cookie Secure khi bật HTTPS, gợi ý đăng nhập lần đầu, bỏ DI trùng).
- W12: test tích hợp HTTP cho mọi luồng ghi + phân quyền (review #2, #10); lỗi tìm ra sẽ sửa ở giai đoạn B.

## Các đề xuất đã làm

| # | Trạng thái | Đề xuất | Giai đoạn / ghi chú |
|---|---|---|---|---|
| 1 | Đã làm | Phân trang + sắp xếp danh sách Gia sư, Học viên, Lớp học, Học phí | C4 |
| 2 | Đã làm | Nhật ký thao tác; bảng và nâng cấp riêng cho CSDL web | G1 |
| 3 | Đã làm | Giao diện điện thoại, sidebar thu gọn và điểm danh dạng thẻ | D1 |
| 4 | Đã làm | Xuất Excel cho công nợ, lớp học, kết quả học tập | D2 |
| 5 | Đã làm | Log hệ thống và trang lỗi tiếng Việt | C2 |
| 6 | Đã làm | Sao lưu CSDL web theo yêu cầu, tự động và khôi phục ngay trong web | E6, I1 |
| 7 | Đã làm | Dịch vụ Windows, IIS và hướng dẫn LAN/HTTPS | E7, I2 |
| 8 | Đã làm | Bộ kiểm thử giao diện Playwright | E8 |
| 9 | Đã làm | CI GitHub Actions cho build, test và E2E | C1 |
| 10 | Đã làm | Kiểm tra đồng bộ script CSDL giữa app và web | C3 |

## Bổ sung trong đợt G

- **Đã làm:** biểu đồ Tổng quan (G2), cải thiện bảo mật/hiệu năng (G3), in phiếu thu (G4), giao diện tối và PWA (G5), nhắc nợ (G6), thời khóa biểu tuần (G8), tìm kiếm nhanh (G9), nhập Excel (G10).
- **Đã làm:** review đợt G được ghi tại `docs/web-review-G.md` (G7).
