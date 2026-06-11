# How to report a defect from the field

## Audience
Field workers, technicians, and supervisors

## Product
Field Companion

## Support Status
Intended workflow partially supported by current routes/docs

## Purpose
Capture a defect concern from the field and route it to the product that owns the record.

## Before You Start
- MaintainArr owns asset defects.
- StaffArr owns personnel incidents.
- LoadArr owns inventory holds and receiving exceptions.
- Field Companion is only the capture surface.
- Current task panels support inspections and work-order updates; a direct generic defect report action should be confirmed.

## Steps
1. Open Field Companion.
2. Open the related task for the asset, inspection, receiving work, or route.
3. Review the source product and record context.
4. Use the defect, fail, exception, or notes action exposed by the task.
5. Describe the issue and severity.
6. Add evidence if the task allows it.
7. Submit or save the task update.
8. Open the owning product, such as MaintainArr or LoadArr, to verify the defect or exception record was created or updated.

## What Happens Next
The owning product records the defect, exception, or incident. Field Companion should not become the final source record.

## Troubleshooting
- If there is no defect action, record the issue in the task notes only if that is permitted and escalate to the owning product workflow.
- If the issue is a personnel incident, use StaffArr incident workflow rather than MaintainArr defect.

## Related How-To Documents
- [How to create a defect report](../maintainarr/create-a-defect-report.md)
- [How to report an incident](../staffarr/report-an-incident.md)

