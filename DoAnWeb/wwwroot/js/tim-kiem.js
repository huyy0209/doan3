(function () {
    "use strict";

    var form = document.getElementById("vo-tim-kiem-form");
    var input = document.getElementById("vo-tim-kiem-q");
    var hop = document.getElementById("vo-tim-kiem-goi-y");
    if (!form || !input || !hop) { return; }

    var CHO_MS = 250;
    var henGio = null;
    var phienBan = 0;
    var dsDong = [];
    var dangChon = -1;

    var NHOM = [
        { khoa: "hocViens", nhan: "Học viên" },
        { khoa: "giaSus", nhan: "Gia sư" },
        { khoa: "lops", nhan: "Lớp học" }
    ];

    function dongHop() {
        hop.hidden = true;
        hop.textContent = "";
        input.setAttribute("aria-expanded", "false");
        dsDong = [];
        dangChon = -1;
    }

    function boChon() {
        if (dangChon >= 0 && dsDong[dangChon]) {
            dsDong[dangChon].classList.remove("is-chon");
            dsDong[dangChon].setAttribute("aria-selected", "false");
        }
        dangChon = -1;
    }

    function chon(index) {
        if (!dsDong.length) { return; }
        boChon();
        dangChon = (index + dsDong.length) % dsDong.length;
        var dong = dsDong[dangChon];
        dong.classList.add("is-chon");
        dong.setAttribute("aria-selected", "true");
        if (dong.scrollIntoView) { dong.scrollIntoView({ block: "nearest" }); }
    }

    function taoDong(item) {
        var a = document.createElement("a");
        a.className = "tk-goiy-dong";
        a.href = item.url || "#";
        a.setAttribute("role", "option");
        a.setAttribute("aria-selected", "false");

        var chinh = document.createElement("span");
        chinh.className = "tk-goiy-chinh";
        chinh.textContent = item.tieuDe || "(không tên)";
        a.appendChild(chinh);

        if (item.phu) {
            var phu = document.createElement("span");
            phu.className = "tk-goiy-phu";
            phu.textContent = item.phu;
            a.appendChild(phu);
        }
        return a;
    }

    function ve(data) {
        hop.textContent = "";
        dsDong = [];
        dangChon = -1;

        var tong = 0;
        NHOM.forEach(function (nhom) {
            var ds = data[nhom.khoa] || [];
            if (!ds.length) { return; }
            tong += ds.length;

            var tieuDe = document.createElement("div");
            tieuDe.className = "tk-goiy-nhom";
            tieuDe.textContent = nhom.nhan;
            hop.appendChild(tieuDe);

            ds.forEach(function (item) {
                var dong = taoDong(item);
                hop.appendChild(dong);
                dsDong.push(dong);
            });
        });

        if (!tong) {
            var trong = document.createElement("div");
            trong.className = "tk-goiy-trong";
            trong.textContent = "Không có gợi ý.";
            hop.appendChild(trong);
        }

        hop.hidden = false;
        input.setAttribute("aria-expanded", "true");
    }

    function tai(q, phien) {
        fetch("/TimKiem/GoiY?q=" + encodeURIComponent(q), {
            headers: { "Accept": "application/json" },
            credentials: "same-origin"
        })
            .then(function (response) {
                if (!response.ok || response.redirected ||
                    !(response.headers.get("content-type") || "").includes("application/json")) {
                    return null;
                }
                return response.json();
            })
            .then(function (data) {
                if (phien !== phienBan) { return; }
                if (data) { ve(data); } else { dongHop(); }
            })
            .catch(function (error) {
                if (phien === phienBan) { dongHop(); }
                console.error("Không tải được gợi ý tìm kiếm.", error);
            });
    }

    input.addEventListener("input", function () {
        var q = input.value.trim();
        phienBan++;
        if (henGio) { window.clearTimeout(henGio); }
        if (q.length < 2) {
            dongHop();
            return;
        }
        var phien = phienBan;
        henGio = window.setTimeout(function () { tai(q, phien); }, CHO_MS);
    });

    input.addEventListener("keydown", function (event) {
        if (event.key === "ArrowDown") {
            if (dsDong.length) { event.preventDefault(); chon(dangChon + 1); }
        } else if (event.key === "ArrowUp") {
            if (dsDong.length) { event.preventDefault(); chon(dangChon - 1); }
        } else if (event.key === "Enter") {
            if (dangChon >= 0 && dsDong[dangChon]) {
                event.preventDefault();
                window.location.href = dsDong[dangChon].href;
            }
        } else if (event.key === "Escape") {
            if (!hop.hidden) { event.preventDefault(); dongHop(); }
        }
    });

    document.addEventListener("click", function (event) {
        if (!form.contains(event.target)) { dongHop(); }
    });

    function dangGo(element) {
        if (!element) { return false; }
        var tag = (element.tagName || "").toLowerCase();
        return tag === "input" || tag === "textarea" || tag === "select" || element.isContentEditable === true;
    }

    document.addEventListener("keydown", function (event) {
        if (event.defaultPrevented) { return; }
        var laSlash = event.key === "/" && !event.ctrlKey && !event.metaKey &&
            !event.altKey && !dangGo(event.target);
        var laCtrlK = (event.ctrlKey || event.metaKey) && !event.altKey &&
            (event.key === "k" || event.key === "K");
        if (!laSlash && !laCtrlK) { return; }

        event.preventDefault();
        input.focus();
        input.select();
    });
}());
