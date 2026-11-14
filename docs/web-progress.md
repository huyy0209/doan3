# Tiến độ chuyển lên Web (coordinator)

> File này đủ để coordinator mới làm tiếp mà không cần lịch sử chat. Cập nhật + commit mỗi khi đổi trạng thái.
> Kế hoạch chi tiết: `docs/web-plan.md`. Tiến độ từng worker: `docs/progress/<mã task>.md`.

## Giai đoạn hiện tại
**GĐ3 – Hoàn thiện: XONG.** Mọi task đã merge, build pass, test 240/240, start-web/stop-web/README, web-review.md. Đã push nhánh huyy0209/cập-nhật-lên-web (không merge/push master).

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
| GĐ1 | xong (fa9768e) | coordinator | cập-nhật-lên-web | huyy0209/cập-nhật-lên-web | |
| W0 | đã merge (e.g. merge commit sau 31515ec) | copilot (dispatch ctx_02e2349e5312) | web-w0 | huyy0209/web-w0 | chuyển 8 service phụ (HocTap, HocPhi, MonHoc, TongQuan, ThongBao, CaiDat, ThongKe, BaoCao). Lúc đầu worktree tạo từ 6f6a35f (thiếu DoAnWeb) → coordinator ff lên 8cd7a1d |
| W7 | đã merge (329c780) | opencode / DeepSeek V4.1 Flash Free (ctx_55791c8eaee1) | web-w7 | huyy0209/web-w7 | giao diện khung giống app: site.css, _Layout, đăng nhập, icon |
| W1 | đã merge (soát: admin 200, gs1 bị chặn trang QL) | copilot (ctx_74844e637f60) | web-w1 | huyy0209/web-w1 | GS, HV, Tài khoản. Prompt đầu bị rớt (Copilot "policy still loading") → gửi lại |
| W2 | đã merge (fix trùng namespace cf4d878) | copilot (ctx_e3f5757d236e) – trước: opencode hết quota, coordinator commit hộ 04331e9 | web-w2 | huyy0209/web-w2 | Lớp, ghép lớp, Môn học (Môn học sau khi có MonHocService của W0) |
| W3 | đã merge (240/240 pass, DB QuanLyGiaSu_Test) | copilot (ctx_21286689504e) – trước: dsh (ctx_d81e86f6b82c) hết số dư | web-w3 | huyy0209/web-w3 | xUnit, DB QuanLyGiaSu_Test |
| W5 | đã merge (fix trùng namespace ad77cd0) | copilot (ctx_6c7fa2f46c0b) | web-w5 | huyy0209/web-w5 | Điểm danh, kết quả, học bù |
| W6 | đã merge (soát: QL 200, GS bị chặn TongQuan/HocPhi) | copilot (ctx_42f8a24ce2b6) – trước: opencode hết quota, chưa viết gì | web-w6 | huyy0209/web-w6 | Học phí, Tổng quan, Lịch, Cài đặt, Thông báo |
| W4 | xong – coordinator tự review (antigravity hết quota, muse lỗi 402 billing) | web-w4 | huyy0209/web-w4 | chỉ chạy sau khi W1,W2,W3,W5,W6 merge |

Đã báo W1, W2, W3 merge nhánh coordinator (fix ToTable – trước đó mọi truy vấn lỗi "Invalid object name").

## Quyết định đã chốt
Xem `docs/web-plan.md` mục 6.2 (net8+RollForward, làm đủ màn hình giống app, test trên DB `QuanLyGiaSu_Test`,
ghi MaMon, tạo admin khi bảng rỗng, demo `admin/Admin@2026`, `gs1/123456`).

## Lịch sử đổi agent
| Thời điểm | Task | Agent cũ | Agent mới | Lý do |
|---|---|---|---|---|
| 2026-11-15 | W3 | dsh (deepseek-v4-pro) | copilot | "Insufficient Balance" (hết số dư). Đã dừng worker, giữ worktree; test chưa commit vì build lỗi (thiếu using EF Core) → ghi vào docs/progress/W3.md nhánh web-w3, copilot làm tiếp |

| 2026-11-15 | W2 | opencode (DeepSeek V4.1 Flash Free) | opencode (GLM-5.3-Flash Free) → copilot | "Free usage exceeded" (chờ 6h35m). Người dùng yêu cầu đổi model opencode: thử GLM-5.3 cũng hết → quota tính theo cả tài khoản OpenCode free. Code dở build pass → coordinator commit hộ 04331e9 + sửa trùng namespace |
| 2026-11-15 | W6 | opencode (DeepSeek V4.1 Flash Free) | opencode (GLM-5.3) → copilot | như W2; W6 chưa viết gì |

- Người dùng cấp thêm agent dự phòng: **muse**.
- 2026-11-15: thêm **Z.ai** (https://api.z.ai/api/anthropic) qua Claude Code CLI: script `C:\Users\Admin\bin\claude-zai.cmd` (key lấy từ biến môi trường `Z_AI_API_KEY`, cấu hình riêng `%USERPROFILE%\.claude-zai`). Giao việc: `orca terminal create --worktree <wt> --command "C:\Users\Admin\bin\claude-zai.cmd"` rồi `orca orchestration worker-start --task <id> --terminal <handle> --worktree <wt>`. Thứ tự dự phòng mới: copilot → zai → muse. Đã thử OK 2026-11-15: key hợp lệ nhưng tài khoản Z.ai chưa nạp gói (lỗi 1113) nên chỉ dùng model miễn phí `glm-4.5-flash` (script ép model qua `Z_AI_MODEL`; ngữ cảnh thật ~128k, Claude CLI tưởng 1M nên tránh giao task quá lớn). `glm-4.7-flash` lúc thử bị quá tải (1305).
- 2026-11-15 W4: antigravity → muse. Lý do: 503 capacity (đã chờ 2 phút, gửi tiếp tục 1 lần) rồi "Individual quota reached, resets in 164h". Theo bảng dự phòng là coordinator tự review; dùng muse vì người dùng vừa cấp. Muse lỗi 402 "Billing verification failed" → coordinator tự review 10 mục (docs/web-review.md).

## Đợt 2 (sau khi push)
| Task | Trạng thái | Agent | Worktree | Nội dung |
|---|---|---|---|---|
| W8 | đã merge (QuanLyGiaSu.bat web/stop thử OK, 240/240) | copilot (ctx_c372eff249a2) | web-w8 | gộp .bat thành QuanLyGiaSu.bat + scripts/, dọn file thừa DoAnWeb, .gitignore |
| W9 | đã merge (copilot W9b sửa lại; logo + favicon + QuanLyGiaSu.ico, shortcut Desktop thử OK) | copilot (ctx_e7f9150f4856) – trước: zai làm hỏng | web-w9 | logo SVG, favicon, assets/QuanLyGiaSu.ico |

Thử model mới 2026-11-15: Qwen Code (OAuth free ngừng 15/04/2026) và Gemini CLI (Code Assist cá nhân ngừng, chuyển Antigravity) KHÔNG dùng được miễn phí khi đăng nhập tài khoản; cần key (OpenRouter / Google AI Studio).

| W10 | đang chạy | claude-sonnet-5-5 (ctx_7ce40fb5dcff) | web-w10 | sửa review #1 (khóa tài khoản), #3 (trùng lịch song song), #4 (lộ lỗi CSDL) |

Đánh giá zai (glm-4.5-flash): chạy được nhưng chất lượng thấp, tự báo xong khi chưa đạt → chỉ giao việc rất nhỏ và luôn soát lại.

Chính sách model (người dùng chốt 2026-11-15): được dùng worker Claude; coordinator tự chọn Haiku (việc đơn giản) / Sonnet (code) / Opus (review, lỗi khó). **Fable chỉ khi miễn phí hoặc khuyến mãi không tính tiền.** Thứ tự dự phòng: copilot → claude-sonnet → zai (việc nhỏ) → muse.

## Tách 2 nhánh (người dùng chốt 2026-11-15: `app` và `web`, giữ nhánh huyy0209/cập-nhật-lên-web)
- `app`: xong, đã push (03ba338) – worktree `../ban-app`. = master + QuanLyGiaSu.bat (chỉ mục app) + scripts/ + assets/ icon, README cập nhật.
- `web`: chờ W10 merge vào nhánh này rồi tạo từ đây, bỏ DoAnWD/ + script app, menu chỉ mục web.

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
