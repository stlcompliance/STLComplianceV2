#!/usr/bin/env python3
"""Validate generated Compliance Core baseline rule-pack CSV bundles."""

from __future__ import annotations

import generate_baseline_rulepacks


if __name__ == "__main__":
    raise SystemExit(generate_baseline_rulepacks.main(["--validate-only"]))
