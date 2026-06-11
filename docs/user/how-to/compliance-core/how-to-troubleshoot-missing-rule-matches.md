# How to troubleshoot missing rule matches

## Audience
Compliance admins and reviewers.

## Purpose
Find why an evaluation, finding, or workflow gate did not match expected rules.

## Before You Start
- Compliance Core access.
- The evaluation, fact source, rulepack, or workflow gate being reviewed.

## Steps
1. Open Compliance Core.
2. Review the situation or record in **Evaluation**, **Theoretical situation**, or **Findings**.
3. Check whether the relevant rulepack is active and mapped.
4. Check whether the facts needed by the rule are present.
5. Check **Fact sources** for source freshness or missing product data.
6. Check **Mappings** for citation, requirement, and evidence links.
7. Re-run evaluation where allowed.

## What Happens Next
A corrected fact, mapping, or rulepack should produce the expected match if the rule applies.

## Troubleshooting
- If facts are missing, update the owning product or fact source.
- If the rulepack is inactive, activate or publish it only through authorized compliance workflows.
- If the rule should not apply, document the reason or exemption where the workflow supports it.

## Related Docs
- [How to evaluate a theoretical situation](how-to-evaluate-a-theoretical-situation.md)
- [Common record types](../../reference/common-record-types.md)
