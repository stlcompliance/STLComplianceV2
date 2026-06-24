# RoutArr Granular End-Goal Markdown Package

This package defines RoutArr at the domain-object level.

## Files

- `routarr_00_scope_and_boundaries.md`
- `routarr_01_dispatch_route_trip_model.md`
- `routarr_02_stop_proof_exception_model.md`
- `routarr_03_driver_equipment_compliance_model.md`
- `routarr_04_dock_appointment_load_visibility_model.md`
- `routarr_05_workflows_status_events_apis.md`
- `routarr_06_transportation_demand_model.md`
- `routarr_07_carrier_tender_routing_guide_model.md`
- `routarr_08_freight_rating_accessorial_cost_model.md`
- `routarr_09_visibility_tracking_telematics_model.md`
- `routarr_10_planning_optimization_capacity_model.md`
- `routarr_11_yard_trailer_gate_drop_hook_model.md`
- `routarr_12_multimodal_service_level_model.md`
- `routarr_13_transportation_finance_packet_contribution_model.md`
- `routarr_14_tenant_settings_model.md`
- `routarr_15_dispatch_workspace_navigation_and_execution.md`

## Purpose

RoutArr owns transportation planning and execution for STL Compliance / ARR:

- Dispatch plans
- Routes
- Trips
- Stops
- Driver assignment
- Vehicle/trailer assignment context
- ETA and arrival/departure events
- Route exceptions
- Proof of pickup
- Proof of delivery
- Dock appointment notifications to LoadArr
- Transportation readiness checks
- Transportation execution status
- Transportation demand before trip creation
- Carrier tender execution and routing-guide decisions
- Freight rating snapshots, accessorial events, and cost variance facts
- Visibility event normalization from Field Companion, carrier, ELD, telematics, and manual check-call sources
- Rules-based planning, consolidation suggestions, and dispatch capacity feasibility snapshots
- Transportation yard, trailer, gate, drop/hook, and detention/dwell events
- Multimodal service-level and requirement modeling
- Transportation finance packet contributions for OrdArr/SupplyArr handoff packets
- Tenant-scoped transportation operations settings, effective settings, scoped overrides, settings audit, and settings events

RoutArr does not own driver/person truth, qualification truth, asset maintenance truth, inventory truth, receiving truth, customer master truth, order lifecycle truth, document file truth, quality hold/release truth, regulatory meaning, reporting read models, or accounting execution.
