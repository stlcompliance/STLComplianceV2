# How to assign a driver

## Audience
Dispatchers and transportation supervisors

## Product
RoutArr

## Support Status
Supported by current UI/API with readiness checks

## Purpose
Assign a StaffArr person to a route or trip as the driver.

## Before You Start
- StaffArr owns the driver person record.
- TrainArr owns driver qualification status.
- RoutArr owns the trip assignment decision.

## Steps
1. Open RoutArr.
2. Open Trips, Routes, Dispatch board, or Route planner.
3. Select the trip or route.
4. Open the assignment area.
5. Choose the driver assignment action when available.
6. Search for and select the driver.
7. Review qualification, availability, restrictions, and validation blockers.
8. Save the assignment.
9. Confirm the trip or dispatch plan reflects the assigned driver.

## What Happens Next
RoutArr records the assignment while consuming StaffArr and TrainArr readiness information.

## Troubleshooting
- If the driver is missing, create or correct the person in StaffArr.
- If the driver is blocked, review TrainArr qualifications, StaffArr restrictions, or schedule availability.

## Related How-To Documents
- [How to create a person](../staffarr/create-a-person.md)
- [How to review expiring certifications](../trainarr/review-expiring-certifications.md)

