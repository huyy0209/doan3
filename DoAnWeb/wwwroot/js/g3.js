(function () {
    "use strict";

    document.querySelectorAll("[data-submit-on-change]").forEach(function (element) {
        element.addEventListener("change", function () { element.form?.submit(); });
    });

    document.querySelectorAll("[data-xac-nhan]").forEach(function (element) {
        element.addEventListener("click", function (event) {
            if (!window.confirm(element.dataset.xacNhan)) event.preventDefault();
        });
    });

    var paymentModal = document.getElementById("w6-xoa-thanh-toan");
    if (paymentModal) paymentModal.addEventListener("show.bs.modal", function (event) {
        var button = event.relatedTarget;
        document.getElementById("w6-xoa-ma-tt").value = button.getAttribute("data-ma-tt");
        document.getElementById("w6-xoa-thanh-toan-text").textContent = button.getAttribute("data-khoan");
    });

    var markAll = document.getElementById("diemDanhTatCa");
    if (markAll) markAll.addEventListener("click", function () {
        document.querySelectorAll('select[name$=".DiemDanh"]').forEach(function (select) {
            if (select.closest("tr")?.dataset.daXepBu !== "true") select.value = "Có mặt";
        });
    });
    document.querySelectorAll('select[name$=".DiemDanh"]').forEach(function (select) {
        select.addEventListener("change", function () {
            if (select.value === "Vắng có phép" || select.value === "Vắng không phép") {
                var input = select.closest("tr")?.querySelector('input[name$=".DiemText"]');
                if (input) input.value = "";
            }
        });
    });
}());
