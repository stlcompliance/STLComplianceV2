# Companion App Feature Set

## Product Definition

The companion app is a field-first mobile experience for entitled users who need to perform daily work without navigating the full desktop suite. It is not a separate source of truth. It calls the owning product APIs.

## Principle

The app shows tasks first, products second. Users see what they need to do, what they are allowed to do, and what is blocked, while ownership boundaries remain enforced by APIs.

## Core Features

- Unified task inbox
- Product switcher for entitled products
- Assigned inspections and work orders from MaintainArr
- Assigned trips and DVIR tasks from RoutArr
- Training assignments from TrainArr
- Receiving/count/approval tasks from SupplyArr where permitted
- StaffArr incidents/acknowledgements where permitted
- Photo/document/signature evidence capture
- QR/barcode scan support where useful
- Offline-resilient task capture
- Clear sync state
- Idempotency keys
- Push-notification readiness
- Plain blocked/denied reason messages

## API Rules

- NexArr token, tenant, and entitlement validation remains mandatory.
- Product APIs enforce permissions.
- Offline submissions are validated server-side during sync.
- Evidence uploads use owning product endpoints.

## Completion Definition

A field user can open one mobile experience, see assigned work across products, complete permitted tasks, capture evidence, and submit actions to the correct product APIs without needing to understand backend boundaries.
