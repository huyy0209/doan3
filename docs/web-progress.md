# Tiến độ chuyển lên Web (coordinator)

> File này đủ để coordinator mới làm tiếp mà không cần lịch sử chat. Cập nhật + commit mỗi khi đổi trạng thái.
> Kế hoạch chi tiết: `docs/web-plan.md`. Tiến độ từng worker: `docs/progress/<mã task>.md`.

## Giai đoạn hiện tại
**GĐ1 – Dựng khung: ĐANG LÀM** (người dùng đã "duyệt" 2026-11-14, quyết định ở web-plan.md mục 6.2).

## Thông tin môi trường
- Repo worktree: `C:\Users\Admin\orca\workspaces\doan3\cập-nhật-lên-web`, nhánh `huyy0209/cập-nhật-lên-web`.
- CLI có trên PATH: `copilot` (shim VS Code), `dsh` (npm), `opencode` (npm). `antigravity` không có trên PATH (Orca có agent id `antigravity`).
- Agent id Orca ví dụ trong help: `claude, codex, cursor, antigravity, muse, zcode, opencode, opencode2`.
- .NET: chỉ SDK 10.0.301 / runtime 10.0.9. `dotnet-ef` 10.0.8 cài global.
- SQL Server `.` có DB `QuanLyGiaSu` (đủ bảng tới v7+, 33 tài khoản; `admin` QuanLy, `gs1..` GiaSu mật khẩu mẫu `123456`).
- `dotnet build DoAnWD.slnx` lỗi sẵn (WinForms thiếu package EF6 qua packages.config) – không phải do web.

## Trạng thái task
| Task | Trạng thái | Agent | Worktree | Nhánh | Ghi chú |
|---|---|---|---|---|---|
| GĐ0 | xong, đã duyệt | coordinator | cập-nhật-lên-web | huyy0209/cập-nhật-lên-web | 3b08cb1 |
| GĐ1 | đang chạy | coordinator | cập-nhật-lên-web | huyy0209/cập-nhật-lên-web | |
| W1 | chưa giao | (copilot) | – | – | GS, HV, Tài khoản |
| W2 | chưa giao | (opencode) | – | – | Lớp, ghép lớp, Môn học |
| W3 | chưa giao | (dsh) | – | – | xUnit, DB QuanLyGiaSu_Test |
| W5 | chưa giao | (copilot) | – | – | Điểm danh, kết quả, học bù |
| W6 | chưa giao | (opencode) | – | – | Học phí, Tổng quan, Lịch, Cài đặt, Thông báo |
| W4 | chưa giao | (antigravity) | – | – | chỉ chạy sau khi W1,W2,W3,W5,W6 merge |

## Quyết định đã chốt
Xem `docs/web-plan.md` mục 6.2 (net8+RollForward, làm đủ màn hình giống app, test trên DB `QuanLyGiaSu_Test`,
ghi MaMon, tạo admin khi bảng rỗng, demo `admin/Admin@2026`, `gs1/123456`).

## Lịch sử đổi agent
| Thời điểm | Task | Agent cũ | Agent mới | Lý do |
|---|---|---|---|---|
| – | – | – | – | – |

## Bước tiếp theo
1. Đợi người dùng trả lời "duyệt" + câu hỏi.
2. GĐ1: dựng `DoAnWeb/`, commit từng phần nhỏ, cuối cùng commit "feat(web): dựng khung ASP.NET Core".
