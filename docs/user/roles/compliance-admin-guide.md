# Compliance Admin Guide

## What This Role Does
A compliance admin manages Compliance Core rule, citation, requirement, import, evidence mapping, findings, reports, and audit-readiness workflows.

## What This Role Can Usually Access
- Compliance Core Dashboard, Registry, Mappings, Findings, Evaluation, Theoretical situation, Evidence mapping, Fact sources, Imports, Reports, Operator, and Admin.
- Compliance Core import and simulation actions where compliance_admin or tenant_admin access is allowed.
- Audit package exports when compliance admin or reviewer access allows it.

## What This Role Usually Cannot Access
- Does not execute maintenance, dispatch, warehouse, training, procurement, or stored document workflows.
- Does not replace legal counsel.
- Does not own RecordArr stored files.

## Common Daily Tasks
- Review governing bodies and citations.
- Import rule reference data.
- Review and map staged import rows.
- Evaluate theoretical situations.
- Review missing evidence and findings.
- Prepare audit-ready compliance context.

## Records This Role Works With
- governing body
- citation
- rule pack
- requirement
- evidence requirement
- finding
- fact source
- import batch
- theoretical situation

## Notifications This Role May Receive
- Import review tasks
- findings needing action
- missing evidence warnings
- audit package status

## Common Issues
- Import commit is blocked without compliance admin or platform admin authority.
- Rule matches are missing because facts, mappings, or rulepacks are incomplete.
- Evidence files live in RecordArr or owning products, not only in Compliance Core.

## Related How-To Documents
- [How to import rule reference data](../how-to/compliance-core/how-to-import-rule-reference-data.md)
- [Rule import to evaluation](../workflows/rule-import-to-evaluation.md)
