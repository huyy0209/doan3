(function () {
    "use strict";

    const svgNs = "http://www.w3.org/2000/svg";
    document.querySelectorAll("[data-score-chart]").forEach(function (container) {
        const points = Array.from(container.querySelectorAll(".ht-score-point"))
            .map(function (node) {
                return { date: node.dataset.date, score: Number(node.dataset.score) };
            })
            .filter(function (point) { return Number.isFinite(point.score); });
        const svg = document.createElementNS(svgNs, "svg");
        svg.setAttribute("viewBox", "0 0 700 160");
        svg.setAttribute("role", "img");
        svg.setAttribute("aria-label", "Biểu đồ điểm test theo ngày");
        svg.setAttribute("preserveAspectRatio", "none");

        const left = 38, right = 688, top = 14, bottom = 128;
        [0, 5, 8, 10].forEach(function (value) {
            const y = bottom - value * (bottom - top) / 10;
            const line = document.createElementNS(svgNs, "line");
            line.setAttribute("x1", String(left));
            line.setAttribute("x2", String(right));
            line.setAttribute("y1", String(y));
            line.setAttribute("y2", String(y));
            line.setAttribute("class", "ht-chart-grid");
            svg.appendChild(line);

            const label = document.createElementNS(svgNs, "text");
            label.setAttribute("x", "4");
            label.setAttribute("y", String(y + 4));
            label.setAttribute("class", "ht-chart-label");
            label.textContent = String(value);
            svg.appendChild(label);
        });

        if (points.length === 0) {
            const empty = document.createElementNS(svgNs, "text");
            empty.setAttribute("x", "350");
            empty.setAttribute("y", "78");
            empty.setAttribute("text-anchor", "middle");
            empty.setAttribute("class", "ht-chart-empty");
            empty.textContent = "Chưa có bài test nào.";
            svg.appendChild(empty);
        } else {
            const coords = points.map(function (point, index) {
                const x = points.length === 1 ? (left + right) / 2 : left + 8 + index * (right - left - 16) / (points.length - 1);
                const y = bottom - Math.max(0, Math.min(10, point.score)) * (bottom - top) / 10;
                return { x: x, y: y, point: point };
            });
            if (coords.length > 1) {
                const line = document.createElementNS(svgNs, "polyline");
                line.setAttribute("points", coords.map(function (p) { return p.x + "," + p.y; }).join(" "));
                line.setAttribute("class", "ht-chart-line");
                svg.appendChild(line);
            }
            coords.forEach(function (coordinate, index) {
                const circle = document.createElementNS(svgNs, "circle");
                circle.setAttribute("cx", String(coordinate.x));
                circle.setAttribute("cy", String(coordinate.y));
                circle.setAttribute("r", "4");
                circle.setAttribute("class", "ht-chart-point");
                const title = document.createElementNS(svgNs, "title");
                title.textContent = coordinate.point.date + ": " + coordinate.point.score;
                circle.appendChild(title);
                svg.appendChild(circle);
                if (index === 0 || index === coords.length - 1) {
                    const date = document.createElementNS(svgNs, "text");
                    date.setAttribute("x", String(coordinate.x));
                    date.setAttribute("y", "151");
                    date.setAttribute("text-anchor", "middle");
                    date.setAttribute("class", "ht-chart-label");
                    date.textContent = coordinate.point.date.slice(5).replace("-", "/");
                    svg.appendChild(date);
                }
            });
        }
        container.replaceChildren(svg);
    });

    const xepModalElement = document.getElementById("xepHocBuModal");
    const confirmModalElement = document.getElementById("xacNhanHocTapModal");
    if (!xepModalElement || !confirmModalElement) return;

    const xepModal = bootstrap.Modal.getOrCreateInstance(xepModalElement);
    const confirmModal = bootstrap.Modal.getOrCreateInstance(confirmModalElement);
    const xepForm = document.getElementById("xepHocBuForm");
    const xepDay = document.getElementById("xepNgay");
    const xepShift = document.getElementById("xepCa");
    const xepTeacher = document.getElementById("xepGiaSu");
    const xepError = document.getElementById("hocBuLoi");
    const teacherReason = document.getElementById("xepGiaSuLyDo");
    let activeMakeup = null;
    let confirmAction = null;
    let teacherRequest = 0;

    function showConfirm(message, action) {
        document.getElementById("xacNhanHocTapNoiDung").textContent = message;
        confirmAction = action;
        confirmModal.show();
    }

    function localDateValue() {
        const now = new Date();
        return now.getFullYear() + "-" + String(now.getMonth() + 1).padStart(2, "0") + "-" +
            String(now.getDate()).padStart(2, "0");
    }

    async function loadTeachers() {
        if (!activeMakeup || !xepDay.value || !xepShift.value) return;
        const request = ++teacherRequest;
        const previous = xepTeacher.value || activeMakeup.maGs;
        const url = new URL("/HocTap/GiaSuHocBu", window.location.origin);
        url.searchParams.set("ngay", xepDay.value);
        url.searchParams.set("ca", xepShift.value);
        url.searchParams.set("monHoc", activeMakeup.mon);
        teacherReason.textContent = "Đang tải gia sư...";
        xepTeacher.replaceChildren();
        try {
            const response = await fetch(url);
            if (!response.ok) throw new Error("Không tải được danh sách gia sư.");
            const teachers = await response.json();
            if (request !== teacherRequest) return;
            teachers.forEach(function (teacher) {
                const option = document.createElement("option");
                option.value = teacher.maGS;
                option.textContent = teacher.hoTen + " (" + (teacher.chuyenMon || "") + ")" +
                    (teacher.ban ? " - bận: " + teacher.ban : "");
                option.dataset.reason = teacher.ban || "";
                xepTeacher.appendChild(option);
            });
            if (!teachers.length) {
                teacherReason.textContent = "Không có gia sư phù hợp.";
                return;
            }
            const preferred = Array.from(xepTeacher.options).find(function (option) { return option.value === previous; });
            if (preferred) xepTeacher.value = preferred.value;
            updateTeacherReason();
        } catch (error) {
            if (request === teacherRequest) teacherReason.textContent = error.message || "Không tải được danh sách gia sư.";
        }
    }

    function updateTeacherReason() {
        const option = xepTeacher.selectedOptions[0];
        teacherReason.textContent = option
            ? (option.dataset.reason ? "Gia sư đang bận: " + option.dataset.reason + ". Vẫn có thể xếp học chung." : "Gia sư rảnh ở ca này.")
            : "";
        teacherReason.className = "small mt-1 " + (option && option.dataset.reason ? "text-warning" : "text-success");
    }

    document.querySelectorAll(".js-open-makeup").forEach(function (button) {
        button.addEventListener("click", function () {
            activeMakeup = { maBuoi: button.dataset.maBuoi, maGs: button.dataset.maGs, mon: button.dataset.mon };
            document.getElementById("xepMaBuoiVang").value = activeMakeup.maBuoi;
            document.getElementById("xepHocBuMoTa").textContent =
                "Bù cho buổi vắng " + button.dataset.ngayVang + " của lớp " + activeMakeup.mon + ".";
            xepError.classList.add("d-none");
            document.getElementById("boQuaCanhBao").value = "false";
            xepDay.value = localDateValue();
            const otherShift = Array.from(xepShift.options).find(function (option) { return option.value !== button.dataset.ca; });
            xepShift.value = otherShift ? otherShift.value : (xepShift.options[0]?.value || "");
            xepModal.show();
            loadTeachers();
        });
    });

    xepDay.addEventListener("change", loadTeachers);
    xepShift.addEventListener("change", loadTeachers);
    xepTeacher.addEventListener("change", updateTeacherReason);

    async function submitMakeup() {
        const submit = xepForm.querySelector('button[type="submit"]');
        submit.disabled = true;
        xepError.classList.add("d-none");
        try {
            const response = await fetch(xepForm.action, { method: "POST", body: new FormData(xepForm) });
            const result = await response.json();
            const warning = result.canhBao || result.CanhBao;
            const error = result.loi || result.Loi;
            if (!response.ok) throw new Error(error || "Không thể xếp học bù.");
            if (warning) {
                showConfirm(warning, function () {
                    document.getElementById("boQuaCanhBao").value = "true";
                    submitMakeup();
                });
            } else if (error) {
                xepError.textContent = error;
                xepError.classList.remove("d-none");
            } else {
                window.location.reload();
            }
        } catch (error) {
            xepError.textContent = error.message || "Không thể xếp học bù.";
            xepError.classList.remove("d-none");
        } finally {
            submit.disabled = false;
        }
    }

    xepForm.addEventListener("submit", function (event) {
        event.preventDefault();
        document.getElementById("boQuaCanhBao").value = "false";
        submitMakeup();
    });

    document.querySelectorAll(".js-cancel-makeup").forEach(function (button) {
        button.addEventListener("click", function () {
            showConfirm("Hủy buổi học bù ngày " + button.dataset.ngay +
                "? Điểm danh và điểm của buổi bù sẽ bị xóa, buổi vắng được giữ nguyên.", function () {
                    document.getElementById("huyMaBuoiBu").value = button.dataset.maBuoiBu;
                    document.getElementById("huyHocBuForm").submit();
                });
        });
    });

    document.getElementById("xacNhanHocTapDongY").addEventListener("click", function () {
        const action = confirmAction;
        confirmAction = null;
        confirmModal.hide();
        if (action) action();
    });
})();
