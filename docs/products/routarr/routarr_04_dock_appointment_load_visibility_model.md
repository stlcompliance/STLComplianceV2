# RoutArr — Dock Appointment and Load Visibility Model

## Dock appointment notification

RoutArr notifies LoadArr about inbound transportation and dock appointment events when RoutArr has visibility or controls the move.

RoutArr does not own receiving. LoadArr owns receiving workflow, dock receiving execution, staging, putaway, inventory balance, and stock ledger.

```text
DockAppointmentNotification
- dockAppointmentNotificationId
- tenantId
- notificationNumber
- sourceTripId
- sourceRouteId
- sourceStopId
- loadarrExpectedReceiptRef
- staffarrSiteId
- staffarrDockLocationId
- appointmentType
  - request
  - update
  - cancel
  - eta_update
  - arrival
  - departure
  - delay
  - exception
- requestedWindowStart
- requestedWindowEnd
- confirmedWindowStart
- confirmedWindowEnd
- eta
- status
  - draft
  - sent
  - acknowledged
  - confirmed
  - rejected
  - updated
  - canceled
  - completed
- carrierSnapshot
- driverSnapshot
- vehicleSnapshot
- trailerSnapshot
- sourceProduct
- sourceObjectRef
- rejectionReason
- sentAt
- acknowledgedAt
- confirmedAt
- canceledAt
```

## Dock appointment response

```text
DockAppointmentResponse
- dockAppointmentResponseId
- tenantId
- dockAppointmentNotificationId
- respondingProduct
  - loadarr
  - routarr
- status
  - accepted
  - rejected
  - proposed_alternative
  - canceled
  - completed
- confirmedWindowStart
- confirmedWindowEnd
- assignedDockLocationId
- message
- respondedAt
- respondedByPersonId
```

## Transportation load visibility

TransportationLoadVisibility is RoutArr’s view of what is being transported. It is not inventory truth.

```text
TransportationLoadVisibility
- transportationLoadVisibilityId
- tenantId
- loadNumber
- tripId
- routeId
- sourceProduct
  - ordarr
  - loadarr
  - supplyarr
  - maintainarr
  - manual
- sourceObjectRef
- loadType
  - inbound_receipt
  - outbound_order
  - internal_transfer
  - customer_return
  - supplier_return
  - maintenance_transfer
  - mixed
- status
  - planned
  - ready
  - staged
  - loaded
  - in_transit
  - delivered
  - exception
  - canceled
- originLocationRef
- destinationLocationRef
- customerRef
- supplierRef
- orderRefs
- expectedReceiptRefs
- itemSummarySnapshot
- handlingRequirements
- temperatureRequirement
- hazmatFlag
- weightSnapshot
- volumeSnapshot
- sealNumber
- documentRefs
- createdAt
- updatedAt
```

## Load item summary

```text
LoadItemSummary
- loadItemSummaryId
- transportationLoadVisibilityId
- itemRef
- itemDescriptionSnapshot
- quantitySnapshot
- unitOfMeasure
- lotNumber
- serialNumber
- sourceProductLineRef
- handlingNotes
```

## Carrier reference

If a carrier is a supplier/vendor, SupplyArr owns the master. RoutArr stores route-specific snapshot/context.

```text
CarrierSnapshot
- supplierRef
- carrierNameSnapshot
- mcNumberSnapshot
- dotNumberSnapshot
- contactSnapshot
- phoneSnapshot
- statusSnapshot
```

## Transportation appointment

A TransportationAppointment is a RoutArr-side appointment object used for planning. LoadArr owns dock receiving schedule/confirmation for receiving operations.

```text
TransportationAppointment
- transportationAppointmentId
- tenantId
- appointmentNumber
- tripId
- stopId
- appointmentType
  - pickup
  - delivery
  - dock
  - customer
  - supplier
  - internal
- status
  - requested
  - confirmed
  - rejected
  - rescheduled
  - arrived
  - completed
  - canceled
- requestedWindowStart
- requestedWindowEnd
- confirmedWindowStart
- confirmedWindowEnd
- locationRef
- contactSnapshot
- loadVisibilityRef
- sourceProduct
- sourceObjectRef
- notes
```

## Load readiness check

RoutArr may ask LoadArr/OrdArr whether outbound load/order is ready for transportation.

```text
LoadReadinessCheck
- loadReadinessCheckId
- tenantId
- tripId
- routeId
- sourceProduct
  - loadarr
  - ordarr
  - supplyarr
  - maintainarr
- sourceObjectRef
- status
  - ready
  - not_ready
  - partially_ready
  - blocked
  - unknown
- readinessDetails
- blockerRefs
- checkedAt
```

## Inbound dock appointment workflow

```text
1. RoutArr has inbound trip/load visibility.
2. RoutArr creates DockAppointmentNotification.
3. RoutArr sends appointment request/update to LoadArr.
4. LoadArr validates dock/location receiving availability.
5. LoadArr confirms, rejects, or proposes alternative.
6. RoutArr updates TransportationAppointment.
7. RoutArr sends ETA updates as trip progresses.
8. Driver arrives.
9. RoutArr sends arrival event.
10. LoadArr performs receiving.
11. Driver departs.
12. RoutArr sends departure event.
```

## Outbound load readiness workflow

```text
1. OrdArr order requires delivery.
2. LoadArr picks/stages inventory.
3. RoutArr requests LoadReadinessCheck.
4. LoadArr returns ready/not ready/blocked.
5. If ready, trip can be released if other validations pass.
6. If not ready, RoutArr creates blocker.
7. Once LoadArr stages/updates, RoutArr resolves blocker.
```

## Supplier pickup workflow

```text
1. SupplyArr or OrdArr creates pickup need.
2. RoutArr creates route/trip/stop.
3. Supplier location/contact context is attached.
4. Driver executes pickup.
5. Proof of pickup is captured.
6. Load visibility status updates.
7. Receiving/transfer/order status updates flow to source products.
```

## Customer return workflow

```text
1. CustomArr/OrdArr creates return context.
2. RoutArr creates pickup trip.
3. Driver captures return proof/photos/documents.
4. LoadArr receives returned goods.
5. AssurArr handles quality issue if needed.
6. OrdArr/CustomArr receive status updates.
```

## Dock/load events

```text
routarr.dock_appointment.requested
routarr.dock_appointment.updated
routarr.dock_appointment.confirmed
routarr.dock_appointment.rejected
routarr.dock_appointment.canceled
routarr.dock_appointment.eta_updated
routarr.dock_appointment.arrived
routarr.dock_appointment.departed
routarr.dock_appointment.completed

routarr.transportation_load.created
routarr.transportation_load.ready
routarr.transportation_load.staged
routarr.transportation_load.loaded
routarr.transportation_load.in_transit
routarr.transportation_load.delivered
routarr.transportation_load.exception
routarr.transportation_load.canceled

routarr.load_readiness.checked
routarr.load_readiness.blocked
routarr.load_readiness.ready
```
