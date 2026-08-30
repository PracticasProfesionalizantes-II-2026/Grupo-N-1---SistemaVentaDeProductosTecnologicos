# Coordinación de agentes de TotalTech

Este archivo coordina los especialistas del repositorio que contiene `Frontend/`
y `Totaltech/`. No reemplaza la documentación funcional ni autoriza cambios por
sí mismo.

## Estructura provisoria verificada

- `../Frontend/`: aplicación ASP.NET Core MVC con Razor, CSS y JavaScript.
- `../Totaltech/`: backend ASP.NET Core Minimal API con lógica, repositorios y EF Core.
- `../Frontend/photos/`: mockups y recursos visuales de referencia.
- Los PDF de la raíz contienen requisitos funcionales y contratos API previstos.
- La solución actual incluye solamente `Totaltech`; no asumir que compila el
  frontend cuando se compila la solución.

Antes de trabajar, confirmar que la raíz Git sea la carpeta padre de
`ControlProyecto/`. Existe una copia Git exterior con otra copia anidada; detenerse
si la raíz no coincide para evitar pérdida de código.

## Enrutamiento obligatorio

- Leer `CleanCode.md` para cambios no triviales, refactors o calidad.
- Leer `Auditor.md` para Git, contratos, seguridad, datos o cambios transversales.
- Leer `Frontend.md` antes de modificar archivos bajo `Frontend/`.
- Leer `Backend.md` antes de modificar archivos bajo `Totaltech/`.
- Para un cambio MVC -> API, aplicar `Frontend.md`, `Backend.md`, `CleanCode.md`
  y `Auditor.md` en ese orden de análisis.

## Reglas operativas

1. Inspeccionar estado Git, archivos, contratos y dependencias antes de modificar.
2. Distinguir hechos, evidencia, inferencias, riesgos y recomendaciones.
3. Preferir el menor cambio verificable y apropiado para el nivel académico.
4. No introducir frameworks, microservicios, CQRS ni capas nuevas sin necesidad
   demostrada y autorización.
5. No mezclar funcionalidad, refactor, formato, dependencias y cambios Git salvo
   que sean técnicamente inseparables.
6. Preservar cambios locales y contribuciones válidas de todas las ramas.
7. Compilar y probar los proyectos afectados; compilar no basta para declarar éxito.
8. No ejecutar commits, push, merges, rebases, resets, limpieza destructiva,
   migraciones ni cambios de esquema sin autorización explícita.
9. Seguridad, contratos públicos, migraciones, datos y conflictos destructivos
   requieren aprobación individual.

## Flujo de trabajo

```text
Descubrir -> comprender -> medir impacto -> proponer -> autorizar cuando aplique
-> implementar -> validar -> auditar -> reportar
```

El especialista del dominio propone o implementa. `CleanCode.md` revisa calidad y
`Auditor.md` revisa riesgo, trazabilidad y regresiones.

## Formato mínimo de reporte

- Objetivo y alcance.
- Hechos y evidencia (`archivo:símbolo` o comando).
- Cambios realizados o propuestos.
- Riesgo, blast radius y compatibilidad.
- Validaciones y resultado.
- Pendientes y autorizaciones necesarias.

## Definition of Ready

- Problema y comportamiento esperado comprendidos.
- Evidencia y causa raíz disponibles.
- Dependencias, contratos y blast radius identificados.
- Solución mínima, pruebas y rollback definidos.
- Autorización obtenida cuando corresponda.

## Definition of Done

- Solo se implementó el alcance aprobado.
- Backend, frontend y contratos afectados fueron validados.
- No se introdujeron regresiones conocidas ni secretos.
- Git contiene únicamente los cambios esperados.
- La documentación quedó coherente o la divergencia fue registrada.
