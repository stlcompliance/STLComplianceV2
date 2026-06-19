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
  - ordinance
  - permit_condition
  - license_condition
  - consent_order
  - settlement
  - court_order
  - standard
  - adopted_code
  - incorporated_standard
  - guidance
  - interpretation
  - proposed_rule
  - customer_requirement
  - internal_policy
  - insurer_requirement
  - contract
  - certification_framework
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

## Binding classification

BindingClassification distinguishes legal effect from source format.

```text
BindingClassification
- bindingClassificationKey
  - binding_law
  - binding_named_scope
  - binding_through_adoption
  - binding_to_incorporated_extent
  - interpretive
  - nonbinding_pending
  - contractual_obligation
  - contractual_risk_transfer
  - voluntary_or_contractual
  - organization_imposed_control
- displayName
- description
- reviewRequired
- counselReviewRecommended
```

Examples:

```text
- OSHA regulation -> binding_law
- local fire code edition adopted by ordinance -> binding_through_adoption
- ISO standard incorporated by FDA regulation -> binding_to_incorporated_extent
- agency FAQ -> interpretive
- proposed rule -> nonbinding_pending
- insurance policy condition -> contractual_risk_transfer
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
- bindingClassificationRef
- lifecycleStatus
  - proposed
  - final_not_effective
  - effective
  - stayed
  - enjoined
  - vacated
  - superseded
  - repealed
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
- bindingClassificationRefs
- primaryAppBindings
- contributingAppBindings
- evidenceProducingAppBindings
- reportingAppBinding
- activationFactRefs
- implementationPriority
  - foundation
  - operational_baseline
  - supply_chain_quality
  - vertical_or_international
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
Universal business baseline
- Legal entity and business authority
- Tax and statutory financial obligations
- Employment and labor
- Workplace safety
- Privacy and personal data
- Cybersecurity
- AI and automated decisions
- Marketing and communications
- Electronic records and signatures
- Commercial transactions
- Consumer protection and accessibility
- Competition and procurement integrity
- Anti-corruption, sanctions, and fraud

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

PHMSA / hazmat transportation
- Hazmat registration
- Classification and hazardous-material table lookup
- Packaging, marking, labeling, placarding, shipping papers, emergency response
- Training, security plans, loading, unloading, segregation, attendance, incident reporting

Environmental and facility
- RCRA hazardous and solid waste
- SPCC and water discharge
- EPCRA and CERCLA releases
- Air permits and refrigerants
- TSCA and FIFRA
- Storage tanks and adopted fire/building codes

Supply chain and product
- OFAC, EAR, ITAR, customs, forced-labor import controls, antidumping/countervailing duties
- CPSC, NHTSA, FDA, USDA, EPA, FCC, product labeling, recall, traceability
- Packaging, recycling, deposits, EPR, Proposition 65, weights and measures

Food, pharmaceutical, device, and quality
- FSMA, preventive controls, FSVP, traceability, sanitary transportation, reportable food
- Drug CGMP, dietary supplement CGMP, medical-device QMSR, MDR, corrections/removals
- DSCSA, DEA, MoCRA, state licensing

Government contracting
- FAR, agency supplements, DFARS, grant rules, clause-level labor, cybersecurity, domestic-preference, ethics, and disclosure obligations

Internal/customer
- Customer required PPE
- Customer delivery documentation
- Site-specific training
- Insurance/certificate requirements
```

The full target law map is maintained in `compliancecore_07_legal_map_and_rulepack_catalog.md`.

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
