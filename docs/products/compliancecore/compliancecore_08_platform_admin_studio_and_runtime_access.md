# Compliance Core — Platform Admin Studio and Runtime Access

## Access boundary

The Compliance Core administrative studio is available only to server-validated NexArr platform administrators. Every studio page and rule/catalog/import/mapping administration API enforces that boundary.

Compliance Core runtime operation is available to every tenant and user through authorized product workflows. Products may resolve questionnaires, normalize facts, evaluate applicability, determine evidence requirements, explain readiness/blockers, and request citations without granting the user studio access.

## Studio navigation

Organize by understandable work:

- Catalog & Vocabulary
- Governing Bodies, Jurisdictions & Sources
- Rulepacks & Requirements
- Applicability & Fact Requirements
- Evidence Mapping
- Questionnaires & Tenant Profiles
- Evaluations & TSE
- Imports, Review Queues & Publish
- Audit, Diagnostics & Settings

## Rule/detail pages

Show plain-language meaning, applicability inputs, exceptions/exemptions, required facts, evidence requirements, citations, mappings, versions, tests, and activation history. Raw JSON is an advanced technical disclosure, not the primary view.

## Runtime result contract

Return decision status, plain-language explanation, missing facts, supporting citations, evidence requirements, confidence/source, effective version, and recommended owner-product action. Products render results in their own page archetypes.
