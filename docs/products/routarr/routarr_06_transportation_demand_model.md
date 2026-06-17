# RoutArr - Transportation Demand Model

TransportationDemand is the RoutArr-owned record for a movement need before it becomes a route or trip.

It is not a LoadArr load, an OrdArr order, a SupplyArr procurement demand, or a customer master record. It is the transportation-facing demand that says what must move, from where, to where, by when, and under what constraints.

## TransportationDemand

```text
TransportationDemand
- transportationDemandId
- tenantId
- demandNumber
- title
- description
- status
  - draft
  - ready_for_planning
  - planning
  - planned
  - assigned
  - tender_required
  - tendered
  - accepted
  - dispatched
  - in_transit
  - delivered
  - closed
  - canceled
  - blocked
- sourceRefs
- originLocationRef
- destinationLocationRef
- requestedPickupWindow
- requestedDeliveryWindow
- promisedPickupWindow
- promisedDeliveryWindow
- scheduledPickupWindow
- scheduledDeliveryWindow
- transportMode
- serviceLevel
- equipmentRequirement
- handlingRequirements
- customerRefs
- orderRefs
- vendorRefs
- loadarrReadinessRefs
- requirementRefs
- planningStatus
- tenderStatus
- ratingStatus
- visibilityStatus
- freshnessState
- routeRef
- tripRef
- dispatchPlanRef
- createdAt
- createdByPersonId
- updatedAt
- canceledAt
- cancelReason
```

## Source references

TransportationDemand may be created from OrdArr, LoadArr, SupplyArr, CustomArr, MaintainArr, Field Companion, an integration, or a dispatcher. Source references are snapshots or links only.

```text
TransportationDemandSourceRef
- sourceProduct
- sourceObjectType
- sourceObjectId
- sourceObjectNumber
- displayNameSnapshot
- statusSnapshot
- snapshotAt
- freshnessState
```

## Demand lines and requirements

Demand lines summarize what RoutArr needs for transportation planning. They are not inventory balances or stock ledger lines.

```text
TransportationDemandLine
- demandLineId
- transportationDemandId
- lineNumber
- sourceProduct
- sourceObjectRef
- descriptionSnapshot
- quantitySnapshot
- unitOfMeasure
- weightSnapshot
- volumeSnapshot
- palletCountSnapshot
- handlingRequirementSnapshot
```

```text
TransportationDemandRequirement
- requirementId
- transportationDemandId
- requirementType
  - equipment
  - temperature
  - hazmat
  - seal
  - proof
  - document
  - dock
  - access
  - customer
  - supplier
  - compliance
  - service_level
  - other
- sourceProduct
- sourceRequirementRef
- required
- status
  - pending
  - satisfied
  - waived
  - failed
  - blocked
- evidenceRefs
```

## Lifecycle workflow

```text
1. Source product or dispatcher creates TransportationDemand.
2. RoutArr validates source refs and required planning facts.
3. Demand becomes ready_for_planning.
4. Planner consolidates, sequences, rates, and checks feasibility.
5. Demand is linked to route/trip, assigned to private fleet, or tendered.
6. Execution updates demand status through dispatched, in_transit, delivered, closed, canceled, or blocked.
```

## Events

```text
routarr.transportation_demand.created
routarr.transportation_demand.ready_for_planning
routarr.transportation_demand.planning_started
routarr.transportation_demand.planned
routarr.transportation_demand.assigned
routarr.transportation_demand.tender_required
routarr.transportation_demand.dispatched
routarr.transportation_demand.delivered
routarr.transportation_demand.closed
routarr.transportation_demand.canceled
routarr.transportation_demand.blocked
```

