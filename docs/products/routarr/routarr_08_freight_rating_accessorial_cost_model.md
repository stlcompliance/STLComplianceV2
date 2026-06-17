# RoutArr - Freight Rating, Accessorial, and Cost Model

RoutArr owns operational transportation rating snapshots and freight cost facts. External finance systems own invoices, bills, payments, tax, ledger, reconciliation, and accounting close.

## Freight rating

```text
FreightRating
- freightRatingId
- transportationDemandId
- tripId
- ratingNumber
- status
  - estimated
  - quoted
  - planned
  - actualized
  - variance_detected
  - audit_exception
  - canceled
- buyRateEstimate
- sellRateEstimate
- plannedFreightCost
- actualFreightCost
- currencyCode
- rateSourceSnapshot
- fuelSurcharge
- linehaulAmount
- stopOffCharge
- detentionAmount
- layoverAmount
- tonuAmount
- lumperAmount
- tollAmount
- driverAssistAmount
- waitingTimeAmount
- outOfRouteMiles
- emptyMiles
- allocationSnapshot
- varianceAmount
- varianceReason
- auditStatus
```

## Accessorial

```text
FreightAccessorial
- accessorialId
- freightRatingId
- transportationDemandId
- tripId
- accessorialType
  - fuel_surcharge
  - stop_off
  - detention
  - layover
  - tonu
  - lumper
  - toll
  - driver_unload
  - driver_assist
  - re_delivery
  - waiting_time
  - out_of_route
  - other
- amount
- currencyCode
- status
  - estimated
  - pending_review
  - approved
  - rejected
  - actualized
- sourceEventRef
- evidenceRefs
```

## Finance boundary

```text
RoutArr contributes freight facts to OrdArr invoice-ready packets and SupplyArr bill-ready packets.
RoutArr does not create invoices, bills, payments, tax records, ledger entries, or accounting reconciliation records.
```

## Events

```text
routarr.freight_rate.estimated
routarr.freight_rate.actualized
routarr.accessorial.created
routarr.accessorial.approved
routarr.accessorial.rejected
routarr.freight_cost.variance_detected
routarr.freight_audit.exception_created
```

