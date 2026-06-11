# How to generate work from a preventive maintenance program

## Audience
Maintenance planners and supervisors

## Product
MaintainArr

## Support Status
Intended workflow partially supported by current routes/docs

## Purpose
Turn due preventive maintenance into executable work orders.

## Before You Start
- PM programs must exist and have eligible assets.
- The product docs describe PM-driven work generation; exact UI trigger labels should be confirmed.

## Steps
1. Open MaintainArr.
2. Open PM programs or Work orders.
3. Find due PM work or the relevant PM program.
4. Review due assets, due date or meter threshold, required tasks, and parts expectations.
5. Use the generate or create-work action when available.
6. Review the created work order.
7. Assign the work, plan parts, and schedule the work order.
8. Track completion in Work orders.

## What Happens Next
MaintainArr owns both the PM program and the resulting work order. Parts availability remains owned by LoadArr.

## Troubleshooting
- If no generation action is visible, check whether a background PM scan created the work order automatically or whether this tenant still needs the PM generation UI.
- If a generated work order is blocked for parts, follow the parts request workflow.

## Related How-To Documents
- [How to request parts for a work order](../maintainarr/request-parts-for-a-work-order.md)

