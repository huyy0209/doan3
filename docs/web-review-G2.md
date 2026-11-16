# Review vòng 2 đợt G (G6, G8, G9, G10, G11)

Đã đối chiếu các merge G6/G8/G9/G10/G11 trên nhánh `web` và kiểm tra luồng service/controller tương ứng. Mức: Cao / TB / Thấp. Mỗi mục nêu cách tái hiện và hướng xử lý; trạng thái sẽ được cập nhật cùng commit sửa.

| # | Mức | Vị trí | Tái hiện / tác động | Đề xuất | Trạng thái |
|---|---|---|---|---|---|
| 1 | Cao | `DoAnWeb/Services/NhapExcelService.cs:58` | Tải `.xlsx` có kích thước nén dưới 2 MB nhưng chứa một entry XML/ZIP bung ra rất lớn; `XLWorkbook` đọc trước khi kiểm tra dung lượng giải nén. Có thể làm tiêu thụ bộ nhớ CPU cao dù đã giới hạn byte upload và số hàng sau khi parse. | Kiểm tra archive trước khi mở workbook, giới hạn tổng dung lượng giải nén và số entry; thêm test upload ZIP bomb nhỏ nhưng có entry khai báo dung lượng vượt ngưỡng. **Đã sửa** – commit sẽ điền sau. |
| 2 | TB | `DoAnWeb/Services/NhapExcelService.cs:74` | Xem trước một file học viên trên CSDL lớn gọi `ToList()` toàn bộ entity `HocVien` chỉ để lấy hai số điện thoại; nạp cả địa chỉ/ghi chú và mọi bản ghi vào bộ nhớ. | Thu thập số điện thoại trong tệp trước, truy vấn có điều kiện theo các số này và chỉ chiếu hai cột điện thoại; giữ test trùng hiện có và bổ sung kiểm tra luồng xem trước. **Đã sửa** – commit sẽ điền sau. |
| 3 | Thấp | `DoAnWeb/Services/HomNayService.cs:69` | Gia sư tải khối Hôm nay gọi `GetLopTrongNgay` riêng cho từng ngày trong 7 ngày (cùng ngày hôm nay được tải hai lần), tạo nhiều lượt truy vấn theo mỗi lần render badge/trang. | Có thể lấy lịch 7 ngày trong một truy vấn rồi nhóm theo ngày nếu profiler cho thấy đây là điểm nghẽn. Chỉ ghi nhận, chưa sửa. |

Không thấy đường truy cập IDOR qua `maGS`/`id` hoặc `q` trong các luồng đã rà: lịch chi tiết lọc theo gia sư hiện tại; tìm kiếm giới hạn học viên/lớp theo quan hệ dạy và escape wildcard LIKE. Badge dùng khóa cache có mã tài khoản, vai trò và mã gia sư; số liệu nhắc nợ lấy công thức công nợ hiện có.
