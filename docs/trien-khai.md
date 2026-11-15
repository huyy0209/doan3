# Triển khai bản web (dịch vụ Windows + mạng LAN + HTTPS)

> Đề xuất #7 trong `docs/de-xuat.md`. Tài liệu này hướng dẫn đưa bản web `DoAnWeb`
> chạy **như một dịch vụ Windows** (tự chạy khi khởi động máy, không cần mở cửa sổ),
> cho **máy khác trong mạng LAN** truy cập, và bật **HTTPS** nếu cần.
>
> Chạy thử không cần cài gì: `powershell -ExecutionPolicy Bypass -File scripts\cai-dich-vu-web.ps1 -WhatIf`
> — chỉ in ra các lệnh sẽ chạy, không thay đổi máy.

## 1. Cần có trước khi cài

| Thành phần | Ghi chú |
|---|---|
| Windows 10/11 hoặc Windows Server | Dịch vụ Windows chỉ có trên Windows |
| .NET SDK 8 trở lên (để `dotnet publish`) | `dotnet --version`. Máy chỉ có runtime ASP.NET Core mới hơn vẫn **chạy** được nhờ `RollForward=Major`, nhưng cài đặt cần SDK |
| SQL Server (2016+) có CSDL `QuanLyGiaSu` | Chưa có thì chạy `QuanLyGiaSu.bat cai-csdl` trước |
| Quyền **Administrator** | Cài/gỡ dịch vụ và mở tường lửa đều cần |
| Tài khoản đăng nhập SQL Server phù hợp | Xem mục 7 |

Cài dịch vụ **không** đổi CSDL, không tạo bảng mới. Dữ liệu vẫn là CSDL `QuanLyGiaSu`
dùng chung với bản app WinForms.

## 2. Cài dịch vụ web

Có hai cách, cùng gọi `scripts/cai-dich-vu-web.ps1`:

```bat
rem Cách 1: menu — chuột phải QuanLyGiaSu.bat, chọn "Run as administrator", rồi chọn 9
QuanLyGiaSu.bat

rem Cách 2: lệnh thẳng trong PowerShell đã mở bằng quyền Administrator
QuanLyGiaSu.bat dich-vu
```

Script làm lần lượt:

1. `dotnet publish DoAnWeb\DoAnWeb.csproj -c Release -o "%ProgramFiles%\QuanLyGiaSuWeb"`
   (nếu dịch vụ đang chạy thì dừng trước để không bị khóa file `.dll`).
2. `sc.exe create "QuanLyGiaSuWeb" binPath= "<dotnet.exe>" "<DoAnWeb.dll>" start= auto`
   → dịch vụ **tự chạy khi khởi động máy**, chạy dưới tài khoản `LocalSystem`.
3. Đặt biến môi trường riêng của dịch vụ (khóa registry `HKLM\SYSTEM\CurrentControlSet\Services\QuanLyGiaSuWeb`):
   - `ASPNETCORE_URLS=http://0.0.0.0:5080` — nghe trên **mọi card mạng**, không chỉ localhost
   - `ASPNETCORE_ENVIRONMENT=Production`
4. Đặt mô tả dịch vụ và tự khởi động lại nếu tiến trình web thoát bất thường
   (`sc.exe failure … restart/60000`, tối đa 3 lần trong 1 ngày).
5. Tùy chọn `-MoTuongLua`: thêm rule tường lửa `New-NetFirewallRule` cho cổng TCP 5080,
   **chỉ profile Private** (mạng nội bộ) — không mở ra mạng công cộng.
6. `sc.exe start "QuanLyGiaSuWeb"`.

Tham số hay dùng:

| Tham số | Mặc định | Ý nghĩa |
|---|---|---|
| `-ThuMuc <đường dẫn>` | `%ProgramFiles%\QuanLyGiaSuWeb` | Nơi publish và chạy dịch vụ |
| `-Cong <số>` | `5080` | Cổng HTTP (dùng cho cả rule tường lửa) |
| `-Url <địa chỉ>` | `http://0.0.0.0:<Cong>` | Ghi đè `ASPNETCORE_URLS` (ví dụ chỉ định HTTPS) |
| `-TenDichVu <tên>` | `QuanLyGiaSuWeb` | Tên dịch vụ trong `services.msc` |
| `-MoTuongLua` | tắt | Mở cổng TCP cho máy khác trong LAN |
| `-GoBo` | – | Gỡ dịch vụ + rule tường lửa |
| `-WhatIf` | – | Chỉ in lệnh, không thay đổi gì |

Ví dụ cài vào ổ D, cổng 8080, mở luôn tường lửa:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\cai-dich-vu-web.ps1 `
    -ThuMuc "D:\Web\QuanLyGiaSu" -Cong 8080 -MoTuongLua
```

Nếu script báo **cần quyền Administrator**: đóng cửa sổ, chuột phải
`QuanLyGiaSu.bat` (hoặc Windows PowerShell) → **Run as administrator** rồi làm lại.
Chế độ `-WhatIf` vẫn chạy được khi không phải Administrator vì chỉ in lệnh.

## 3. Kiểm tra dịch vụ và xem log

```powershell
Get-Service QuanLyGiaSuWeb                 # phải là Running
sc.exe query QuanLyGiaSuWeb                # trạng thái chi tiết
Invoke-WebRequest http://localhost:5080/TaiKhoan/DangNhap -UseBasicParsing | Select StatusCode
```

- Bật/tắt tay: `sc.exe stop QuanLyGiaSuWeb` / `sc.exe start QuanLyGiaSuWeb`,
  hoặc mở `services.msc`.
- **Log Serilog** nằm trong thư mục cài: `%ProgramFiles%\QuanLyGiaSuWeb\logs\doanweb-YYYYMMDD.log`
  (giữ 14 ngày). Khi chạy `dotnet run` thì log ở `DoAnWeb\logs\`.
- Nếu dịch vụ dừng ngay khi khởi động, xem:
  - `Event Viewer` → `Windows Logs` → `Application` (nguồn `QuanLyGiaSuWeb`),
  - file log trên (nếu ứng dụng kịp ghi),
  - chạy tay để thấy lỗi: `cd "%ProgramFiles%\QuanLyGiaSuWeb"` rồi `dotnet DoAnWeb.dll`.

## 4. Cho máy khác trong mạng LAN truy cập

1. Trên máy chủ, xem địa chỉ IPv4: `ipconfig` (hoặc `Get-NetIPAddress -AddressFamily IPv4`).
2. Máy chủ phải nghe trên `0.0.0.0` — script đã đặt `ASPNETCORE_URLS=http://0.0.0.0:5080`.
   Nếu tự cấu hình, đừng để `http://localhost:5080` (máy khác không vào được).
3. Mở cổng trên tường lửa máy chủ (một trong hai cách):

   ```bat
   rem a) chạy lại script kèm -MoTuongLua (khuyến nghị)
   QuanLyGiaSu.bat dich-vu -MoTuongLua

   rem b) mở tay trong PowerShell (quyền Administrator) — chỉ mạng Private
   New-NetFirewallRule -DisplayName "QuanLyGiaSuWeb (TCP 5080)" -Direction Inbound `
       -Protocol TCP -LocalPort 5080 -Action Allow -Profile Private
   ```

4. Từ máy khác mở trình duyệt: `http://<IP-máy-chủ>:5080`, ví dụ `http://192.168.2.4:5080`.

Lưu ý:

- Windows có thể hỏi **cho phép ứng dụng qua tường lửa** khi chạy lần đầu; chỉ tick
  **Private networks**.
- Mạng đang để ở profile **Public** (quán cà phê, wifi lạ) thì rule Private **không** áp dụng —
  đổi sang Private hoặc thêm rule cho profile tương ứng một cách có chủ đích.
- Một số mạng wifi có tính năng "AP isolation" chặn máy ↔ máy; khi đó dùng cáp LAN
  hoặc mạng nội bộ khác.
- Kiểm tra nhanh từ máy khác: `Test-NetConnection 192.168.2.4 -Port 5080`.

## 5. HTTPS cho mạng LAN

HTTP trong LAN ai cũng đọc được (kể cả mật khẩu đăng nhập). Muốn an toàn hơn thì bật HTTPS.
Chứng chỉ **phải do máy khác tin cậy**, nếu không trình duyệt sẽ cảnh báo đỏ.

### 5.1. Tạo chứng chỉ

**Cách A — `mkcert` (khuyến nghị cho LAN, có CA riêng dễ tin cậy):**

```powershell
winget install FiloSottile.mkcert
mkcert -install                                    # tạo CA cục bộ (một lần)
mkcert -pkcs12 -p12-file C:\ProgramData\QuanLyGiaSu\chung-chi\quanlygiasu.pfx `
    192.168.2.4 localhost 127.0.0.1
# mkcert hỏi mật khẩu cho file .pfx — nhập một mật khẩu, dùng lại ở bước 5.2
```

Thay `192.168.2.4` bằng IP máy chủ (thêm tên máy nếu muốn, ví dụ `maychu.local`).
Máy khác cần cài CA của mkcert: chép file `rootCA.pem` (xem `mkcert -CAROOT`)
sang máy đó và cài vào **Trusted Root Certification Authorities**, hoặc chạy
`mkcert -install` trên máy đó.

**Cách B — chứng chỉ dev của .NET (nhanh, chỉ nên dùng để thử):**

```powershell
dotnet dev-certs https -ep C:\ProgramData\QuanLyGiaSu\chung-chi\quanlygiasu.pfx -p "MatKhauCuaBan"
```

Chứng chỉ này chỉ tin cậy trên chính máy chủ; máy khác vẫn bị cảnh báo trừ khi
cài chứng chỉ dev của máy chủ sang.

### 5.2. Cấu hình Kestrel

Sao chép `DoAnWeb\appsettings.Production.example.json` (đã có trong thư mục publish)
thành `appsettings.Production.json` **cạnh `DoAnWeb.dll`** rồi sửa:

```json
{
  "Kestrel": {
    "Endpoints": {
      "Http":  { "Url": "http://0.0.0.0:5080" },
      "Https": {
        "Url": "https://0.0.0.0:5443",
        "Certificate": {
          "Path": "C:\\ProgramData\\QuanLyGiaSu\\chung-chi\\quanlygiasu.pfx",
          "Password": "MatKhauThatCuaFilePfx"
        }
      }
    }
  },
  "BaoMat": { "BatBuocHttps": true }
}
```

- `BaoMat:BatBuocHttps=true` làm cookie đăng nhập chỉ gửi qua HTTPS. **Chỉ bật khi HTTPS đã chạy được**,
  nếu không sẽ không đăng nhập được.
- Khai báo `Kestrel:Endpoints` thì `ASPNETCORE_URLS` bị bỏ qua; nhớ thêm rule tường lửa cho cổng HTTPS:

  ```powershell
  New-NetFirewallRule -DisplayName "QuanLyGiaSuWeb (TCP 5443)" -Direction Inbound `
      -Protocol TCP -LocalPort 5443 -Action Allow -Profile Private
  ```

- Muốn **chỉ HTTPS**, bỏ hẳn mục `Http`. Giữ cả hai thì người dùng gõ `http://` sẽ được
  chuyển sang `https://` (nhờ `UseHttpsRedirection`).
- File chứa mật khẩu chứng chỉ: **không commit** lên git, cấp quyền đọc chỉ cho tài khoản chạy dịch vụ.
- Sau khi sửa cấu hình: `sc.exe stop QuanLyGiaSuWeb` rồi `sc.exe start QuanLyGiaSuWeb`.
- Kiểm tra: `https://<IP-máy-chủ>:5443` — ổ khóa phải xanh (không cảnh báo) trên máy đã tin cậy CA.

## 6. Cập nhật phiên bản mới

Chạy lại đúng lệnh cài (mục 2). Script tự dừng dịch vụ cũ, publish đè, tạo lại dịch vụ
với tham số mới rồi khởi động. `appsettings.Production.json` trong thư mục cài **được giữ nguyên**
(publish không xóa file không thuộc project) — nhưng nên sao lưu trước.

## 7. Tài khoản chạy dịch vụ và quyền SQL Server

Mặc định dịch vụ chạy dưới **`LocalSystem`** (`NT AUTHORITY\SYSTEM`). Chuỗi kết nối trong
`appsettings.json` dùng `Trusted_Connection=True`, nghĩa là SQL Server phải cho `NT AUTHORITY\SYSTEM`
đăng nhập:

```sql
-- Chạy trong SQL Server Management Studio (một lần, trên máy chủ CSDL)
USE master;
CREATE LOGIN [NT AUTHORITY\SYSTEM] FROM WINDOWS;
USE QuanLyGiaSu;
CREATE USER [NT AUTHORITY\SYSTEM] FOR LOGIN [NT AUTHORITY\SYSTEM];
ALTER ROLE db_datareader ADD MEMBER [NT AUTHORITY\SYSTEM];
ALTER ROLE db_datawriter ADD MEMBER [NT AUTHORITY\SYSTEM];
-- Bản web có ghi dữ liệu; không cần quyền DDL (không migration).
```

Nếu CSDL nằm trên máy khác hoặc công ty quy định không dùng `LocalSystem`, chạy dịch vụ bằng
tài khoản riêng: mở `services.msc` → `QuanLyGiaSuWeb` → **Log On** → *This account*, cấp quyền
đọc thư mục cài (log, chứng chỉ) và quyền đăng nhập SQL cho tài khoản đó.
Cách khác: dùng SQL Authentication trong `appsettings.Production.json` (`User Id=…;Password=…`)
và không commit file đó.

## 8. Gỡ dịch vụ

```bat
rem Menu: chuột phải QuanLyGiaSu.bat → Run as administrator → chọn 9 rồi chọn gỡ,
rem hoặc lệnh thẳng:
QuanLyGiaSu.bat dich-vu -GoBo
```

Script dừng và xóa dịch vụ, gỡ rule tường lửa, và **giữ nguyên** thư mục cài để xem log.
Xóa hẳn (sau khi chắc chắn không cần):

```powershell
Remove-Item -LiteralPath "$env:ProgramFiles\QuanLyGiaSuWeb" -Recurse -Force
```

Xem trước thao tác gỡ mà không làm gì: thêm `-WhatIf`.

## 9. Xử lý sự cố thường gặp

| Hiện tượng | Nguyên nhân / cách xử lý |
|---|---|
| `[X] Cần quyền Administrator` | Mở PowerShell / `QuanLyGiaSu.bat` bằng **Run as administrator** |
| Máy khác không vào được, máy chủ vẫn OK | Chưa mở tường lửa (`-MoTuongLua`), mạng đang profile Public, hoặc URL đang là `localhost` |
| Dịch vụ `Running` nhưng trình duyệt lỗi 500 | Không kết nối được CSDL: kiểm tra chuỗi kết nối, quyền của `NT AUTHORITY\SYSTEM`, SQL Server đã bật TCP/IP |
| Dịch vụ dừng ngay sau khi start | Lỗi trong `appsettings.Production.json` (đường dẫn/mật khẩu chứng chỉ sai) — xem Event Viewer + chạy tay `dotnet DoAnWeb.dll` |
| Đăng nhập xong bị đá ra liên tục | `BaoMat:BatBuocHttps=true` nhưng đang vào bằng HTTP → dùng HTTPS hoặc tạm đặt lại `false` |
| Trình duyệt cảnh báo chứng chỉ | Máy khác chưa tin cậy CA (mkcert/dev cert) hoặc chứng chỉ không có IP máy chủ trong SAN |
| Cổng 5080 bị chiếm | `netstat -ano \| findstr :5080`; cài lại với `-Cong 5081` (kèm `-MoTuongLua` để mở đúng cổng) |
| Log không thấy file mới | Xem đúng thư mục cài: `%ProgramFiles%\QuanLyGiaSuWeb\logs\`; kiểm tra quyền ghi của tài khoản dịch vụ |

## 10. Chạy thử không cài (xem trước)

```powershell
# Xem các lệnh cài sẽ chạy
powershell -ExecutionPolicy Bypass -File scripts\cai-dich-vu-web.ps1 -WhatIf -MoTuongLua

# Xem các lệnh gỡ sẽ chạy
powershell -ExecutionPolicy Bypass -File scripts\cai-dich-vu-web.ps1 -GoBo -WhatIf
```

Kết quả xem trước được ghi lại trong `docs/progress/E7.md`.
