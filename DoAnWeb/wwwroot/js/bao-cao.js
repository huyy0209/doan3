document.addEventListener('click', function (event) {
    var button = event.target.closest('[data-bao-cao-in]');
    if (button) window.print();
});
