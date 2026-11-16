document.addEventListener("click", function (event) {
    if (event.target.closest("[data-in-an]")) window.print();
});
