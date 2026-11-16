document.addEventListener("DOMContentLoaded", () => {
    const printButton = document.getElementById("ld-print");
    if (printButton) printButton.addEventListener("click", () => window.print());
});
