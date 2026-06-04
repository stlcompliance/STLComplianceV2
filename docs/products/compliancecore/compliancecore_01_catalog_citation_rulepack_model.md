# Compliance Core — Catalog, Citation, and Rulepack Model

## Governing body

A GoverningBody is an authority, standard owner, customer authority, or internal policy authority that can issue requirements.

```text
GoverningBody
- governingBodyId
- governingBodyKey
- displayName
- abbreviation
- description
- jurisdictionType
  - federal
  - state
  - local
  - international
  - industry
  - customer
  - internal
  - insurer
  - other
- country
- state
- locality
- websiteUrl
- status
  - draft
  - active
  - inactive
  - deprecated
  - archived
- replacedByGoverningBodyRef
- createdAt
- updatedAt
```

## Jurisdiction

```text
Jurisdiction
- jurisdictionId
- jurisdictionKey
- displayName
- jurisdictionType
  - country
  - state
  - province
  - county
  - city
  - region
  - site
  - customer
  - internal
- parentJurisdictionId
- country
- state
- locality
- status
  - active
  - inactive
  - deprecated
```

## Regulation source

A RegulationSource is the source container for citations and requirements. Examples: CFR title, OSHA standard, customer standard, internal policy manual, industry standard.

```text
RegulationSource
- regulationSourceId
- sourceKey
- title
- description
- sourceType
  - cfr
  - statute
  - regulation
  - standard
  - guidance
  - interpretation
  - customer_requirement
  - internal_policy
  - insurer_requirement
  - contract
  - other
- governingBodyId
- jurisdictionRefs
- publicationRef
- sourceUrl
- effectiveAt
- supersededAt
- status
  - draft
  - active
  - superseded
  - deprecated
  - archived
- versionLabel
- createdAt
- updatedAt
```

## Citation

A Citation is a stable reference to a section, paragraph, clause, appendix, table, or interpretive item from a RegulationSource.

```text
Citation
- citationId
- citationKey
- regulationSourceId
- parentCitationId
- displayCitation
- title
- citationText
- titleNumber
- subtitle
- chapter
- subchapter
- part
- subpart
- section
- paragraph
- appendix
- table
- clause
- citationPath
- effectiveAt
- supersededAt
- status
  - active
  - superseded
  - reserved
  - removed
  - deprecated
- replacedByCitationRef
- sourceUrl
- notes
```

## Citation relationship

```text
CitationRelationship
- relationshipId
- sourceCitationId
- targetCitationId
- relationshipType
  - parent_child
  - references
  - modifies
  - supersedes
  - interpreted_by
  - exception_to
  - definition_for
  - related_to
- status
```

## Controlled catalog

Compliance Core owns controlled compliance catalogs that other products can consume.

```text
ControlledCatalog
- catalogId
- catalogKey
- title
- description
- status
  - draft
  - active
  - deprecated
  - archived
- ownerProduct: compliancecore
- entryRefs
- version
- createdAt
- updatedAt
```

## Controlled catalog entry

```text
ControlledCatalogEntry
- entryId
- catalogId
- entryKey
- displayName
- description
- status
  - active
  - inactive
  - deprecated
- sortOrder
- parentEntryId
- aliases
- metadata
```

## Core catalogs

```text
- governing_bodies
- jurisdictions
- regulation_sources
- citation_types
- requirement_types
- evidence_types
- applicability_subject_types
- asset_compliance_categories
- training_compliance_categories
- document_compliance_categories
- incident_compliance_categories
- maintenance_compliance_categories
- transportation_compliance_categories
- inventory_compliance_categories
- supplier_compliance_categories
- customer_requirement_categories
- exception_types
- exemption_types
- severity_levels
- confidence_levels
- retention_triggers
```

## Alias

Aliases help normalize user language and imported text.

```text
Alias
- aliasId
- phrase
- normalizedKey
- aliasType
  - acronym
  - synonym
  - abbreviation
  - common_name
  - misspelling
  - legacy_term
  - product_term
- targetObjectType
  - governing_body
  - citation
  - rulepack
  - requirement
  - evidence_type
  - catalog_entry
- targetObjectRef
- status
  - active
  - inactive
  - deprecated
```

## Rulepack

A Rulepack is a packaged set of related requirements, applicability logic, evidence requirements, and citations.

```text
Rulepack
- rulepackId
- rulepackKey
- title
- description
- domain
  - fleet
  - workplace_safety
  - hazmat
  - training
  - maintenance
  - warehouse
  - transportation
  - environmental
  - quality
  - customer
  - supplier
  - document_control
  - internal
  - other
- status
  - draft
  - review
  - active
  - superseded
  - deprecated
  - archived
- version
- versionLabel
- governingBodyRefs
- jurisdictionRefs
- regulationSourceRefs
- citationRefs
- requirementRefs
- applicabilityRuleRefs
- exceptionRefs
- exemptionRefs
- evidenceTypeRefs
- retentionRuleRefs
- effectiveAt
- expiresAt
- supersededByRulepackRef
- ownerPersonId
- reviewerPersonId
- approvedByPersonId
- approvedAt
- createdAt
- updatedAt
- auditTrail
```

## Rulepack version

```text
RulepackVersion
- rulepackVersionId
- rulepackId
- version
- versionLabel
- status
  - draft
  - review
  - active
  - superseded
  - archived
- changeSummary
- effectiveAt
- supersededAt
- requirementSnapshot
- citationSnapshot
- approvedByPersonId
- approvedAt
```

## Rulepack family examples

```text
FMCSA
- Driver qualification file requirements
- Hours of service records
- Vehicle inspection, repair, and maintenance
- Annual inspections
- Roadside inspection handling
- ELD documentation and supporting records
- Accident register
- Drug and alcohol program references if in scope

OSHA
- Hazard communication
- PPE
- Lockout/tagout
- Powered industrial trucks
- Walking-working surfaces
- Recordkeeping
- Emergency action/fire prevention
- Machine guarding
- Respiratory protection if applicable

MSHA
- Part 46 training
- Workplace examinations
- Hazard reporting
- Contractor/customer mining-site applicability

EPA
- Hazardous waste handling
- Spill response evidence
- Environmental recordkeeping

Internal/customer
- Customer required PPE
- Customer delivery documentation
- Site-specific training
- Insurance/certificate requirements
```

## Rulepack lifecycle

```text
1. Rulepack is drafted.
2. Citations are mapped.
3. Requirements are created.
4. Applicability logic is defined.
5. Evidence requirements are defined.
6. Exceptions/exemptions are mapped.
7. Rulepack is validated.
8. Reviewer approves.
9. Rulepack becomes active.
10. Superseded versions remain available for historical evaluation.
```

## Events

```text
compliancecore.governing_body.created
compliancecore.governing_body.updated
compliancecore.catalog.created
compliancecore.catalog.updated
compliancecore.citation.created
compliancecore.citation.updated
compliancecore.rulepack.created
compliancecore.rulepack.submitted_for_review
compliancecore.rulepack.approved
compliancecore.rulepack.activated
compliancecore.rulepack.superseded
compliancecore.rulepack.deprecated
compliancecore.alias.created
compliancecore.alias.updated
```
