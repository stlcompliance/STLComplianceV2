# How to access Compliance Core as a platform admin

## Audience
Platform admins and compliance admins

## Product
Compliance Core

## Support Status
Supported by current UI/API

## Purpose
Open the Compliance Core administrative studio through the suite while preserving its platform-admin-only boundary.

## Before You Start
- NexArr must confirm server-side platform-admin status. A product-local “compliance admin” role does not grant studio access.
- Compliance Core owns rule interpretation and evidence requirements, not operational execution.

## Steps
1. Sign in to STL Compliance.
2. Use the product switcher to open Compliance Core.
3. Open the administrative studio.
4. Confirm the tenant context before changing rulepacks, vocabulary, imports, exemptions, or evidence requirements.
5. Use Registry, Mappings, Findings, Evaluation, Evidence mapping, and Reports for domain work.
6. Return to NexArr Platform Admin for login, tenant-membership, or session changes.

## What Happens Next
Compliance Core changes can affect evidence requirements, rule matching, audit packages, and product blockers.

## Troubleshooting
- If Compliance Core is unavailable, confirm platform-admin status, session security, and Compliance Core operational state.
- If a product workflow is blocked by compliance logic, fix the rule or evidence mapping in Compliance Core rather than editing the product source record.

