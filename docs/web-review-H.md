# Review vòng 3: H1 giao diện điện thoại và H2 thông báo

Rà soát merge `45f879d` (H1) và `731587f` (H2) trên nhánh `web`, ngày 16/11/2026.

## Kết quả

Không phát hiện lỗi mức Cao hoặc Trung bình cần sửa; do đó không có commit sửa mã. Các mục đã rà soát (tái hiện/đối chiếu bằng code và test hiện có):

1. **Đạt — IDOR — `DoAnWeb/Services/ThongBaoService.cs:32,49,110,125`; `DoAnWeb.Tests/Integration/ThongBaoHttpTests.cs:29-46,60-73`.** Gửi ID thông báo của tài khoản khác khi đăng nhập gia sư; danh sách không hiện và POST không đổi trạng thái thông báo kia. Tiếp tục giữ điều kiện theo `MaTK` trong mọi truy vấn.
2. **Đạt — chuyển hướng — `DoAnWeb/Controllers/ThongBaoController.cs:40-47`; test `ThongBaoHttpTests.cs:47-58`.** Tạo thông báo có `DuongDan=https://example.invalid/` rồi bấm; ứng dụng đánh dấu của người hiện tại nhưng quay về `/ThongBao`, không chuyển ra ngoài. Duy trì kiểm tra `Url.IsLocalUrl` trước `LocalRedirect`.
3. **Đạt — antiforgery — `ThongBaoController.cs:38-51`; `Views/ThongBao/Index.cshtml:16-18,32-34`; `Views/Shared/Components/ThongBao/Default.cshtml:24-26`.** POST thiếu token bị middleware/filter từ chối; hai POST có attribute và các form phát token.
4. **Đạt — tác vụ nền khi CSDL lỗi — `DoAnWeb/Services/ThongBaoNen.cs:15-34`; `CapNhatTrangThaiNen.cs:25-39`.** Lỗi khi truy vấn được bắt và ghi log; vòng lặp vẫn chạy, không có vòng retry nóng.
5. **Đạt — trùng thông báo và transaction — `ThongBaoService.cs:132-133,176-196`; `LopHocService.cs:170-218,308-375,595-623`.** Gọi nhắc nợ hai lần trong ngày chỉ tạo một thông báo; tạo/sửa/hủy lớp và thông báo cùng commit hoặc rollback.
6. **Đạt — cache giữa người dùng và hiệu năng — `ThongBaoService.cs:14,25-40,234`.** Hai tài khoản có khóa cache khác nhau; menu lấy tối đa 10 mục, count chưa đọc, cache 20 giây và xóa cache sau thao tác ghi.
7. **Đạt — CSS máy tính — `DoAnWeb/wwwroot/css/site.css:1650-1733`.** Các quy tắc H1 kiểm tra ở viewport 768px trở lên không áp dụng; phần điều chỉnh điện thoại nằm trong media query tối đa 767.98px.
