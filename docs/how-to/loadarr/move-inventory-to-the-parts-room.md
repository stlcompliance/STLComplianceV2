# How to move inventory to the parts room

## Audience
Warehouse users and maintenance parts coordinators

## Product
LoadArr

## Support Status
Supported by current UI/API

## Purpose
Move stocked inventory from receiving, staging, or warehouse storage to a parts room location.

## Before You Start
- LoadArr owns inventory movement and stock ledger.
- StaffArr owns the internal location identity for rooms and physical places.
- MaintainArr may request parts but does not own stock movement.

## Steps
1. Open LoadArr.
2. Open Inventory, Putaway, or Transfers.
3. Find the item or receipt line to move.
4. Confirm current location, quantity, lot, serial, and hold state.
5. Choose transfer, putaway, or move depending on the current page workflow.
6. Select the parts room destination.
7. Enter quantity and any required handling notes.
8. Submit the movement.
9. Confirm the new balance and movement history.

## What Happens Next
LoadArr updates stock location and ledger history. MaintainArr can then request or consume availability for work orders.

## Troubleshooting
- If the parts room location is missing, create or correct it in StaffArr and LoadArr location rules.
- If quantity is reserved or on hold, resolve reservation or hold rules before moving.

