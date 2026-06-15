# CustomArr Granular End-Goal Markdown Package

This package defines CustomArr at the domain-object level.

## Files

- `customarr_00_scope_and_boundaries.md`
- `customarr_01_customer_account_and_profile_model.md`
- `customarr_02_contacts_and_locations_model.md`
- `customarr_03_requirements_contracts_preferences_model.md`
- `customarr_04_onboarding_review_risk_communication_model.md`
- `customarr_05_workflows_status_events_apis.md`
- `customarr_all_in_one_granular_featureset.md`

## Purpose

CustomArr is the source of truth for customers of tenants across STL Compliance / ARR.

CustomArr owns:

- Customer accounts
- Customer account hierarchy
- Customer groups
- Customer aliases
- Customer external system mappings
- Customer contacts
- Customer contact authorization scope
- Customer external locations
- Bill-to / ship-to / pickup / dropoff / service location identity
- Customer-specific requirements
- Customer-specific preferences
- Customer service profiles
- Customer onboarding
- Customer approvals
- Customer holds
- Customer contract references
- Customer risk snapshots
- Customer communications
- Customer exceptions
- Customer merge history
- Customer audit trail

CustomArr does not own platform tenant identity, internal person truth, internal location truth, supplier/vendor truth, inventory truth, procurement truth, customer order lifecycle, route execution, warehouse execution, maintenance execution, regulatory meaning, actual document/file storage, reporting read models, or accounting execution.
