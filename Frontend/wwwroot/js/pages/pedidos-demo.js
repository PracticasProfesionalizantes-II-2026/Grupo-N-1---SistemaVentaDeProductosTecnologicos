import { crearStore, estados, dinero, texto, renderResumen, feedback } from "./compra-demo-store.js";

const raiz = document.querySelector("[data-demo-page]");
if (raiz) {
    const contenido = raiz.querySelector("[data-demo-content]");
    const borrar = raiz.querySelector("[data-demo-clear]");
    let store;
    const detalleUrl = id => raiz.dataset.detalleUrl.replace("__id__", encodeURIComponent(id));
    function renderListado() {
        const pedidos = store.leer();
        contenido.replaceChildren();
        for (const pedido of pedidos) {
            const tarjeta = texto("article", "", "compra-demo__panel");
            const enlace = texto("a", "Ver pedido demo", "btn btn-outline-primary");
            enlace.href = detalleUrl(pedido.id);
            tarjeta.append(texto("h2", pedido.id), texto("p", new Date(pedido.fecha).toLocaleString("es-AR")),
                texto("p", pedido.estado, "compra-demo__estado"), texto("p", "Total: " + dinero(pedido.carrito.total)), enlace);
            contenido.append(tarjeta);
        }
        borrar.disabled = pedidos.length === 0;
        feedback(raiz, pedidos.length === 0 ? "Todavía no tenés pedidos demo. Iniciá una simulación desde tu carrito." : pedidos.length + " pedido(s) ficticio(s) guardado(s).");
    }
    function renderDetalle(pedido) {
        contenido.hidden = false;
        renderResumen(raiz.querySelector("[data-demo-resumen]"), pedido.carrito, pedido);
        if (raiz.dataset.demoPage === "confirmacion") {
            raiz.querySelector("[data-demo-detail]").href = detalleUrl(pedido.id);
            feedback(raiz, "Pago demo aprobado. No se realizó ningún cobro.");
            return;
        }
        const lista = raiz.querySelector("[data-demo-timeline]");
        lista.replaceChildren();
        const indice = estados.indexOf(pedido.estado);
        estados.forEach((estado, posicion) => {
            const paso = texto("li", estado);
            if (posicion <= indice) paso.className = "compra-demo__paso-completo";
            if (posicion === indice) paso.setAttribute("aria-current", "step");
            lista.append(paso);
        });
        raiz.querySelector("[data-demo-advance]").disabled = indice === estados.length - 1;
        feedback(raiz, "Estado ficticio: " + pedido.estado);
    }
    try {
        store = crearStore(raiz.dataset.userId);
        if (raiz.dataset.demoPage === "listado") renderListado();
        else {
            const pedido = store.buscar(raiz.dataset.orderId);
            if (!pedido) feedback(raiz, "No se encontró este pedido demo en tu historial de este navegador.", true);
            else renderDetalle(pedido);
        }
    } catch (error) {
        contenido.replaceChildren();
        contenido.hidden = true;
        feedback(raiz, error.message, true);
        // Permitir recuperar sólo la clave del usuario actual, aun si su historial es inválido.
        if (borrar && store) borrar.disabled = false;
    }

    if (borrar) {
        const panel = raiz.querySelector("[data-demo-clear-confirm]");
        borrar.addEventListener("click", () => {
            panel.hidden = false;
            raiz.querySelector("[data-demo-clear-no]").focus();
        });
        raiz.querySelector("[data-demo-clear-no]").addEventListener("click", () => {
            panel.hidden = true;
            borrar.focus();
        });
        raiz.querySelector("[data-demo-clear-yes]").addEventListener("click", () => {
            try {
                store.borrar();
                panel.hidden = true;
                contenido.hidden = false;
                renderListado();
                feedback(raiz, "Se borraron únicamente tus pedidos demo de este navegador.", false, true);
            } catch (error) { feedback(raiz, error.message, true, true); }
        });
    }
    raiz.querySelector("[data-demo-advance]")?.addEventListener("click", () => {
        try { renderDetalle(store.avanzar(raiz.dataset.orderId)); }
        catch (error) { feedback(raiz, error.message, true, true); }
    });
}
