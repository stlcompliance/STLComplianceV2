# OrdArr - Workflows, Status Logic, Events, and APIs

## Current MVP endpoints

The implementation in this repo currently exposes these workspace/order endpoints:

- `GET /api/v1/workspace/summary`
- `GET /api/v1/workspace/orders`
- `GET /api/v1/workspace/orders/{orderId}`
- `GET /api/v1/workspace/handoffs`
- `GET /api/v1/workspace/completion-packets`
- `GET /api/v1/workspace/reports/summary`
- `POST /api/v1/orders`
- `POST /api/v1/orders/{orderId}/submit`
- `POST /api/v1/orders/{orderId}/approve`
- `POST /api/v1/orders/{orderId}/accept`
- `POST /api/v1/orders/{orderId}/lines`
- `POST /api/v1/orders/{orderId}/holds`
- `POST /api/v1/orders/{orderId}/holds/{holdId}/release`
- `POST /api/v1/orders/{orderId}/returns`
- `POST /api/v1/orders/{orderId}/cancel`
- `GET /api/v1/orders/{orderId}/lines`
- `GET /api/v1/orders/{orderId}/holds`
- `GET /api/v1/orders/{orderId}/timeline`
- `GET /api/v1/orders/{orderId}/returns`
- `GET /api/v1/integrations/orders/{orderId}/readiness`

The larger integration API list below remains the aspirational contract for future phases.

## Major workflow: customer order intake

1. User or integration creates an order/request.
2. OrdArr resolves CustomArr customer, contact, location, and requirement refs.
3. OrdArr checks StaffArr authority for the actor.
4. OrdArr evaluates required approvals, compliance checks, and target products.
5. User submits the order/request.
6. OrdArr moves status to `submitted` or `triage`.
7. OrdArr emits order/request events.

## Major workflow: triage to product handoffs

1. OrdArr reviews order lines, requirements, due dates, and target product needs.
2. OrdArr requests eligibility/readiness checks from owning products.
3. OrdArr creates idempotent handoffs to LoadArr, RoutArr, MaintainArr, SupplyArr, RecordArr, Compliance Core, AssurArr, or Field Companion as needed.
4. Target products accept, reject, or block the handoff.
5. OrdArr updates coordination state without taking ownership of target records.

## Major workflow: fulfillment coordination

1. LoadArr reports fulfillment, receiving, inventory, or exception status.
2. RoutArr reports transport, proof, ETA, or exception status.
3. MaintainArr reports maintenance/work status where applicable.
4. SupplyArr reports procurement status where applicable.
5. AssurArr and Compliance Core report blockers or requirement status where applicable.
6. OrdArr updates order coordination read models and customer-facing request status.

## Major workflow: completion and closeout

1. Execution products report required work complete.
2. OrdArr evaluates closeout checklist.
3. RecordArr stores supporting documents and completion packet records.
4. Compliance Core confirms evidence requirement status where needed.
5. AssurArr confirms quality release where needed.
6. OrdArr approves completion packet.
7. OrdArr creates invoice-ready or bill-ready packet decisions.
8. OrdArr closes the order/request.

## OrdArr emitted events

- `ordarr.order_request.created`
- `ordarr.order_request.submitted`
- `ordarr.order_request.triage_started`
- `ordarr.order_request.approved`
- `ordarr.order_request.blocked`
- `ordarr.order_request.unblocked`
- `ordarr.order_request.handoff_requested`
- `ordarr.order_request.in_progress`
- `ordarr.order_request.partially_fulfilled`
- `ordarr.order_request.fulfilled`
- `ordarr.order_request.completed`
- `ordarr.order_request.closed`
- `ordarr.order_request.cancelled`
- `ordarr.order_line.created`
- `ordarr.order_line.updated`
- `ordarr.order_requirement.created`
- `ordarr.order_requirement.satisfied`
- `ordarr.order_exception.opened`
- `ordarr.order_exception.resolved`
- `ordarr.order_handoff.requested`
- `ordarr.order_handoff.accepted`
- `ordarr.order_handoff.rejected`
- `ordarr.order_handoff.blocked`
- `ordarr.order_handoff.completed`
- `ordarr.completion_packet.ready_for_review`
- `ordarr.completion_packet.stored`
- `ordarr.invoice_ready_packet.ready`
- `ordarr.invoice_ready_packet.sent`
- `ordarr.bill_ready_packet.ready`
- `ordarr.bill_ready_packet.sent`

## Integration APIs OrdArr should expose

POST /api/v1/integrations/order-requests

- Create an order/request from an approved source.
- Requires idempotency key.

GET /api/v1/integrations/order-requests/{orderRequestId}

- Return order/request summary, status, source refs, and freshness metadata.

GET /api/v1/integrations/order-requests/{orderRequestId}/status

- Return customer-facing and internal coordination status.

POST /api/v1/integrations/order-requests/{orderRequestId}/status-updates

- Accept source-product status facts without transferring execution ownership.

POST /api/v1/integrations/order-requests/{orderRequestId}/handoffs

- Create target product handoff requests.

POST /api/v1/integrations/order-handoffs/{orderHandoffId}/responses

- Accept target product handoff accept/reject/block/complete responses.

GET /api/v1/integrations/order-requests/{orderRequestId}/completion-readiness

- Return closeout checklist, blockers, missing evidence, and source freshness.

POST /api/v1/integrations/order-requests/{orderRequestId}/completion-packets

- Create or update an OrdArr completion packet coordination record.

POST /api/v1/integrations/order-requests/{orderRequestId}/invoice-ready-packets

- Create an operational invoice-ready packet for external finance handoff.

POST /api/v1/integrations/order-requests/{orderRequestId}/bill-ready-packets

- Create an operational bill-ready packet for external finance handoff.

## APIs OrdArr should consume

NexArr:

- POST /api/v1/platform/handoff/redeem
- POST /api/v1/platform/service-tokens/introspect
- GET /api/v1/platform/tenants/{tenantId}/entitlements/{productKey}

CustomArr:

- GET /api/v1/integrations/customers/{customerId}
- GET /api/v1/integrations/customer-locations/{customerLocationId}
- POST /api/v1/integrations/customer-eligibility-checks
- POST /api/v1/integrations/customer-requirement-checks

StaffArr:

- GET /api/v1/integrations/people/{personId}
- GET /api/v1/integrations/locations/{locationId}
- POST /api/v1/integrations/authority-checks

SupplyArr:

- GET /api/v1/integrations/items/{itemId}
- POST /api/v1/integrations/purchase-requests
- GET /api/v1/integrations/purchase-requests/{purchaseRequestId}/status

LoadArr:

- POST /api/v1/integrations/availability-checks
- POST /api/v1/integrations/fulfillment-requests
- GET /api/v1/integrations/fulfillment-requests/{fulfillmentRequestId}/status

RoutArr:

- POST /api/v1/integrations/dispatch-requests
- GET /api/v1/integrations/trips/{tripId}/status
- GET /api/v1/integrations/stops/{stopId}/proof

MaintainArr:

- POST /api/v1/integrations/work-requests
- GET /api/v1/integrations/work-orders/{workOrderId}/status

Compliance Core:

- POST /api/v1/compliancecore/evaluations
- GET /api/v1/compliancecore/evidence-requirements

RecordArr:

- POST /api/v1/integrations/record-packages
- POST /api/v1/integrations/attachments
- GET /api/v1/integrations/records/{recordId}

AssurArr:

- GET /api/v1/integrations/quality-holds
- GET /api/v1/integrations/assurance-cases/{caseId}/status

## Permissions

- `ordarr.order_requests.read`
- `ordarr.order_requests.create`
- `ordarr.order_requests.update`
- `ordarr.order_requests.submit`
- `ordarr.order_requests.triage`
- `ordarr.order_requests.approve`
- `ordarr.order_requests.cancel`
- `ordarr.order_requests.close`
- `ordarr.order_handoffs.create`
- `ordarr.order_handoffs.manage`
- `ordarr.completion_packets.review`
- `ordarr.financial_packets.prepare`
- `ordarr.admin`

## Admin surfaces

OrdArr admin can manage:

- order/request type configuration
- order line templates
- triage queues
- handoff routing rules
- closeout checklist configuration
- completion packet templates
- invoice-ready and bill-ready packet rules
- source-product integration settings

NexArr remains final authority for platform admin, tenant entitlement, product launch, service clients, service tokens, and handoff trust.
