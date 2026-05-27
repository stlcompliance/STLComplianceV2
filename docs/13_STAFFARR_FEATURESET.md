# StaffArr Feature Set

## Product Definition

StaffArr is the workforce readiness backbone: people, org, permissions, certifications, incidents, readiness, and personnel history.

## Owns

- people
- person profile
- org units
- sites
- departments
- teams
- positions
- roles
- permissions
- certifications
- manual overrides
- readiness
- incidents
- person history
- audit packages

## Does Not Own

- login credentials
- tenant entitlement
- training workflow evidence
- maintenance execution
- dispatch execution
- procurement
- rule packs

## Core Features

- people directory
- person profile
- org tree
- site/department/team assignments
- manager hierarchy
- role templates
- permission assignment
- certification grants
- readiness calculation
- incident intake
- training blocker display
- person timeline
- audit package export
- product-facing readiness API

## Required API Surfaces

- `/api/people`
- `/api/org-units`
- `/api/sites`
- `/api/departments`
- `/api/teams`
- `/api/positions`
- `/api/roles`
- `/api/permissions`
- `/api/certifications`
- `/api/readiness`
- `/api/incidents`
- `/api/person-history`
- `/api/audit-packages`
- `/health`

## Completion Definition

Every product can ask who a person is, where they belong, what they can do, whether they are ready, and what history proves the answer.
