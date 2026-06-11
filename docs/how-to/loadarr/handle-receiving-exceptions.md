# How to handle receiving exceptions

## Audience
Warehouse receivers, inventory supervisors, and supply chain users

## Product
LoadArr

## Support Status
Supported by current UI/API

## Purpose
Resolve mismatches, damage, shortages, overages, or paperwork issues during receiving.

## Before You Start
- LoadArr owns receiving exceptions and stock movement outcome.
- SupplyArr owns vendor/order follow-up.
- RecordArr stores documents or photos when evidence is required.

## Steps
1. Open LoadArr.
2. Open Receiving or Exceptions.
3. Select the receipt or exception.
4. Review expected quantity, received quantity, item identity, lot, serial, vendor, and source order context.
5. Classify the exception using the options shown on the page.
6. Add notes and evidence references when required.
7. Create a hold or quarantine record if the stock should not be used.
8. Save the exception handling decision.
9. Notify SupplyArr if procurement or vendor follow-up is needed.

## What Happens Next
LoadArr records the exception and stock state. SupplyArr handles vendor/order consequences, and RecordArr can store supporting evidence.

## Troubleshooting
- If the item is damaged, do not put it into available stock until the hold or quarantine path is resolved.
- If the expected order is wrong, correct procurement context in SupplyArr rather than changing stock history.

