# Rule Import to Evaluation

## Purpose
Move rule reference data from import into usable evaluation and reporting.

## Who Participates
- Compliance admin
- Compliance reviewer
- Auditor

## Starting Point
Rule reference data needs to be imported or updated.

## Main Steps
1. Import reference data in Compliance Core.
2. Review staged rows.
3. Map rows to governing bodies, citations, requirements, or evidence requirements.
4. Commit the reviewed import with authorized access.
5. Run evaluation or theoretical situation checks.
6. Review findings, missing evidence warnings, and reports.

## Products Involved
- Compliance Core owns rule import, mapping, and evaluation.
- RecordArr stores supporting files.
- ReportArr reports posture.

## Records Created or Updated
- import batch
- staged row
- governing body
- citation
- requirement
- evidence requirement
- evaluation result
- finding

## Where Users May Get Stuck
- Import validation errors.
- Mapping permissions missing.
- Commit access missing.
- No rule matches because facts or mappings are incomplete.

## Related How-To Docs
- [How to import rule reference data](../how-to/compliance-core/how-to-import-rule-reference-data.md)
- [How to troubleshoot missing rule matches](../how-to/compliance-core/how-to-troubleshoot-missing-rule-matches.md)
