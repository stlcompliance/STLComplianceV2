import argparse
import json
import secrets
import subprocess
from pathlib import Path

from deploy import GCLOUD, PRODUCTS, validate


def command(project, *arguments, data=None):
    result = subprocess.run([GCLOUD, *arguments, "--project", project, "--quiet"],
                            input=data, capture_output=True, text=True)
    if result.returncode:
        diagnostic = result.stderr
        for argument in arguments:
            if argument.startswith("--password="):
                diagnostic = diagnostic.replace(argument.split("=", 1)[1], "[REDACTED]")
        if data:
            diagnostic = diagnostic.replace(data, "[REDACTED]")
        raise RuntimeError(f"Google Cloud operation failed: {' '.join(arguments[:3])}. {diagnostic.strip()}")
    return result.stdout.strip()


def ensure_secret(project, name, value_factory, runtime):
    existing = json.loads(command(project, "secrets", "list", "--filter", f"name:{name}", "--format=json"))
    if not any(secret["name"].endswith("/" + name) for secret in existing):
        command(project, "secrets", "create", name, "--replication-policy=automatic")
    versions = json.loads(command(project, "secrets", "versions", "list", name,
                                  "--filter=state:ENABLED", "--format=json"))
    if versions:
        version = max(int(item["name"].rsplit("/", 1)[-1]) for item in versions)
    else:
        created = json.loads(command(project, "secrets", "versions", "add", name,
                                     "--data-file=-", "--format=json", data=value_factory()))
        version = int(created["name"].rsplit("/", 1)[-1])
    command(project, "secrets", "add-iam-policy-binding", name,
            "--member", f"serviceAccount:{runtime}", "--role=roles/secretmanager.secretAccessor")
    return f"{name}:{version}"


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--project", required=True)
    parser.add_argument("--instance", required=True)
    parser.add_argument("--runtime-account", required=True)
    parser.add_argument("--admin-email", required=True)
    parser.add_argument("--redis-url", required=True)
    parser.add_argument("--output", default=str(Path(__file__).with_name("deployment.json")))
    options = parser.parse_args()
    instance = json.loads(command(options.project, "sql", "instances", "describe", options.instance, "--format=json"))
    if instance["state"] != "RUNNABLE":
        raise RuntimeError("The replacement instance must be RUNNABLE.")
    prefix = options.instance
    runtime = options.runtime_account
    config = {"runtimeServiceAccount": runtime, "cloudSqlInstance": instance["connectionName"],
              "redisUrl": options.redis_url, "network": "default", "subnet": "default",
              "sharedSecrets": {}, "nexarrSecrets": {}, "databaseSecrets": {}, "urls": {},
              "serviceEnvironment": {"nexarr-api": {"Seed__FirstAdminEmail": options.admin_email}}}
    for key, suffix in (("AUTH_SIGNING_KEY", "auth"), ("SERVICE_TOKEN_SIGNING_KEY", "service-auth"),
                        ("STL_INTEGRATION_BOOTSTRAP_SECRET", "bootstrap")):
        config["sharedSecrets"][key] = ensure_secret(options.project, f"{prefix}-{suffix}",
                                                    lambda: secrets.token_urlsafe(48), runtime)
    for key, suffix in (("TENANT_INTEGRATION_ENCRYPTION_KEY", "encryption"), ("Seed__FirstAdminPassword", "admin-password")):
        config["nexarrSecrets"][key] = ensure_secret(options.project, f"{prefix}-{suffix}",
                                                    lambda: secrets.token_urlsafe(48), runtime)
    existing_databases = {item["name"] for item in json.loads(command(
        options.project, "sql", "databases", "list", "--instance", options.instance, "--format=json"))}
    existing_users = {item["name"] for item in json.loads(command(
        options.project, "sql", "users", "list", "--instance", options.instance, "--format=json"))}
    for product in PRODUCTS:
        name = product.lower()
        password_reference = ensure_secret(options.project, f"{prefix}-{name}-password",
                                           lambda: secrets.token_urlsafe(48), runtime)
        secret_name, version = password_reference.split(":")
        password = command(options.project, "secrets", "versions", "access", version, "--secret", secret_name)
        if name not in existing_users:
            command(options.project, "sql", "users", "create", name,
                    "--instance", options.instance, f"--password={password}")
        if name not in existing_databases:
            command(options.project, "sql", "databases", "create", name, "--instance", options.instance)
        connection = (f"Host=/cloudsql/{instance['connectionName']};Database={name};Username={name};"
                      f"Password={password};Maximum Pool Size=10;Timeout=30")
        config["databaseSecrets"][name] = ensure_secret(options.project, f"{prefix}-{name}-database",
                                                       lambda: connection, runtime)
        print(f"Prepared {product} database and pinned secret references.", flush=True)
    command(options.project, "projects", "add-iam-policy-binding", options.project,
            "--member", f"serviceAccount:{runtime}", "--role=roles/cloudsql.client", "--condition=None")
    validate(config)
    Path(options.output).write_text(json.dumps(config, indent=2) + "\n", encoding="utf-8")
    print("Configuration saved without secret values. Administrator password is in Secret Manager.")


if __name__ == "__main__":
    main()
