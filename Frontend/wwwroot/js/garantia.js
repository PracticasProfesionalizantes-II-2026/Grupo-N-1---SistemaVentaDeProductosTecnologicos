(() => {
    const form = document.getElementById('garantia-form');
    if (!form) return;

    const enviar = document.getElementById('garantia-enviar');
    const dialog = document.getElementById('garantia-confirmacion');
    const aceptar = document.getElementById('garantia-aceptar');
    const fecha = document.getElementById('garantia-fecha');
    const campos = [
        { input: document.getElementById('garantia-nombre'), mensaje: 'Ingresá tu nombre.' },
        { input: document.getElementById('garantia-email'), mensaje: 'Ingresá un email válido.' },
        { input: document.getElementById('garantia-producto'), mensaje: 'Ingresá el nombre o modelo del producto.' },
        { input: fecha, mensaje: 'Ingresá una fecha de compra válida.' },
        { input: document.getElementById('garantia-problema'), mensaje: 'Describí el problema de tu producto.' }
    ];

    function actualizarFechaMaxima() {
        // Usar la fecha local evita rechazar compras de hoy por diferencias con UTC.
        const hoy = new Date();
        fecha.max = [hoy.getFullYear(), String(hoy.getMonth() + 1).padStart(2, '0'),
            String(hoy.getDate()).padStart(2, '0')].join('-');
    }

    function validar(campo) {
        const input = campo.input;
        input.value = input.value.trim();
        input.setCustomValidity('');
        const mensaje = input.validity.valid ? '' :
            input === fecha && input.validity.rangeOverflow ?
                'La fecha de compra no puede ser futura.' : campo.mensaje;
        input.setCustomValidity(mensaje);
        input.setAttribute('aria-invalid', String(!!mensaje));
        document.getElementById(`${input.id}-error`).textContent = mensaje;
        return !mensaje;
    }

    campos.forEach(campo => {
        campo.input.addEventListener('input', () => {
            campo.input.setCustomValidity('');
            campo.input.removeAttribute('aria-invalid');
            document.getElementById(`${campo.input.id}-error`).textContent = '';
        });
    });

    actualizarFechaMaxima();
    form.noValidate = true;
    enviar.disabled = false;
    form.addEventListener('submit', event => {
        event.preventDefault();
        actualizarFechaMaxima();
        let primerInvalido;
        campos.forEach(campo => {
            if (!validar(campo) && !primerInvalido) primerInvalido = campo.input;
        });
        if (primerInvalido) {
            primerInvalido.focus();
            return;
        }
        form.reset();
        campos.forEach(campo => campo.input.removeAttribute('aria-invalid'));
        dialog.showModal();
        aceptar.focus();
    });

    aceptar.addEventListener('click', () => dialog.close());
    dialog.addEventListener('keydown', event => {
        if (event.key === 'Tab') {
            event.preventDefault();
            aceptar.focus();
        }
    });
    dialog.addEventListener('close', () => enviar.focus());
})();
