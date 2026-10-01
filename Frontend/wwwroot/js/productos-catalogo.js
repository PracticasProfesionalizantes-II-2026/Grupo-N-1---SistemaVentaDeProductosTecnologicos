document.querySelectorAll('form[data-eliminar-producto]').forEach(form => {
    form.addEventListener('submit', event => {
        if (!window.confirm(`¿Eliminar el producto “${form.dataset.eliminarProducto}”? Esta acción no se puede deshacer.`)) {
            event.preventDefault();
        }
    });
});
