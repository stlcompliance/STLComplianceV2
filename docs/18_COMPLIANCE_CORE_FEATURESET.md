# Compliance Core Feature Set

## Product Definition

Compliance Core is the authority layer for vocabulary, keys, rule packs, mappings, SDS/HazCom, and evaluation patterns.

## Owns

- controlled vocabulary
- compliance keys
- material keys
- regulatory mappings
- rule packs
- rule versions
- SDS/HazCom references
- source metadata
- evaluation patterns
- findings
- audit packages

## Does Not Own

- login
- tenant operational records
- people
- assets
- work orders
- routes
- purchase orders
- training workflow records

## Core Features

- vocabulary registry
- alias mapping
- compliance keys
- material keys
- rule packs
- fact requirements
- regulatory mappings
- SDS/HazCom references
- internal resolve API
- internal validate API
- findings
- rule publication
- audit package

## Required API Surfaces

- `/api/vocabulary`
- `/api/compliance-keys`
- `/api/material-keys`
- `/api/regulatory-mappings`
- `/api/rule-packs`
- `/api/rule-versions`
- `/api/sds`
- `/api/hazcom`
- `/api/findings`
- `/api/internal/resolve`
- `/api/internal/validate`
- `/health`

## Completion Definition

Products can map facts to controlled vocabulary, rule packs, keys, material keys, and evaluation results without duplicating regulatory logic.
