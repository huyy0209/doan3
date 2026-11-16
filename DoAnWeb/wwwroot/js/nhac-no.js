/*
 * Nhắc nợ học phí (G6): nút "Sao chép tin nhắn" soạn sẵn lời nhắc lịch sự,
 * chép vào clipboard bằng navigator.clipboard, có đường dự phòng cho trình duyệt
 * không hỗ trợ (http nội bộ). Không dùng onclick/script nội tuyến để hợp CSP.
 */
(function () {
    "use strict";

    var THOI_GIAN_HIEN_LAI = 2000;
    var nutDaChon = document.querySelectorAll(".js-nhac-no-copy");
    if (!nutDaChon.length) { return; }

    function taoTinNhan(tenTrungTam, hocVien, mon, soTien) {
        var dong = [];
        dong.push("Kính gửi Quý phụ huynh em " + hocVien + ",");
        dong.push(tenTrungTam + " xin thông báo: học phí" + (mon ? " môn " + mon : "") +
            " của em hiện còn thiếu " + soTien + " đ.");
        dong.push("Kính mong Quý phụ huynh thu xếp đóng học phí để trung tâm tiếp tục duy trì lớp học cho em.");
        dong.push("Xin chân thành cảm ơn Quý phụ huynh!");
        return dong.join("\n");
    }

    function chepDuPhong(noiDung) {
        var vung = document.createElement("textarea");
        vung.value = noiDung;
        vung.setAttribute("readonly", "readonly");
        vung.setAttribute("aria-hidden", "true");
        vung.style.position = "fixed";
        vung.style.top = "-1000px";
        vung.style.opacity = "0";
        document.body.appendChild(vung);
        vung.select();
        var thanhCong = false;
        try {
            thanhCong = document.execCommand("copy");
        } catch (e) {
            thanhCong = false;
        }
        document.body.removeChild(vung);
        return thanhCong;
    }

    function chep(noiDung) {
        if (navigator.clipboard && window.isSecureContext) {
            return navigator.clipboard.writeText(noiDung).then(function () { return true; },
                function () { return chepDuPhong(noiDung); });
        }
        return Promise.resolve(chepDuPhong(noiDung));
    }

    function baoKetQua(nut, thanhCong) {
        var nhan = nut.querySelector(".js-nhac-no-copy-text");
        if (!nhan) { return; }
        if (!nut.getAttribute("data-nhan-goc")) {
            nut.setAttribute("data-nhan-goc", nhan.textContent);
        }
        nhan.textContent = thanhCong ? "Đã sao chép" : "Không sao chép được";
        nut.classList.add(thanhCong ? "is-copied" : "is-copy-failed");
        window.setTimeout(function () {
            nhan.textContent = nut.getAttribute("data-nhan-goc");
            nut.classList.remove("is-copied", "is-copy-failed");
        }, THOI_GIAN_HIEN_LAI);
    }

    Array.prototype.forEach.call(nutDaChon, function (nut) {
        nut.addEventListener("click", function () {
            var noiDung = taoTinNhan(
                nut.getAttribute("data-ten-trung-tam") || "",
                nut.getAttribute("data-hoc-vien") || "",
                nut.getAttribute("data-mon") || "",
                nut.getAttribute("data-so-tien") || "0");
            chep(noiDung).then(function (thanhCong) { baoKetQua(nut, thanhCong); });
        });
    });
}());
