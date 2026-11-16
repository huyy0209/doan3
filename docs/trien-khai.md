# Triển khai bản web (dịch vụ Windows + mạng LAN + HTTPS)

> Đề xuất #7 trong `docs/de-xuat.md`. Tài liệu này hướng dẫn đưa bản web `DoAnWeb`
> chạy **như một dịch vụ Windows** (tự chạy khi khởi động máy, không cần mở cửa sổ),
> cho **máy khác trong mạng LAN** truy cập, và bật **HTTPS** nếu cần.
>
> Chạy thử không cần cài gì: `powershell -ExecutionPolicy Bypass -File scripts\cai-dich-vu-web.ps1 -WhatIf`
> — chỉ in ra các lệnh sẽ chạy, không thay đổi máy.

## Chạy bằng Docker (tùy chọn)

Docker Compose khởi chạy SQL Server 2022, tạo riêng CSDL `QuanLyGiaSu_Web` từ
`Database/0*.sql`, rồi chạy web trên cổng `5080`. CSDL của app WinForms
`QuanLyGiaSu` không được dùng. Endpoint kiểm tra `/suc-khoe` không yêu cầu đăng nhập,
chỉ trả trạng thái kết nối ngắn gọn (`Healthy`/`Unhealthy`), không trả thông tin kết nối.

Cài Docker Desktop hoặc Docker Engine có Compose v2, mở PowerShell ở thư mục gốc repo:

```powershell
Copy-Item .env.example .env
# Sửa MSSQL_SA_PASSWORD trong .env thành mật khẩu riêng đạt yêu cầu của SQL Server.
docker compose build
docker compose up -d
docker compose ps
Invoke-WebRequest http://localhost:5080/suc-khoe -UseBasicParsing | Select-Object StatusCode, Content
```

Lần đầu, Compose chờ SQL Server sẵn sàng, chạy một lần dịch vụ `khoi-tao-csdl`, rồi mới
khởi động `web`. Dữ liệu mẫu tắt mặc định; muốn nạp các script `Database/11`–`16`, đặt
`NAP_DU_LIEU_MAU=true` trong `.env` trước khi khởi tạo CSDL. Nếu đổi lựa chọn sau khi CSDL
đã được tạo, script nâng cấp có thể chạy lại nhưng các script dữ liệu mẫu chỉ chèn dữ liệu
mới theo luật riêng của chúng.

Volume `sql-data` giữ dữ liệu CSDL qua lần dừng/chạy lại; volume `sql-backups` được chia sẻ
giữa SQL Server và web để tính năng sao lưu/khôi phục có thể đọc các file SQL Server tạo.
Log web nằm ở `DoAnWeb/logs/`. `docker compose down` giữ dữ liệu; `docker compose down -v`
xóa cả dữ liệu CSDL và bản sao lưu. Không chia sẻ file `.env` vì nó chứa mật khẩu SA.

Compose healthcheck gọi `/suc-khoe`; trạng thái chỉ `Healthy` khi ứng dụng truy vấn được
CSDL. Có thể xem log khi khởi động lỗi bằng `docker compose logs -f sql khoi-tao-csdl web`.

## 1. Cần có trước khi cài

| Thành phần | Ghi chú |
|---|---|
| Windows 10/11 hoặc Windows Server | Dịch vụ Windows chỉ có trên Windows |
| .NET SDK 8 trở lên (để `dotnet publish`) | `dotnet --version`. Máy chỉ có runtime ASP.NET Core mới hơn vẫn **chạy** được nhờ `RollForward=Major`, nhưng cài đặt cần SDK |
| SQL Server (2016+) có CSDL `QuanLyGiaSu_Web` | Chưa có thì chạy `QuanLyGiaSu.bat cai-csdl` trước. CSDL này riêng với app WinForms `QuanLyGiaSu`. |
| Quyền **Administrator** | Cài/gỡ dịch vụ và mở tường lửa đều cần |
| Tài khoản đăng nhập SQL Server phù hợp | Xem mục 7 |

Cài dịch vụ **không** đổi CSDL, không tạo bảng mới. Bản web dùng CSDL riêng
`QuanLyGiaSu_Web`; không dùng CSDL `QuanLyGiaSu` của app WinForms.

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

### Dùng trên điện thoại

**Cách 1 — cùng mạng Wi-Fi:** cài/chạy dịch vụ web trên máy chủ như mục 2, mở cổng LAN bằng
`QuanLyGiaSu.bat dich-vu -MoTuongLua`, rồi kết nối điện thoại vào cùng Wi-Fi. Trên điện thoại,
mở `http://<IP-máy-chủ>:5080` (ví dụ `http://192.168.2.4:5080`). Cách này dùng HTTP; chỉ sử dụng
trên mạng riêng đáng tin cậy, hoặc cấu hình HTTPS ở mục 5 trước khi nhập thông tin đăng nhập.

**Cách 2 — Tailscale HTTPS:** cài Tailscale trên máy chạy web và điện thoại, đăng nhập cả hai vào
cùng tailnet, rồi đảm bảo bản web đang chạy trên cổng 5080. Chạy `QuanLyGiaSu.bat tu-xa` trên máy
chạy web; script kiểm tra đăng nhập Tailscale và cổng web, sau đó in địa chỉ HTTPS dạng
`https://<máy>.<tailnet>.ts.net`. Mở địa chỉ đó trên điện thoại khi Tailscale đang kết nối; trong
menu trình duyệt chọn **Thêm vào màn hình chính** để cài PWA. Tắt truy cập từ xa bằng
`QuanLyGiaSu.bat tu-xa -Tat`. Không dùng `tailscale funnel` vì Funnel công khai web ra Internet.

Khi dùng HTTPS, đặt `BaoMat:BatBuocHttps` thành `true` trong cấu hình Production để cookie đăng
nhập luôn được bảo vệ bằng HTTPS; chỉ bật sau khi đã xác nhận HTTPS hoạt động. Đổi ngay mật khẩu
mặc định `admin` / `admin123` sau lần đăng nhập đầu tiên.

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

## 11. Triển khai lên IIS

IIS phù hợp khi máy chủ đã dùng Windows Server/IIS hoặc cần quản lý binding và chứng
chỉ tập trung. `DoAnWeb\DoAnWeb.csproj` nhắm `net8.0` và đặt `RollForward=Major`:
cài **.NET 8 Hosting Bundle** (bao gồm ASP.NET Core Module V2/ANCM và runtime .NET 8)
để khớp chính xác với ứng dụng. Có thể chạy trên ASP.NET Core runtime major mới hơn
nhờ `RollForward=Major`, nhưng phải cài Hosting Bundle của phiên bản đó để có ANCM.
Sau khi cài/cập nhật Hosting Bundle, khởi động lại IIS hoặc máy chủ. Script chỉ kiểm tra
và báo thành phần còn thiếu, không tải hay cài IIS/Hosting Bundle thay bạn.

Mở PowerShell bằng **Run as administrator**, hoặc dùng menu **8. Cài / cập nhật IIS**:

```bat
QuanLyGiaSu.bat iis -WhatIf
QuanLyGiaSu.bat iis
```

Script `scripts\cai-iis.ps1` publish `Release`, tạo hoặc cập nhật App Pool cùng tên
site (mặc định `QuanLyGiaSuWeb`) với **No Managed Code**, binding HTTP `*:5080`, và
thư mục `%ProgramFiles%\QuanLyGiaSuIIS`. Có thể đổi tên, cổng và thư mục:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\cai-iis.ps1 `
    -TenSite "DoAnGiaSu" -Cong 8080 -ThuMuc "D:\Web\DoAnGiaSu"
```

Lệnh publish tương đương (chạy tại thư mục gốc repo):

```powershell
dotnet publish DoAnWeb\DoAnWeb.csproj -c Release -o "D:\Web\DoAnGiaSu" --nologo
```

ASP.NET Core Web SDK tự sinh `web.config` vào thư mục publish để IIS gọi ANCM; không
cần chép hoặc commit `web.config` riêng. Script kiểm tra file này trước khi thiết lập
site. App Pool dùng danh tính ảo `IIS AppPool\<tên-site>`: script cấp Read/Execute
cho thư mục ứng dụng và Modify cho `logs` cùng `sao-luu` bên dưới thư mục triển khai.
Nếu đổi vị trí backup qua `SaoLuu:ThuMuc`, cấp thêm Modify cho App Pool trên thư mục
đó. SQL Server cũng cần quyền ghi tại vị trí backup vì chính dịch vụ SQL Server tạo
file `.bak`; khi SQL Server ở máy khác, dùng đường dẫn chia sẻ phù hợp và cấp quyền
cho danh tính dịch vụ SQL Server trên máy chủ SQL.

### Cấu hình Production, SQL và HTTPS

Tạo `appsettings.Production.json` cạnh `DoAnWeb.dll` với cấu hình riêng của máy chủ
(không chép mẫu Kestrel nếu không dùng chứng chỉ Kestrel). Ví dụ cho SQL Server cùng
máy và backup trong thư mục do script tạo:

```json
{
  "ConnectionStrings": {
    "QuanLyGiaSu": "Server=.;Database=QuanLyGiaSu_Web;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
  },
  "SaoLuu": {
    "ThuMuc": "C:\\Program Files\\QuanLyGiaSuIIS\\sao-luu"
  },
  "BaoMat": {
    "BatBuocHttps": true
  }
}
```

Chỉ đặt `BaoMat:BatBuocHttps=true` sau khi HTTPS binding đã hoạt động. Không commit
mật khẩu hay file cấu hình có bí mật. IIS mặc định chạy ứng dụng ở môi trường
`Production`. Có thể dùng biến môi trường .NET thay cho file, với dấu hai chấm trong
tên cấu hình thay bằng hai dấu gạch dưới, ví dụ `ConnectionStrings__QuanLyGiaSu` và
`BaoMat__BatBuocHttps`. Có thể cấu hình qua `environmentVariables` trong `web.config`
sinh từ publish; sau khi đổi cấu hình, recycle App Pool. Chuỗi kết nối mẫu trong
`appsettings.json` dùng Windows Authentication tới `QuanLyGiaSu_Web`, không phải CSDL
WinForms `QuanLyGiaSu`.

Nếu SQL Server cùng máy, cấp quyền tối thiểu cho danh tính App Pool trên CSDL web:

```sql
USE master;
CREATE LOGIN [IIS APPPOOL\QuanLyGiaSuWeb] FROM WINDOWS;
USE QuanLyGiaSu_Web;
CREATE USER [IIS APPPOOL\QuanLyGiaSuWeb] FOR LOGIN [IIS APPPOOL\QuanLyGiaSuWeb];
ALTER ROLE db_datareader ADD MEMBER [IIS APPPOOL\QuanLyGiaSuWeb];
ALTER ROLE db_datawriter ADD MEMBER [IIS APPPOOL\QuanLyGiaSuWeb];
```

Thay tên App Pool nếu `-TenSite` khác. Nếu SQL Server ở máy khác, dùng danh tính
Windows/domain phù hợp (App Pool identity cục bộ không tự trở thành tài khoản SQL hợp
lệ trên máy từ xa), hoặc SQL login và giữ bí mật trong cấu hình Production. Không cấp
quyền DDL nếu ứng dụng không cần thay đổi schema.

Trong IIS Manager, mở **Sites → \<site> → Bindings… → Add**, chọn `https`, cổng `443`
(hoặc cổng đã cấp), hostname nếu cần, và chứng chỉ TLS máy khách tin cậy. Mở cổng tại
tường lửa theo chính sách mạng. Có thể giữ binding HTTP để chuyển hướng HTTPS; đặt
`BaoMat:BatBuocHttps=true` chỉ sau khi HTTPS đã kiểm tra thành công. Ứng dụng đã bật
forwarded headers cho `X-Forwarded-For` và `X-Forwarded-Proto`; cấu hình hiện tại chỉ
tin proxy loopback. Giữ giới hạn proxy tin cậy đó. Nếu TLS được kết thúc ở reverse
proxy bên ngoài IIS, chỉ cấu hình đúng địa chỉ proxy tin cậy, không tin header
forwarded từ mọi nguồn.

### Sự cố IIS thường gặp

| Lỗi | Nguyên nhân thường gặp / cách xử lý |
|---|---|
| **500.19** | IIS không đọc được cấu hình: `web.config` hỏng/không hợp lệ, mục cấu hình bị khóa hoặc thiếu module ASP.NET Core. Kiểm tra XML trong file publish, cài Hosting Bundle đúng kiến trúc và khởi động lại IIS. |
| **500.30** | Ứng dụng không khởi động: thiếu ASP.NET Core Runtime tương thích, cấu hình Production/chuỗi SQL sai hoặc không kết nối được CSDL. Kiểm tra Event Viewer → Windows Logs → Application, cài đúng Hosting Bundle và chạy `dotnet --list-runtimes`. |
| Không ghi được log hoặc backup | Cấp Modify cho `IIS AppPool\<tên-site>` trên `logs`/`SaoLuu:ThuMuc`; đồng thời cấp quyền ghi backup cho tài khoản dịch vụ SQL Server. |

Để chẩn đoán sâu, kiểm tra log Windows Application và `logs\doanweb-YYYYMMDD.log`.
Có thể bật `stdoutLogEnabled` trong `web.config` tạm thời và đặt `stdoutLogFile`
vào thư mục được App Pool ghi; tắt lại ngay sau khi lấy log vì stdout log không tự
xoay vòng và có thể chiếm dung lượng.
