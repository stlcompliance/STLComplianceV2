# Platform Reference Data API

## Admin APIs

Admin endpoints are Platform Reference Data service endpoints that require NexArr platform-admin validation.

- `GET /api/v1/reference-data/datasets`
- `POST /api/v1/reference-data/datasets`
- `GET /api/v1/reference-data/sources`
- `POST /api/v1/reference-data/sources`
- `GET /api/v1/reference-data/imports`
- `POST /api/v1/reference-data/imports`
- `GET /api/v1/reference-data/imports/{jobId}`
- `GET /api/v1/reference-data/imports/{jobId}/staging-records`
- `POST /api/v1/reference-data/staging-records/{id}/approve`
- `POST /api/v1/reference-data/staging-records/{id}/reject`
- `POST /api/v1/reference-data/staging-records/{id}/merge`
- `POST /api/v1/reference-data/staging-records/{id}/escalate`
- `POST /api/v1/reference-data/datasets/{datasetId}/publish`
- `GET /api/v1/reference-data/publish-history`
- `GET /api/v1/reference-data/crosswalks`
- `POST /api/v1/reference-data/crosswalks`

## Product-consumption APIs

- `GET /api/v1/reference-data/catalogs/{datasetKey}/entities`
- `GET /api/v1/reference-data/entities/{id}`
- `GET /api/v1/reference-data/lookup`
- `GET /api/v1/reference-data/crosswalks/resolve`

## Specialized lookup routes

- `GET /api/v1/reference-data/vehicles/decode-vin?vin={vin}&modelYear={year}`
- `GET /api/v1/reference-data/products/lookup-gtin?gtin={gtin}`
- `GET /api/v1/reference-data/sds/lookup?manufacturer={name}&product={name}`
- `GET /api/v1/reference-data/chemicals/lookup?cas={cas}`

## Request shape guidance

The API should return view-ready business objects, not raw persistence rows.

Recommended response fields:

- canonical identifiers
- human-readable display names
- status
- published version
- confidence
- source evidence summaries
- tenant overlay summaries
- crosswalk summaries
- freshness or publish timestamps

## Scope and authorization

- Platform-admin routes require server-side NexArr admin validation.
- Product-consumption routes require service tokens.
- Lookup routes should accept published/active scope only unless explicitly requested by an admin.

## Error behavior

Use stable machine-readable error codes for:

- not found
- unauthorized
- forbidden
- validation failure
- conflict
- stale source
- publish blocked
- review required

Admin UI should present plain-language explanations for those cases.
