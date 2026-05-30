#!/usr/bin/env python3
"""Validate generated Title 49 Compliance Core rule-pack CSV bundles."""

from __future__ import annotations

import generate_title49_rulepacks


if __name__ == "__main__":
    raise SystemExit(generate_title49_rulepacks.main(["--validate-only"]))
