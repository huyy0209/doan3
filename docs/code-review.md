# BÁO CÁO ĐÁNH GIÁ CHẤT LƯỢNG MÃ NGUỒN (CODE REVIEW)
**Dự án**: Phần mềm Quản lý Gia sư (DoAnWD)  
**Công nghệ**: C# WinForms (.NET Framework 4.8), Entity Framework 6.0, Microsoft SQL Server  
**Kiến trúc**: 3 tầng (UI → BLL → DAL) kết hợp Repository Pattern  
**Ngày đánh giá**: 14/11/2026  
**Trạng thái**: Hoàn thành đánh giá chi tiết toàn bộ mã nguồn  

---

## 1. TÓM TẮT TỔNG QUAN CHẤT LƯỢNG

### 1.1. Đánh giá chung
Dự án **DoAnWD** là ứng dụng desktop WinForms phục vụ quản lý trung tâm gia sư với quy mô vừa và nhỏ. Hệ thống đã triển khai được kiến trúc phân tầng UI → BLL → DAL và có nhiều nỗ lực cải tiến kỹ thuật đáng ghi nhận:
- **Tổ chức mã nguồn**: Cấu trúc thư mục tương đối rõ ràng (`UI/`, `BLL/`, `DAL/`, `Models/`, `Common/`), phân tách trách nhiệm giữa các tầng.
- **Bảo mật mật khẩu**: Triển khai băm mật khẩu PBKDF2 (HMAC-SHA256, salt 16-byte ngẫu nhiên, 100.000 vòng lặp, constant-time compare), có cơ chế khóa tạm sau 5 lần nhập sai.
- **An toàn truy vấn**: 100% truy vấn dữ liệu thông qua Entity Framework 6 (LINQ to Entities), không sử dụng nối chuỗi SQL động trong C#, loại bỏ triệt để nguy cơ SQL Injection trực tiếp từ ứng dụng.
- **Giao diện hiện đại**: Sử dụng bộ theme "Vở ô li" với double-buffering, custom controls (NavButton, ShiftRibbon, MonthGrid), trải nghiệm người dùng WinForms mượt mà.

Tuy nhiên, hệ thống còn tồn tại một số **lỗ hổng nghiêm trọng về tính toàn vẹn dữ liệu, giao dịch (transaction), kiểm tra xung đột lịch và phân quyền**:
1. **Thiếu Database Transaction**: Các thao tác nghiệp vụ phức tạp (ghép lớp, hủy lớp, sửa lớp, xóa gia sư) thực hiện nhiều lần `SaveChanges()` độc lập; nếu bước sau gặp lỗi, bước trước không được rollback, dẫn đến dữ liệu mồ côi hoặc sai lệch trạng thái.
2. **Khóa ngoại không được kiểm tra toàn diện**: Xóa gia sư chỉ kiểm tra lớp chủ nhiệm (`LopHoc`), bỏ qua các buổi dạy học bù (`BuoiHoc.MaGSDay`), gây crash ứng dụng do lỗi vi phạm FK Constraint từ SQL Server.
3. **Trùng lịch tiềm ẩn**: Thuật toán kiểm tra xung đột lịch (`TimXungDot`) chỉ kiểm tra bảng `LopHoc`, hoàn toàn bỏ qua các buổi học bù (`BuoiHoc`) đã xếp lịch cho gia sư hoặc học viên.
4. **Race Condition / Concurrency**: Không có cơ chế khóa (locking / isolation level Serializable / optimistic concurrency token) khi ghép lớp hoặc đóng học phí, dẫn đến nguy cơ trùng lịch và đóng vượt học phí nếu có nhiều thao tác đồng thời.
5. **Hiệu năng & N+1**: Tìm kiếm gia sư/học viên thực hiện kéo toàn bộ bảng về bộ nhớ (`_dbSet.ToList()`) trước khi lọc; một số hàm dùng `Load()` tường minh tạo ra mẫu truy vấn N+1.
6. **Rò rỉ logic nghiệp vụ vào UI**: Các Form giao diện (đặc biệt là `frmHocVien` và `frmChucNangGhepLop`) quá lớn (God Objects), tự tính toán lịch học, tự quản lý trạng thái nghiệp vụ.

---

## 2. BẢNG THỐNG KÊ SỐ LƯỢNG PHÁT HIỆN

| Mức độ nghiêm trọng | Định nghĩa | Số lượng phát hiện |
| :--- | :--- | :---: |
| 🔴 **Nghiêm trọng (Critical)** | Lỗi gây sai lệch dữ liệu vĩnh viễn, crash hệ thống, hoặc mất toàn vẹn giao dịch. | **4** |
| 🟠 **Cao (High)** | Lỗi logic nghiệp vụ quan trọng, lỗ hổng phân quyền, hoặc suy giảm hiệu năng nghiêm trọng. | **4** |
| 🟡 **Trung bình (Medium)** | Lỗi tiềm ẩn trong trường hợp biên (edge case), nuốt lỗi ngoại lệ, hoặc số liệu hiển thị chưa chính xác. | **6** |
| 🟢 **Thấp (Low)** | Code smell, code rác/dead code, vi phạm chuẩn thiết kế C# / CSDL cần refactor. | **5** |
| **TỔNG CỘNG** | | **19** |

---

## 3. DANH SÁCH CHI TIẾT CÁC PHÁT HIỆN

### 3.1. Mức độ: Nghiêm trọng (Critical)

---

#### [CRIT-01] Thiếu Database Transaction trong các thao tác nhiều bước gây bất nhất dữ liệu
- **Vị trí**: `DoAnWD/BLL/Services/LopHocService.cs:170-176`, `264-268`, `466-473`, `494-499`
- **Trạng thái**: [ĐÃ SỬA] Bọc các lần ghi + đồng bộ trạng thái gia sư trong một `DbContextTransaction` chung cho `GhepLop`, `SuaLop`, `HuyLop`, `Delete`.
- **Mô tả vấn đề**:
  Trong các phương thức `GhepLop`, `SuaLop`, `HuyLop` và `Delete` của `LopHocService`, hệ thống thực hiện nhiều thao tác ghi dữ liệu và gọi `SaveChanges()` riêng rẽ nhiều lần mà không bọc trong một Database Transaction:
  ```csharp
  // LopHocService.cs:170-175 (GhepLop)
  _lopHocRepo.Add(lopHoc);
  _lopHocRepo.SaveChanges(); // Lưu lớp học vào CSDL

  // 6. Đồng bộ trạng thái gia sư
  _giaSuRepo.UpdateStatusBasedOnLopHoc(y.MaGS);
  _giaSuRepo.SaveChanges(); // Lưu trạng thái gia sư vào CSDL
  ```
- **Tình huống gây lỗi cụ thể**:
  1. Khi người dùng ghép lớp mới (`GhepLop`), lệnh `_lopHocRepo.SaveChanges()` thực thi thành công, bản ghi `LopHoc` đã ghi vào SQL Server.
  2. Ngay sau đó, quá trình đồng bộ trạng thái gia sư (`_giaSuRepo.SaveChanges()`) bị ngắt đột ngột (mất kết nối mạng tới SQL Server, timeout, deadlock, hoặc lỗi bộ nhớ).
  3. Lớp học mới đã tồn tại nhưng trạng thái gia sư vẫn ở trạng thái cũ (ví dụ vẫn là "Rảnh" thay vì "Đang dạy" hoặc "Bận"). Hệ thống rơi vào trạng thái dữ liệu bất nhất vĩnh viễn.
  4. Tương tự trong `HuyLop`: lớp đã được đánh dấu `DaHuy` và xóa các buổi học bù tương lai, nhưng nếu bước cập nhật trạng thái gia sư thất bại thì gia sư vẫn bị giữ trạng thái "Đang dạy" dù không còn lớp nào hoạt động.
- **Đề xuất sửa chữa**:
  Do `_lopHocRepo` và `_giaSuRepo` cùng dùng chung một thể hiện `_context`, chỉ cần gọi `_context.SaveChanges()` MỘT LẦN duy nhất sau khi hoàn tất tất cả thay đổi trên cả hai entity. Hoặc bọc toàn bộ khối xử lý trong `DbContextTransaction`:
  ```csharp
  using (var tx = _context.Database.BeginTransaction())
  {
      try
      {
          _lopHocRepo.Add(lopHoc);
          _giaSuRepo.UpdateStatusBasedOnLopHoc(y.MaGS);
          _context.SaveChanges();
          tx.Commit();
      }
      catch
      {
          tx.Rollback();
          throw;
      }
  }
  ```

---

#### [CRIT-02] Crash ứng dụng do vi phạm Foreign Key khi xóa gia sư từng dạy học bù
- **Vị trí**: `DoAnWD/BLL/Services/GiaSuService.cs:111-135` và `DoAnWD/DAL/Repositories/GiaSuRepository.cs:84-88`
- **Trạng thái**: [ĐÃ SỬA] `GiaSuService.Delete` kiểm tra thêm `BuoiHoc.MaGSDay` trước khi xóa, chặn xóa và gợi ý chuyển sang "Tạm nghỉ".
- **Mô tả vấn đề**:
  Trong script `04_NangCap_v4.sql` (dòng 19), cột `MaGSDay` trong bảng `BuoiHoc` được tạo với khóa ngoại:
  ```sql
  ALTER TABLE dbo.BuoiHoc ADD MaGSDay INT NULL CONSTRAINT FK_BuoiHoc_GiaSuDay REFERENCES dbo.GiaSu(MaGS);
  ```
  Tuy nhiên, hàm `GiaSuService.Delete(int maGS)` chỉ kiểm tra ràng buộc thông qua `CountAllLopHoc(maGS)`. Hàm này chỉ kiểm tra số lượng lớp mà gia sư là chủ nhiệm (`giaSu.LopHocs.Count`), hoàn toàn KHÔNG kiểm tra bảng `BuoiHoc` xem gia sư đó có xuất hiện trong các buổi dạy học bù (`MaGSDay == maGS`) hay không.
- **Tình huống gây lỗi cụ thể**:
  1. Gia sư Trần Thị B là giáo viên mới, chưa được phân công lớp chủ nhiệm nào (`LopHocs.Count == 0`).
  2. Lớp Toán của gia sư Nguyễn Văn A có học viên vắng, quản lý xếp buổi học bù và chỉ định gia sư Trần Thị B dạy buổi bù đó (`BuoiHoc.MaGSDay = B`).
  3. Sau đó, quản lý quyết định xóa hồ sơ gia sư Trần Thị B khỏi hệ thống.
  4. Hàm `GiaSuService.Delete` chạy: `HasActiveLopHoc` trả về `false`, `CountAllLopHoc` trả về `0`. Lệnh `_giaSuRepo.Remove(giaSu)` được gọi.
  5. Khi gọi `_giaSuRepo.SaveChanges()`, SQL Server từ chối xóa và ném lỗi `SqlException: The DELETE statement conflicted with the REFERENCE constraint "FK_BuoiHoc_GiaSuDay"...`. Ứng dụng ném lỗi chưa được xử lý phù hợp và thao tác bị thất bại.
- **Đề xuất sửa chữa**:
  Trong `GiaSuService.Delete`, bổ sung kiểm tra sự tồn tại của các buổi học bù do gia sư giảng dạy trước khi cho phép xóa:
  ```csharp
  var ctx = _giaSuRepo.Context;
  bool daDayBu = ctx.BuoiHocs.Any(b => b.MaGSDay == maGS);
  if (daDayBu)
  {
      throw new InvalidOperationException(
          $"Không thể xóa!\n\nGia sư '{giaSu.HoTen}' đã từng tham gia dạy các buổi học bù trong hệ thống.\n\n" +
          $"Hãy chuyển trạng thái sang \"{Constants.GiaSuStatus.TamNghi}\" để lưu giữ lịch sử giảng dạy.");
  }
  ```

---

#### [CRIT-03] Race Condition dẫn đến trùng lịch gia sư/học viên khi ghép lớp đồng thời
- **Vị trí**: `DoAnWD/BLL/Services/LopHocService.cs:148-172`
- **Trạng thái**: Đã kiểm chứng trong mã nguồn
- **Mô tả vấn đề**:
  Hàm `GhepLop` thực hiện kiểm tra xung đột lịch thông qua `TimXungDot` (đọc dữ liệu hiện có trong DB), sau đó mới thực hiện `_lopHocRepo.Add(lopHoc)` và `SaveChanges()`.
  Cơ sở dữ liệu SQL Server không có ràng buộc Unique Constraint hay Exclusion Constraint nào đối với bộ `(MaGS, CaHoc, ThoiGianBatDau, ThoiGianKetThuc)`.
- **Tình huống gây lỗi cụ thể**:
  1. Quản lý 1 và Quản lý 2 cùng thao tác ghép lớp trên hai máy trạm khác nhau (hoặc hai phiên làm việc).
  2. Cả hai người cùng chọn gia sư Nguyễn Văn An cho ca Tối (18h-20h), cùng khoảng ngày từ 01/12 đến 31/12, các thứ 2-4-6.
  3. Cả hai luồng cùng chạy `TimXungDot`: tại thời điểm này, chưa có bản ghi nào được commit nên cả hai đều nhận kết quả `null` (không có xung đột).
  4. Cả hai luồng cùng gọi `_lopHocRepo.Add` và `SaveChanges()`. Cả hai bản ghi đều được thêm thành công vào bảng `LopHoc`.
  5. Hậu quả: Gia sư Nguyễn Văn An bị gán trùng 2 lớp học cùng một ca trong cùng một khung giờ, vi phạm quy tắc nghiệp vụ cốt lõi của trung tâm.
- **Đề xuất sửa chữa**:
  - Ở tầng CSDL: Xem xét bổ sung kiểm tra xung đột qua trigger kiểm soát lịch hoặc sử dụng mức cô lập giao dịch `IsolationLevel.Serializable` trong transaction ghép lớp.
  - Ở tầng ứng dụng: Dùng `sp_getapplock` trong SQL Server để tạo lock độc quyền trên mã gia sư/học viên trong suốt quá trình từ lúc kiểm tra trùng lịch đến khi commit lớp học.

---

#### [CRIT-04] Race Condition dẫn đến thu vượt công nợ (Overpayment) khi ghi nhận học phí đồng thời
- **Vị trí**: `DoAnWD/BLL/Services/HocPhiService.cs:97-113`
- **Trạng thái**: Đã kiểm chứng trong mã nguồn
- **Mô tả vấn đề**:
  Hàm `ThemThanhToan` tính toán số tiền còn nợ hiện tại:
  `decimal conNo = (lop.HocPhi ?? 0) - lop.ThanhToans.Sum(t => t.SoTien);`
  Sau đó kiểm tra `if (soTien > conNo) throw ...;` rồi mới thêm bản ghi `ThanhToan`. Quá trình này không sử dụng khóa bản ghi (record locking) hay Optimistic Concurrency Control (RowVersion).
- **Tình huống gây lỗi cụ thể**:
  1. Lớp học có học phí 2.000.000đ, học viên đã đóng 1.000.000đ, số còn nợ là 1.000.000đ.
  2. Phụ huynh thanh toán chuyển khoản và cùng lúc một người nhà đến trung tâm nộp tiền mặt. Hai nhân viên thu ngân cùng mở form đóng tiền và nhập 1.000.000đ.
  3. Cả hai luồng cùng truy vấn `lop.ThanhToans.Sum(t => t.SoTien)` và cùng nhận kết quả đã đóng 1.000.000đ -> `conNo = 1.000.000đ`.
  4. Cả hai lần kiểm tra `soTien <= conNo` đều hợp lệ. Cả hai lệnh `db.ThanhToans.Add(tt)` đều `SaveChanges()` thành công.
  5. Hậu quả: Tổng số tiền đóng thành 2.000.000đ + 1.000.000đ = 3.000.000đ, vượt quá học phí 1.000.000đ. Công nợ bị âm và sai lệch báo cáo tài chính.
- **Đề xuất sửa chữa**:
  Bổ sung cột `RowVersion` (kiểu `TIMESTAMP` trong SQL Server) vào bảng `LopHoc` để EF tự động kích hoạt kiểm tra Optimistic Concurrency, hoặc thực hiện giao dịch với khóa cập nhật (`UPDLOCK`) khi đọc thông tin thanh toán của lớp.

---

### 3.2. Mức độ: Cao (High)

---

#### [HIGH-01] Hàm kiểm tra trùng lịch `TimXungDot` bỏ qua hoàn toàn các buổi học bù
- **Vị trí**: `DoAnWD/BLL/Services/LopHocService.cs:336-377`
- **Trạng thái**: [ĐÃ SỬA] `TimXungDot` truy vấn thêm `BuoiHoc` (buổi học bù) theo gia sư/học viên, ca và khoảng ngày để phát hiện xung đột.
- **Mô tả vấn đề**:
  Phương thức `TimXungDot` chỉ truy vấn bảng `LopHoc` thông qua `_lopHocRepo.GetLopHoatDongCuaGiaSuHoacHocVien(maGS, maHV)`. Nó hoàn toàn không truy vấn bảng `BuoiHoc` để kiểm tra các buổi học bù đã được xếp lịch trước đó (`MaBuoiGoc IS NOT NULL`).
- **Tình huống gây lỗi cụ thể**:
  1. Gia sư Lê Minh Cường được xếp dạy một buổi học bù vào Chiều muộn (16h-18h) thứ Sáu ngày 27/11 (lưu trong bảng `BuoiHoc`).
  2. Một tuần sau, quản lý mở form ghép lớp mới cho gia sư Cường với học viên khác, lịch học Chiều muộn (16h-18h) thứ 3-5-6 từ 20/11 đến 31/12.
  3. `TimXungDot` chỉ kiểm tra các lớp trong bảng `LopHoc`, thấy gia sư Cường chưa có lớp chính khóa nào vào giờ đó nên kết luận hợp lệ.
  4. Lớp mới được tạo thành công. Đến ngày 27/11, gia sư Cường vừa phải dạy buổi thường của lớp mới, vừa phải dạy buổi học bù của lớp cũ trong cùng một ca.
- **Đề xuất sửa chữa**:
  Trong `TimXungDot`, truy vấn thêm các bản ghi `BuoiHoc` của gia sư (`MaGSDay == maGS`) và học viên (`b.LopHoc.MaHV == maHV`) có `MaBuoiGoc != null`, nằm trong khoảng ngày `[batDau, ketThuc]` và trùng `CaHoc` để cảnh báo xung đột.

---

#### [HIGH-02] Lỗ hổng kiểm tra quyền ở tầng BLL khi `PhienDangNhap` chưa đăng nhập hoặc phụ thuộc tham số từ UI
- **Vị trí**: `DoAnWD/BLL/Services/TaiKhoanService.cs:95-99` và `DoAnWD/BLL/Services/HocTapService.cs:219, 240, 536, 592`
- **Trạng thái**: [ĐÃ SỬA] `PhienDangNhap.YeuCauQuanLy` chặn cả khi chưa đăng nhập; `HocTapService` tự đọc `PhienDangNhap.GioiHanGiaSu` thay vì tin tham số `chiChoGiaSu` từ UI.
- **Mô tả vấn đề**:
  1. `PhienDangNhap.YeuCauQuanLy()` được cài đặt như sau:
     ```csharp
     public static void YeuCauQuanLy()
     {
         if (DaDangNhap && !LaQuanLy)
             throw new UnauthorizedAccessException("Chỉ quản lý mới được thực hiện thao tác này.");
     }
     ```
     Nếu `DaDangNhap == false` (chưa thiết lập phiên đăng nhập), hàm này **cho qua** mà không ném lỗi.
  2. Trong `HocTapService`, các hàm nhạy cảm (`LuuCacBuoi`, `XepHocBu`, `HuyHocBu`) nhận tham số kiểm tra quyền `int? chiChoGiaSu = null`. BLL không tự xác thực người dùng hiện tại mà tin tưởng hoàn toàn vào tham số do tầng UI truyền xuống.
- **Tình huống gây lỗi cụ thể**:
  1. Nếu tầng UI bị lỗi hoặc có một đoạn code gọi dịch vụ mà quên truyền `chiChoGiaSu` (mặc định là `null`), một tài khoản có vai trò "Gia sư" có thể chỉnh sửa điểm danh, sửa điểm bài test, hoặc xóa lịch học bù của học viên thuộc về lớp của gia sư khác.
  2. Nếu một phương thức dịch vụ được gọi ngoài luồng đăng nhập (ví dụ qua API mở rộng trong tương lai hoặc background task), nó sẽ mặc nhiên chạy với quyền quản trị vì `DaDangNhap == false`.
- **Đề xuất sửa chữa**:
  - Sửa `PhienDangNhap.YeuCauQuanLy()` thành: `if (!DaDangNhap || !LaQuanLy) throw new UnauthorizedAccessException(...);`
  - Trong `HocTapService`, loại bỏ việc nhận `chiChoGiaSu` từ tham số bên ngoài; thay vào đó, trực tiếp đọc `PhienDangNhap.GioiHanGiaSu` ngay tại tầng nghiệp vụ.

---

#### [HIGH-03] Tiết lộ tài khoản tồn tại (User Enumeration) qua thông báo lỗi đăng nhập
- **Vị trí**: `DoAnWD/BLL/Services/TaiKhoanService.cs:176-180`
- **Trạng thái**: [ĐÃ SỬA] Mọi trường hợp đăng nhập sai (sai tài khoản, sai mật khẩu, đang tạm khóa) đều trả cùng một câu "Sai tên đăng nhập hoặc mật khẩu.".
- **Mô tả vấn đề**:
  Trong `DangNhap`:
  - Nếu tên đăng nhập không tồn tại: ném `UnauthorizedAccessException("Sai tên đăng nhập hoặc mật khẩu.")`.
  - Nếu tên đăng nhập tồn tại nhưng sai mật khẩu từ lần 3 trở lên: ném `"Sai tên đăng nhập hoặc mật khẩu. Còn 2 lần thử."` hoặc `"Nhập sai 5 lần liên tiếp. Tài khoản tạm khóa 5 phút."`
- **Tình huống gây lỗi cụ thể**:
  Kẻ tấn công không cần biết mật khẩu, chỉ cần thử danh sách các tên đăng nhập phổ biến (`admin`, `manager`, `gia_su_lan`...). Nếu phản hồi có chuỗi "Còn X lần thử" hoặc "Tài khoản tạm khóa", kẻ tấn công biết chính xác tài khoản đó tồn tại trên hệ thống để phục vụ cho các đợt tấn công tiếp theo.
- **Đề xuất sửa chữa**:
  Luôn trả về thông báo lỗi chuẩn `Constants.Messages.SaiDangNhap` ("Sai tên đăng nhập hoặc mật khẩu.") để tránh rò rỉ thông tin người dùng.

---

#### [HIGH-04] Kéo toàn bộ bảng về bộ nhớ (`_dbSet.ToList()`) gây nghẽn hiệu năng khi tìm kiếm
- **Vị trí**: `DoAnWD/DAL/Repositories/GiaSuRepository.cs:142`, `DoAnWD/DAL/Repositories/HocVienRepository.cs:22`
- **Trạng thái**: [ĐÃ SỬA] `Search` lọc thô bằng SQL (collation không phân biệt dấu, tham số hóa) trước khi nạp về RAM và chuẩn hóa tiếng Việt.
- **Mô tả vấn đề**:
  Hàm `Search(string keyword)` của cả `GiaSuRepository` và `HocVienRepository` đều thực thi:
  ```csharp
  // GiaSuRepository.cs:142
  return _dbSet.ToList().Where(g => TimKiem.Khop(keyword, g.HoTen, g.SDT, g.ChuyenMon, g.TrangThai)).ToList();
  ```
  Phương thức `_dbSet.ToList()` buộc Entity Framework gửi câu truy vấn `SELECT * FROM GiaSu` (hoặc `HocVien`), tải toàn bộ dữ liệu của tất cả các dòng về bộ nhớ máy trạm, sau đó mới áp dụng bộ lọc `TimKiem.Khop`.
- **Tình huống gây lỗi cụ thể**:
  Trong các form `frmGiaSu` và `frmHocVien`, sự kiện `txtTim.TextChanged` kích hoạt tìm kiếm liên tục mỗi khi người dùng gõ phím. Khi dữ liệu đạt hàng nghìn bản ghi, mỗi phím gõ sẽ kéo toàn bộ bảng qua mạng, gây giật lag giao diện, nghẽn băng thông CSDL và làm tăng đột biến dung lượng RAM tiêu thụ.
- **Đề xuất sửa chữa**:
  1. Ở mức ngắn hạn: Thêm debounce timer cho ô tìm kiếm (chỉ tìm sau khi người dùng dừng gõ 300ms).
  2. Ở mức kiến trúc: Thực hiện lọc sơ bộ bằng câu lệnh SQL LINQ `IQueryable` trước (ví dụ `g.HoTen.Contains(keyword) || g.SDT.Contains(keyword)`), sau đó chỉ nạp các kết quả tiềm năng về RAM để chuẩn hóa tiếng Việt.
  3. Ở mức tối ưu: Thêm cột `HoTenKhongDau` được tính toán tự động trong SQL Server và đánh chỉ mục Non-Clustered Index.

---

### 3.3. Mức độ: Trung bình (Medium)

---

#### [MED-01] Dead Code và lỗ hổng kiểm tra null trong `LopHoc.TrungLich`
- **Vị trí**: `DoAnWD/Models/LopHoc.cs:238-240`
- **Trạng thái**: Đã kiểm chứng trong mã nguồn
- **Mô tả vấn đề**:
  Đoạn code trong `TrungLich`:
  ```csharp
  if (!TrungCaHoc(other) || !TrungThoiGian(other)) return false; // Dòng 238
  if (!ThoiGianBatDau.HasValue || !ThoiGianKetThuc.HasValue ||
      !other.ThoiGianBatDau.HasValue || !other.ThoiGianKetThuc.HasValue) return true; // Dòng 239-240
  ```
  Trong hàm `TrungThoiGian(other)` (dòng 210-212), nếu bất kỳ giá trị thời gian nào của 2 lớp là `null`, nó lập tức trả về `false`.
  Điều này khiến `!TrungThoiGian(other)` ở dòng 238 luôn là `true`, và hàm `TrungLich` đã lập tức `return false`.
  Do đó, khối code ở dòng 239-240 (`return true` khi thiếu thời gian) là **Dead Code hoàn toàn không bao giờ được thực thi**.
  Hậu quả phụ: Nếu một lớp học cũ trong CSDL bị thiếu ngày bắt đầu/kết thúc (cột cho phép `NULL` theo schema `01_QuanLyGiaSu.sql`), hàm `TrungLich` sẽ coi như không trùng lịch với bất kỳ lớp nào, dẫn đến bỏ lọt việc kiểm tra trùng ca học.
- **Đề xuất sửa chữa**:
  Xóa bỏ dòng dead code 239-240. Đối với các lớp thiếu thời gian, quy định rõ hành vi: nếu cùng ca học mà thiếu ngày thì mặc định coi là có nguy cơ trùng lịch.

---

#### [MED-02] Hiển thị ngày mặc định `01/01` trong `GetLopTrongNgay` khi mất liên kết buổi gốc
- **Vị trí**: `DoAnWD/BLL/Services/HocTapService.cs:163, 170`
- **Trạng thái**: [ĐÃ SỬA] Kiểm tra kết quả `TryGetValue`; thiếu buổi gốc thì ghi "Học bù buổi không rõ" thay vì ngày mặc định 01/01.
- **Mô tả vấn đề**:
  Khi lấy danh sách học viên đến học bù:
  ```csharp
  DateTime goc;
  ngayGoc.TryGetValue(b.MaBuoiGoc.Value, out goc);
  // ...
  GhiChu = $"Học bù buổi {goc:dd/MM}"
  ```
  Nếu bản ghi buổi học gốc bị xóa hoặc id `MaBuoiGoc` không tìm thấy trong từ điển `ngayGoc`, biến `goc` không được gán và giữ giá trị mặc định `default(DateTime)` (`01/01/0001`).
- **Tình huống gây lỗi cụ thể**:
  Giao diện điểm danh hiển thị dòng chữ gây hiểu nhầm: `Học bù buổi 01/01`, khiến người dùng lầm tưởng học viên đang bù cho ngày Tết Dương lịch.
- **Đề xuất sửa chữa**:
  Kiểm tra giá trị trả về của `TryGetValue`:
  ```csharp
  string textNgayGoc = ngayGoc.TryGetValue(b.MaBuoiGoc.Value, out goc) ? goc.ToString("dd/MM") : "không rõ";
  GhiChu = $"Học bù buổi {textNgayGoc}";
  ```

---

#### [MED-03] Cách tính doanh thu phản ánh sai dòng tiền thực tế
- **Vị trí**: `DoAnWD/BLL/Services/ThongKeService.cs:67-73`
- **Trạng thái**: Đã kiểm chứng trong mã nguồn
- **Mô tả vấn đề**:
  Hàm `GetStatistics` tính doanh thu như sau:
  ```csharp
  DoanhThuDangHoatDong = _context.LopHocs
      .Where(l => hoatDong.Contains(l.TrangThai))
      .Sum(l => l.HocPhi) ?? 0,
  DoanhThuThangNay = _context.LopHocs
      .Where(l => l.TrangThai != Constants.LopHocStatus.DaHuy &&
                  l.ThoiGianBatDau >= dauThang && l.ThoiGianBatDau < dauThangSau)
      .Sum(l => l.HocPhi) ?? 0
  ```
  Hệ thống tính "doanh thu" hoàn toàn dựa trên học phí danh nghĩa của lớp học (`LopHoc.HocPhi`), thay vì số tiền thực tế phụ huynh đã nộp qua bảng `ThanhToan`.
- **Tình huống gây lỗi cụ thể**:
  Một lớp học khai giảng trong tháng này có học phí 5.000.000đ nhưng phụ huynh chưa nộp đồng nào (còn nợ 100%), thống kê vẫn báo "Doanh thu tháng này: 5.000.000đ". Ngược lại, phụ huynh nộp 10.000.000đ học phí cho các khóa học tương lai trong tháng này thì số tiền thực thu đó lại không được tính vào doanh thu tháng này.
- **Đề xuất sửa chữa**:
  Tách bạch hai chỉ số trên báo cáo:
  1. "Tổng giá trị lớp học khai giảng" (tính theo `LopHoc.HocPhi`).
  2. "Thực thu trong tháng" (tính theo `_context.ThanhToans.Where(t => t.NgayDong >= dauThang && t.NgayDong < dauThangSau).Sum(t => t.SoTien)`).

---

#### [MED-04] Tính sai công nợ và hiển thị số tiền còn nợ cho lớp đã bị hủy
- **Vị trí**: `DoAnWD/BLL/Services/HocPhiService.cs:43, 61`
- **Trạng thái**: Đã kiểm chứng trong mã nguồn
- **Mô tả vấn đề**:
  Trong `HocPhiService.GetCongNo()`:
  - Câu query lọc: `.Where(x => !(x.TrangThai == huy && x.DaDong == 0))`
  - Nếu lớp đã hủy (`DaHuy`) nhưng phụ huynh trước đó đã nộp một phần tiền (ví dụ học phí 2.000.000đ, đã nộp 500.000đ), lớp này vẫn được lấy ra.
  - Công nợ được tính: `ConNo = Math.Max(0, hp - x.DaDong) = 1.500.000đ`.
- **Tình huống gây lỗi cụ thể**:
  Học viên hủy lớp giữa chừng và không tiếp tục học. Trên trang Quản lý Học phí, lớp này vẫn hiện "Còn nợ: 1.500.000đ" và trạng thái "Còn nợ", khiến nhân viên trung tâm nhầm tưởng học viên vẫn đang nợ tiền học và gọi điện đòi nợ sai quy định.
- **Đề xuất sửa chữa**:
  Với lớp có trạng thái `DaHuy`, gán `ConNo = 0` và tình trạng là "Đã hủy" (hoặc chỉ ghi nhận phần chênh lệch nếu trung tâm có quy định hoàn tiền/thu phí hủy).

---

#### [MED-05] Nuốt lỗi ngoại lệ (Swallowed Exceptions) ở các Service nền và Timer
- **Vị trí**: `DoAnWD/BLL/Services/NotificationService.cs:72, 89, 104`, `DoAnWD/BLL/Services/CaiDatService.cs:39`, `DoAnWD/UI/TrangChu.cs:266, 340, 346, 353, 453`, `DoAnWD/UI/Pages/frmHocVien.BuoiHoc.cs:300`
- **Trạng thái**: Đã kiểm chứng trong mã nguồn
- **Mô tả vấn đề**:
  Nhiều khối `catch` bắt ngoại lệ nhưng bỏ trống hoặc chỉ ghi ra `Debug.WriteLine`. Khi biên dịch ứng dụng ở chế độ Release, các lệnh `Debug.WriteLine` hoàn toàn bị trình biên dịch loại bỏ (strip out), khiến các lỗi xảy ra trong quá trình chạy thực tế biến mất không dấu vết.
- **Tình huống gây lỗi cụ thể**:
  - Trong `CaiDatService.cs:39`: nếu kết nối CSDL bị lỗi, hàm `catch` ngầm trả về `new ThongTinTrungTam()` rỗng mà không có bất kỳ cảnh báo nào.
  - Trong `TrangChu.cs:266`: timer tự động cập nhật trạng thái lớp mỗi phút nuốt toàn bộ ngoại lệ kết nối DB, quản lý không hề biết tiến trình cập nhật nền đã dừng hoạt động.
- **Đề xuất sửa chữa**:
  Triển khai logging tập trung (sử dụng thư viện log hoặc ghi file text đơn giản trong thư mục dữ liệu ứng dụng) và hiển thị chỉ báo trạng thái kết nối trên thanh status bar khi có lỗi nền.

---

#### [MED-06] DbContext Lifetime không nhất quán và nguy cơ rò rỉ bộ nhớ
- **Vị trí**: `DoAnWD/UI/Pages/frmCalendarView.cs:31-32, 197-201`, `DoAnWD/UI/Dialogs/frmChucNangGhepLop.cs:27`, `DoAnWD/UI/TrangChu.cs:382`
- **Trạng thái**: Đã kiểm chứng trong mã nguồn
- **Mô tả vấn đề**:
  Một số Form như `frmCalendarView` khởi tạo `LopHocService` và `GiaSuService` làm biến thành viên private (`new LopHocService()`). Bản thân `LopHocService` lại giữ một thể hiện `GiaSuContextDB` cố định.
  Trong khi đó, `TrangChu.cs` lưu trữ các Form con trong từ điển `_trang` và không bao giờ giải phóng các Form này trong suốt thời gian ứng dụng chạy (`Show()` và `Hide()`).
  Điều này dẫn đến việc DbContext của `frmCalendarView` sống cùng tuổi thọ với ứng dụng (long-lived DbContext).
- **Tình huống gây lỗi cụ thể**:
  1. Khi dữ liệu lớp học được cập nhật ở màn hình khác (sử dụng DbContext ngắn hạn riêng), DbContext của `frmCalendarView` không tự động nhận biết các thay đổi của những entity đã được nó cache trước đó.
  2. DbContext tồn tại lâu ngày sẽ tích tụ cache các entity được load qua các tháng, gây rò rỉ bộ nhớ (memory leak).
- **Đề xuất sửa chữa**:
  Chuyển sang mô hình DbContext per-operation (mỗi khi `LoadCalendar()` thì tạo và giải phóng Service/DbContext bằng khối `using`), tương tự cách làm chuẩn xác đã áp dụng trong `HocPhiService` và `HocTapService`.

---

### 3.4. Mức độ: Thấp (Low)

---

#### [LOW-01] Kiến trúc Form quá lớn (God Form) và rò rỉ logic nghiệp vụ vào UI
- **Vị trí**: `DoAnWD/UI/Pages/frmHocVien.cs` (gồm 3 partial files: `frmHocVien.cs`, `BuoiHoc.cs`, `DiemDanh.cs`, tổng 1.167 dòng) và `DoAnWD/UI/Dialogs/frmChucNangGhepLop.cs` (637 dòng)
- **Trạng thái**: Đã kiểm chứng trong mã nguồn
- **Mô tả vấn đề**:
  Các form này vi phạm nguyên tắc Single Responsibility. Ví dụ trong `frmChucNangGhepLop.cs`: form vừa chịu trách nhiệm vẽ giao diện, vừa tính toán thời gian bắt đầu/kết thúc ca (`LayKhoang`), vừa kiểm tra điều kiện trùng lịch in-memory (`LopTrung`), vừa gợi ý tên lớp (`GoiYTenLop`).
- **Đề xuất sửa chữa**:
  Tách các logic tính toán và kiểm tra sang tầng BLL (`LopHocService`). Các UserControl nên được phân rã thành các component độc lập.

---

#### [LOW-02] Bảng `LopHoc` lưu tên môn học dạng chuỗi thay vì khóa ngoại `MaMon`
- **Vị trí**: `DoAnWD/Models/LopHoc.cs:28`, `Database/01_QuanLyGiaSu.sql:51`, `Database/05_NangCap_v5.sql:23-32`
- **Trạng thái**: Đã kiểm chứng trong mã nguồn
- **Mô tả vấn đề**:
  Dù bản nâng cấp v5 đã tạo bảng danh mục `MonHoc` với khóa chính `MaMon INT`, bảng `LopHoc` vẫn tiếp tục lưu `MonHoc NVARCHAR(50)` dạng chuỗi thuần túy không có khóa ngoại.
  Khi đổi tên môn học trong `MonHocService.Luu`, hệ thống phải thực hiện quét toàn bộ bảng `LopHoc` và `GiaSu` để cập nhật chuỗi thủ công:
  ```csharp
  // MonHocService.cs:98-99
  foreach (var l in db.LopHocs.Where(l => l.MonHoc == cu).ToList()) l.MonHoc = ten;
  foreach (var g in db.GiaSus.Where(g => g.ChuyenMon == cu).ToList()) g.ChuyenMon = ten;
  ```
- **Tình huống gây lỗi cụ thể**:
  Nếu một gia sư có chuyên môn là "Toán, Lý", lệnh `g.ChuyenMon == cu` sẽ không khớp và chuyên môn gia sư không được cập nhật theo tên môn mới. Đồng thời, tên lớp cũ (`TenLop`) đã được sinh ra trước đó vẫn giữ tên môn cũ, gây bất nhất thông tin.
- **Đề xuất sửa chữa**:
  Bổ sung cột khóa ngoại `MaMon INT NULL CONSTRAINT FK_LopHoc_MonHoc REFERENCES dbo.MonHoc(MaMon)` trong bảng `LopHoc`.

---

#### [LOW-03] Không dọn dẹp các tệp tin HTML tạm sinh ra khi xuất phiếu báo
- **Vị trí**: `DoAnWD/BLL/Services/BaoCaoService.cs:113-118`
- **Trạng thái**: Đã kiểm chứng trong mã nguồn
- **Mô tả vấn đề**:
  Mỗi lần người dùng bấm "Phiếu báo kết quả", phương thức `TaoPhieuBao` sinh một file HTML ngẫu nhiên trong thư mục `%TEMP%\QuanLyGiaSu\` với tên chứa timestamp (`PhieuBao_..._yyyyMMdd_HHmm.html`) và mở trình duyệt.
  Hệ thống không có bất kỳ cơ chế nào để xóa các file tạm này sau khi sử dụng.
- **Đề xuất sửa chữa**:
  Thêm phương thức tự động xóa các file tạm có thời gian tạo cũ hơn 3 ngày trong thư mục `%TEMP%\QuanLyGiaSu\` khi khởi động ứng dụng (`Program.cs`).

---

#### [LOW-04] Thiếu chỉ mục UNIQUE cho cột `TaiKhoan.MaGS` trong CSDL
- **Vị trí**: `Database/02_NangCap_v2.sql:63-74`, `DoAnWD/BLL/Services/TaiKhoanService.cs:264`
- **Trạng thái**: [ĐÃ SỬA] Thêm `Database/08_NangCap_v8.sql` tạo filtered unique index `UX_TaiKhoan_MaGS` (chạy lại được, báo lỗi nếu dữ liệu đang trùng `MaGS`).
- **Mô tả vấn đề**:
  Nghiệp vụ quy định mỗi gia sư chỉ được cấp tối đa một tài khoản đăng nhập.
  Tầng BLL có kiểm tra `if (maGS.HasValue && db.TaiKhoans.Any(t => t.MaGS == maGS.Value)) throw ...;`
  Tuy nhiên, trong CSDL script `02_NangCap_v2.sql`, bảng `TaiKhoan` chỉ có ràng buộc Unique cho `TenDangNhap`, hoàn toàn không có Unique Index cho `MaGS`.
- **Đề xuất sửa chữa**:
  Bổ sung chỉ mục có điều kiện trong script CSDL:
  ```sql
  CREATE UNIQUE INDEX UX_TaiKhoan_MaGS ON dbo.TaiKhoan(MaGS) WHERE MaGS IS NOT NULL;
  ```

---

#### [LOW-05] Sử dụng `throw new Exception` thay vì Custom/Specific Exception
- **Vị trí**: `DoAnWD/BLL/Services/GiaSuService.cs:108`, `DoAnWD/BLL/Services/HocVienService.cs:75`
- **Trạng thái**: Đã kiểm chứng trong mã nguồn
- **Mô tả vấn đề**:
  Trong phương thức `Delete` của `GiaSuService` và `HocVienService`:
  `throw new Exception("Không tìm thấy gia sư.");`
  Việc ném lớp ngoại lệ cơ sở `System.Exception` là vi phạm chuẩn thiết kế C# (CA1065 / CA2201), làm cho tầng gọi UI khó phân biệt giữa lỗi nghiệp vụ (entity không tồn tại) và các lỗi nghiêm trọng ngoài dự kiến (Out of memory, Null Reference...).
- **Đề xuất sửa chữa**:
  Đổi sang ném `KeyNotFoundException` hoặc `InvalidOperationException`.

---

## 4. BẢNG TỔNG HỢP THEO CÁC TRỌNG TÂM ĐÁNH GIÁ

| Lĩnh vực trọng tâm | Tình trạng đánh giá | Các phát hiện liên quan |
| :--- | :--- | :--- |
| **Transaction & Toàn vẹn dữ liệu** | ⚠️ Báo động: Nhiều thao tác đa bước không có DbTransaction, nguy cơ dữ liệu mồ côi cao | CRIT-01, CRIT-02 |
| **Concurrency & Race Condition** | ⚠️ Báo động: Không có kiểm soát đồng thời khi xếp lịch và đóng học phí | CRIT-03, CRIT-04 |
| **Kiểm tra trùng lịch (Schedule Collision)** | ⚠️ Có lỗi logic: Bỏ sót toàn bộ các buổi học bù trong `TimXungDot` | HIGH-01, MED-01 |
| **Bảo mật & Phân quyền** | ⚠️ Khá tốt về hash mật khẩu (PBKDF2), nhưng hở user enumeration và lỏng phân quyền BLL | HIGH-02, HIGH-03, LOW-04 |
| **SQL Injection** |  An toàn: 100% truy vấn qua EF LINQ, không có chuỗi SQL động | *Không phát hiện lỗ hổng* |
| **Hiệu năng & N+1** | ⚠️ Cần tối ưu: Kéo cả bảng về RAM khi search, một số hàm dùng `Load()` tuần tự | HIGH-04, MED-06 |
| **Xử lý ngoại lệ (Exception Handling)** | ⚠️ Nhiều chỗ nuốt lỗi bằng `Debug.WriteLine` hoặc `catch { }` rỗng | MED-05, LOW-05 |
| **Tính toán tài chính & Ngày giờ** | ⚠️ Thống kê doanh thu tính trên học phí danh nghĩa thay vì dòng tiền thực thu; nợ lớp hủy | MED-02, MED-03, MED-04 |
| **Kiến trúc & Refactor** | ⚠️ UI chứa nhiều logic nghiệp vụ, Form kích thước lớn (God Forms), thiếu chuẩn hóa CSDL | LOW-01, LOW-02, LOW-03 |

---

## 5. KẾ HOẠCH HÀNH ĐỘNG VÀ KHUYẾN NGHỊ KHẮC PHỤC

### 5.1. Ưu tiên 1 (Khắc phục ngay lập tức - Hotfix)
1. **Bổ sung kiểm tra buổi học bù khi xóa gia sư**: Thêm điều kiện chặn xóa nếu gia sư đã có bản ghi trong `BuoiHoc.MaGSDay` ([CRIT-02]).
2. **Gộp `SaveChanges` và thêm Transaction**: Trong `LopHocService` (`GhepLop`, `SuaLop`, `HuyLop`), gom các lệnh `SaveChanges()` thành 1 giao dịch duy nhất ([CRIT-01]).
3. **Bổ sung kiểm tra `BuoiHoc` vào `TimXungDot`**: Quét các buổi học bù khi ghép lớp để chống xung đột lịch học bù ([HIGH-01]).
4. **Sửa thông báo đăng nhập**: Loại bỏ thông báo chi tiết số lần thử để chống dò quét tài khoản ([HIGH-03]).

### 5.2. Ưu tiên 2 (Cải tiến trong sprint tiếp theo)
1. **Tối ưu tìm kiếm**: Tránh `_dbSet.ToList()` trước khi lọc không dấu; thêm debounce time 300ms trên UI ([HIGH-04]).
2. **Sửa tính toán doanh thu & công nợ**: Tính doanh thu theo dòng tiền thực thu từ `ThanhToan`; loại trừ công nợ của các lớp học đã hủy ([MED-03], [MED-04]).
3. **Chuẩn hóa DbContext Lifetime**: Bỏ biến DbContext instance sống lâu trong `frmCalendarView`, chuyển sang `using (var db = ...)` ngắn hạn ([MED-06]).
4. **Bổ sung Unique Index CSDL**: Thêm unique index cho `TaiKhoan.MaGS` ([LOW-04]).

### 5.3. Ưu tiên 3 (Tái cấu trúc dài hạn - Refactoring)
1. **Phân rã God Form**: Tách `frmHocVien` và `frmChucNangGhepLop` thành các UserControl và Presenter độc lập theo mô hình MVP ([LOW-01]).
2. **Chuẩn hóa quan hệ môn học**: Chuyển `LopHoc.MonHoc` sang sử dụng khóa ngoại `MaMon` liên kết với bảng `MonHoc` ([LOW-02]).
3. **Cơ chế dọn dẹp file tạm và ghi log chuẩn**: Thay thế `Debug.WriteLine` bằng giải pháp ghi log ra file cho môi trường Production ([MED-05], [LOW-03]).

---

## 6. KHÔNG SỬA

Các mục dưới đây được giữ nguyên trong đợt sửa lỗi này, có ghi lý do:

| Mã lỗi | Lý do không sửa |
|---|---|
| [CRIT-03] Race condition khi ghép lớp đồng thời | Ứng dụng là WinForms chạy trên một máy, mỗi lúc chỉ một người quản lý thao tác nên không có hai phiên ghép lớp chạy song song. Tình huống "hai quản lý trên hai máy trạm" không xảy ra với cách triển khai hiện tại. Nếu sau này triển khai nhiều máy dùng chung CSDL thì xử lý bằng `sp_getapplock` hoặc `IsolationLevel.Serializable` như đề xuất. |
| [CRIT-04] Race condition khi ghi nhận học phí đồng thời | Cùng lý do: một người dùng, một phiên, các lần đóng tiền diễn ra tuần tự trên cùng giao diện. Kiểm tra `soTien > conNo` hiện có đã chặn thu vượt trong trường hợp tuần tự. Khi chuyển sang nhiều máy trạm thì bổ sung cột `RowVersion` cho `LopHoc`. |
