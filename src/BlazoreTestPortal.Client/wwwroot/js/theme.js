window.appTheme = {
    toggle: function () {
        var current = document.documentElement.getAttribute('data-bs-theme') || 'light';
        var next    = current === 'dark' ? 'light' : 'dark';
        document.documentElement.setAttribute('data-bs-theme', next);
        localStorage.setItem('theme', next);
    },
    init: function () {
        var saved = localStorage.getItem('theme') || 'light';
        document.documentElement.setAttribute('data-bs-theme', saved);
    }
};
