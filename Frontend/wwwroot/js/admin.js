(() => {
    const shell = document.querySelector("[data-admin-shell]");
    const toggle = document.querySelector("[data-admin-menu-toggle]");
    const closeButton = document.querySelector("[data-admin-menu-close]");
    const sidebar = document.querySelector("[data-admin-sidebar]");

    if (!shell || !toggle || !closeButton || !sidebar) {
        return;
    }

    const setMenuState = (isOpen, returnFocus = false) => {
        shell.classList.toggle("admin-menu-open", isOpen);
        toggle.setAttribute("aria-expanded", isOpen.toString());
        document.body.classList.toggle("admin-menu-lock", isOpen);

        if (isOpen) {
            sidebar.querySelector("a")?.focus();
        } else if (returnFocus) {
            toggle.focus();
        }
    };

    toggle.addEventListener("click", () => {
        setMenuState(!shell.classList.contains("admin-menu-open"));
    });

    closeButton.addEventListener("click", () => setMenuState(false, true));

    sidebar.addEventListener("click", event => {
        if (event.target.closest("a")) {
            setMenuState(false);
        }
    });

    document.addEventListener("keydown", event => {
        if (event.key === "Escape" && shell.classList.contains("admin-menu-open")) {
            setMenuState(false, true);
        }
    });

    window.matchMedia("(min-width: 769px)").addEventListener("change", event => {
        if (event.matches) {
            setMenuState(false);
        }
    });
})();
