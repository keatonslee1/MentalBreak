// Enforce a 16:9 (contain/letterbox) viewport for Unity WebGL canvas.
// Web-only: safe to modify without rebuilding Unity.
(function () {
  "use strict";

  const TARGET_ASPECT = 16 / 9;

  function clampInt(n, min, max) {
    if (!Number.isFinite(n)) return min;
    return Math.max(min, Math.min(max, Math.round(n)));
  }

  function getElements() {
    const container = document.getElementById("unity-container");
    const wrapper = document.getElementById("unity-aspect");
    const canvas = document.getElementById("unity-canvas");
    if (!container || !wrapper || !canvas) return null;
    return { container, wrapper, canvas };
  }

  function fit16x9() {
    const els = getElements();
    if (!els) return;

    const rect = els.container.getBoundingClientRect();
    const availW = rect.width;
    const availH = rect.height;
    if (!(availW > 0 && availH > 0)) return;

    const availAspect = availW / availH;
    let targetW;
    let targetH;

    if (availAspect > TARGET_ASPECT) {
      // Wider than 16:9 -> fit by height
      targetH = availH;
      targetW = availH * TARGET_ASPECT;
    } else {
      // Taller/narrower than 16:9 -> fit by width
      targetW = availW;
      targetH = availW / TARGET_ASPECT;
    }

    // Set wrapper size (CSS pixels)
    const cssW = clampInt(targetW, 1, availW);
    const cssH = clampInt(targetH, 1, availH);
    els.wrapper.style.width = cssW + "px";
    els.wrapper.style.height = cssH + "px";

    // Set canvas render size to match displayed size (keep config.devicePixelRatio = 1)
    // This ensures Unity creates/updates its backbuffer to the same size as the letterboxed view.
    if (els.canvas.width !== cssW) els.canvas.width = cssW;
    if (els.canvas.height !== cssH) els.canvas.height = cssH;
  }

  function scheduleFit() {
    // Two-phase: immediate + next frame to catch layout changes (fonts/nav height/etc)
    fit16x9();
    requestAnimationFrame(fit16x9);
  }

  function init() {
    // Only activate on pages that include the Unity elements.
    if (!getElements()) return;

    // Initial sizing before Unity bootstraps (script is included before createUnityInstance()).
    scheduleFit();

    window.addEventListener("resize", scheduleFit, { passive: true });
    window.addEventListener("orientationchange", scheduleFit, { passive: true });

    if (window.visualViewport) {
      window.visualViewport.addEventListener("resize", scheduleFit, { passive: true });
      window.visualViewport.addEventListener("scroll", scheduleFit, { passive: true });
    }

    document.addEventListener("fullscreenchange", scheduleFit);
    document.addEventListener("webkitfullscreenchange", scheduleFit);
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", init);
  } else {
    init();
  }
})();



