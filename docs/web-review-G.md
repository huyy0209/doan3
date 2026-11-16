# Review đợt G

Rà soát tĩnh các thay đổi G1–G5 và F1 trên `web`; không chạy ứng dụng và không truy cập/ghi CSDL.

## Trung bình

1. **Trung bình — `DoAnWeb/Services/NhatKySaveChangesInterceptor.cs:129`, `DoAnWeb/Views/NhatKy/ChiTiet.cshtml:21`** — Bộ chặn ghi gần như mọi thuộc tính của bản ghi, chỉ bỏ qua tên có `MatKhau`, `Password` hoặc `Hash`; trang ChiTiet hiển thị nguyên các giá trị cũ/mới. Vì vậy các lần sửa/xóa Học viên, Gia sư, thanh toán lưu lâu thông tin như số điện thoại, địa chỉ, ghi chú và dữ liệu tài chính; người có vai trò `QuanLy` đều xem được. **Tái hiện:** đăng nhập quản lý, sửa địa chỉ/số điện thoại học viên hoặc ghi chú thanh toán, mở Nhật ký → Chi tiết; cả giá trị cũ lẫn mới hiện nguyên văn, kể cả sau khi dữ liệu gốc đã bị xóa. **Đề xuất:** chỉ ghi danh sách trường cần kiểm toán theo từng bảng, che/mã hóa trường liên hệ và giới hạn quyền/ thời hạn lưu chi tiết nhạy cảm.

2. **Trung bình — `DoAnWeb/Services/NhatKyService.cs:30-31`** — `TaoTrang` gọi `ToList()` trước khi `PhanTrang.Tao`, nên mỗi lần mở/lọc/chuyển trang đều tải toàn bộ bản ghi phù hợp vào bộ nhớ rồi mới cắt trang; lọc mặc định không giới hạn thời gian. Nhật ký được giữ tối đa một năm nên lượng ghi lớn sẽ làm trang quản trị chậm và tăng bộ nhớ theo số bản ghi. **Tái hiện:** tạo lượng lớn bản ghi nhật ký, mở `/NhatKy?trang=1`; truy vấn vẫn đọc/materialize tất cả kết quả của cả năm dù chỉ hiển thị một trang. **Đề xuất:** giữ `IQueryable`, đếm và áp dụng `Skip/Take` trong SQL; truy vấn các tùy chọn lọc bằng `Distinct` riêng như hiện tại.

3. **Trung bình — `DoAnWeb/Common/SoTien.cs:13-18`** — `TryParse` bỏ mọi ký tự không phải chữ số thay vì từ chối định dạng sai. Dấu âm hoặc ký tự lạ vì thế bị xóa và số tiền được ghi thành một giá trị dương khác, không báo lỗi cho người dùng. **Tái hiện:** gửi biểu mẫu ghi học phí với `ThanhToan.SoTien=-250.000`; chuỗi được đổi thành `250000` và khoản thu 250.000 đồng được chấp nhận. Tương tự `1,5` thành 15. **Đề xuất:** xác thực chuỗi theo các định dạng tiền được hỗ trợ (dấu phân nhóm hợp lệ, hậu tố `đ` tùy chọn), từ chối dấu âm/chữ thừa/nhóm dấu sai trước khi parse.
