const CACHE_NAME = "quan-ly-gia-su-v1";
const OFFLINE_URL = "/offline.html";
const TAI_NGUYEN_TINH = /^\/(lib|css|js|img|fonts)\//i;

self.addEventListener("install", event => {
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then(cache => cache.add(OFFLINE_URL))
            .then(() => self.skipWaiting())
    );
});

self.addEventListener("activate", event => {
    event.waitUntil(
        caches.keys()
            .then(keys => Promise.all(keys
                .filter(key => key.startsWith("quan-ly-gia-su-") && key !== CACHE_NAME)
                .map(key => caches.delete(key))))
            .then(() => self.clients.claim())
    );
});

self.addEventListener("fetch", event => {
    const request = event.request;
    const url = new URL(request.url);
    if (request.method !== "GET" || url.origin !== self.location.origin) return;

    if (request.mode === "navigate") {
        event.respondWith(fetch(request).catch(() => caches.match(OFFLINE_URL)));
        return;
    }

    if (!TAI_NGUYEN_TINH.test(url.pathname)) return;
    // Tách bản sao ngay khi có phản hồi từ mạng (trước khi trình duyệt đọc body), tránh lỗi "body already used"
    let banSao = null;
    const response = caches.match(request).then(cached => cached || fetch(request).then(ketQua => {
        banSao = ketQua.clone();
        return ketQua;
    }));
    event.respondWith(response);
    event.waitUntil(response.then(() => {
        if (!banSao) return;
        const contentType = banSao.headers.get("content-type") || "";
        if (banSao.ok && !contentType.includes("text/html")) {
            return caches.open(CACHE_NAME).then(cache => cache.put(request, banSao));
        }
    }));
});
