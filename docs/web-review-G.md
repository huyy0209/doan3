# Review đợt G

Review gốc tĩnh các thay đổi F1/G1–G5; các sửa lỗi dưới đây đã được build và test trên CSDL test riêng.

## Trung bình — đã sửa

1. **Đã sửa — `DoAnWeb/Services/NhatKySaveChangesInterceptor.cs:129`, `DoAnWeb/Services/NhatKyService.cs:75` — commit `88d4f6e369374b21cfba8508d93a600148d652b6`.** Nhật ký từng ghi và hiển thị nguyên các giá trị liên hệ/định danh trong chi tiết. **Tái hiện trước khi sửa:** quản lý sửa địa chỉ hoặc số điện thoại học viên, mở Nhật ký → Chi tiết sẽ thấy giá trị cũ/mới đầy đủ. **Cách sửa:** các trường tên chứa `SoDienThoai`, `SDT`, `DienThoai`, `Email`, `DiaChi`, `CCCD`, `CMND` được che, chỉ giữ 3 ký tự cuối; Chi tiết cũng che bản ghi cũ đã có; các trường mật khẩu/hash tiếp tục bị bỏ qua.

2. **Đã sửa — `DoAnWeb/Services/NhatKyService.cs:26-57` — commit `7a99523d154872935ec789c6c7e72b9cd37fd7ae`.** Trang nhật ký từng nạp toàn bộ tập kết quả vào bộ nhớ trước khi phân trang và mặc định không giới hạn thời gian. **Tái hiện trước khi sửa:** mở `/NhatKy?trang=1` khi có nhiều bản ghi; trang chỉ hiển thị một trang nhưng truy vấn tải cả tập kết quả. **Cách sửa:** đếm và áp dụng sắp xếp, `Skip/Take` trên `IQueryable` trong SQL; mặc định lọc từ 29 ngày trước đến hôm nay (30 ngày lịch) nếu cả hai ngày đều chưa chọn.

3. **Đã sửa — `DoAnWeb/Common/SoTien.cs:13-58` — commit `daad582e08b5fc9ce055e452d23d64021b169798`.** Bộ đọc tiền từng xóa mọi ký tự ngoài chữ số nên chuỗi như `-250.000` hoặc `1,5` bị đổi thành khoản tiền dương khác. **Tái hiện trước khi sửa:** nhập chuỗi sai định dạng vào học phí lớp hoặc thanh toán; hệ thống có thể chấp nhận số đã bị đổi. **Cách sửa:** chỉ chấp nhận số nguyên dương với nhóm 3 chữ số dùng nhất quán `.`/`,`/khoảng trắng, hậu tố tùy chọn `d`/`đ`/`VND`; các biểu mẫu ghép lớp, sửa lớp và thu học phí hiển thị lỗi tiếng Việt ngay cạnh trường tiền. Đã thêm test cho các định dạng hợp lệ và sai theo yêu cầu.
