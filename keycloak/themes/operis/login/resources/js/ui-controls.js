(function () {
  var STORAGE_KEY = "oi_kc_theme_mode";
  var LOCALE_COOKIE = "KEYCLOAK_LOCALE";

  function setPatternflyDarkClass(resolvedTheme) {
    var root = document.documentElement;
    var darkClass = "pf-v5-theme-dark";
    if (resolvedTheme === "dark") {
      root.classList.add(darkClass);
    } else {
      root.classList.remove(darkClass);
    }
  }

  function applyTheme(mode, resolvedTheme) {
    var root = document.documentElement;
    var body = document.body;
    root.setAttribute("data-oi-theme-mode", mode);
    root.setAttribute("data-oi-theme", resolvedTheme);
    if (body) {
      body.setAttribute("data-oi-theme-mode", mode);
      body.setAttribute("data-oi-theme", resolvedTheme);
    }
    setPatternflyDarkClass(resolvedTheme);
    var buttons = document.querySelectorAll("[data-oi-theme-option]");
    buttons.forEach(function (btn) {
      btn.classList.toggle("is-active", btn.getAttribute("data-oi-theme-option") === mode);
    });
  }

  function resolveSystemTheme() {
    return window.matchMedia && window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
  }

  function initializeTheme() {
    var mode = localStorage.getItem(STORAGE_KEY) || "system";
    if (mode === "system") {
      applyTheme(mode, resolveSystemTheme());
    } else {
      applyTheme(mode, mode);
    }
    var themeButtons = document.querySelectorAll("[data-oi-theme-option]");
    themeButtons.forEach(function (btn) {
      btn.addEventListener("click", function () {
        var picked = btn.getAttribute("data-oi-theme-option") || "system";
        localStorage.setItem(STORAGE_KEY, picked);
        if (picked === "system") {
          applyTheme(picked, resolveSystemTheme());
          return;
        }
        applyTheme(picked, picked);
      });
    });
    if (window.matchMedia) {
      window.matchMedia("(prefers-color-scheme: dark)").addEventListener("change", function () {
        if ((localStorage.getItem(STORAGE_KEY) || "system") === "system") {
          applyTheme("system", resolveSystemTheme());
        }
      });
    }
  }

  function updateUrlLocale(locale) {
    var url = new URL(window.location.href);
    url.searchParams.set("kc_locale", locale);
    url.searchParams.set("ui_locales", locale);
    document.cookie = LOCALE_COOKIE + "=" + locale + "; Path=/; SameSite=Lax";
    window.location.href = url.toString();
  }

  function initializeLocaleSwitch() {
    var langButtons = document.querySelectorAll("[data-oi-locale]");
    langButtons.forEach(function (btn) {
      btn.addEventListener("click", function () {
        var locale = btn.getAttribute("data-oi-locale");
        if (!locale) return;
        updateUrlLocale(locale);
      });
    });
  }

  function init() {
    initializeTheme();
    initializeLocaleSwitch();
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", init);
  } else {
    init();
  }
})();
