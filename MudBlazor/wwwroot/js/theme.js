window.appTheme = {
  toggle: function () {
    const current = document.documentElement.getAttribute("data-bs-theme") || "dark";
    const next = current === "dark" ? "light" : "dark";
    document.documentElement.setAttribute("data-bs-theme", next);
    localStorage.setItem("theme", next);
  },
  init: function () {
    const saved = localStorage.getItem("theme") || "dark";
    document.documentElement.setAttribute("data-bs-theme", saved);
  }
};
