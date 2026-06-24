# Workflow Pack — Incident to Retraining

## Purpose

This workflow defines how personnel incidents are centralized in StaffArr and evaluated by TrainArr for remediation, retraining, or qualification impact.

## Trigger

```text
StaffArr incident created
```

Possible origins:

```text
- StaffArr manual entry
- MaintainArr defect/work-order safety context
- RoutArr trip exception
- LoadArr warehouse incident
- AssurArr quality finding
- Field Companion report
- self-report
```

## Participating products

```text
StaffArr
TrainArr
MaintainArr
RoutArr
LoadArr
AssurArr
RecordArr
Compliance Core
Field Companion
ReportArr
```

## Source-of-truth table

| Business truth | Owner |
|---|---|
| person profile, incident record, personnel history | StaffArr |
| training program, assignment, remediation, certificate/qualification | TrainArr |
| operational source record | originating product |
| stored evidence | RecordArr |
| regulatory meaning/evidence requirements | Compliance Core |
| reports/read models | ReportArr |
| mobile report/capture surface | Field Companion |

## Main flow

1. Incident is created in StaffArr or reported by product event.
2. StaffArr links person, role, location, and source product context.
3. StaffArr classifies incident type and severity.
4. StaffArr determines whether training/certification review is needed.
5. StaffArr creates handoff to TrainArr when training-related.
6. TrainArr evaluates applicable programs, qualifications, and remediation rules.
7. TrainArr creates remediation assignment if needed.
8. Field Companion may show remediation task.
9. Trainee/trainer complete required steps/signoffs.
10. TrainArr issues, renews, suspends, or leaves qualification unchanged as appropriate.
11. StaffArr receives training outcome for person history display.
12. Operational products consume readiness/qualification status.
13. RecordArr stores incident evidence/remediation package.
14. ReportArr projects trends and recurrence.

## Required events

```text
staffarr.incident.created
staffarr.incident.classified
staffarr.incident.forwarded_to_trainarr
trainarr.remediation.assigned
trainarr.training_assignment.completed
trainarr.qualification.issued
trainarr.qualification.suspended
trainarr.qualification.expired
recordarr.package.completed
```

## Required handoffs

```text
originating product -> staffarr: incident context
staffarr -> trainarr: remediation evaluation
trainarr -> staffarr: qualification/remediation outcome
staffarr -> recordarr: incident package
trainarr -> recordarr: training completion package
```

## Blockers

Common blockers:

```text
- person not identified
- source product record missing
- incident severity requires review
- qualification suspended
- remediation assignment overdue
- evidence missing
- manager approval required
```

## Operational readiness effects

Products should check qualification/readiness before allowing high-risk actions.

Examples:

```text
- RoutArr blocks dispatch if driver qualification is suspended.
- MaintainArr blocks certified inspection assignment if technician qualification is missing.
- LoadArr blocks powered equipment operation if operator qualification is missing.
```

## Field Companion behavior

Field Companion may support incident report capture, photo/evidence upload, assigned retraining task, signoff step completion, and acknowledgement.

Field Companion does not own incident or training truth.

## Evidence package

RecordArr package should include:

```text
- incident record
- source product context
- photos/files
- witness/statement records if modeled
- StaffArr classification
- TrainArr remediation assignment
- completion/signoff evidence
- qualification outcome
- related blockers/overrides
```

## Non-goals

StaffArr does not own training program truth.

TrainArr does not own the personnel incident record.

Originating products do not maintain separate personnel incident systems.
