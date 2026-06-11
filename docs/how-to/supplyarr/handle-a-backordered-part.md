# How to handle a backordered part

## Audience
Supply chain users, maintenance planners, and inventory coordinators

## Product
SupplyArr

## Support Status
Supported by current UI/API with cross-product follow-up

## Purpose
Respond when a needed part cannot be fulfilled immediately.

## Before You Start
- SupplyArr owns procurement and vendor context.
- LoadArr owns physical inventory and reservations.
- MaintainArr owns maintenance work orders and parts demand.

## Steps
1. Open SupplyArr.
2. Open Planning or Purchasing Exceptions.
3. Find the backordered part or demand reference.
4. Review source demand from MaintainArr, LoadArr, RoutArr, TrainArr, StaffArr, or another product.
5. Check available vendors, price, lead time, and existing orders.
6. Create or update a purchase request, vendor order, or exception response when available.
7. Communicate expected availability back to the requesting product.
8. Monitor receiving in LoadArr and work impact in the originating product.

## What Happens Next
SupplyArr coordinates procurement response. LoadArr updates availability when goods are received, and the originating product owns the work impact.

## Troubleshooting
- If demand came from a work order, coordinate with MaintainArr before substituting parts.
- If inventory appears available but demand is still short, check LoadArr reservations and holds.

## Related How-To Documents
- [How to request parts for a work order](../maintainarr/request-parts-for-a-work-order.md)
- [How to confirm parts available for a work order](../loadarr/confirm-parts-available-for-a-work-order.md)

