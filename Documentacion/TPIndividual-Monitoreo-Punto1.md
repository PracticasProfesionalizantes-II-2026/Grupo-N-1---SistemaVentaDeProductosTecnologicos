# Trabajo Práctico Individual: Monitoreo de aplicaciones

## Punto 1: diseño de tres métricas

**Aplicación elegida:** TotalTech, sistema de venta de productos tecnológicos desarrollado para la práctica profesionalizante. Permite consultar productos, gestionar usuarios y confirmar compras mediante un frontend web y una API conectada a SQL Server.

Para monitorear esta aplicación elijo tres métricas: cantidad de solicitudes por segundo, tiempo de respuesta y porcentaje de errores del servidor. Permiten relacionar la carga del sistema con su velocidad y sus fallos. Se calculan por separado para la API y el frontend, usando ventanas de cinco minutos.

## 1. Cantidad de solicitudes por segundo

**Qué mide:** la cantidad promedio de solicitudes HTTP funcionales terminadas por segundo durante los últimos cinco minutos. Su unidad es solicitudes/segundo. Se obtiene a partir del contador implementado `totaltech_http_requests_total`.

**Para qué la usaría:** para identificar los horarios de mayor actividad, comparar la carga entre el frontend y la API y relacionar un aumento de tráfico con posibles demoras o errores. También permite detectar una caída inesperada de actividad, que debe investigarse junto con la disponibilidad del servicio.

**Por qué la elegí:** TotalTech recibe solicitudes al consultar productos, iniciar sesión y operar con el carrito. Conocer esa carga ayuda a decidir cuándo investigar la capacidad del sistema. Un contador acumulado aislado no permite comparar períodos; por eso calculo su tasa por segundo. Las solicitudes no representan usuarios únicos ni ventas.

**Cómo se calcula:** tasa de crecimiento del contador en cinco minutos, agrupada por aplicación (`job`).

```promql
sum by (job) (
  rate(totaltech_http_requests_total{job=~"totaltech_api|totaltech_frontend"}[5m])
)
```

**Ejemplo de interpretación:** pasar de 2 a 20 solicitudes por segundo muestra un aumento de carga. Si además sube el tiempo de respuesta, investigaría las operaciones más utilizadas. Estos valores son ilustrativos, no mediciones realizadas.

## 2. Tiempo de respuesta HTTP: percentil 95

**Qué mide:** el tiempo de procesamiento de las solicitudes en el servidor, expresado en segundos. Uso el percentil 95 (p95): un valor estimado por debajo del cual se encuentra aproximadamente el 95 % de las duraciones observadas. Se calcula con el histograma implementado `totaltech_http_request_duration_seconds`.

**Para qué la usaría:** para detectar lentitud al consultar el catálogo o realizar operaciones, localizar las rutas que tardan más y comparar el rendimiento antes y después de un cambio. Ante una demora investigaría las llamadas entre frontend y API y los comandos a SQL Server.

**Por qué la elegí:** un sistema puede responder correctamente pero resultar incómodo si tarda demasiado. El promedio puede ocultar solicitudes lentas; el p95 permite observar la experiencia de una parte amplia de las solicitudes. Mide procesamiento del servidor, no el tiempo total de carga y renderizado en el navegador.

**Cómo se calcula:** percentil 95 de los histogramas de los últimos cinco minutos, agrupados por aplicación.

```promql
histogram_quantile(
  0.95,
  sum by (job, le) (
    rate(totaltech_http_request_duration_seconds_bucket{job=~"totaltech_api|totaltech_frontend"}[5m])
  )
)
```

**Criterio inicial de alerta implementado:** p95 mayor a 1 segundo, con al menos 20 solicitudes en la ventana de cinco minutos y condición sostenida durante cinco minutos. Es un umbral inicial que debe ajustarse con mediciones del uso real.

## 3. Porcentaje de errores HTTP del servidor

**Qué mide:** el porcentaje de solicitudes terminadas con códigos HTTP 500 a 599 respecto del total de solicitudes registradas en los últimos cinco minutos. Su unidad es porcentaje. Se deriva del contador `totaltech_http_requests_total`, filtrando su etiqueta `code`.

**Para qué la usaría:** para detectar fallos que impiden completar operaciones y priorizar su diagnóstico. Si el porcentaje aumenta, revisaría las rutas afectadas, los registros de errores y la disponibilidad de las dependencias antes de atribuir una causa.

**Por qué la elegí:** la confiabilidad es fundamental en una aplicación de ventas. El porcentaje permite comparar períodos con distinto volumen de tráfico: diez errores sobre cien solicitudes tienen un impacto relativo diferente de diez sobre diez mil. Los códigos 4xx se analizan aparte, porque pueden representar entradas inválidas o accesos rechazados; esta métrica se enfoca en fallos del servidor.

**Cómo se calcula:** tasa de respuestas 5xx dividida por la tasa total, multiplicada por cien y agrupada por aplicación.

```promql
100 * sum by (job) (
  rate(totaltech_http_requests_total{job=~"totaltech_api|totaltech_frontend",code=~"5.."}[5m])
)
/ sum by (job) (
  rate(totaltech_http_requests_total{job=~"totaltech_api|totaltech_frontend"}[5m])
)
```

**Criterio inicial de alerta implementado:** más de 5 % de errores, con al menos 20 solicitudes en la ventana de cinco minutos y condición sostenida durante cinco minutos. Por ejemplo, 8 respuestas 5xx entre 100 solicitudes equivalen al 8 %. Es un ejemplo ilustrativo.

## Correspondencia con la implementación existente

Las tres métricas están respaldadas por la instrumentación compartida en `Observabilidad/Instrumentacion/ObservabilityExtensions.cs`, utilizada por ambos proyectos. Dos instrumentos —un contador y un histograma— permiten calcular las tres métricas diseñadas; no es necesario crear un contador adicional para el porcentaje de errores.

Prometheus obtiene los datos mediante los endpoints locales `/metrics` de la API y del frontend. La configuración está en `Observabilidad/prometheus.yml`; las alertas de latencia y errores están en `Observabilidad/alertas.yml`. La guía de ejecución se encuentra en [Observabilidad.md](Observabilidad.md).

La recolección está habilitada en Development y depende de `Observability:Enabled`. Excluye `/metrics` y los archivos estáticos contemplados por la instrumentación. Las etiquetas usan rutas normalizadas y no incluyen datos personales ni credenciales. Los contadores se reinician con el proceso, por lo que las consultas utilizan `rate` para calcular tasas considerando esos reinicios.

Las consultas necesitan tráfico y al menos dos extracciones en la ventana. Si no existen muestras, pueden no devolver series; si no hubo solicitudes, el porcentaje no tiene un denominador válido. La ausencia de datos no demuestra por sí sola que el servicio esté funcionando bien. El porcentaje utiliza todas las solicitudes registradas como denominador, incluidas las canceladas, y cuenta únicamente 5xx en el numerador.

Este documento cubre el diseño, la utilidad y la justificación de las tres métricas solicitadas en el punto 1. La entrega en Classroom debe realizarse por separado.
