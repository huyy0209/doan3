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

})();
