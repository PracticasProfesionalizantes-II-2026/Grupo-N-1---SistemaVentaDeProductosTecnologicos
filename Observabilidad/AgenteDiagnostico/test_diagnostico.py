"""Tests aislados: no requieren TotalTech, Prometheus ni SQL en ejecución."""
import json
import tempfile
import unittest
from pathlib import Path
from unittest.mock import patch

import diagnostico as d


class TestsDiagnostico(unittest.TestCase):
    def test_alerta_firing_genera_json_y_deduplica(self):
        alert = {"state": "firing", "activeAt": "2026-10-23T15:00:00Z",
                 "labels": {"alertname": "TotaltechErroresElevados", "job": "totaltech_api"}}

        def get_mock(base, path, params=None):
            self.assertEqual("/api/v1/alerts", path)
            return {"alerts": [alert]}

        def query_mock(base, expr, at=None):
            self.assertIsNotNone(at)
            return [{"metric": {"route": "/productos/catalogo"}, "value": [1, "0.3"]}]

        with tempfile.TemporaryDirectory() as tmp:
            base = Path(tmp)
            (base / "API-20261023.stderr.log").write_text("fail: SqlException timeout\n", encoding="utf8")
            seen = set()
            with patch.object(d, "prom_get", side_effect=get_mock), patch.object(d, "query", side_effect=query_mock):
                d.run("http://127.0.0.1:9090", base, base / "out", seen)
                d.run("http://127.0.0.1:9090", base, base / "out", seen)
            reports = list((base / "out").glob("*.json"))
            self.assertEqual(len(reports), 1)
            saved = json.loads(reports[0].read_text(encoding="utf8"))
            self.assertEqual(saved["ruta_candidata"], "/productos/catalogo")
            self.assertEqual(saved["indicadores_logs"]["indicadores"]["sql_exception"], 1)
            self.assertEqual(saved["tipo"], "hipotesis_de_diagnostico_no_confirmada")
            self.assertNotIn("SqlException timeout", reports[0].read_text(encoding="utf8"))

    def test_no_firing_no_reporte(self):
        with tempfile.TemporaryDirectory() as tmp, patch.object(d, "prom_get", return_value={"alerts": [{"state": "pending", "labels": {"alertname": "TotaltechLatenciaElevada", "job": "totaltech_api"}}]}):
            d.run("http://127.0.0.1:9090", Path(tmp), Path(tmp) / "salida", set())
            self.assertFalse((Path(tmp) / "salida").exists())

    def test_descarta_jobs_desconocidos(self):
        with self.assertRaises(ValueError):
            d.diagnose("http://127.0.0.1:9090", {"labels": {"alertname": "TotaltechLatenciaElevada", "job": "externo"}}, Path("."))

    def test_no_prioriza_valores_cero_nan_o_inf(self):
        alert = {"state": "firing", "labels": {"alertname": "TotaltechLatenciaElevada", "job": "totaltech_api"}}
        rows = [{"metric": {"route": "/productos/catalogo"}, "value": [1, value]}
                for value in ["NaN", "+Inf", "0"]]
        with tempfile.TemporaryDirectory() as tmp, patch.object(d, "query", return_value=rows):
            report = d.diagnose("http://127.0.0.1:9090", alert, Path(tmp))
        self.assertIsNone(report["ruta_candidata"])
        json.dumps(report, allow_nan=False)

    def test_consultas_fallidas_conservan_informe_parcial(self):
        alert = {"labels": {"alertname": "TotaltechErroresElevados", "job": "totaltech_frontend"}}
        with tempfile.TemporaryDirectory() as tmp, patch.object(d, "query", side_effect=TimeoutError):
            report = d.diagnose("http://127.0.0.1:9090", alert, Path(tmp))
        self.assertEqual(len(report["errores_consulta"]), 4)
        self.assertIsNone(report["ruta_candidata"])

    def test_lectura_de_logs_acotada_y_sin_contenido_crudo(self):
        with tempfile.TemporaryDirectory() as tmp:
            path = Path(tmp) / "API.stdout.log"
            path.write_bytes(b"SqlException secreto\n" + b"x\n" * d.MAX_LOG_BYTES + b"fail: timeout\n")
            report = d.collect_log_indicators(Path(tmp))
        self.assertEqual(report["indicadores"]["sql_exception"], 0)
        self.assertEqual(report["indicadores"]["timeout"], 1)
        self.assertEqual(report["indicadores"]["http_exception"], 1)
        self.assertNotIn("secreto", json.dumps(report))

    def test_solo_etiquetas_permitidas(self):
        rows = [{"metric": {"route": "/productos/{id}", "email": "privado", "token": "secreto"}, "value": [1, "1"]}]
        self.assertEqual(d.numeric_samples(rows), [{"labels": {"route": "/productos/{id}"}, "valor": 1.0}])

    def test_rechaza_url_remota_y_redireccion(self):
        with self.assertRaises(ValueError):
            d.prom_get("http://externo:9090", "/api/v1/alerts")
        with self.assertRaises(ValueError):
            d.NoRedirects().redirect_request(None, None, 302, "", {}, "http://externo/")

    def test_rechaza_respuesta_prometheus_invalida(self):
        from unittest.mock import MagicMock
        response = MagicMock()
        response.headers = {}
        response.read.return_value = b'[]'
        response.__enter__.return_value = response
        with patch.object(d.LOCAL_HTTP, "open", return_value=response), self.assertRaises(ValueError):
            d.prom_get("http://127.0.0.1:9090", "/api/v1/alerts")

    def test_query_vector_y_momento_comun(self):
        with patch.object(d, "prom_get", return_value={"resultType": "vector", "result": []}) as get:
            self.assertEqual(d.query("http://127.0.0.1:9090", "up", "2026-10-23T15:00:00Z"), [])
        self.assertEqual(get.call_args.args[2]["time"], "2026-10-23T15:00:00Z")
        with patch.object(d, "prom_get", return_value={"resultType": "matrix", "result": []}), self.assertRaises(ValueError):
            d.query("http://127.0.0.1:9090", "up")

    def test_recuperacion_y_nuevo_incidente(self):
        alert = {"state": "firing", "activeAt": "2026-10-23T15:00:00Z", "labels": {"alertname": "TotaltechLatenciaElevada", "job": "totaltech_api"}}
        with tempfile.TemporaryDirectory() as tmp, patch.object(d, "query", return_value=[]):
            seen = set()
            with patch.object(d, "prom_get", side_effect=[{"alerts": [alert]}, {"alerts": []}, {"alerts": [alert]}]):
                d.run("http://127.0.0.1:9090", Path(tmp), Path(tmp) / "out", seen)
                self.assertEqual(len(seen), 1)
                d.run("http://127.0.0.1:9090", Path(tmp), Path(tmp) / "out", seen)
                self.assertFalse(seen)
                d.run("http://127.0.0.1:9090", Path(tmp), Path(tmp) / "out", seen)
            self.assertEqual(len(list((Path(tmp) / "out").glob("*.json"))), 2)

    def test_ollama_fallido_no_impide_informe(self):
        alert = {"state": "firing", "activeAt": "2026-10-23T15:00:00Z", "labels": {"alertname": "TotaltechErroresElevados", "job": "totaltech_api"}}
        with tempfile.TemporaryDirectory() as tmp, patch.object(d, "prom_get", return_value={"alerts": [alert]}), patch.object(d, "query", return_value=[]), patch.object(d, "analyze_with_local_ai", side_effect=TimeoutError):
            d.run("http://127.0.0.1:9090", Path(tmp), Path(tmp) / "out", set(), "modelo-local")
            saved = json.loads(next((Path(tmp) / "out").glob("*.json")).read_text(encoding="utf8"))
        self.assertEqual(saved["analisis_ia"]["estado"], "no_disponible")

    def test_ollama_solo_recibe_agregados(self):
        from unittest.mock import MagicMock
        response = MagicMock()
        response.read.return_value = b'{"response":"{\\"hipotesis\\":[]}"}'
        response.__enter__.return_value = response
        with patch.object(d.LOCAL_HTTP, "open", return_value=response) as opened:
            result = d.analyze_with_local_ai({"confianza": "baja"}, "modelo-local")
        request = opened.call_args.args[0]
        self.assertEqual(request.full_url, "http://127.0.0.1:11434/api/generate")
        self.assertFalse(json.loads(request.data)["stream"])
        self.assertEqual(result["estado"], "hipotesis_no_verificada")


if __name__ == "__main__":
    unittest.main()
