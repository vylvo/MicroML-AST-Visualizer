// Add SVG display helpers

document.addEventListener('DOMContentLoaded', function () {
    // Add zoom controls functionality if they exist
    const zoomInBtn = document.getElementById('zoomInBtn');
    const zoomOutBtn = document.getElementById('zoomOutBtn');
    const resetZoomBtn = document.getElementById('resetZoomBtn');
    const svgContainer = document.querySelector('.ast-svg-container');

    let scale = 1;

    if (svgContainer && zoomInBtn && zoomOutBtn && resetZoomBtn) {
        zoomInBtn.addEventListener('click', function () {
            scale += 0.1;
            applySvgZoom();
        });

        zoomOutBtn.addEventListener('click', function () {
            if (scale > 0.2) {
                scale -= 0.1;
                applySvgZoom();
            }
        });

        resetZoomBtn.addEventListener('click', function () {
            scale = 1;
            applySvgZoom();
        });

        function applySvgZoom() {
            const svg = svgContainer.querySelector('svg');
            if (svg) {
                svg.style.transform = `scale(${scale})`;
                svg.style.transformOrigin = 'center top';
            }
        }
    }

    // Ensure SVG is fully visible
    const svgs = document.querySelectorAll('.ast-svg-container svg');
    svgs.forEach(svg => {
        // Set viewBox if not set already
        if (!svg.getAttribute('viewBox')) {
            const width = svg.getAttribute('width');
            const height = svg.getAttribute('height');
            if (width && height) {
                svg.setAttribute('viewBox', `0 0 ${width} ${height}`);
            }
        }

        // Make svg responsive while maintaining aspect ratio
        svg.style.width = '100%';
        svg.style.height = 'auto';
    });
});