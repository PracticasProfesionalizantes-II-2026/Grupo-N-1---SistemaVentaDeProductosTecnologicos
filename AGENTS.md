# Coordinación de agentes de TotalTech

Este archivo gobierna todo el repositorio que contiene `Frontend/` y `Totaltech/`.
Su función es coordinar especialistas; no reemplaza la documentación funcional ni
autoriza cambios por sí mismo.

## Estructura provisoria verificada

- `Frontend/`: aplicación ASP.NET Core MVC con Razor, CSS y JavaScript.
- `Totaltech/`: backend ASP.NET Core Minimal API con lógica, repositorios y EF Core.
- `Frontend/photos/`: mockups y recursos visuales de referencia.
- `Documentación - Grupo 1 -TotalTech.pdf`: requisitos funcionales del negocio.
- `Documentación de API.pdf`: contratos API previstos; puede diferir del código.
- La solución actual incluye solamente `Totaltech`; no asumir que compila el
  frontend cuando se compila la solución.

Antes de trabajar, confirmar que `git rev-parse --show-toplevel` devuelve la
carpeta que contiene este archivo. Existe una copia Git exterior con una carpeta
anidada; detenerse si el directorio raíz no coincide para evitar pérdida de código.

## Enrutamiento obligatorio

- Leer `CleanCode.md` para cambios no triviales, refactors o revisiones de calidad.
- Leer `Auditor.md` para Git, contratos, seguridad, datos, cambios transversales o
  cualquier operación con riesgo de regresión.
- Leer `Frontend/Frontend.md` antes de modificar archivos bajo `Frontend/`.
- Leer `Totaltech/Backend.md` antes de modificar archivos bajo `Totaltech/`.
- Para un cambio MVC -> API, aplicar `Frontend/Frontend.md`,
  `Totaltech/Backend.md`, `CleanCode.md` y `Auditor.md` en ese orden de análisis.

## Reglas operativas

1. Inspeccionar antes de modificar: estado Git, archivos afectados, contratos y
   dependencias.
2. Distinguir hechos, evidencia, inferencias, riesgos y recomendaciones.
3. Preferir el menor cambio verificable y apropiado para un proyecto académico.
4. No introducir React, Angular, Vue, Tailwind, microservicios, CQRS, event
   sourcing ni capas nuevas sin una necesidad demostrada y autorización.
5. No mezclar funcionalidad, refactor, formato, dependencias y cambios Git en una
   misma modificación salvo que sean inseparables.
6. Preservar cambios locales y contribuciones válidas de todas las ramas.
7. Compilar y probar los proyectos afectados; compilar no basta para declarar
   éxito.
8. No ejecutar commits, push, merges, rebases, resets, limpieza destructiva,
   migraciones de base de datos ni cambios de esquema sin autorización explícita.
9. Los cambios de autenticación, autorización, contratos públicos, migraciones,
   datos o resolución destructiva de conflictos requieren aprobación individual.

## Flujo de trabajo

```text
Descubrir -> comprender -> medir impacto -> proponer -> autorizar cuando aplique
-> implementar -> validar -> auditar -> reportar
```

El responsable del dominio propone o implementa. `CleanCode.md` revisa calidad y
`Auditor.md` revisa riesgo, trazabilidad y regresiones.

## Formato mínimo de reporte

- Objetivo y alcance.
- Hechos y evidencia (`archivo:símbolo` o comando).
- Cambios realizados o propuestos.
- Riesgo, blast radius y compatibilidad.
- Validaciones ejecutadas y resultado.
- Pendientes, incertidumbres y autorizaciones necesarias.

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
- La documentación relevante quedó coherente o la divergencia fue registrada.

