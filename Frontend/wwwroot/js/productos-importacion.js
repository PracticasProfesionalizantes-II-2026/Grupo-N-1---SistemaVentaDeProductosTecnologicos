(() => {
    const form = document.querySelector('[data-importacion-revision]');
    if (!form) return;
    const confirmar = form.querySelector('[data-importacion-confirmar]');
    const estado = form.querySelector('[data-importacion-estado]');
    const invalidar = () => {
        confirmar.disabled = true;
        estado.textContent = 'Hay cambios pendientes. Validá el lote antes de confirmar.';
    };
    form.addEventListener('input', invalidar);
    form.addEventListener('change', invalidar);
    let enviando = false;
    form.addEventListener('submit', event => {
        if (enviando) { event.preventDefault(); return; }
        enviando = true;
        // No deshabilitar el submitter: el navegador debe conservar su formaction.
        form.setAttribute('aria-busy', 'true');
        estado.textContent = 'Procesando el lote. Esperá el resultado antes de volver a enviar.';
    });
})();
