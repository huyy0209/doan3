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
| W0 | đang chạy | copilot (dispatch ctx_02e2349e5312) | web-w0 | huyy0209/web-w0 | chuyển 8 service phụ (HocTap, HocPhi, MonHoc, TongQuan, ThongBao, CaiDat, ThongKe, BaoCao). Lúc đầu worktree tạo từ 6f6a35f (thiếu DoAnWeb) → coordinator ff lên 8cd7a1d |
| W7 | đang chạy | opencode / DeepSeek V4.1 Flash Free (ctx_55791c8eaee1) | web-w7 | huyy0209/web-w7 | giao diện khung giống app: site.css, _Layout, đăng nhập, icon |
| W1 | đang chạy | copilot (ctx_74844e637f60) | web-w1 | huyy0209/web-w1 | GS, HV, Tài khoản. Prompt đầu bị rớt (Copilot "policy still loading") → gửi lại |
| W2 | đang chạy | opencode (ctx_9d9eae0856d5) | web-w2 | huyy0209/web-w2 | Lớp, ghép lớp, Môn học (Môn học sau khi có MonHocService của W0) |
| W3 | đang chạy | dsh / deepseek-v4-pro (ctx_d81e86f6b82c) | web-w3 | huyy0209/web-w3 | xUnit, DB QuanLyGiaSu_Test |
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

## Lưu ý vận hành
- Run Orca: `run_b78b76a4f111`. Spec từng task: scratchpad coordinator (W0.md, W7.md, W1..W3-full.md); nội dung tóm tắt ở web-plan mục 6.
- Copilot hay rớt prompt đầu ("Managed account policy is still loading") → sau khi start phải `worker-read` kiểm, rớt thì `send` lại.
- `worker-start --worktree new-child` có lúc lấy base cũ (W0 lấy 6f6a35f) → luôn truyền `--base-branch huyy0209/cập-nhật-lên-web`.
- GĐ1 coordinator đã xong lõi (commit 1091c3b): Program.cs, auth, entity, Common, TaiKhoan/GiaSu/HocVien/LopHoc service, tác vụ nền.
  Còn chờ: W0 (service phụ) + W7 (layout) merge → gọi `AddDichVuPhu()` trong Program.cs, filter đặt `ViewData["TenTrungTam"]`,
  commit cuối GĐ1 "feat(web): dựng khung ASP.NET Core". Sau đó giao W5 (copilot) và W6 (opencode).

## Bước tiếp theo
1. Đợi người dùng trả lời "duyệt" + câu hỏi.
2. GĐ1: dựng `DoAnWeb/`, commit từng phần nhỏ, cuối cùng commit "feat(web): dựng khung ASP.NET Core".
