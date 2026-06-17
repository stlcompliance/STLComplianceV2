# RoutArr - Multimodal and Service-Level Model

RoutArr is road/private-fleet oriented in the current implementation but must not model itself into a road-only corner.

## Supported mode values

```text
- private_fleet
- dedicated_carrier
- truckload
- ltl
- parcel
- intermodal
- rail
- drayage
- ocean
- air
- courier
- shuttle
- internal_transfer
```

## Mode-specific requirement ref

```text
ModeSpecificRequirementRef
- modeRequirementRefId
- transportationDemandId
- transportMode
- requirementType
- sourceProduct
- sourceRequirementRef
- summarySnapshot
- documentRequirementRefs
- status
  - pending
  - satisfied
  - waived
  - blocked
```

## Service-level fields

```text
TransportationDemand.serviceLevel
TransportationDemand.equipmentRequirement
TransportationDemand.carrierServiceRef
TransportationDemand.modeSpecificRequirementRefs
TransportationDemand.modeSpecificDocumentRequirementRefs
```

## Rule

Mode fields allow planning, rating, tendering, and documents to vary by mode. Implementing a mode field does not mean RoutArr owns ocean, rail, air, parcel, or drayage external execution systems.

