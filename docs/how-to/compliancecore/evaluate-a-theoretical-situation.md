# How to evaluate a theoretical situation

## Audience
Compliance admins, risk analysts, and product owners

## Product
Compliance Core

## Support Status
Supported by current UI/API

## Purpose
Test how rules and evidence requirements apply to a scenario before or outside live execution.

## Before You Start
- Compliance Core owns applicability logic and theoretical situation evaluation.
- Theoretical evaluation does not change operational source records.

## Steps
1. Open Compliance Core.
2. Open Theoretical situation or Evaluation.
3. Create or select the scenario to evaluate.
4. Enter the operating facts requested by the page, such as location, activity, asset, role, or material context.
5. Run the evaluation.
6. Review applicable requirements, evidence expectations, blockers, and findings.
7. Adjust the scenario facts if you are comparing alternatives.
8. Use the results to update rule mappings, evidence requirements, or product workflow guidance when appropriate.

## What Happens Next
Compliance Core returns rule applicability and evidence expectations for the scenario. Products still execute their own workflows.

## Troubleshooting
- If the result is missing an expected rule, check governing body, citation, rulepack, vocabulary, and evidence mappings.
- If the scenario facts come from a live product record, correct the live record in the owning product.

