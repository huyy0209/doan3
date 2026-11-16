(function () {
    "use strict";

    var canvasDoanhThu = document.getElementById("tq-doanh-thu");
    var canvasLopMon = document.getElementById("tq-lop-mon");
    var canvasHocVien = document.getElementById("tq-hoc-vien-moi");
    if (!canvasDoanhThu) { return; }

    var ghiChu = document.getElementById("tq-chart-note");
    var mau = ["#1B7A5E", "#F3C44F", "#2B6FD6", "#C4403C", "#B4730F", "#6B8F7F", "#8E6BB5", "#23825B"];
    var soVi = new Intl.NumberFormat("vi-VN");
    var tienVi = new Intl.NumberFormat("vi-VN", { maximumFractionDigits: 0 });

    function hienThiGhiChu(text) {
        if (!ghiChu) { return; }
        ghiChu.textContent = text;
        ghiChu.hidden = !text;
    }

    function dinhDangTien(v) {
        return tienVi.format(v || 0) + " đ";
    }

    if (!window.Chart) {
        hienThiGhiChu("Không tải được thư viện biểu đồ, số liệu vẫn xem được ở các thẻ phía trên.");
        return;
    }

    Chart.defaults.font.family = "'Segoe UI', system-ui, -apple-system, sans-serif";
    Chart.defaults.color = "#4A5A55";
    Chart.defaults.plugins.legend.labels.usePointStyle = true;

    // 1. Doanh thu học phí 12 tháng gần nhất (cột)
    function veDoanhThu(items) {
        new Chart(canvasDoanhThu, {
            type: "bar",
            data: {
                labels: items.map(function (x) { return x.nhan; }),
                datasets: [{
                    label: "Đã thu",
                    data: items.map(function (x) { return x.soTien; }),
                    backgroundColor: "#1B7A5E",
                    borderRadius: 6,
                    maxBarThickness: 42
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        callbacks: {
                            label: function (c) { return " " + dinhDangTien(c.parsed.y); }
                        }
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: { callback: function (v) { return tienVi.format(v); } },
                        grid: { color: "#EAF0ED" }
                    },
                    x: { grid: { display: false } }
                }
            }
        });
    }

    // 2. Số lớp đang học theo môn (tròn/donut)
    function veLopMon(items) {
        if (!items.length) {
            canvasLopMon.parentNode.innerHTML = '<div class="vo-empty">Chưa có lớp nào đang học.</div>';
            return;
        }
        new Chart(canvasLopMon, {
            type: "doughnut",
            data: {
                labels: items.map(function (x) { return x.tenMon; }),
                datasets: [{
                    data: items.map(function (x) { return x.soLop; }),
                    backgroundColor: items.map(function (_, i) { return mau[i % mau.length]; }),
                    borderColor: "#FFFFFF",
                    borderWidth: 2
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: "58%",
                plugins: {
                    legend: { position: "bottom", labels: { boxWidth: 10 } },
                    tooltip: {
                        callbacks: {
                            label: function (c) { return " " + c.label + ": " + c.parsed + " lớp"; }
                        }
                    }
                }
            }
        });
    }

    // 3. Số học viên mới theo tháng (đường)
    function veHocVien(items) {
        new Chart(canvasHocVien, {
            type: "line",
            data: {
                labels: items.map(function (x) { return x.nhan; }),
                datasets: [{
                    label: "Học viên mới",
                    data: items.map(function (x) { return x.soLuong; }),
                    borderColor: "#2B6FD6",
                    backgroundColor: "rgba(43, 111, 214, .14)",
                    fill: true,
                    tension: .3,
                    pointRadius: 3,
                    pointBackgroundColor: "#2B6FD6"
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        callbacks: {
                            label: function (c) { return " " + c.parsed.y + " học viên"; }
                        }
                    }
                },
                scales: {
                    y: { beginAtZero: true, ticks: { precision: 0 }, grid: { color: "#EAF0ED" } },
                    x: { grid: { display: false } }
                }
            }
        });
    }

    // 4. Tỉ lệ chuyên cần tháng này (ô số + thanh)
    function veChuyenCan(cc) {
        var tyLe = cc && typeof cc.tyLe === "number" ? cc.tyLe : 0;
        var oSo = document.getElementById("tq-cc-ty-le");
        var thanh = document.getElementById("tq-cc-fill");
        var chiTiet = document.getElementById("tq-cc-chi-tiet");

        if (oSo) { oSo.textContent = soVi.format(tyLe) + "%"; }
        if (thanh) {
            thanh.style.width = Math.max(0, Math.min(100, tyLe)) + "%";
            var hop = thanh.parentNode;
            if (hop) { hop.setAttribute("aria-valuenow", String(Math.round(tyLe))); }
        }
        if (chiTiet) {
            chiTiet.textContent = cc && cc.tongBuoi > 0
                ? cc.coMat + "/" + cc.tongBuoi + " buổi có mặt"
                : "Chưa có buổi nào được điểm danh trong tháng này.";
        }
    }

    fetch("/TongQuan/BieuDo", { headers: { "Accept": "application/json" }, credentials: "same-origin" })
        .then(function (r) {
            if (!r.ok) { throw new Error("HTTP " + r.status); }
            return r.json();
        })
        .then(function (d) {
            hienThiGhiChu("");
            veDoanhThu(d.doanhThu || []);
            veLopMon(d.lopTheoMon || []);
            veHocVien(d.hocVienMoi || []);
            veChuyenCan(d.chuyenCan);
        })
        .catch(function () {
            hienThiGhiChu("Không tải được số liệu biểu đồ. Thử làm mới trang.");
        });
})();
