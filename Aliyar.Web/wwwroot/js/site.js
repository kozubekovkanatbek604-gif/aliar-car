// Highlight active nav link
(function () {
  const path = window.location.pathname.replace(/\/$/, "") || "/";
  document.querySelectorAll(".app-nav .nav-link").forEach((link) => {
    const href = link.getAttribute("href");
    if (!href) return;
    const linkPath = new URL(link.href, window.location.origin).pathname.replace(/\/$/, "") || "/";
    if (linkPath === path || (linkPath !== "/" && path.startsWith(linkPath))) {
      link.classList.add("active");
    }
  });
})();
