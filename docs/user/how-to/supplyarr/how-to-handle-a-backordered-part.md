# How to handle a backordered part

## Audience
SupplyArr buyers, managers, and warehouse coordinators.

## Purpose
Review and act on a part that cannot be fulfilled on the expected schedule.

## Before You Start
- SupplyArr access.
- Backorder or vendor order context.
- Part and vendor information.
- LoadArr stock context if inventory is involved.

## Steps
1. Open SupplyArr.
2. Open **Purchasing** > **Exceptions** or **Vendor orders**.
3. Find the backordered part or order line.
4. Review vendor status, lead time, and required date.
5. Update vendor order status if you are allowed.
6. If stock is needed for maintenance, check the related MaintainArr parts demand.
7. If physical inventory is expected, coordinate with LoadArr receiving.
8. Record notes or follow-up actions in the available workflow.

## What Happens Next
SupplyArr owns procurement status. LoadArr owns inventory availability after goods are received.

## Troubleshooting
- If no vendor status exists, contact the vendor or check vendor portal updates.
- If inventory appears short, verify LoadArr balances.
- If the backorder blocks a work order, review MaintainArr supply readiness.

## Related Docs
- [Parts not available](../../troubleshooting/parts-not-available.md)
- [Part request to receiving workflow](../../workflows/part-request-to-receiving.md)
