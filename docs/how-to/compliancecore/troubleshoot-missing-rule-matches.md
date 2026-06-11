# How to troubleshoot missing rule matches

## Audience
Compliance admins and product owners

## Product
Compliance Core

## Support Status
Supported by current UI/API

## Purpose
Find why a rule, evidence requirement, or compliance blocker did not appear where expected.

## Before You Start
- Compliance Core owns rule interpretation and matching.
- Products own the operational facts that are evaluated.

## Steps
1. Open Compliance Core.
2. Open Evaluation, Findings, Evidence mapping, or Rule pack diff depending on the symptom.
3. Identify the product, source record, scenario, or evidence item that should have matched.
4. Review the rulepack, citation, requirement, vocabulary, applicability logic, and evidence mapping.
5. Check whether the product supplied the needed facts or evidence reference.
6. Use Change impact or Reports to see whether a recent rulepack change affected the match.
7. Correct the rule/mapping in Compliance Core or the source fact in the owning product.
8. Re-run evaluation or review findings after correction.

## What Happens Next
Corrected mappings and rules should flow into future evaluations and audit reporting. Source records remain owned by their products.

## Troubleshooting
- If the product fact is wrong, fix it in the owning product rather than Compliance Core.
- If a document exists but is not matched, check RecordArr capture and Evidence mapping.

