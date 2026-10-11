import argparse
import json
import tempfile
import unittest
from pathlib import Path
from unittest.mock import patch

import deploy


class DeploymentTests(unittest.TestCase):
    def setUp(self):
        self.config = json.loads((Path(__file__).parent / "deployment.example.json").read_text())
        self.config["runtimeServiceAccount"] = "runtime@example.iam.gserviceaccount.com"
        self.config["vpcConnector"] = "suite-connector"
        self.options = argparse.Namespace(project="example", region="us-central1", repository="suite", tag="build-id")
        self.urls = {product.lower() + "-api": f"https://{product.lower()}-api.example.com"
                     for product in deploy.PRODUCTS}
        self.urls.update({frontend: f"https://{frontend}.example.com" for frontend in deploy.FRONTENDS})

    def test_secret_versions_are_required_and_pinned(self):
        deploy.validate(self.config)
        self.config["sharedSecrets"]["AUTH_SIGNING_KEY"] = "auth-signing-key:latest"
        with self.assertRaises(ValueError):
            deploy.validate(self.config)

    def test_missing_database_configuration_fails(self):
        del self.config["databaseSecrets"]["ledgarr"]
        with self.assertRaises(ValueError):
            deploy.validate(self.config)

    def test_suspended_database_stops_before_other_operations(self):
        with patch.object(deploy, "gcloud", return_value="SUSPENDED") as command:
            with self.assertRaisesRegex(RuntimeError, "restore database"):
                deploy.preflight(self.options, self.config)
            self.assertEqual(command.call_count, 1)

    def test_direct_vpc_subnet_must_match_network(self):
        self.config.pop("vpcConnector")
        with patch.object(deploy, "gcloud", side_effect=["RUNNABLE", "projects/example/networks/wrong"]):
            with self.assertRaisesRegex(RuntimeError, "subnet"):
                deploy.preflight(self.options, self.config)

    def test_direct_vpc_deployment_uses_network_and_subnet(self):
        self.config.pop("vpcConnector")
        deploy.validate(self.config)
        with tempfile.TemporaryDirectory() as directory:
            output = Path(directory)
            (output / "build.tsv").write_text("nexarr-api\t-\t-\tapi\n")
            with patch.object(deploy.subprocess, "run") as command:
                deploy.deploy(self.options, self.config, output)
                arguments = command.call_args.args[0]
                self.assertIn("--network", arguments)
                self.assertIn("--subnet", arguments)
                self.assertNotIn("--vpc-connector", arguments)

    def test_document_storage_is_mounted_only_on_owning_service(self):
        self.config["storageBuckets"] = {"recordarr-api": "private-recordarr-documents"}
        deploy.validate(self.config)
        with tempfile.TemporaryDirectory() as directory:
            output = Path(directory)
            with patch.object(deploy, "preflight"), patch.object(deploy, "gcloud", side_effect=["123", "[]"]):
                deploy.prepare(self.options, self.config, output)
            environment = json.loads((output / "recordarr-api.json").read_text())
            self.assertEqual(environment["DocumentStorage__RootPath"], "/mnt/documents")
            (output / "build.tsv").write_text("recordarr-api\t-\t-\tapi\n")
            with patch.object(deploy.subprocess, "run") as command:
                deploy.deploy(self.options, self.config, output)
                arguments = command.call_args.args[0]
                self.assertIn("--add-volume", arguments)
                self.assertIn("volume=documents,mount-path=/mnt/documents", arguments)

    def test_unknown_storage_owner_is_rejected(self):
        self.config["storageBuckets"] = {"nexarr-api": "private-documents"}
        with self.assertRaises(ValueError):
            deploy.validate(self.config)

    def test_every_product_has_launch_and_integration_destinations(self):
        environment = deploy.runtime_environment("nexarr-api", self.urls)
        for product in deploy.PRODUCTS:
            self.assertEqual(environment[f"{product}__BaseUrl"], self.urls[product.lower() + "-api"])
            if product != "NexArr":
                self.assertEqual(environment[f"Launch__Products__{product.lower()}__BaseUrl"],
                                 self.urls[product.lower() + "-frontend"])
        self.assertEqual(environment["Launch__Products__nexarr__BaseUrl"], self.urls["suite-frontend"])
        self.assertFalse(any("localhost" in value for value in environment.values()))

    def test_worker_job_destinations_are_rewritten(self):
        environment = deploy.runtime_environment("shared-worker", self.urls)
        self.assertEqual(environment["TrainArrQualificationExpiration__TrainArrBaseUrl"], self.urls["trainarr-api"])
        self.assertFalse(any("localhost" in value for value in environment.values()))

    def test_marketing_employment_intake_uses_staffarr(self):
        self.assertEqual(deploy.frontend_api("stlcompliancesite"), "staffarr-api")
        self.assertEqual(deploy.frontend_api("fieldcompanion-frontend"), "nexarr-api")

    def test_prepare_discovers_all_builds_and_frontend_proxy_destinations(self):
        with tempfile.TemporaryDirectory() as directory:
            output = Path(directory)
            with patch.object(deploy, "preflight"), patch.object(deploy, "gcloud", side_effect=["123456", "[]"]):
                deploy.prepare(self.options, self.config, output)
            rows = (output / "build.tsv").read_text().splitlines()
            self.assertEqual(len(rows), 40)
            self.assertEqual(len(set(row.split("\t")[0] for row in rows)), 40)
            public_settings = json.loads((output / "frontend-build.json").read_text())
            self.assertEqual(public_settings["VITE_SUITE_LOGIN_URL"], "https://suite-frontend-123456.us-central1.run.app/login")
            self.assertEqual(public_settings["VITE_STAFFARR_API_BASE"], "")
            for frontend in deploy.FRONTENDS:
                settings = json.loads((output / f"{frontend}.json").read_text())
                self.assertIn(deploy.frontend_api(frontend), settings["API_UPSTREAM"])
            settings = json.loads((output / "staffarr-worker.json").read_text())
            self.assertEqual(settings["STL_INTEGRATION_TOKEN_AUTO_PROVISION"], "false")
            settings = json.loads((output / "shared-worker.json").read_text())
            self.assertEqual(settings["STL_INTEGRATION_TOKEN_AUTO_PROVISION"], "true")

    def test_deploy_stops_on_failure(self):
        with tempfile.TemporaryDirectory() as directory:
            output = Path(directory)
            (output / "build.tsv").write_text("nexarr-api\t-\t-\tapi\nstaffarr-api\t-\t-\tapi\n")
            with patch.object(deploy.subprocess, "run", side_effect=RuntimeError("deployment failed")) as command:
                with self.assertRaises(RuntimeError):
                    deploy.deploy(self.options, self.config, output)
                self.assertEqual(command.call_count, 1)
                arguments = command.call_args.args[0]
                self.assertIn("--set-secrets", arguments)
                self.assertIn("--set-cloudsql-instances", arguments)
                self.assertNotIn("--add-cloudsql-instances", arguments)
                self.assertIn("--vpc-egress=private-ranges-only", arguments)


if __name__ == "__main__":
    unittest.main()
