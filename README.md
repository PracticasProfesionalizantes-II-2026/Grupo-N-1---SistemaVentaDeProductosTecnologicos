# TotalTech
## Referencia actual de la API

- [Documentación de API actualizada (PDF)](output/pdf/Documentacion-de-API.pdf)
- [Referencia editable: rutas, permisos, contratos y ejemplos](Documentacion/API.md)

Edición del 1 de octubre de 2026, contrastada con las 81 operaciones del backend.
Incluye autenticación JWT, catálogo paginado, imágenes de productos, carrito,
confirmación de pedidos, pagos y permisos por recurso. El PDF de
`Documentacion/Documentación de API.pdf` se conserva como documento histórico.

Para regenerar ambos archivos desde esta raíz, usar un intérprete Python con
`reportlab` disponible:

```powershell
python Documentacion/tools/generar_api.py
```

El generador extrae rutas y esquemas del código. Las notas y ejemplos requieren
revisión cuando cambien las reglas de negocio. No inicia la aplicación ni aplica
migraciones.

## Imágenes del catálogo

El administrador puede editar y eliminar productos desde sus tarjetas en **Productos**.
La edición admite una imagen JPG, PNG o WebP de hasta 5 MB. Al guardar sin elegir
otro archivo se conserva la imagen actual, incluso al cambiar el nombre.

La migración `AgregarImagenProducto` agrega la columna opcional `Productos.ImagenUrl`.
Debe aplicarse antes de ejecutar la versión actualizada del backend; el inicio en
desarrollo ya usa la configuración existente `Database:ApplyMigrations`.
Las pruebas de migración y eliminación utilizan bases LocalDB desechables
`TotaltechTests_<GUID>`.

Los archivos se guardan en `Frontend/wwwroot/uploads/productos`, fuera del control
de versiones. Al publicar, conservar esa carpeta y sus permisos de escritura;
respaldarla junto con la base de datos. Reemplazar una imagen no borra archivos
anteriores. Las rutas de las imágenes originales se mantienen como alternativa
para productos que todavía no tienen una imagen guardada.

## Integrantes:
- Daiana Chinellato
- Facundo Sola


## Documentación
### 2025
1. [Documentación del proyecto - V1](https://docs.google.com/document/d/1idN0nMrU14dppRBvilRWNK-GQ6nmxF0acxeDvIJIo_k/edit?usp=sharing)
2. [TotalTech (Sitio Web) - Mockups](https://www.canva.com/design/DAGoTYBn5R4/PfSsqsz8WeJDvvYYRsEFwQ/edit?utm_content=DAGoTYBn5R4&utm_campaign=designshare&utm_medium=link2&utm_source=sharebutton)
3. [Modo Admin - Mockup](https://www.canva.com/design/DAGqLYRgPZY/HbV_7BlRgIPMg8gacrfS9g/edit?utm_content=DAGqLYRgPZY&utm_campaign=designshare&utm_medium=link2&utm_source=sharebutton)
4. [Entidades en TotalTech](https://docs.google.com/document/d/1PgBgvqaqTyXGyQlAV0TaM2t2NB27jsaP67cjlDrJkS4/edit?usp=sharing)
5. [Caso de uso - finalizar compra](https://docs.google.com/document/d/1mUmk0ukjldsHGFpCCWxbOn7o7U-sOEqPokAhohkMDGo/edit?tab=t.0)
6. [Diagrama de Clases](https://lucid.app/lucidchart/0f14ca39-b612-47e3-9553-0a5563bd2c7a/edit?invitationId=inv_5640e91e-a9b6-4d73-8e42-e457f405b683&page=0_0#)

### 2026
1. [Documentación del Proyecto-V2](https://docs.google.com/document/d/14_6aZhFY5ACk26mPNe4lj_6eVJhEZgfSD8BGV72ycTA/edit?usp=drive_link)
2. [Documentacioón de API](https://drive.google.com/file/d/1uB4R9esellhJg2zGNR4G1xXL_0wX-4_U/view?usp=drive_link)

