/* =====================================================================
   Hành vi giao diện dùng chung – "Vở ô li"
   (giữ mảnh, không phụ thuộc thư viện ngoài; Bootstrap bundle đã nạp riêng)
   ===================================================================== */
(function () {
    "use strict";

    // 1) Toast thông báo: tự tắt như ThongBao.cs (2,2s + 35ms mỗi ký tự, tối đa 6s),
    //    bấm vào thì tắt ngay. Lỗi (đỏ) để lâu hơn một chút cho người dùng kịp đọc.
    function ganToast() {
        document.querySelectorAll("[data-vo-toast]").forEach(function (toast) {
            var chu = (toast.textContent || "").trim();
            var laLoi = toast.classList.contains("vo-toast-danger");
            var cho = Math.min(laLoi ? 9000 : 6000, 2200 + chu.length * 35);

            var hen = window.setTimeout(function () { tatToast(toast); }, cho);
            toast.addEventListener("click", function () {
                window.clearTimeout(hen);
                tatToast(toast);
            });
        });
    }

    function tatToast(toast) {
        if (!toast || toast.classList.contains("is-hiding")) { return; }
        toast.classList.add("is-hiding");
        window.setTimeout(function () {
            if (toast.parentNode) { toast.parentNode.removeChild(toast); }
        }, 260);
    }

    // 2) Màn hình hẹp: nút trên thanh tiêu đề mở/đóng sidebar.
    function ganNutSidebar() {
        var app = document.getElementById("vo-app");
        var nut = document.getElementById("vo-sidebar-toggle");
        if (!app || !nut) { return; }

        function datTrangThai(mo) {
            app.classList.toggle("is-sidebar-open", mo);
            document.body.classList.toggle("vo-menu-open", mo);
            nut.setAttribute("aria-expanded", mo ? "true" : "false");
            nut.setAttribute("aria-label", mo ? "Đóng menu" : "Mở menu");
        }

        nut.addEventListener("click", function () {
            datTrangThai(!app.classList.contains("is-sidebar-open"));
        });

        // Bấm ra ngoài sidebar (trên màn hẹp) thì đóng lại.
        document.addEventListener("click", function (e) {
            if (!app.classList.contains("is-sidebar-open")) { return; }
            if (app.querySelector(".vo-sidebar").contains(e.target) || nut.contains(e.target)) { return; }
            datTrangThai(false);
        });

        document.addEventListener("keydown", function (e) {
            if (e.key === "Escape" && app.classList.contains("is-sidebar-open")) {
                datTrangThai(false);
                nut.focus();
            }
        });
    }

    // 3) Tiện ích cho các worker khác: thêm/xóa class trạng thái cho một dòng bảng.
    window.Vo = window.Vo || {};
    window.Vo.dongDangChon = function (dong) {
        if (!dong) { return; }
        document.querySelectorAll("table.vo-table tbody tr.is-selected").forEach(function (d) {
            d.classList.remove("is-selected");
        });
        dong.classList.add("is-selected");
    };

    document.addEventListener("DOMContentLoaded", function () {
        ganToast();
        ganNutSidebar();
    });
})();
