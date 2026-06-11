# How to assign a driver

## Audience
Dispatchers and RoutArr managers.

## Purpose
Assign a StaffArr person as driver on a RoutArr trip.

## Before You Start
- RoutArr access.
- Driver assignment access.
- Trip record.
- Driver person record in StaffArr.
- Driver qualification or readiness data where required.

## Steps
1. Open RoutArr.
2. Open **Trips** or **Dispatch board**.
3. Select the trip.
4. Open the driver assignment area.
5. Choose the driver.
6. Review driver eligibility or validation blockers.
7. Save the assignment.
8. If blocked, open **Validation blockers** and resolve the source issue before dispatch.

## What Happens Next
RoutArr records the driver assignment. Driver person data remains owned by StaffArr, and qualifications remain owned by TrainArr.

## Troubleshooting
- If the driver is missing, check StaffArr.
- If assignment is blocked, check TrainArr qualification and StaffArr readiness.
- If you can view but not assign, check routarr.dispatch.assign access.

## Related Docs
- [Missing permission](../../troubleshooting/missing-permission.md)
- [Dispatch to completion workflow](../../workflows/dispatch-to-completion.md)
