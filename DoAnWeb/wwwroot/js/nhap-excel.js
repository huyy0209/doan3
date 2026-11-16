document.addEventListener('DOMContentLoaded', () => {
    const loai = document.getElementById('loaiHoSo');
    const mau = document.getElementById('tai-mau');
    if (loai && mau) {
        loai.addEventListener('change', () => {
            mau.href = `/NhapExcel/TaiMau?loai=${encodeURIComponent(loai.value)}`;
        });
    }
});
