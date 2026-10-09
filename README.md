# TotalTech

## Métricas locales con Prometheus

Iniciar la API con el perfil **TotaltechLocalDb** y el frontend con **http** (comandos debajo) y ejecutar desde esta raíz:

```powershell
.\Observabilidad\iniciar-prometheus.ps1
```

Abrir [Prometheus](http://127.0.0.1:9090), comprobar ambos servicios en [Targets](http://127.0.0.1:9090/targets) y consultar [Alertas](http://127.0.0.1:9090/alerts). Detener con Ctrl+C. El script utiliza la distribución Windows 3.15.0 descargada en `ControlProyecto/`; no inicia aplicaciones ni aplica migraciones.

La [guía de observabilidad](Documentacion/Observabilidad.md) incluye instalación, configuración, seguridad, consultas y diagnóstico. `/metrics` está habilitado en Development y restringido a loopback.

## Conexión SQL Server en Development

La API utiliza `ConnectionStrings:DefaultConnection` tal como esté configurada. User Secrets y variables de entorno pueden sobrescribir `appsettings.Development.json`. Para una base ya registrada, configurar el servidor y el catálogo (`Database`); el arranque no añade `AttachDBFilename` ni busca archivos `.mdf` en el perfil del usuario. Una adjunción indicada explícitamente en la configuración se conserva.

Si aparece el error `1801` (la base ya existe), verificar que la instancia configurada pueda abrir ese catálogo. En el equipo diagnosticado, `MSSQLLocalDB` y `TotaltechLocalDb` registran `TotaltechDev` con los mismos archivos. Si una instancia los está usando, la otra no puede abrirlos. Se comprobó que `MSSQLLocalDB` abre la base existente cuando la otra está detenida. No eliminar ni volver a adjuntar sus archivos para resolver ese conflicto.

El perfil optativo **TotaltechLocalDb** selecciona explícitamente el servidor `(localdb)\MSSQLLocalDB` y el catálogo `TotaltechDev`, sin adjuntar archivos. El nombre del perfil identifica el arranque local de TotalTech; no es el nombre de la instancia SQL. Esta instancia es la que aparece al ejecutar `sqllocaldb info` en la terminal de VS Code del usuario. La versión anterior del perfil apuntaba a la instancia personalizada `TotaltechLocalDb`, que esa terminal no encuentra. Los perfiles generales `http` y `https` conservan su configuración. Desde esta raíz, usar dos terminales:

```powershell
dotnet run --project Totaltech/Totaltech.csproj --launch-profile TotaltechLocalDb
```

```powershell
dotnet run --project Frontend/Frontend.csproj --launch-profile http
```

La API escucha en `5070` y el frontend en `5087`. Detener cada proceso con **Ctrl+C** antes de iniciar otra copia. Para usar el perfil de solución **TotalTech completo**, seleccionar también **TotaltechLocalDb** como perfil de arranque del proyecto API: el perfil de solución inicia ambos proyectos, pero por sí solo no fija la conexión SQL. Elegir ese modo o las dos terminales. Una instancia previa en segundo plano también ocupa los mismos puertos.

Usar una sola instancia SQL para esos archivos: no iniciar a la vez otra copia de la API que seleccione la instancia personalizada mediante User Secrets. Si aparece `The specified LocalDB instance does not exist`, comprobar `sqllocaldb info` desde la misma terminal y seleccionar una instancia que allí exista. El perfil local fija la conexión para evitar que una variable heredada o User Secrets elijan el otro servidor.

Validación del 09/10/2026: el perfil corregido compiló e inició la API en `5070` contra `MSSQLLocalDB`. La base existente conservó sus tres migraciones; no se aplicaron migraciones pendientes. El catálogo y `/metrics` respondieron `200`. Se cerró la API y, tras comprobar que no quedaban otras sesiones, se detuvo la instancia SQL iniciada para la prueba para liberar los archivos.

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

