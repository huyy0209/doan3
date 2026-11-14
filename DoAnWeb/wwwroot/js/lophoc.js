(function () {
    "use strict";

    function normalize(value) {
        return String(value || "")
            .normalize("NFD")
            .replace(/[\u0300-\u036f]/g, "")
            .replace(/đ/gi, function (letter) { return letter === "Đ" ? "D" : "d"; })
            .toLowerCase()
            .trim();
    }

    function setHidden(element, hidden) {
        if (element) { element.hidden = hidden; }
    }

    function formatDate(date) {
        return date.getFullYear() + "-" + String(date.getMonth() + 1).padStart(2, "0") + "-" +
            String(date.getDate()).padStart(2, "0");
    }

    function formatDisplayDate(value, includeYear) {
        var date = parseDate(value);
        if (!date) { return ""; }
        var result = String(date.getDate()).padStart(2, "0") + "/" +
            String(date.getMonth() + 1).padStart(2, "0");
        return includeYear ? result + "/" + date.getFullYear() : result;
    }

    function parseDate(value) {
        if (!value) { return null; }
        var parts = value.split("-").map(Number);
        if (parts.length !== 3 || !parts[0] || !parts[1] || !parts[2]) { return null; }
        return new Date(parts[0], parts[1] - 1, parts[2]);
    }

    function addMonthsClamped(date, months) {
        var day = date.getDate();
        var result = new Date(date.getFullYear(), date.getMonth() + months, 1);
        var maxDay = new Date(result.getFullYear(), result.getMonth() + 1, 0).getDate();
        result.setDate(Math.min(day, maxDay));
        return result;
    }

    function selectedDays(checkboxes) {
        return Array.prototype.map.call(checkboxes, function (checkbox) {
            return checkbox.checked ? Number(checkbox.dataset.thu) : null;
        }).filter(function (day) { return day !== null; }).sort(function (a, b) { return a - b; });
    }

    function daysValue(days) {
        return days.length === 0 || days.length === 7 ? "" : days.join(",");
    }

    function daySet(value) {
        if (!value) { return new Set([2, 3, 4, 5, 6, 7, 8]); }
        var days = value.split(",").map(Number).filter(function (day) { return day >= 2 && day <= 8; });
        return days.length === 0 ? new Set([2, 3, 4, 5, 6, 7, 8]) : new Set(days);
    }

    function overlaps(lop, maNguoi, loai, ca, batDau, ketThuc, thuHoc) {
        if (!ca || !batDau || !ketThuc || String(lop.ca || "").toLowerCase() !== ca.toLowerCase()) {
            return false;
        }
        if (Number(loai === "hv" ? lop.maHV : lop.maGS) !== Number(maNguoi)) { return false; }

        var dauCu = lop.batDau;
        var ketCu = lop.ketThuc;
        if (!dauCu || !ketCu) { return true; }
        var dau = batDau > dauCu ? batDau : dauCu;
        var ket = ketThuc < ketCu ? ketThuc : ketCu;
        if (dau > ket) { return false; }

        var thuMoi = daySet(thuHoc);
        var thuCu = daySet(lop.thu);
        var ngay = parseDate(dau);
        var ngayCuoi = parseDate(ket);
        for (var i = 0; i < 7 && ngay <= ngayCuoi; i++) {
            var thu = ngay.getDay() === 0 ? 8 : ngay.getDay() + 1;
            if (thuMoi.has(thu) && thuCu.has(thu)) { return true; }
            ngay.setDate(ngay.getDate() + 1);
        }
        return false;
    }

    function initConfirmModal() {
        var modal = document.getElementById("modal-xac-nhan");
        if (!modal) { return; }
        modal.addEventListener("show.bs.modal", function (event) {
            var trigger = event.relatedTarget;
            if (!trigger) { return; }
            var form = document.getElementById("form-xac-nhan");
            var title = document.getElementById("modal-xac-nhan-title");
            var body = document.getElementById("modal-xac-nhan-body");
            var button = document.getElementById("modal-xac-nhan-nut");
            form.action = trigger.dataset.xacNhanAction || "";
            title.textContent = trigger.dataset.xacNhanTitle || "Xác nhận";
            body.textContent = trigger.dataset.xacNhanBody || "";
            button.textContent = trigger.dataset.xacNhanNut || "Đồng ý";
            button.classList.toggle("btn-danger", /xóa/i.test(button.textContent));
            button.classList.toggle("btn-primary", !/xóa/i.test(button.textContent));
        });
    }

    function initEditDays() {
        var page = document.getElementById("sua-lop-page");
        var form = document.getElementById("form-sua-lop");
        if (!page || !form) { return; }
        var checks = document.querySelectorAll(".thu-check-edit");
        var hidden = document.getElementById("thu-hoc-edit");
        var hint = document.getElementById("so-buoi-hint");
        var start = parseDate(page.dataset.batDau);
        var endInput = document.getElementById("ngay-ket-thuc");
        if (!checks.length || !hidden || !hint || !endInput) { return; }

        function update() {
            var days = selectedDays(checks);
            hidden.value = daysValue(days);
            Array.prototype.forEach.call(checks, function (checkbox) {
                checkbox.closest(".thu-o").classList.toggle("is-selected", checkbox.checked);
            });
            if (!start || !parseDate(endInput.value) || days.length === 0) {
                hint.textContent = days.length === 0 ? "Chọn ít nhất một thứ học." : "";
                hint.classList.toggle("text-danger", days.length === 0);
                return;
            }

            var end = parseDate(endInput.value);
            if (end < start) {
                hint.textContent = "Ngày kết thúc phải sau ngày bắt đầu.";
                hint.classList.add("text-danger");
                return;
            }
            var count = 0;
            var cursor = new Date(start.getFullYear(), start.getMonth(), start.getDate());
            while (cursor <= end) {
                var day = cursor.getDay() === 0 ? 8 : cursor.getDay() + 1;
                if (days.indexOf(day) !== -1) { count++; }
                cursor.setDate(cursor.getDate() + 1);
            }
            hint.textContent = count + " buổi học trong khoảng ngày đã chọn.";
            hint.classList.remove("text-danger");
        }

        Array.prototype.forEach.call(checks, function (checkbox) {
            checkbox.addEventListener("change", update);
        });
        endInput.addEventListener("change", update);
        form.addEventListener("submit", function (event) {
            if (selectedDays(checks).length === 0 || !parseDate(endInput.value) || !start || parseDate(endInput.value) < start) {
                event.preventDefault();
                update();
            }
        });
        update();
    }

    function initGhepLop() {
        var page = document.getElementById("ghep-lop-page");
        if (!page) { return; }

        var form = document.getElementById("form-ghep-lop");
        var data = JSON.parse(document.getElementById("du-lieu-ghep").textContent);
        var hocViens = data.hocViens || [];
        var giaSus = data.giaSus || [];
        var lops = data.lops || [];
        var hvCards = Array.prototype.slice.call(document.querySelectorAll("#ds-hv .ghep-item"));
        var gsCards = Array.prototype.slice.call(document.querySelectorAll("#ds-gs .ghep-item"));
        var hvSearch = document.getElementById("tim-hv");
        var gsSearch = document.getElementById("tim-gs");
        var monSelect = document.getElementById("cbo-mon");
        var capSelect = document.getElementById("cbo-cap");
        var capWrap = document.getElementById("khung-cap");
        var caCards = Array.prototype.slice.call(document.querySelectorAll("#ca-grid .ca-o"));
        var thuChecks = Array.prototype.slice.call(document.querySelectorAll(".thu-check"));
        var thuHidden = document.getElementById("thu-hoc");
        var startInput = document.getElementById("ngay-bat-dau");
        var endInput = document.getElementById("ngay-ket-thuc");
        var priceInput = document.getElementById("hoc-phi");
        var nameInput = document.getElementById("ten-lop");
        var status = document.getElementById("ghep-status");
        var summary = document.getElementById("ghep-tom-tat");
        var submit = document.getElementById("nut-ghep");
        var onlySubject = document.getElementById("chi-cung-mon");
        var onlyFree = document.getElementById("chi-trong");
        var hvEmpty = document.getElementById("hv-trong");
        var gsEmpty = document.getElementById("gs-trong");
        var nameTouched = nameInput.value.trim().length > 0;
        var requestSequence = 0;
        var requestTimer = 0;
        var checking = false;
        var latestStatus = { mucDo: "trong" };

        function getChecked(selector) {
            return document.querySelector(selector + ":checked");
        }

        function selectedHV() {
            var radio = getChecked(".ghep-radio-hv");
            return radio ? Number(radio.value) : 0;
        }

        function selectedGS() {
            var radio = getChecked(".ghep-radio-gs");
            return radio ? Number(radio.value) : 0;
        }

        function selectedCa() {
            var radio = getChecked("#ca-grid input[type=radio]");
            return radio ? radio.value : "";
        }

        function selectedThu() {
            return daysValue(selectedDays(thuChecks));
        }

        function currentName() {
            return nameInput.value.trim();
        }

        function updateHiddenDays() {
            var days = selectedDays(thuChecks);
            thuHidden.value = daysValue(days);
            Array.prototype.forEach.call(thuChecks, function (checkbox) {
                checkbox.closest(".thu-o").classList.toggle("is-selected", checkbox.checked);
            });
            return days;
        }

        function personLopConflict(ma, loai, ca) {
            var days = selectedDays(thuChecks);
            if (!days.length || !parseDate(startInput.value) || !parseDate(endInput.value)) { return null; }
            var start = startInput.value;
            var end = endInput.value;
            return lops.find(function (lop) {
                return overlaps(lop, ma, loai, ca, start, end, selectedThu());
            }) || null;
        }

        function updateStudentInfo() {
            var id = selectedHV();
            var hv = hocViens.find(function (item) { return Number(item.ma) === id; });
            var info = document.getElementById("hv-info");
            if (!hv) {
                info.textContent = "";
                return;
            }
            var parts = [];
            if (hv.tenPh) { parts.push("Phụ huynh " + hv.tenPh); }
            if (hv.sdt || hv.sdtPh) { parts.push(hv.sdtPh || hv.sdt); }
            if (hv.trinhDo) { parts.push(hv.trinhDo); }
            if (hv.soLop) { parts.push(hv.soLop + " lớp đang học" + (hv.lopText ? ": " + hv.lopText : "")); }
            info.textContent = parts.join(" · ");
        }

        function updateCapDo() {
            var option = monSelect.options[monSelect.selectedIndex];
            var mon = (data.mons || []).find(function (item) {
                return item.ten === (option ? option.value : "");
            });
            var caps = mon && Array.isArray(mon.caps) ? mon.caps : [];
            var hv = hocViens.find(function (item) { return Number(item.ma) === selectedHV(); });
            var capHocVien = hv && caps.find(function (cap) {
                return normalize(cap) === normalize(hv.trinhDo);
            });
            capSelect.replaceChildren();
            caps.forEach(function (cap, index) {
                var item = document.createElement("option");
                item.value = cap;
                item.textContent = cap;
                item.selected = capHocVien ? cap === capHocVien : index === 0;
                capSelect.appendChild(item);
            });
            setHidden(capWrap, caps.length === 0);
        }

        function updateStudentCards() {
            var query = normalize(hvSearch.value);
            var visible = 0;
            hvCards.forEach(function (card) {
                var matches = normalize([card.dataset.ten, card.dataset.sdt, card.dataset.ph,
                    card.dataset.sdtph, card.dataset.truong, card.dataset.trinhdo].join(" ")).includes(query);
                setHidden(card, !matches);
                if (matches) { visible++; }
                else {
                    var radio = card.querySelector("input[type=radio]");
                    if (radio.checked) { radio.checked = false; }
                }
            });
            setHidden(hvEmpty, visible !== 0);
        }

        function updateTutorCards() {
            var query = normalize(gsSearch.value);
            var mon = normalize(monSelect.value);
            var ca = selectedCa();
            var onlyMon = onlySubject.checked && !!mon;
            var onlyAvailable = onlyFree.checked && !!ca;
            var selected = selectedGS();
            var visible = 0;

            gsCards.forEach(function (card) {
                var tutor = giaSus.find(function (item) { return Number(item.ma) === Number(card.dataset.ma); });
                if (!tutor) { return; }
                var searchMatch = normalize([card.dataset.ten, card.dataset.sdt, card.dataset.chuyenmon].join(" "))
                    .includes(query);
                var subjectMatch = !onlyMon || normalize(tutor.chuyenMon) === mon;
                var caIndex = ca ? caCards.findIndex(function (item) { return item.dataset.ca === ca; }) : -1;
                var accepts = !ca || caIndex < 0 || Boolean(tutor.nhanCa && tutor.nhanCa[caIndex]);
                var conflict = ca ? personLopConflict(tutor.ma, "gs", ca) : null;
                var available = accepts && !conflict;
                var visibleByFilters = searchMatch && subjectMatch && (!onlyAvailable || available);
                var tag = card.querySelector(".gs-tag");

                tag.textContent = !ca ? "" : conflict ? "Trùng lịch" : !accepts ? "Không nhận ca này" : "Còn trống";
                tag.className = "gs-tag" + (conflict ? " is-busy" : !accepts ? " is-unavailable" : ca ? " is-free" : "");
                card.classList.toggle("is-busy", !!conflict);
                setHidden(card, !visibleByFilters);
                if (visibleByFilters) { visible++; }
                else {
                    var radio = card.querySelector("input[type=radio]");
                    if (radio.checked) { radio.checked = false; }
                }
                card.dataset.available = available ? "1" : "0";
                card.dataset.soLop = String(Number(tutor.soLop) || 0);
                card.dataset.tenSort = normalize(tutor.ten);
            });

            gsCards.sort(function (a, b) {
                var availability = Number(b.dataset.available) - Number(a.dataset.available);
                return availability || Number(a.dataset.soLop) - Number(b.dataset.soLop) ||
                    a.dataset.tenSort.localeCompare(b.dataset.tenSort);
            }).forEach(function (card) {
                card.parentNode.appendChild(card);
            });
            setHidden(gsEmpty, visible !== 0);
            if (selected && !gsCards.some(function (card) {
                return Number(card.dataset.ma) === selected && !card.hidden &&
                    card.querySelector("input[type=radio]").checked;
            })) {
                var selectedRadio = document.querySelector('.ghep-radio-gs[value="' + selected + '"]');
                if (selectedRadio) { selectedRadio.checked = false; }
            }
        }

        function updateCaCards() {
            var hv = selectedHV();
            var thu = selectedThu();
            caCards.forEach(function (card) {
                var busy = hv && personLopConflict(hv, "hv", card.dataset.ca);
                card.classList.toggle("is-selected", !!card.querySelector("input").checked);
                card.classList.toggle("is-busy", !!busy);
                card.querySelector(".ca-ban").textContent = busy ? "Bận" : "";
                card.title = busy ? "Học viên bị trùng lịch ở ca này." : "";
            });
        }

        function updateSummary() {
            var hv = hocViens.find(function (item) { return Number(item.ma) === selectedHV(); });
            var gs = giaSus.find(function (item) { return Number(item.ma) === selectedGS(); });
            var ca = selectedCa();
            var monOption = monSelect.options[monSelect.selectedIndex];
            var mon = monOption && monOption.value ? monOption.value : "";
            var price = priceInput.value.replace(/\D/g, "");
            var missing = [];
            if (!hv) { missing.push("học viên"); }
            if (!mon) { missing.push("môn học"); }
            if (!ca) { missing.push("ca học"); }
            if (!gs) { missing.push("gia sư"); }
            if (!price || Number(price) <= 0 || price.length > 12) { missing.push("học phí"); }
            if (selectedDays(thuChecks).length === 0) { missing.push("thứ học"); }
            if (!parseDate(startInput.value) || !parseDate(endInput.value)) { missing.push("ngày học"); }

            if (missing.length) {
                summary.textContent = "Còn thiếu: " + missing.join(", ") + ".";
            } else {
                var cap = capSelect.value ? " (" + capSelect.value + ")" : "";
                summary.textContent = hv.ten + " học " + mon + cap + " với " + gs.ten + ", " +
                    (status.dataset.moTa || "") + " ca " + ca + ", " +
                    formatDisplayDate(startInput.value, false) + " – " +
                    formatDisplayDate(endInput.value, true) + ", " + Number(price).toLocaleString("vi-VN") + " đ.";
            }
            submit.disabled = checking || missing.length > 0 || latestStatus.mucDo === "loi";
        }

        function setStatus(result) {
            latestStatus = result || { mucDo: "loi", thongBao: "Không nhận được kết quả kiểm tra lịch." };
            status.className = "ghep-status is-" + (latestStatus.mucDo || "loi");
            status.textContent = latestStatus.thongBao || "";
            status.dataset.moTa = latestStatus.moTa || "";
            updateSummary();
        }

        function checkSchedule() {
            var sequence = ++requestSequence;
            window.clearTimeout(requestTimer);
            checking = true;
            latestStatus = { mucDo: "trong" };
            status.className = "ghep-status is-trong";
            status.textContent = "Đang kiểm tra lịch học…";
            status.dataset.moTa = "";
            updateSummary();
            if (selectedDays(thuChecks).length === 0) {
                checking = false;
                setStatus({ mucDo: "loi", thongBao: "Chọn ít nhất một thứ học." });
                return;
            }
            requestTimer = window.setTimeout(function () {
                var query = new URLSearchParams({
                    maHV: String(selectedHV() || ""),
                    maGS: String(selectedGS() || ""),
                    monHoc: monSelect.value || "",
                    caHoc: selectedCa(),
                    batDau: startInput.value,
                    ketThuc: endInput.value,
                    thuHoc: selectedThu(),
                    tenLop: currentName()
                });
                fetch(page.dataset.kiemtraUrl + "?" + query.toString(), {
                    headers: { "Accept": "application/json" }
                }).then(function (response) {
                    if (!response.ok) { throw new Error("HTTP " + response.status); }
                    return response.json();
                }).then(function (result) {
                    if (sequence === requestSequence) {
                        checking = false;
                        setStatus(result);
                    }
                }).catch(function () {
                    if (sequence === requestSequence) {
                        checking = false;
                        setStatus({ mucDo: "loi", thongBao: "Không thể kiểm tra lịch lúc này. Hãy thử lại." });
                    }
                });
            }, 120);
        }

        function suggestName(force) {
            if (force) { nameTouched = false; }
            if (nameTouched) { return; }
            var query = new URLSearchParams({
                monHoc: monSelect.value || "",
                maHV: String(selectedHV() || ""),
                caHoc: selectedCa()
            });
            fetch(page.dataset.goiytenUrl + "?" + query.toString(), {
                headers: { "Accept": "application/json" }
            }).then(function (response) {
                if (!response.ok) { throw new Error("HTTP " + response.status); }
                return response.json();
            }).then(function (result) {
                if (!nameTouched && result.ten) { nameInput.value = result.ten; }
                checkSchedule();
                updateSummary();
            }).catch(function () {
                setStatus({ mucDo: "loi", thongBao: "Không thể lấy gợi ý tên lớp lúc này." });
            });
        }

        hvSearch.addEventListener("input", function () {
            updateStudentCards();
            updateStudentInfo();
            updateCapDo();
            updateCaCards();
            updateTutorCards();
            suggestName(false);
            checkSchedule();
        });
        gsSearch.addEventListener("input", function () {
            updateTutorCards();
            checkSchedule();
        });
        onlySubject.addEventListener("change", function () {
            updateTutorCards();
            checkSchedule();
        });
        onlyFree.addEventListener("change", function () {
            updateTutorCards();
            checkSchedule();
        });
        monSelect.addEventListener("change", function () {
            updateCapDo();
            updateTutorCards();
            suggestName(false);
            checkSchedule();
        });
        document.getElementById("goi-y-ten").addEventListener("click", function () { suggestName(true); });
        nameInput.addEventListener("input", function () {
            nameTouched = true;
            checkSchedule();
        });
        priceInput.addEventListener("input", updateSummary);
        priceInput.addEventListener("blur", function () {
            var value = priceInput.value.replace(/\D/g, "");
            if (value) { priceInput.value = Number(value).toLocaleString("vi-VN"); }
            updateSummary();
        });
        startInput.addEventListener("change", function () {
            updateCaCards();
            updateTutorCards();
            checkSchedule();
        });
        endInput.addEventListener("change", function () {
            updateCaCards();
            updateTutorCards();
            checkSchedule();
        });
        document.querySelectorAll("[data-thang]").forEach(function (button) {
            button.addEventListener("click", function () {
                var start = parseDate(startInput.value);
                if (!start) { return; }
                endInput.value = formatDate(addMonthsClamped(start, Number(button.dataset.thang)));
                updateCaCards();
                updateTutorCards();
                checkSchedule();
            });
        });
        document.querySelectorAll(".ghep-radio-hv").forEach(function (radio) {
            radio.addEventListener("change", function () {
                updateStudentInfo();
                updateCapDo();
                updateCaCards();
                updateTutorCards();
                suggestName(false);
                checkSchedule();
            });
        });
        document.querySelectorAll(".ghep-radio-gs").forEach(function (radio) {
            radio.addEventListener("change", function () {
                updateTutorCards();
                checkSchedule();
            });
        });
        caCards.forEach(function (card) {
            card.querySelector("input").addEventListener("change", function () {
                updateCaCards();
                updateTutorCards();
                suggestName(false);
                checkSchedule();
            });
        });
        thuChecks.forEach(function (checkbox) {
            checkbox.addEventListener("change", function () {
                updateHiddenDays();
                updateCaCards();
                updateTutorCards();
                checkSchedule();
            });
        });
        form.addEventListener("submit", function (event) {
            updateHiddenDays();
            updateSummary();
            if (submit.disabled || latestStatus.mucDo === "loi" || selectedDays(thuChecks).length === 0) {
                event.preventDefault();
            }
        });

        document.querySelectorAll("#ca-grid .ca-o").forEach(function (card) {
            card.classList.toggle("is-selected", !!card.querySelector("input").checked);
        });
        updateHiddenDays();
        updateStudentCards();
        updateStudentInfo();
        updateCapDo();
        updateCaCards();
        updateTutorCards();
        updateSummary();
        suggestName(false);
        checkSchedule();
    }

    document.addEventListener("DOMContentLoaded", function () {
        initConfirmModal();
        initEditDays();
        initGhepLop();
    });
})();
