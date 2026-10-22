import { crearStore, carritoValido, direccionDemo, pagoDemo, renderResumen, feedback } from "./compra-demo-store.js";

const raiz = document.querySelector('[data-demo-page="checkout"]');
const form = raiz?.querySelector("[data-demo-form]");
if (form) {
    const boton = form.querySelector("button");
    const escenario = form.querySelector("select");
    const carga = raiz.querySelector("[data-demo-loading]");
    let store;
    let intento;
    let snapshot;
    let temporizador;
    let procesando = false;
    let disponible = false;
    const confirmacion = id => {
        const url = new URL(raiz.dataset.confirmacionUrl, window.location.origin);
        url.searchParams.set("id", id);
        window.location.replace(url.href);
    };
    const mostrarProcesamiento = activo => {
        procesando = activo;
        boton.disabled = activo || !disponible;
        escenario.disabled = activo;
        carga.hidden = !activo;
        form.setAttribute("aria-busy", String(activo));
    };
    try {
        store = crearStore(raiz.dataset.userId);
        const url = new URL(window.location.href);
        if (url.searchParams.get("nuevo") === "true") {
            store.nuevoIntento();
            // Consumir la intención de empezar de nuevo para que recargar conserve el mismo intento.
            url.searchParams.delete("nuevo");
            window.history.replaceState(null, "", url.href);
        }
        store.leer();
        intento = store.leerIntento();
        snapshot = intento?.carrito ?? JSON.parse(document.getElementById("demo-carrito").textContent);
        if (!carritoValido(snapshot)) throw new Error("El resumen demo no es válido. Volvé a cargar el carrito.");
        renderResumen(raiz.querySelector("[data-demo-resumen]"), snapshot);
        if (intento && store.buscar(intento.id)) confirmacion(intento.id);
        disponible = true;
        boton.disabled = false;
        if (intento) feedback(raiz, "Retomás el mismo intento con la copia original del carrito.");
    } catch (error) {
        feedback(raiz, error.message, true);
    }

    form.addEventListener("submit", event => {
        event.preventDefault();
        if (procesando || !disponible) return;
        try {
            if (!intento) {
                intento = { id: "DEMO-" + crypto.randomUUID(), fecha: new Date().toISOString(), carrito: snapshot };
            }
            store.guardarIntento(intento);
            if (store.buscar(intento.id)) { confirmacion(intento.id); return; }
            const resultado = escenario.value;
            mostrarProcesamiento(true);
            feedback(raiz, "Procesando pago ficticio…");
            temporizador = window.setTimeout(() => {
                temporizador = undefined;
                try {
                    if (resultado === "rechazo") {
                        feedback(raiz, "Pago ficticio rechazado. No se creó ningún pedido. Elegí otro resultado y reintentá.", true, true);
                    } else if (resultado === "demora") {
                        feedback(raiz, "Se agotó el tiempo del pago ficticio. No se creó ningún pedido. Podés reintentar.", true, true);
                    } else {
                        store.guardarPedido({ ...intento, direccion: direccionDemo, pago: pagoDemo, estado: "Confirmado" });
                        confirmacion(intento.id);
                        return;
                    }
                } catch (error) {
                    feedback(raiz, error.message, true, true);
                }
                mostrarProcesamiento(false);
            }, resultado === "demora" ? 6000 : 2000);
        } catch (error) {
            feedback(raiz, error.message, true, true);
            mostrarProcesamiento(false);
        }
    });

    window.addEventListener("pagehide", () => {
        window.clearTimeout(temporizador);
        if (procesando) {
            mostrarProcesamiento(false);
            feedback(raiz, "La simulación se interrumpió. Podés retomar este intento.");
        }
    });
    window.addEventListener("pageshow", event => {
        if (!event.persisted || !intento || !store) return;
        try { if (store.buscar(intento.id)) confirmacion(intento.id); }
        catch (error) { feedback(raiz, error.message, true); }
    });
}
