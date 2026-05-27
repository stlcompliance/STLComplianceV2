# API Conventions and Service Contracts

## API Style

- REST-style resource routes
- JSON bodies
- OpenAPI / Swagger
- Common error shape
- Pagination for lists
- Server-side tenant enforcement
- Server-side permission enforcement
- Health endpoints

## Error Shape

```json
{
  "errorId": "uuid",
  "code": "string",
  "message": "Human readable message",
  "details": {},
  "correlationId": "uuid"
}
```

## Pagination Shape

```json
{
  "items": [],
  "page": 1,
  "pageSize": 50,
  "totalCount": 123,
  "hasNextPage": true
}
```

## Health

Every API exposes:

- `/health`
- `/health/ready`

Health checks verify service, database, required config, Redis where used, and dependency reachability where safe.

## Service Token Claims

- issuer
- audience
- service client ID
- source product
- allowed products
- tenant scope
- action scope
- expiration
- key ID

## Contract Examples

- StaffArr readiness: `GET /api/people/{personId}/readiness`
- TrainArr qualification check: `POST /api/qualification-checks`
- MaintainArr asset readiness: `GET /api/assets/{assetId}/readiness`
- RoutArr dispatchability: `POST /api/dispatchability-checks`
- SupplyArr part availability: `GET /api/parts/{partId}/availability`
- Compliance Core validation: `POST /api/internal/validate`

## Idempotency

State-changing commands that may be retried support idempotency keys, including qualification publication, work-order generation, dispatch assignment, purchase request creation, and rule publication.
