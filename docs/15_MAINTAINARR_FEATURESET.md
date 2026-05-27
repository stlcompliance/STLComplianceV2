# MaintainArr Feature Set

## Product Definition

MaintainArr is the asset maintenance and readiness system.

## Owns

- assets
- asset hierarchy
- asset classes/types
- inspections
- inspection templates
- defects
- work orders
- preventive maintenance
- PM schedules
- maintenance history
- asset readiness
- labor records
- part-consumption snapshots

## Does Not Own

- people source of truth
- training records
- full procurement
- vendor source of truth
- dispatch execution
- rule packs

## Core Features

- asset creation/classification
- meter/usage tracking
- inspection templates
- dynamic inspections
- defect capture
- work-order lifecycle
- labor/evidence capture
- PM due-state
- auto WO/inspection generation
- asset readiness endpoint
- SupplyArr parts demand
- audit package
- voice-guided inspection readiness

## Required API Surfaces

- `/api/assets`
- `/api/asset-classes`
- `/api/asset-types`
- `/api/inspections`
- `/api/inspection-templates`
- `/api/defects`
- `/api/work-orders`
- `/api/preventive-maintenance`
- `/api/maintenance-history`
- `/api/asset-readiness`
- `/api/technician-refs`
- `/health`

## Completion Definition

A real shop can create assets, inspect them, capture defects, complete work orders, run PM, prove readiness, request parts through SupplyArr, and generate defensible evidence.
