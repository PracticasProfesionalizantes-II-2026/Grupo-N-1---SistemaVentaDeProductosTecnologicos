(() => {
    const categorias = document.querySelector(".public-catalog .catalog-categories");
    if (!categorias) return;

    const encabezado = categorias.querySelector("summary");
    const escritorio = window.matchMedia("(min-width: 768px)");
    let abiertoEnMovil = categorias.dataset.mobileOpen === "true";

    function actualizar() {
        categorias.open = escritorio.matches || abiertoEnMovil;
        encabezado.tabIndex = escritorio.matches ? -1 : 0;
    }

    encabezado.addEventListener("click", event => {
        if (escritorio.matches) {
            event.preventDefault();
        } else {
            abiertoEnMovil = !categorias.open;
        }
    });
    escritorio.addEventListener("change", actualizar);
    actualizar();
})();
