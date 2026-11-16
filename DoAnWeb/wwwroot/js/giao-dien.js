(() => {
    const khoa = "vo-theme";
    const cacCheDo = ["auto", "light", "dark"];
    const layCheDoLuu = () => {
        try {
            const cheDo = window.localStorage.getItem(khoa);
            return cacCheDo.includes(cheDo) ? cheDo : "auto";
        } catch (loi) {
            console.warn("Không đọc được lựa chọn giao diện đã lưu.", loi);
            return "auto";
        }
    };
    const cheDoMay = window.matchMedia("(prefers-color-scheme: dark)");
    let cheDo = layCheDoLuu();

    const apDungCheDo = () => {
        const giaoDien = cheDo === "auto" ? (cheDoMay.matches ? "dark" : "light") : cheDo;
        document.documentElement.dataset.bsTheme = giaoDien;
        const nut = document.querySelector("[data-theme-toggle]");
        if (nut) {
            const ten = cheDo === "auto" ? "Tự động" : cheDo === "dark" ? "Tối" : "Sáng";
            nut.textContent = `Giao diện: ${ten}`;
            nut.setAttribute("aria-label", `Giao diện hiện tại: ${ten}. Nhấn để đổi.`);
            nut.title = `Đang dùng giao diện ${ten}`;
        }
    };

    apDungCheDo();
    cheDoMay.addEventListener("change", () => {
        if (cheDo === "auto") apDungCheDo();
    });

    document.addEventListener("DOMContentLoaded", () => {
        const nut = document.querySelector("[data-theme-toggle]");
        if (nut) {
            nut.addEventListener("click", () => {
                cheDo = cacCheDo[(cacCheDo.indexOf(cheDo) + 1) % cacCheDo.length];
                try {
                    window.localStorage.setItem(khoa, cheDo);
                } catch (loi) {
                    console.warn("Không lưu được lựa chọn giao diện.", loi);
                }
                apDungCheDo();
            });
            apDungCheDo();
        }

        if ("serviceWorker" in navigator) {
            navigator.serviceWorker.register("/service-worker.js")
                .catch(loi => console.warn("Không đăng ký được service worker.", loi));
        }
    });
})();
