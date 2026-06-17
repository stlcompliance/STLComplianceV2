# RoutArr - Carrier Tender and Routing Guide Model

RoutArr owns tender events for a specific TransportationDemand. SupplyArr owns carrier/vendor master records, carrier contacts, rate agreements, insurance files, commercial documents, and carrier performance source records.

## Routing guide step

```text
RoutingGuideStep
- routingGuideStepId
- transportationDemandId
- sequence
- carrierSupplierRef
- carrierSnapshot
- tenderMethod
  - portal
  - api
  - email
  - edi
  - phone
  - manual
- serviceLevel
- equipmentRequirement
- laneSnapshot
- rateAgreementSnapshotRef
- fallbackType
  - none
  - next_carrier
  - private_fleet
  - dedicated
  - spot_quote
- status
  - available
  - skipped
  - selected
  - exhausted
```

## Tender

```text
CarrierTender
- tenderId
- transportationDemandId
- tenderNumber
- routingGuideStepRef
- carrierSupplierRef
- carrierSnapshot
- tenderMethod
- status
  - draft
  - created
  - sent
  - accepted
  - rejected
  - expired
  - countered
  - withdrawn
  - fallback_required
- expiresAt
- sentAt
- respondedAt
- declineReason
- counterSummary
- proposedAlternative
- auditTrail
```

## Ownership rules

```text
SupplyArr owns carrier identity and commercial eligibility.
Compliance Core evaluates compliance meaning.
RecordArr stores insurance, signed rate sheets, BOLs, PODs, and carrier-submitted documents.
RoutArr owns the tender lifecycle for the movement and its dispatch impact.
```

## Events

```text
routarr.tender.created
routarr.tender.sent
routarr.tender.accepted
routarr.tender.rejected
routarr.tender.expired
routarr.tender.countered
routarr.tender.withdrawn
routarr.routing_guide.step_selected
routarr.carrier.assigned
```

