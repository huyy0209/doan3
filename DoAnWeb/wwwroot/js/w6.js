(function () {
    "use strict";

    var badgeHost = document.getElementById("vo-badge-thongbao");
    var toastHost = document.getElementById("vo-toasts");
    if (!badgeHost || !toastHost) { return; }

    var storageKey = "doanweb.w6.upcoming.notified";
    var polling = false;

    function showBadge(count) {
        badgeHost.textContent = "";
        if (!count) { return; }

        var link = document.createElement("a");
        link.className = "w6-notification-badge";
        link.href = "/LopHoc";
        link.title = count + " lớp sắp bắt đầu trong 24 giờ tới";
        link.setAttribute("aria-label", link.title);
        link.innerHTML = '<svg class="vo-icon" viewBox="0 0 16 16" aria-hidden="true" focusable="false" xmlns="http://www.w3.org/2000/svg"><path d="M4.2 11.2h7.6l-1.1-1.7V7a2.7 2.7 0 0 0-5.4 0v2.5l-1.1 1.7Z"/><path d="M8 2.2V1.2M6.7 12.6a1.35 1.35 0 0 0 2.6 0"/></svg>';

        var number = document.createElement("span");
        number.className = "w6-notification-count";
        number.textContent = count > 99 ? "99+" : String(count);
        link.appendChild(number);
        badgeHost.appendChild(link);
    }

    function dismissToast(toast) {
        toast.classList.add("is-hiding");
        window.setTimeout(function () {
            if (toast.parentNode) { toast.parentNode.removeChild(toast); }
        }, 260);
    }

    function showToast(item) {
        var toast = document.createElement("div");
        toast.className = "vo-toast vo-toast-warning";
        toast.setAttribute("role", "status");

        var dot = document.createElement("span");
        dot.className = "vo-toast-dot";
        dot.innerHTML = '<svg class="vo-icon" viewBox="0 0 16 16" aria-hidden="true" focusable="false" xmlns="http://www.w3.org/2000/svg"><path d="M4.2 11.2h7.6l-1.1-1.7V7a2.7 2.7 0 0 0-5.4 0v2.5l-1.1 1.7Z"/><path d="M8 2.2V1.2M6.7 12.6a1.35 1.35 0 0 0 2.6 0"/></svg>';

        var text = document.createElement("span");
        var start = item.batDau ? new Date(item.batDau) : null;
        var time = start && !isNaN(start.getTime())
            ? " · " + start.toLocaleTimeString("vi-VN", { hour: "2-digit", minute: "2-digit" })
            : "";
        var className = item.tenLop || item.monHoc || "Lớp học";
        var student = item.hocVien ? " · " + item.hocVien : "";
        var tutor = item.giaSu ? " · " + item.giaSu : "";
        text.textContent = className + time + student + tutor;

        var link = document.createElement("a");
        link.href = "/LopHoc/Details/" + encodeURIComponent(item.id);
        link.className = "w6-upcoming-toast";
        link.appendChild(dot);
        link.appendChild(text);
        toast.appendChild(link);
        toastHost.appendChild(toast);

        var timer = window.setTimeout(function () { dismissToast(toast); }, 6000);
        toast.addEventListener("click", function () {
            window.clearTimeout(timer);
            dismissToast(toast);
        });
    }

    function poll() {
        if (polling) { return; }
        polling = true;
        fetch("/ThongBao/SapBatDau", { headers: { "Accept": "application/json" }, credentials: "same-origin" })
            .then(function (response) {
                if (!response.ok || response.redirected ||
                    !(response.headers.get("content-type") || "").includes("application/json")) {
                    return null;
                }
                return response.json();
            })
            .then(function (data) {
                if (!data) { return; }
                showBadge(Number(data.count) || 0);

                var previous;
                try {
                    previous = new Set((window.localStorage.getItem(storageKey) || "")
                        .split(",").filter(Boolean));
                } catch (error) {
                    console.error("Không đọc được danh sách thông báo đã hiển thị.", error);
                    return;
                }

                var known = new Set(previous);
                (data.classes || []).forEach(function (item) {
                    var id = String(item.id);
                    if (!previous.has(id)) { showToast(item); }
                    known.add(id);
                });

                try {
                    window.localStorage.setItem(storageKey, Array.from(known).slice(-200).join(","));
                } catch (error) {
                    console.error("Không lưu được danh sách thông báo đã hiển thị.", error);
                }
            })
            .catch(function (error) {
                console.error("Không thể tải thông báo lớp sắp bắt đầu.", error);
            })
            .then(function () { polling = false; });
    }

    poll();
    window.setInterval(poll, 30000);
}());
