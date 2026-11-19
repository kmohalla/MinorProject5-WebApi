// theme.js — runs in <head> before the page is drawn.
// Picks light or dark mode (saved choice first, then the computer's setting)
// and puts it on the <html> tag as data-theme="light" or data-theme="dark".
// site.css then switches every color from that one attribute.
(function () {
    var KEY = 'garden-theme';

    function getSaved() {
        try { return localStorage.getItem(KEY); } catch (e) { return null; }
    }

    var prefersDark = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
    var theme = getSaved() || (prefersDark ? 'dark' : 'light');
    document.documentElement.setAttribute('data-theme', theme);

    // Small helper that app.js uses for the toggle button
    window.gardenTheme = {
        get: function () {
            return document.documentElement.getAttribute('data-theme');
        },
        set: function (newTheme) {
            document.documentElement.setAttribute('data-theme', newTheme);
            try { localStorage.setItem(KEY, newTheme); } catch (e) { }
        }
    };
})();
