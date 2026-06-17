# RoutArr - Planning, Optimization, and Capacity Model

RoutArr uses rules-based planning first. External optimization engines may be added later through explicit integrations.

## Planning scenario

```text
TransportationPlanningScenario
- planningScenarioId
- scenarioNumber
- status
  - draft
  - evaluating
  - suggestions_ready
  - accepted
  - rejected
  - expired
- demandRefs
- routeRefs
- tripRefs
- objective
  - minimize_cost
  - protect_service
  - minimize_empty_miles
  - maximize_capacity
  - balance_cost_service
- hardBlockers
- warnings
- serviceRiskEstimate
- costEstimate
- createdAt
- evaluatedAt
```

## Planning suggestion

```text
TransportationPlanningSuggestion
- suggestionId
- planningScenarioId
- suggestionType
  - consolidate_demands
  - split_demand
  - assign_private_fleet
  - tender_to_carrier
  - sequence_stops
  - delay_for_consolidation
  - use_backhaul
  - change_equipment
- status
  - proposed
  - accepted
  - rejected
  - superseded
- summary
- hardBlockers
- softWarnings
- estimatedCost
- estimatedMiles
- estimatedServiceRisk
- affectedDemandRefs
```

## Driver capacity snapshot

StaffArr owns people, shifts, and availability source data. TrainArr owns qualifications. ELD providers own certified HOS capture. RoutArr owns dispatch feasibility snapshots.

```text
DriverCapacitySnapshot
- driverCapacitySnapshotId
- personId
- source
  - staffarr
  - eld
  - dispatcher
  - system
- shiftWindowStart
- shiftWindowEnd
- hosRemainingMinutes
- driveTimeRemainingMinutes
- onDutyRemainingMinutes
- breakRequiredBy
- domicileLocationRef
- feasibilityStatus
  - feasible
  - warning
  - blocked
  - unknown
- blockerSummary
- snapshotAt
- freshnessState
```

## Events

```text
routarr.planning_scenario.created
routarr.planning_scenario.evaluated
routarr.planning_suggestion.created
routarr.planning_suggestion.accepted
routarr.capacity_snapshot.created
routarr.dispatch_feasibility.checked
```

