// Estos datos sólo describen una demostración local; nunca autorizan operaciones reales.
export const estados = ["Confirmado", "En preparación", "Enviado", "Entregado"];
export const direccionDemo = "Avenida Demo 123, Ciudad de demostración, Argentina.";
export const pagoDemo = "Pago aprobado demo · Tarjeta terminada en 4242";
const version = 1;
const importeValido = valor => typeof valor === "number" && Number.isFinite(valor) && valor >= 0;
const idValido = id => typeof id === "string" && /^DEMO-[a-f0-9-]{36}$/.test(id);
const fechaValida = fecha => typeof fecha === "string" && Number.isFinite(Date.parse(fecha));

export function carritoValido(carrito) {
    return carrito && Array.isArray(carrito.items) && carrito.items.length > 0 &&
        importeValido(carrito.total) && Number.isSafeInteger(carrito.cantidadTotal) && carrito.cantidadTotal > 0 &&
        carrito.items.every(item => item && Number.isSafeInteger(item.idProducto) && item.idProducto > 0 &&
            typeof item.nombre === "string" && Number.isSafeInteger(item.cantidad) && item.cantidad > 0 &&
            importeValido(item.precioUnitario) && importeValido(item.subtotal));
}

function pedidoValido(pedido) {
    return pedido && idValido(pedido.id) && fechaValida(pedido.fecha) && carritoValido(pedido.carrito) &&
        pedido.direccion === direccionDemo && pedido.pago === pagoDemo && estados.includes(pedido.estado);
}

function falloAlmacenamiento() {
    return new Error("No pudimos acceder al almacenamiento del navegador. Permití guardar datos locales y volvé a intentarlo. No se completó ninguna nueva compra demo.");
}

export function crearStore(idUsuario) {
    if (!/^[1-9][0-9]*$/.test(String(idUsuario))) throw new Error("No pudimos identificar tu sesión.");
    const key = "Totaltech.CompraDemo.v1.usuario." + idUsuario;
    const intentoKey = key + ".intento";
    function obtener(storage, clave) {
        try { return window[storage].getItem(clave); }
        catch { throw falloAlmacenamiento(); }
    }
    function escribir(storage, clave, datos) {
        try { window[storage].setItem(clave, JSON.stringify(datos)); }
        catch { throw falloAlmacenamiento(); }
    }
    function eliminar(storage, clave) {
        try { window[storage].removeItem(clave); }
        catch { throw falloAlmacenamiento(); }
    }
    function leer() {
        const raw = obtener("localStorage", key);
        if (raw === null) return [];
        let datos;
        try { datos = JSON.parse(raw); } catch { /* Mostrar recuperación sin borrar datos automáticamente. */ }
        if (!datos || datos.version !== version || !Array.isArray(datos.pedidos) ||
            !datos.pedidos.every(pedidoValido) ||
            new Set(datos.pedidos.map(p => p.id)).size !== datos.pedidos.length)
            throw new Error("El historial demo tiene un formato inválido. Podés borrar tus pedidos demo para comenzar de nuevo.");
        return datos.pedidos;
    }
    function guardar(pedidos) { escribir("localStorage", key, { version, pedidos }); }
    return {
        leer,
        buscar: id => leer().find(pedido => pedido.id === id),
        guardarPedido(pedido) {
            if (!pedidoValido(pedido)) throw new Error("Los datos del pedido demo no son válidos.");
            const pedidos = leer();
            const existente = pedidos.find(p => p.id === pedido.id);
            if (existente) return existente;
            guardar([pedido, ...pedidos]);
            return pedido;
        },
        avanzar(id) {
            const pedidos = leer();
            const pedido = pedidos.find(p => p.id === id);
            if (!pedido) throw new Error("No se encontró este pedido demo en tu historial.");
            const indice = estados.indexOf(pedido.estado);
            pedido.estado = estados[Math.min(indice + 1, estados.length - 1)];
            guardar(pedidos);
            return pedido;
        },
        borrar() {
            eliminar("sessionStorage", intentoKey);
            eliminar("localStorage", key);
        },
        leerIntento() {
            const raw = obtener("sessionStorage", intentoKey);
            if (raw === null) return null;
            let intento;
            try { intento = JSON.parse(raw); } catch { /* Se recupera al iniciar una nueva simulación. */ }
            if (!intento || intento.version !== version || !idValido(intento.id) ||
                !fechaValida(intento.fecha) || !carritoValido(intento.carrito))
                throw new Error("El intento demo no es válido. Volvé al carrito y elegí Simular compra para comenzar de nuevo.");
            return intento;
        },
        guardarIntento(intento) { escribir("sessionStorage", intentoKey, { ...intento, version }); },
        nuevoIntento() { eliminar("sessionStorage", intentoKey); }
    };
}

export const dinero = valor => new Intl.NumberFormat("es-AR", { style: "currency", currency: "ARS" }).format(valor);
export function texto(tag, valor, clase) {
    const nodo = document.createElement(tag);
    nodo.textContent = valor;
    if (clase) nodo.className = clase;
    return nodo;
}
export function renderResumen(destino, carrito, pedido) {
    destino.replaceChildren();
    if (pedido) {
        destino.append(texto("h2", pedido.id), texto("p", new Date(pedido.fecha).toLocaleString("es-AR")));
    }
    for (const item of carrito.items) {
        const linea = texto("div", "", "compra-demo__linea");
        linea.append(texto("strong", item.nombre), texto("span", item.cantidad + " × " + dinero(item.precioUnitario)), texto("span", dinero(item.subtotal)));
        destino.append(linea);
    }
    destino.append(texto("p", "Total: " + dinero(carrito.total), "compra-demo__total"));
    if (pedido) destino.append(texto("p", pedido.direccion), texto("p", pedido.pago));
}
export function feedback(raiz, mensaje, error = false, enfocar = false) {
    const nodo = raiz.querySelector("[data-demo-feedback]");
    nodo.textContent = mensaje;
    nodo.className = error ? "compra-demo__error" : "compra-demo__mensaje";
    if (enfocar) nodo.focus();
}
