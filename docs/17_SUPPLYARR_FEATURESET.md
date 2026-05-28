# SupplyArr Feature Set

## Product Definition

SupplyArr is the vendor, supply, parts, inventory, and procurement system.

## Owns

- vendors
- dealers
- suppliers
- external parties
- parts
- catalogs
- inventory
- purchase requests
- purchase orders
- receiving
- pricing snapshots
- lead-time snapshots
- availability snapshots
- approval workflows

## Does Not Own

- login
- people truth
- maintenance execution
- training records
- dispatch records
- rule packs

## Core Features

- party registry
- part catalog
- manufacturer alias support
- UOM/category support
- inventory locations
- stock/reservation
- reorder evaluation
- purchase request workflow
- purchase order workflow
- receiving
- pricing/lead-time history
- availability history
- MaintainArr demand intake
- RoutArr demand intake
- TrainArr demand intake
- StaffArr demand intake
- vendor reports

## Required API Surfaces

- `/api/vendors`
- `/api/dealers`
- `/api/suppliers`
- `/api/parties`
- `/api/parts`
- `/api/catalogs`
- `/api/inventory`
- `/api/purchase-requests`
- `/api/purchase-orders`
- `/api/receiving`
- `/api/pricing-snapshots`
- `/api/lead-time-snapshots`
- `/api/availability-snapshots`
- `/api/demand-refs`
- `/health`

## Completion Definition

Shop users can request parts in context while SupplyArr remains source of truth for vendors, inventory, approvals, PO, receiving, pricing, and lead-time truth.
