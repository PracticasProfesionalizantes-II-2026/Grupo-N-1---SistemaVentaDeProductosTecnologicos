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

        def query_mock(base, expr):
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


if __name__ == "__main__":
    unittest.main()
