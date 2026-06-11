# Compliance Core User Guide

## What This Product Is For
Compliance Core is for governing body catalogs, rulepacks, regulations, law citations, vocabulary, applicability logic, evidence requirements, exemptions, exceptions, compliance interpretations, evidence classification, compliance gap analysis, theoretical situation evaluation, and audit package logic.

## Who Uses It
- compliance admins
- compliance reviewers
- tenant members who can read compliance results
- auditors with permitted access

## Main Pages
- Compliance dashboard
- Registry
- Governing bodies
- Jurisdictions
- Regulation sources
- Rule packs
- Mappings
- Citations
- Requirements
- Evidence types
- Evidence requirements
- Findings
- Evaluation
- Theoretical situation
- Evidence mapping
- Fact sources
- Imports
- Rule pack diff
- Change impact
- Reports
- Operator
- Admin

## Main Records
- governing body
- jurisdiction
- rule pack
- citation
- requirement
- evidence type
- evidence requirement
- finding
- fact source
- import batch
- theoretical situation

## Common Workflows
- import rule reference data
- review staged import rows
- map documents to requirements
- evaluate theoretical situations
- review citations and governing bodies
- prepare compliance reports

## Permissions Usually Needed
- compliance_admin
- compliance_reviewer
- tenant_member
- compliancecore.import.create
- compliancecore.import.read
- compliancecore.import.validate
- compliancecore.import.map
- compliancecore.import.commit
- compliancecore.simulation.evaluate

## Related Products
- Products provide operational facts and evidence.
- RecordArr stores documents and retained records.
- ReportArr reports compliance posture.

## Common Troubleshooting
- [Product not visible](../troubleshooting/product-not-visible.md)
- [Missing permission](../troubleshooting/missing-permission.md)
- If a page is visible but an action is disabled, check the record status and your role or permission assignment.
- Remember: Compliance Core does not own operational execution, stored document files, training execution, inventory execution, dispatch execution, maintenance execution, customer or vendor relationships, accounting, or legal advice.
