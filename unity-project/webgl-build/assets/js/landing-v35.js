(() => {
  const stage = document.querySelector("[data-mb-hero-stage]");
  const bgImg = document.querySelector("[data-mb-hero-bg]");
  const bgVideo = document.querySelector("[data-mb-hero-video]");
  const dialogueText = document.querySelector("[data-mb-dialogue-text]");
  const choiceMeditation = document.querySelector("[data-mb-choice='meditation']");
  const choicePickup = document.querySelector("[data-mb-choice='pickup']");
  const playButton = document.querySelector("[data-mb-nav-play]");

  if (!stage) return;

  const frames = [
    "assets/images/landing/x8_2560x1440/BG_01_v_02_x8_01.png",
    "assets/images/landing/x8_2560x1440/BG_01_v_02_x8_02.png",
    "assets/images/landing/x8_2560x1440/BG_01_v_02_x8_03.png",
    "assets/images/landing/x8_2560x1440/BG_01_v_02_x8_04.png",
    "assets/images/landing/x8_2560x1440/BG_01_v_02_x8_05.png",
    "assets/images/landing/x8_2560x1440/BG_01_v_02_x8_06.png",
    "assets/images/landing/x8_2560x1440/BG_01_v_02_x8_07.png",
    "assets/images/landing/x8_2560x1440/BG_01_v_02_x8_08.png",
  ];

  const prefersReducedMotion = window.matchMedia?.("(prefers-reduced-motion: reduce)")?.matches;

  const isMobileUA = () => {
    const ua = navigator.userAgent || "";
    return /iPhone|iPad|iPod|Android/i.test(ua);
  };

  const playDestination = () => (isMobileUA() ? "/mobile-play" : "/play");

  // If we're using the MP4 background, let the browser handle animation via <video>.
  // Keep the old frame-swap code as a fallback if someone switches back to images later.
  if (!bgVideo && bgImg) {
    // Preload frames (best effort for smooth animation).
    frames.forEach((src) => {
      const img = new Image();
      img.src = src;
    });

    // Animate background.
    let frameIndex = 0;
    let timer = null;

    // Set initial frame
    bgImg.src = frames[0];

    const startAnimation = () => {
      if (prefersReducedMotion) return;
      if (timer) return;
      timer = window.setInterval(() => {
        frameIndex = (frameIndex + 1) % frames.length;
        bgImg.src = frames[frameIndex];
      }, 110);
    };

    const stopAnimation = () => {
      if (!timer) return;
      window.clearInterval(timer);
      timer = null;
    };

    startAnimation();
    document.addEventListener("visibilitychange", () => {
      if (document.hidden) stopAnimation();
      else startAnimation();
    });
  }

  const setDialogue = (text) => {
    if (!dialogueText) return;
    dialogueText.textContent = text;
  };

  const disableChoices = () => {
    if (choiceMeditation) choiceMeditation.disabled = true;
    if (choicePickup) choicePickup.disabled = true;
  };

  const handleChoice = (text) => {
    setDialogue(text);
    disableChoices();
    window.setTimeout(() => {
      window.location.href = playDestination();
    }, 700);
  };

  if (choiceMeditation) {
    choiceMeditation.addEventListener("click", () => {
      handleChoice("Alice: Hmm... You're some more training.");
    });
  }
  if (choicePickup) {
    choicePickup.addEventListener("click", () => {
      handleChoice("Alice: You're a natural!");
    });
  }

  if (playButton) {
    playButton.addEventListener("click", (e) => {
      e.preventDefault();
      window.location.href = playDestination();
    });
  }
})();


