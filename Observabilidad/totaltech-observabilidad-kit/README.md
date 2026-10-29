# Kit local de observabilidad para TotalTech (prototipo)

Este kit es una propuesta independiente. No se ha aplicado al repositorio GitHub ni probado contra sus servicios en ejecución. No agrega migraciones ni modifica la base SQL.

## Archivos
- `Observabilidad/AgenteDiagnostico/diagnostico.py`: vigila `/api/v1/alerts` de Prometheus, consulta PromQL y resume indicadores de los logs **sin copiar mensajes completos**.
- `Observabilidad/Grafana/provisioning/datasources/prometheus.yml`: datasource local.
- `Observabilidad/Grafana/provisioning/dashboards/providers.yml`: carga automática del dashboard.
- `Observabilidad/Grafana/dashboards/totaltech.json`: dashboard de 8 paneles.

## Pasos en Windows
1. Copiar `Observabilidad/` dentro de la raíz real del repositorio, SIN reemplazar archivos existentes.
2. Iniciar TotalTech y Prometheus utilizando los scripts del proyecto, hasta que `http://127.0.0.1:9090/targets` muestre API y Frontend UP.
3. Instalar Grafana OSS para Windows desde el sitio oficial y detener Grafana antes de configurar provisioning.
4. Copiar el YAML de datasource a `<GRAFANA_HOME>/conf/provisioning/datasources/totaltech.yml` y el YAML de dashboard provider a `<GRAFANA_HOME>/conf/provisioning/dashboards/totaltech.yml`. En algunas instalaciones de Windows el directorio de provisioning está en `<GRAFANA_HOME>/conf/provisioning/`.
5. En una terminal PowerShell definir el path **absoluto** que apunta a `Observabilidad/Grafana/dashboards` antes de iniciar Grafana desde esa misma sesión:

```powershell
$env:TOTALTECH_GRAFANA_DASHBOARDS_PATH = 'C:/RUTA/REPOSITORIO/Observabilidad/Grafana/dashboards'
cd 'C:\RUTA\GRAFANA'
.\bin\grafana.exe server
```

6. Abrir `http://127.0.0.1:3000`; comprobar datasource y dashboard `TotalTech | Estado y Diagnóstico`.
7. En otra terminal, desde la raíz del proyecto:

```powershell
python .\Observabilidad\AgenteDiagnostico\diagnostico.py --once
python .\Observabilidad\AgenteDiagnostico\diagnostico.py --watch --interval 30
```

Los informes aparecen en `Desarrollo/logs/diagnosticos/`, carpeta ya ignorada por Git. Para reproducir una prueba, usar series sintéticas y un servidor simulado, nunca provocar fallos en la base real.

## Interpretación opcional con IA local (Ollama)

Instalar Ollama desde el sitio oficial, descargar un modelo pequeño compatible y comprobar su funcionamiento local. Sustituir `MI_MODELO_LOCAL` por el nombre exacto de un modelo **ya instalado**. El script sólo envía al modelo local agregados de Prometheus e indicadores contados; no copia logs SQL ni tokens.

```powershell
python .\Observabilidad\AgenteDiagnostico\diagnostico.py --watch --interval 30 --ollama-model MI_MODELO_LOCAL
```

Ollama se consulta por `POST http://127.0.0.1:11434/api/generate`, con `stream: false` y `format: json`. Si no responde, el informe determinista se conserva y la sección de IA figura como no disponible. La IA no ejecuta comandos ni modifica datos.

## Limites del MVP
- El monitor se ejecuta sólo mientras su terminal permanece abierta; `--once` no genera archivos si no hay alertas `firing`.
- Sin `--ollama-model`, es un recolector determinista; con la opción, produce hipótesis mediante un modelo de IA local. No implementa todavía herramientas de diagnóstico seleccionadas dinámicamente por el modelo, ni atribución causal de consultas EF Core.
- Los logs de consola de TotalTech podrían no llevar timestamp/correlation-id. Se cuentan señales, pero no se copian cadenas potencialmente sensibles ni se infiere causalidad.
- Para completar un agente con IA, añadir logs estructurados con `trace_id`, `route`, `operation_id` (allowlist) y elapsed_ms. Luego permitir que un modelo local procese exclusivamente informes saneados y formule hipótesis bajo evaluación humana.
- Sólo para Windows local; no es una configuración lista para producción. No exponer puertos ni acceso público, proteger credenciales de Grafana, conservar permisos de solo lectura y evitar información de clientes.
