# How to Create a Training Program

## Audience

Training administrators, qualification owners, safety leaders, and appropriately permissioned program authors.

## Product

TrainArr

## Support Status

Required canonical workflow. A missing Programs or Create route is a product regression, not a tenant enablement state.

## Purpose

Create a reusable TrainArr-owned program that can be assigned to people, positions, teams, sites, or qualification requirements without duplicating completion history or StaffArr person records.

## Before You Start

- Confirm `trainarr.programs.create` or the equivalent program-authoring permission.
- Confirm the governing body, certification type, course category, instructor, or other reference records needed by the program. Use the owner-backed picker and Quick Create where permitted; do not enter foreign IDs or create local shadows.
- Decide whether the program produces training completion only, a qualification, a certificate, or a readiness requirement consumed by another product.
- Gather source material, evidence expectations, evaluation criteria, renewal interval, expiration behavior, and required signoffs.

## Steps

1. Open **TrainArr → Programs**.
2. Select **Create program**.
3. Complete **Basics**: program name, plain-language purpose, category, owner, and intended audience.
4. Complete **Applicability**: relevant positions, teams, sites, activities, assets, governing bodies, law citations, or Compliance Core recommendations. Applicability suggests assignments; it does not copy ownership from StaffArr, MaintainArr, or Compliance Core.
5. Build the ordered learning steps. Each step must identify its delivery type, required content, completion rule, evidence, evaluator, and whether it blocks later steps.
6. Configure assessment and signoff requirements, including passing score, attempts, remediation, observation, instructor approval, or supervisor acknowledgment where applicable.
7. Configure completion output: completion record, qualification, certificate, issued credential, renewal date, expiration, grace period, and readiness effect.
8. Add documents or media through RecordArr-backed references when they must be versioned, retained, or used as evidence.
9. Review the summary. Resolve validation errors, missing references, inaccessible evidence, and contradictory renewal rules.
10. Save as **Draft** for review or **Publish** when the definition is complete. Publishing creates a versioned program definition; it does not silently assign people.

## What Happens Next

- The published version becomes available to assignment rules and manual assignment workflows.
- Existing learners remain tied to the version they started unless an authorized migration explicitly moves them.
- Completion, assessment, remediation, signoff, certificate, and renewal records remain durable and auditable.
- StaffArr and consuming products receive only the qualification/readiness facts they are authorized to use; TrainArr remains the source of truth for training execution.

## Unified Page Expectations

The create page must use the shared page header, sectioned form, owner-backed reference pickers, Quick Create drawer, inline validation, unsaved-change protection, and light/dark tokens. It must not present one oversized form, raw IDs, internal enum names, or success before durable server confirmation.

## Troubleshooting

- **Create action missing:** verify the program-authoring permission and route health. Do not search for a product launch check or tenant feature switch; TrainArr access is permission-based.
- **Reference missing:** use Quick Create when available or create the record in its owning product; the in-progress program must be preserved.
- **Publish blocked:** review the validation summary for incomplete steps, missing evidence, invalid renewal rules, or unavailable owner references.
- **Save failed:** keep the entered work on screen, show a truthful error, and retry only when the operation is safe and idempotent.
