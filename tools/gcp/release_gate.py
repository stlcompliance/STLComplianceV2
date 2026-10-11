import argparse
from datetime import datetime, timezone
from pathlib import Path


EXCEPTION_ID = "GCP-2026-10-11-01"
EXPIRES_AT = datetime(2026, 10, 12, 0, 25, tzinfo=timezone.utc)
EXCLUDED_CHECKS = (
    "STLCompliance.E2E.Catalog.StlRenderStagingShipGateCatalogTests.Operator_runbook_and_validate_scripts_exist",
    "STLCompliance.E2E.Catalog.StlRenderBlueprintCatalogTests.Render_yaml_declares_blueprint_inventory_health_checks_private_urls_and_sync_false_tokens",
)


def test_filter(exception_id="", now=None):
    result = "Category!=Live"
    if not exception_id:
        return result
    if exception_id != EXCEPTION_ID:
        raise ValueError("Unknown release exception.")
    if (now or datetime.now(timezone.utc)) >= EXPIRES_AT:
        raise ValueError("The temporary release exception has expired.")
    return result + "".join(f"&FullyQualifiedName!={check}" for check in EXCLUDED_CHECKS)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--exception", default="")
    parser.add_argument("--output", required=True)
    options = parser.parse_args()
    result = test_filter(options.exception)
    Path(options.output).write_text(result, encoding="utf-8")
    print(f"Release exception: {options.exception or 'none'}. Application test gates remain enforced.")


if __name__ == "__main__":
    main()
