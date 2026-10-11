import argparse
import json
from pathlib import Path

from bootstrap import command
from deploy import STORAGE_SETTINGS, validate


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--project", required=True)
    parser.add_argument("--config", required=True)
    parser.add_argument("--region", default="us-central1")
    options = parser.parse_args()
    path = Path(options.config)
    config = json.loads(path.read_text(encoding="utf-8"))
    buckets = json.loads(command(options.project, "storage", "buckets", "list", "--format=json"))
    existing = {bucket["name"] for bucket in buckets}
    config.setdefault("storageBuckets", {})
    for service in STORAGE_SETTINGS:
        bucket = f"{options.project}-v2-{service.removesuffix('-api')}-documents"
        if bucket not in existing:
            command(options.project, "storage", "buckets", "create", f"gs://{bucket}",
                    "--location", options.region, "--uniform-bucket-level-access",
                    "--public-access-prevention", "--soft-delete-duration=7d")
        command(options.project, "storage", "buckets", "add-iam-policy-binding", f"gs://{bucket}",
                "--member", f"serviceAccount:{config['runtimeServiceAccount']}", "--role=roles/storage.objectUser")
        config["storageBuckets"][service] = bucket
        print(f"Prepared private durable storage for {service}.", flush=True)
    validate(config)
    path.write_text(json.dumps(config, indent=2) + "\n", encoding="utf-8")


if __name__ == "__main__":
    main()
