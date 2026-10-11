import argparse
import json
import re
import shutil
import subprocess
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
PRODUCTS = ["NexArr", "StaffArr", "TrainArr", "MaintainArr", "RoutArr", "SupplyArr",
            "CustomArr", "OrdArr", "LedgArr", "ReportArr", "ComplianceCore", "RecordArr", "AssurArr", "LoadArr"]
WORKERS = ["NexArr", "StaffArr", "TrainArr", "MaintainArr", "RoutArr", "SupplyArr", "LedgArr", "ComplianceCore", "Shared"]
FRONTENDS = ["suite-frontend", "fieldcompanion-frontend"] + [
    product.lower() + "-frontend" for product in PRODUCTS if product != "NexArr"
] + ["stlcompliancesite", "stlcompliancekb"]
GCLOUD = shutil.which("gcloud") or "gcloud"
STORAGE_SETTINGS = {"staffarr-api": "DocumentStorage", "trainarr-api": "EvidenceStorage",
                    "maintainarr-api": "EvidenceStorage", "routarr-api": "CaptureAttachmentStorage",
                    "supplyarr-api": "DocumentStorage", "recordarr-api": "DocumentStorage"}


def gcloud(*arguments):
    return subprocess.check_output([GCLOUD, *arguments, "--quiet"], text=True).strip()


def frontend_api(frontend):
    if frontend == "stlcompliancesite":
        return "staffarr-api"
    if frontend in ("suite-frontend", "fieldcompanion-frontend", "stlcompliancesite", "stlcompliancekb"):
        return "nexarr-api"
    return frontend.replace("-frontend", "-api")


def worker_urls(settings, urls, prefix=""):
    result = {}
    for key, value in settings.items():
        path = f"{prefix}__{key}" if prefix else key
        if isinstance(value, dict):
            result.update(worker_urls(value, urls, path))
        else:
            for product in PRODUCTS:
                if key == f"{product}BaseUrl":
                    result[path] = urls[product.lower() + "-api"]
    return result


def runtime_environment(service, urls, root=ROOT):
    environment = {
        "STL_SERVICE_NAME": service,
        "NexArr__BaseUrl": urls["nexarr-api"],
        "DOTNET_ENVIRONMENT": "Production",
        "ASPNETCORE_ENVIRONMENT": "Production",
    }
    for product in PRODUCTS:
        environment[f"{product}__BaseUrl"] = urls[product.lower() + "-api"]
        environment[f"PlatformProducts__{product}BaseUrl"] = urls[product.lower() + "-api"]
    for frontend in FRONTENDS:
        if not frontend.endswith("-frontend"):
            continue
        key = frontend.removesuffix("-frontend")
        display = next((product for product in PRODUCTS if product.lower() == key),
                       {"suite": "Suite", "fieldcompanion": "FieldCompanion"}.get(key, key))
        environment[f"Cors__{display}FrontendOrigin"] = urls[frontend]
        if key != "suite":
            environment[f"Launch__Products__{key}__BaseUrl"] = urls[frontend]
            environment[f"Launch__Products__{key}__LaunchPath"] = "/launch"
            environment[f"{display}__FrontendBaseUrl"] = urls[frontend]
    environment["Launch__Products__nexarr__BaseUrl"] = urls["suite-frontend"]
    environment["Launch__Products__nexarr__LaunchPath"] = "/app"
    environment["Cors__PublicSiteOrigin"] = urls["stlcompliancesite"]
    if service.endswith("-worker"):
        product = next(product for product in WORKERS if service == product.lower() + "-worker")
        settings_path = root / "workers" / service / f"{product}.Worker" / "appsettings.json"
        if settings_path.exists():
            environment.update(worker_urls(json.loads(settings_path.read_text(encoding="utf-8-sig")), urls))
    return environment


def prepare(options, config, output):
    preflight(options, config)
    project_number = gcloud("projects", "describe", options.project, "--format=value(projectNumber)")
    existing = json.loads(gcloud("run", "services", "list", "--project", options.project,
                                 "--region", options.region, "--format=json"))
    urls = {service["metadata"]["name"]: service.get("status", {}).get("url") for service in existing}
    for service in [product.lower() + "-api" for product in PRODUCTS] + FRONTENDS:
        urls[service] = config.get("urls", {}).get(service) or urls.get(service) or (
            f"https://{service}-{project_number}.{options.region}.run.app")
    for value in urls.values():
        if value and not re.fullmatch(r"https://[a-zA-Z0-9.-]+", value):
            raise ValueError("Service URLs must be HTTPS origins without paths.")
    frontend_env = {
        "VITE_SUITE_URL": urls["suite-frontend"],
        "VITE_SUITE_LOGIN_URL": urls["suite-frontend"] + "/login",
        "VITE_KB_URL": urls["stlcompliancekb"],
        "VITE_KB_BASE_URL": urls["stlcompliancekb"],
        "VITE_SITE_BASE_URL": urls["stlcompliancesite"],
        "VITE_MARKETING_SITE_URL": urls["stlcompliancesite"],
        "VITE_PUBLIC_SITE_BASE": urls["stlcompliancesite"],
        "VITE_STAFFARR_API_BASE": "",
    }
    for frontend in FRONTENDS:
        if frontend.endswith("-frontend") and frontend != "suite-frontend":
            key = frontend.removesuffix("-frontend").upper()
            frontend_env[f"VITE_{key}_FRONTEND_BASE"] = urls[frontend]
    (output / "frontend-build.json").write_text(json.dumps(frontend_env), encoding="utf-8")
    manifest = []
    for product in PRODUCTS:
        service = product.lower() + "-api"
        manifest.append((service, f"apps/{service}/{product}.Api/{product}.Api.csproj", f"{product}.Api", "api"))
    for product in WORKERS:
        service = product.lower() + "-worker"
        manifest.append((service, f"workers/{service}/{product}.Worker/{product}.Worker.csproj", f"{product}.Worker", "worker"))
    manifest.extend((frontend, "-", "-", "frontend") for frontend in FRONTENDS)
    (output / "build.tsv").write_text("\n".join("\t".join(row) for row in manifest) + "\n", encoding="utf-8")
    for service, _, _, kind in manifest:
        environment = ({"API_UPSTREAM": urls[frontend_api(service)]} if kind == "frontend"
                       else runtime_environment(service, urls))
        if kind != "frontend":
            environment["REDIS_URL"] = config["redisUrl"]
            environment["STL_INTEGRATION_TOKEN_AUTO_PROVISION"] = (
                "true" if kind == "api" or service in ("nexarr-worker", "shared-worker") else "false")
        environment.update(config.get("serviceEnvironment", {}).get(service, {}))
        if service in config.get("storageBuckets", {}):
            environment[f"{STORAGE_SETTINGS[service]}__RootPath"] = "/mnt/documents"
        (output / f"{service}.json").write_text(json.dumps(environment), encoding="utf-8")


def deploy(options, config, output):
    manifest = [line.split("\t") for line in (output / "build.tsv").read_text().splitlines()]
    for service, _, _, kind in manifest:
        command = ["run", "deploy", service, "--project", options.project, "--region", options.region,
                   "--image", f"{options.region}-docker.pkg.dev/{options.project}/{options.repository}/{service}:{options.tag}",
                   "--port=8080", "--execution-environment=gen2", "--cpu=1", "--memory=1Gi", "--cpu-boost",
                   "--startup-probe=tcpSocket.port=8080,periodSeconds=20,failureThreshold=30",
                   "--env-vars-file", str(output / f"{service}.json"), "--quiet"]
        if kind == "frontend":
            command.append("--allow-unauthenticated")
        else:
            secrets = dict(config["sharedSecrets"])
            database_key = service.removesuffix("-api").removesuffix("-worker")
            if kind == "api" or service == "ledgarr-worker":
                secrets["DATABASE_URL"] = config["databaseSecrets"][database_key]
            if service == "nexarr-api":
                secrets.update(config["nexarrSecrets"])
            command.extend(["--service-account", config["runtimeServiceAccount"],
                            "--set-cloudsql-instances", config["cloudSqlInstance"],
                            "--vpc-egress=private-ranges-only",
                            "--set-secrets", ",".join(f"{key}={value}" for key, value in secrets.items())])
            if config.get("vpcConnector"):
                command.extend(["--vpc-connector", config["vpcConnector"]])
            else:
                command.extend(["--network", config["network"], "--subnet", config["subnet"]])
            if kind == "worker":
                command.extend(["--no-allow-unauthenticated", "--no-cpu-throttling", "--min-instances=1", "--max-instances=1"])
            else:
                command.append("--allow-unauthenticated")
            if service in config.get("storageBuckets", {}):
                bucket = config["storageBuckets"][service]
                command.extend(["--add-volume", f"name=documents,type=cloud-storage,bucket={bucket},mount-options=metadata-cache-ttl-secs=0",
                                "--add-volume-mount", "volume=documents,mount-path=/mnt/documents"])
        subprocess.run([GCLOUD, *command], check=True)


def preflight(options, config):
    sql_project, _, instance_name = config["cloudSqlInstance"].split(":")
    state = gcloud("sql", "instances", "describe", instance_name,
                   "--project", sql_project, "--format=value(state)")
    if state != "RUNNABLE":
        raise RuntimeError(f"Cloud SQL {instance_name} is {state}; restore database service before deploying.")
    if config.get("vpcConnector"):
        state = gcloud("compute", "networks", "vpc-access", "connectors", "describe", config["vpcConnector"],
                       "--project", options.project, "--region", options.region, "--format=value(state)")
        if state != "READY":
            raise RuntimeError("The Redis VPC connector must be READY before deploying.")
    else:
        network = gcloud("compute", "networks", "subnets", "describe", config["subnet"],
                         "--project", options.project, "--region", options.region, "--format=value(network)")
        if network.rsplit("/", 1)[-1] != config["network"]:
            raise RuntimeError("The Direct VPC subnet does not belong to the configured network.")
    references = {reference for section in ("sharedSecrets", "nexarrSecrets", "databaseSecrets")
                  for reference in config[section].values()}
    for reference in sorted(references):
        secret, version = reference.split(":")
        state = gcloud("secrets", "versions", "describe", version, "--secret", secret,
                       "--project", options.project, "--format=value(state)")
        if state != "ENABLED":
            raise RuntimeError(f"Secret version {reference} must be ENABLED.")


def validate(config):
    for service, bucket in config.get("storageBuckets", {}).items():
        if service not in STORAGE_SETTINGS or not re.fullmatch(r"[a-z0-9][a-z0-9.-]{1,220}[a-z0-9]", bucket):
            raise ValueError("Document buckets must target supported storage services with valid bucket names.")
    for key in ("runtimeServiceAccount", "cloudSqlInstance", "redisUrl"):
        if not config.get(key) or "REPLACE" in config[key]:
            raise ValueError(f"Configure {key} before submitting a deployment.")
    if not config.get("vpcConnector") and not (config.get("network") and config.get("subnet")):
        raise ValueError("Configure a VPC connector or a Direct VPC network and subnet.")
    required_shared = {"AUTH_SIGNING_KEY", "SERVICE_TOKEN_SIGNING_KEY", "STL_INTEGRATION_BOOTSTRAP_SECRET"}
    if not required_shared <= config.get("sharedSecrets", {}).keys():
        raise ValueError("Missing shared signing/bootstrap secret references.")
    if "TENANT_INTEGRATION_ENCRYPTION_KEY" not in config.get("nexarrSecrets", {}):
        raise ValueError("Missing tenant integration encryption secret reference.")
    for product in PRODUCTS:
        if product.lower() not in config.get("databaseSecrets", {}):
            raise ValueError(f"Missing {product} database secret reference.")
    for section in ("sharedSecrets", "nexarrSecrets", "databaseSecrets"):
        for value in config[section].values():
            if not re.fullmatch(r"[a-zA-Z0-9_-]+:[1-9][0-9]*", value):
                raise ValueError("Secret references must use a name and pinned numeric version.")


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("action", choices=["prepare", "deploy"])
    parser.add_argument("--config", required=True)
    parser.add_argument("--project", required=True)
    parser.add_argument("--region", default="us-central1")
    parser.add_argument("--repository", default="stl-compliance")
    parser.add_argument("--tag", required=True)
    options = parser.parse_args()
    config = json.loads(Path(options.config).read_text(encoding="utf-8-sig"))
    validate(config)
    output = ROOT / "artifacts" / "gcp"
    output.mkdir(parents=True, exist_ok=True)
    if options.action == "prepare":
        prepare(options, config, output)
    else:
        deploy(options, config, output)


if __name__ == "__main__":
    main()
