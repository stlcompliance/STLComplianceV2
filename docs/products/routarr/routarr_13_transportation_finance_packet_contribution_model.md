# RoutArr - Transportation Finance Packet Contribution Model

RoutArr contributes transportation facts to OrdArr invoice-ready packets and SupplyArr bill-ready packets. It does not own finance execution.

## Finance packet contribution

```text
TransportationFinancePacketContribution
- contributionId
- contributionNumber
- transportationDemandId
- tripId
- freightRatingRef
- contributionType
  - invoice_ready_context
  - bill_ready_context
  - freight_audit_context
  - accessorial_context
  - claim_context
  - proof_context
- targetProduct
  - ordarr
  - supplyarr
- status
  - draft
  - assembling
  - ready
  - sent
  - accepted
  - rejected
  - superseded
  - canceled
- operationalSummary
- costSnapshot
- accessorialRefs
- proofRefs
- documentPacketRefs
- claimRefs
- externalFinanceSystemSnapshot
- sentAt
- acceptedAt
```

## Freight claim context

```text
FreightClaim
- freightClaimId
- claimNumber
- transportationDemandId
- tripId
- claimAgainstPartyType
  - carrier
  - vendor
  - customer
  - internal
- claimReason
  - damage
  - loss
  - shortage
  - delay
  - service_failure
  - other
- claimAmount
- recoveryAmount
- status
  - requested
  - submitted
  - carrier_review
  - approved
  - denied
  - partial
  - recovered
  - written_off
  - closed
- evidenceRefs
- assurarrNonconformanceRef
- supplyarrPerformanceImpactRef
- ordarrCloseoutImpactRef
```

## Events

```text
routarr.freight_claim.requested
routarr.freight_claim.submitted
routarr.freight_claim.resolved
routarr.finance_packet.contribution_ready
routarr.finance_packet.contribution_sent
```

