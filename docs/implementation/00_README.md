# STL Compliance / Arr Suite Implementation Plan

This package defines the build plan for the full STL Compliance / Arr suite as a greenfield masterplan.

Included documents:

- `01_MILESTONE_MASTERPLAN.md` — full milestone sequence and acceptance gates.
- `02_PRODUCT_IMPLEMENTATION_BACKLOG.md` — product-by-product build plan.
- `03_FEATURE_TO_MILESTONE_MATRIX.md` — every listed feature mapped to a milestone.
- `04_CROSS_PRODUCT_WORKFLOW_PLAN.md` — implementation plan for the suite workflows.
- `05_SHIP_GATE_AND_VALIDATION_PLAN.md` — final proof requirements.
- `feature_to_milestone_matrix.csv` — machine-readable feature coverage matrix.

Core assumptions:

- Every product ships in V1.
- Backend runtime is .NET 10.
- Frontend stack is React + TypeScript + Vite.
- Render is the deployment platform.
- Each product has its own API, worker, PostgreSQL database, and ownership boundary.
- Cross-product behavior uses APIs, events, service tokens, and local references.
- No cross-product database foreign keys.
- No product directly mutates another product database.
