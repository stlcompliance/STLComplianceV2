# RoutArr Feature Set

## Product Definition

RoutArr is the dispatch execution system for routes, trips, drivers, DVIR, proof, and exceptions.

## Owns

- routes
- trips
- dispatch
- stops
- driver assignments
- vehicle references
- route state
- trip state
- DVIR
- proof
- exceptions
- route audit trail

## Does Not Own

- platform identity
- people master data
- training records
- asset maintenance
- procurement
- rule governance

## Core Features

- route planning
- trip execution
- dispatch board
- driver assignment
- equipment assignment
- driver eligibility checks
- asset dispatchability checks
- pre/post trip DVIR
- proof capture
- exception reporting
- incident reporting
- route closeout
- audit packets

## Required API Surfaces

- `/api/routes`
- `/api/dispatch`
- `/api/trips`
- `/api/stops`
- `/api/drivers`
- `/api/driver-eligibility`
- `/api/vehicle-refs`
- `/api/dvir`
- `/api/route-inspections`
- `/api/proof`
- `/api/exceptions`
- `/api/route-completions`
- `/health`

## Completion Definition

Dispatch can assign the right qualified person and ready asset, guide execution, capture proof/exceptions, create DVIR facts, and generate a transportation audit trail.
