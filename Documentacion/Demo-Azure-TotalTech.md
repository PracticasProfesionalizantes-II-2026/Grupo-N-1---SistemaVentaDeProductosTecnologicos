# Demostración de TotalTech — 29 de octubre de 2026

## Preparación

- Confirmar URLs y estado final en Despliegue-TotalTech-Azure-FullStack.md.
- 15 minutos antes: consultar categorías y catálogo de la API; abrir el frontend y esperar reanudación de SQL/cold starts.
- Preparar una sesión pública limpia y, solo si fue validada, una sesión de prueba autenticada. No mostrar contraseñas, tokens, cookies, claves ni valores privados del portal.
- Conservar paquete verificado y demostración local aislada como contingencia.

## Recorrido de 10–12 minutos

1. **Arquitectura (1 min):** navegador HTTPS → MVC → API HTTPS/JWT → cancho/TotatechDB. Frontend y API son Web Apps distintas en el F1 existente.
2. **Aplicación pública (3 min):** https://totaltechfrontend.azurewebsites.net/ → Home → Productos → categoría → búsqueda → precio/disponibilidad → tamaño de página → detalle. Mostrar datos reales, imágenes o su placeholder legítimo y navegación móvil.
3. **Autenticación (2 min, solo si PASS):** ingresar con cuenta dedicada, mostrar protección de una ruta, cerrar sesión. Administración solo con cuenta existente autorizada y módulos previamente comprobados.
4. **Código/CI/CD (2 min):** mostrar proyectos separados, workflow frontend y backend, artefacto MVC, OIDC y ejecución aprobada sobre el SHA desplegado. Mostrar nombres de settings, nunca sus valores secretos.
5. **SQL/calidad (2 min):** mostrar tablas/esquema, cantidades agregadas y evidencia de datos en pantalla. Mostrar TRX, smoke test, logs y controles de sesión/CSRF/uploads. No ejecutar escrituras improvisadas.
6. **Límites (1 min):** explicar F1, pausa SQL, checkout de demostración deshabilitado y diferencias de Pedidos/historial pendientes. No presentar un módulo no probado como operativo.

## Verificación rápida

```powershell
./Desarrollo/Test-TotaltechPublic.ps1 -FrontendUrl 'https://totaltechfrontend.azurewebsites.net/' -ApiUrl 'https://totaltechapp-ghd2etambcgcfgcb.brazilsouth-01.azurewebsites.net/'
```

La prueba debe mostrar cantidades reales y Home/Catalog/Css=200. Revisar además visualmente el navegador, imágenes y consola.

## Contingencia

- Si SQL despierta lentamente, esperar y reintentar los GET públicos; no cambiar firewall ni habilitar migraciones.
- Si F1 agotó cuota, mostrar el estado y pasar a la ejecución local aislada; no subir de SKU automáticamente.
- Si una página falla, capturar la hora/ruta y consultar eventlog.xml. No abrir páginas de excepción públicas.
- Un redeploy de contingencia usará el último ZIP aprobado mediante Entra ID. Conservar uploads en %HOME%/data y no tocar SQL.
