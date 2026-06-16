# STL Compliance V2 Cross-Product Workflow Packs

## Purpose

Workflow packs define how existing products coordinate real business execution without violating source-of-truth boundaries.

## Files

- `order-to-fulfillment.md`
- `procure-to-receive-to-putaway.md`
- `defect-to-work-order-to-parts-to-return-to-service.md`
- `incident-to-retraining.md`
- `quality-hold-release.md`
- `vendor-order-completion-and-dispatch.md`

## Workflow pack rule

Each workflow should identify:

- trigger
- participating products
- source-of-truth table
- main flow
- alternate/blocked flows
- events
- handoffs
- APIs
- evidence
- Field Companion behavior
- ReportArr effects

## Non-goal

These workflow packs do not create a WorkflowArr product.

Workflow ownership remains with the product that owns the source record at each step.
