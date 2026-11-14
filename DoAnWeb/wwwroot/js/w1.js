(() => {
    document.addEventListener("show.bs.modal", event => {
        const modal = event.target;
        const trigger = event.relatedTarget;
        if (!(modal instanceof HTMLElement) || !(trigger instanceof HTMLElement)) return;

        const deleteId = trigger.dataset.w1DeleteId;
        if (deleteId) {
            const input = modal.querySelector("[data-w1-delete-input]");
            const label = modal.querySelector("[data-w1-delete-label]");
            if (input) input.value = deleteId;
            if (label) label.textContent = trigger.dataset.w1DeleteName || "";
        }

        const accountId = trigger.dataset.w1AccountId;
        if (accountId) {
            const input = modal.querySelector("[data-w1-account-input]");
            const name = modal.querySelector("[data-w1-account-name-label]");
            const action = modal.querySelector("[data-w1-account-action-label]");
            if (input) input.value = accountId;
            if (name) name.textContent = trigger.dataset.w1AccountName || "";
            if (action) action.textContent = trigger.dataset.w1AccountAction || "";
        }
    });

    const role = document.querySelector("[data-w1-role]");
    const teacher = document.querySelector("[data-w1-teacher]");
    const teacherField = document.querySelector("[data-w1-teacher-field]");
    const updateTeacherField = () => {
        if (!role || !teacher || !teacherField) return;
        const isTutor = role.value === "GiaSu";
        teacher.disabled = !isTutor;
        teacher.required = isTutor;
        teacherField.classList.toggle("d-none", !isTutor);
    };
    if (role) {
        role.addEventListener("change", updateTeacherField);
        updateTeacherField();
    }
})();
