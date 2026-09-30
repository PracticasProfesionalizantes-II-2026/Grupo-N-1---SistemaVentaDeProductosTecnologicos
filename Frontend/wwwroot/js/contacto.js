(() => {
    const form = document.getElementById('contacto-form');
    const enviar = document.getElementById('contacto-enviar');
    const dialog = document.getElementById('contacto-confirmacion');
    const aceptar = document.getElementById('contacto-aceptar');
    const campos = [
        { input: document.getElementById('contacto-nombre'), mensaje: 'Ingresá tu nombre.' },
        { input: document.getElementById('contacto-email'), mensaje: 'Ingresá un email válido.' },
        { input: document.getElementById('contacto-mensaje'), mensaje: 'Escribí tu mensaje.' }
    ];

    function validar(campo) {
        const input = campo.input;
        input.value = input.value.trim();
        input.setCustomValidity('');
        const mensaje = input.validity.valid ? '' : campo.mensaje;
        input.setCustomValidity(mensaje);
        input.setAttribute('aria-invalid', String(!!mensaje));
        document.getElementById(`${input.id}-error`).textContent = mensaje;
        return !mensaje;
    }

    campos.forEach(campo => {
        campo.input.addEventListener('input', () => {
            if (campo.input.getAttribute('aria-invalid') === 'true') {
                campo.input.setCustomValidity('');
                campo.input.removeAttribute('aria-invalid');
                document.getElementById(`${campo.input.id}-error`).textContent = '';
            }
        });
    });

    form.noValidate = true;
    enviar.disabled = false;
    form.addEventListener('submit', event => {
        event.preventDefault();
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
    dialog.addEventListener('close', () => enviar.focus());
})();
