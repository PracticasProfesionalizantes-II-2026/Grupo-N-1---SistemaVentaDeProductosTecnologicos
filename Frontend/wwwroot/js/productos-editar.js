(() => {
    const archivo = document.getElementById('ImagenArchivo');
    const preview = document.getElementById('imagen-preview');
    const vacia = document.getElementById('imagen-vacia');
    const error = document.getElementById('imagen-error');
    const imagenActual = preview.getAttribute('src');
    let objectUrl;

    function mostrarActual() {
        preview.hidden = !imagenActual;
        vacia.hidden = !!imagenActual;
        if (imagenActual) preview.src = imagenActual;
        else preview.removeAttribute('src');
    }

    archivo.addEventListener('change', () => {
        if (objectUrl) URL.revokeObjectURL(objectUrl);
        objectUrl = null;
        error.textContent = '';
        archivo.setCustomValidity('');
        const imagen = archivo.files[0];
        if (!imagen) return mostrarActual();

        if (!/\.(jpe?g|png|webp)$/i.test(imagen.name) ||
            !['image/jpeg', 'image/png', 'image/webp'].includes(imagen.type)) {
            error.textContent = 'Seleccioná una imagen JPG, PNG o WebP.';
        } else if (!imagen.size || imagen.size > 5 * 1024 * 1024) {
            error.textContent = 'La imagen debe tener contenido y no superar los 5 MB.';
        }
        if (error.textContent) {
            archivo.setCustomValidity(error.textContent);
            return mostrarActual();
        }

        objectUrl = URL.createObjectURL(imagen);
        preview.src = objectUrl;
        preview.hidden = false;
        vacia.hidden = true;
    });

    preview.addEventListener('error', () => {
        if (!objectUrl) return;
        error.textContent = 'No se pudo leer la imagen. Elegí otro archivo.';
        archivo.setCustomValidity(error.textContent);
        URL.revokeObjectURL(objectUrl);
        objectUrl = null;
        mostrarActual();
    });
    window.addEventListener('pagehide', () => { if (objectUrl) URL.revokeObjectURL(objectUrl); });
})();
