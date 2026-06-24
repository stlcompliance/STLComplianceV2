# Compliance Core - Legal Map and Rulepack Catalog

## Purpose

This document defines the target legal universe Compliance Core should map and the modeling rules for turning that universe into executable requirements.

Compliance Core should map laws and other compliance sources only when they create an operational obligation, prohibition, qualification, deadline, filing, inspection, evidence requirement, retention period, consent requirement, or corrective action for a tenant.

Compliance Core must not ingest entire statutes or regulations as undifferentiated documents. The canonical unit is an atomic, versioned requirement connected to the applications that execute the work, produce the evidence, retain the record, or render the report.

## Modeling Rules

Every legal-map entry should become one or more rulepacks. Every rulepack should resolve into atomic requirements with:

- precise citation or source reference
- jurisdiction and territorial scope
- source type and binding classification
- provision version and lifecycle status
- effective, enforcement, repeal, transition, and review dates where known
- regulated actor or role
- regulated object, asset, person, site, shipment, transaction, material, product, or data type
- applicability predicates, thresholds, exemptions, and exceptions
- required or prohibited action
- trigger, deadline, interval, recurrence, and grace period
- required training, qualification, inspection, maintenance, evidence, retention, filing, notification, or attestation
- owning execution app, contributing apps, evidence-producing apps, RecordArr storage expectation, and ReportArr output expectation
- source provenance, confidence, human/counsel review status, and last legal review

Tenant facts drive applicability. NAICS alone is not enough.

## Binding Classification

Compliance Core should support each source type without labeling them as the same thing.

| Source type | Binding classification |
|---|---|
| Statute | Binding law |
| Regulation | Binding law |
| State or local ordinance | Binding law |
| Permit or license condition | Binding for the named entity, site, activity, or asset |
| Consent order or settlement | Binding for the named party and scope |
| Court order | Binding according to the order scope |
| Code or standard adopted by a jurisdiction | Binding through adoption |
| Standard incorporated by regulation | Binding to the extent incorporated |
| Agency interpretation or guidance | Interpretive, not automatically regulation |
| Proposed rule | Nonbinding and pending |
| Customer or vendor contract | Contractual obligation |
| Insurance policy condition | Contractual or risk-transfer obligation |
| Certification framework | Voluntary or contractual unless legally incorporated |
| Internal policy | Organization-imposed control |

ISO 9001, ISO 14001, ISO 45001, SOC 2, PCI DSS, IATF 16949, BRCGS, SQF, and most NIST frameworks are not generic laws. NFPA, IFC, IBC, NEC, ANSI, ASME, API, ASTM, SAE, and similar standards require adoption or incorporation analysis before they are treated as binding law. CMMC and NIST SP 800-171 usually enter through government-contract or regulatory mechanisms rather than as universal private-sector law.

## Universal Business Rulepacks

These packs begin as broad candidates, then narrow through tenant facts, jurisdiction, role, headcount, activities, materials, assets, locations, contracts, and the operational records supplied by owning products.

| Domain | Laws and requirements to map | Primary app bindings |
|---|---|---|
| Legal entity and business authority | Corporation and LLC statutes, foreign qualification, DBAs, registered agents, annual reports, beneficial ownership where applicable, business and professional licenses, permits, renewals, dissolution, reinstatement | LedgArr, RecordArr, ReportArr |
| Tax and statutory financial obligations | Federal income, employment, excise, information-return, W-2, 1099, FICA, FUTA, state withholding, unemployment, sales/use, franchise, property, unclaimed property, fuel taxes, Form 2290, IFTA, IRP, UCR | LedgArr, StaffArr, MaintainArr, RoutArr, OrdArr, ReportArr |
| Employment and labor | FLSA, FMLA, Title VII, ADA Title I, ADEA, Equal Pay Act, GINA, PWFA, PUMP Act, USERRA, NLRA, WARN and mini-WARN, IRCA/I-9, E-Verify, FCRA screening, ERISA, COBRA, ACA, workers compensation, unemployment, state/local wage, leave, scheduling, pay-transparency, personnel-file, garnishment, drug-testing, monitoring, biometric, ban-the-box | StaffArr, LedgArr, TrainArr, RecordArr, Field Companion, ReportArr |
| Workplace safety | OSH Act general duty, 29 CFR Parts 1904 and 1910, applicable 1926, maritime, agriculture, and state-plan OSHA standards, injury reporting, HazCom, PPE, LOTO, powered industrial trucks, machine guarding, electrical safety, respiratory protection, hearing conservation, emergency plans, fire protection, confined spaces, bloodborne pathogens, HAZWOPER, process safety, workplace violence where enacted | StaffArr, TrainArr, MaintainArr, LoadArr, AssurArr, RecordArr, Field Companion, ReportArr |
| Privacy and personal data | FTC Act, state privacy and breach laws, biometric privacy, employee/applicant privacy, geolocation and vehicle-tracking rules, consumer health data, data broker rules, HIPAA/HITECH, GLBA, COPPA, FERPA, DPPA where applicable, minimization, access, deletion, correction, opt-out, consent, retention, processor-contract duties | NexArr, StaffArr, CustomArr, OrdArr, RecordArr, Field Companion, STLComplianceSite |
| Cybersecurity | Safeguards, risk assessments, security programs, incident response, breach notification, vendor security, access control, sector packs for GLBA, HIPAA, SEC, state financial services, government contracts, critical infrastructure | NexArr, RecordArr, AssurArr, ReportArr, all apps as control/evidence contributors |
| AI and automated decisions | Automated employment decision laws, notice, consent, impact assessment, bias audit, human review, explanation, appeal, retention, vendor governance, automated-decision opt-outs under privacy laws | StaffArr, CustomArr, NexArr, Compliance Core, RecordArr |
| Marketing and communications | TCPA, TSR, federal and state Do-Not-Call, CAN-SPAM, mini-TCPA, call recording/wiretap consent, automated texting/dialing, marketing email identification and opt-out, lead-source consent, suppression lists | CustomArr, OrdArr, STLComplianceSite, RecordArr |
| Electronic records and signatures | E-SIGN, state UETA, attribution, consent, integrity, reproducibility, delivery, retention, legal holds, litigation preservation, admissibility, evidence chain | RecordArr, NexArr, Field Companion, every record-producing app |
| Commercial transactions | UCC Articles 2, 2A, 7, and 9, contract formation, goods sale/lease, title, risk of loss, warehouse receipts, bills of lading, secured transactions, warranties, rejection, acceptance, cure, revocation, electronic contracting | OrdArr, SupplyArr, LoadArr, RoutArr, CustomArr, LedgArr, RecordArr |
| Consumer protection and accessibility | FTC Act, state UDAP, Magnuson-Moss, Mail/Internet/Telephone Order Merchandise Rule, subscriptions and auto-renewal, gift cards, pricing/refund disclosures, consumer credit where offered, ADA public accommodation and effective communication, state accessibility | CustomArr, OrdArr, STLComplianceSite, AssurArr, RecordArr |
| Competition and procurement integrity | Sherman Act, Clayton Act, Robinson-Patman, FTC competition provisions, bid rigging, price fixing, commercial bribery, kickbacks, conflicts, procurement disclosures | SupplyArr, CustomArr, OrdArr, LedgArr, RecordArr |
| Anti-corruption, sanctions, and fraud | FCPA, Foreign Extortion Prevention Act, federal program bribery, anti-kickback provisions where applicable, OFAC sanctions, false statements/claims, books and records | SupplyArr, LedgArr, CustomArr, OrdArr, RecordArr, AssurArr |

Privacy should be modeled as federal sector packs plus independently versioned state privacy, breach, biometric, communications, and automated-decision overlays. Electronic records and UCC obligations depend heavily on state enactment. Employment and OSHA packs require state/local overlays and activity-specific applicability.

## Fleet, Transportation, and Logistics Rulepacks

Transportation packs are first-class because they span StaffArr, TrainArr, MaintainArr, RoutArr, LoadArr, RecordArr, and ReportArr.

FMCSA and motor-carrier rulepacks should include 49 CFR Parts 40, 365, 368, 371, 376, 380, 382, 383, 385, 387, 390, 391, 392, 393, 395, 396, and 397, plus applicable state intrastate-carrier, registration, apportioned-registration, fuel-tax, size/weight, oversize/overweight, escort, toll, idling, emissions, and mobile-device rules. These bind driver, carrier, vehicle, insurance, drug-testing, authority, maintenance, and records domains rather than belonging only to RoutArr.

PHMSA hazardous-material transportation rulepacks should include 49 CFR Part 107 and Parts 171-180: registration, classification, hazardous-material tables, packaging authorization, quantity limits, marking, labeling, placarding, shipping descriptions, shipping papers, emergency-response information, training, recurrent training, security plans, loading, unloading, segregation, attendance, storage incidental to transportation, incident reporting, permits, approvals, and packaging/tank inspection or testing. These bind SupplyArr, OrdArr, LoadArr, RoutArr, TrainArr, StaffArr, MaintainArr, AssurArr, and RecordArr to the same shipment or material.

Transportation tax and administrative programs should include IFTA, IRP, UCR, Form 2290, fuel and mileage tax programs, carrier/broker/household-goods/waste-hauler/passenger authority, municipal vehicle permits, and airport, port, secure-facility, and border credentials. Model these as administrative compliance programs even when the source is a compact, permit condition, registration program, or agency rule.

Food and temperature-controlled transportation should include FSMA Sanitary Transportation of Human and Animal Food, 21 CFR Part 1 Subpart O, participant responsibilities for shippers, loaders, carriers, and receivers, equipment suitability, sanitation, temperature controls, records, training, retention, state food-transport rules, and USDA overlays for meat, poultry, or egg products.

Optional modal packs should activate only when tenant facts establish the role: aviation, rail, maritime, public transit, and pipeline.

## Environmental and Facility Rulepacks

Compliance Core should contain a complete environmental spine, not isolated SDS compliance.

| Domain | Principal requirements | Primary apps |
|---|---|---|
| Hazardous and solid waste | RCRA generator status, hazardous-waste determination, accumulation limits, labeling, inspections, manifests, contingency plans, personnel training, universal waste, used oil, batteries, lamps, aerosols, waste tires, state hazardous-waste programs | MaintainArr, LoadArr, SupplyArr, AssurArr, TrainArr, RecordArr, ReportArr |
| Spill prevention and water | Clean Water Act, NPDES, industrial stormwater, SPCC, discharge permits, secondary containment, inspection and response | MaintainArr, LoadArr, AssurArr, StaffArr, RecordArr |
| Emergency planning and releases | EPCRA emergency planning, SDS/list reporting, Tier II, TRI, CERCLA release reporting, state/local emergency planning | SupplyArr, LoadArr, MaintainArr, AssurArr, ReportArr |
| Air emissions | Clean Air Act permits, stationary sources, mobile sources, refrigerants, hazardous air pollutants, state air permits, idling and opacity | MaintainArr, LoadArr, AssurArr, RecordArr |
| Refrigerants | Clean Air Act Sections 608 and 609, technician certification, service practices, recovery, leak/service records, sales/handling, HFC transition | MaintainArr, TrainArr, StaffArr, SupplyArr, RecordArr |
| Chemical control | TSCA, PCB rules, chemical import certifications, reporting, recordkeeping, FIFRA pesticide registration/use, worker protection where applicable | SupplyArr, LoadArr, AssurArr, RecordArr |
| Storage tanks | Federal and state UST/AST registration, release detection, operator training, inspections, financial responsibility, closure, corrective action | MaintainArr, AssurArr, RecordArr, LedgArr |
| Site and fire code | Adopted fire, building, electrical, hazardous-material, occupancy, sprinkler, alarm, egress, storage codes, permits, inspections | StaffArr, MaintainArr, LoadArr, AssurArr, RecordArr |
| Tenant-specific permits | Air, water, waste, sewer, stormwater, tank, fire, hazardous-material, land-use permit conditions | LedgArr, StaffArr, RecordArr, owning operational app |

RCRA is principally in 40 CFR Parts 260-273, with used-oil and underground-storage-tank obligations in additional parts. SPCC, EPCRA, CERCLA reporting, refrigerant, TSCA, and FIFRA requirements need separate applicability logic. Standards such as NFPA, IFC, IBC, NEC, ASTM, and ANSI are not automatically law; Compliance Core must map adoption, edition, amendments, and effective dates.

## Supply Chain, Import, Product, and Quality Rulepacks

International trade and sanctions should include OFAC, EAR, ITAR where applicable, customs classification, valuation, origin, entry, importer-of-record, recordkeeping, forced-labor import controls including UFLPA, antidumping/countervailing duties, foreign-trade zones, bonded warehouses, country-of-origin marking, Lacey Act declarations, chemical import certifications, export licensing, denied-party screening, end-use/end-user/diversion controls, and FCPA supplier controls.

Supplier and product compliance should include conditional packs for CPSC, NHTSA, FDA, USDA, EPA, FCC, state chemical disclosure and restriction laws such as Proposition 65, packaging, labeling, recycling, deposit, EPR, weights and measures, product testing, certification, traceability, complaint reporting, recall, corrective action, counterfeit/stolen goods, wildlife, forced labor, and environmental contraband.

Food and animal-food packs should include FD&C Act, FSMA, 21 CFR Parts 1, 11, 112, 117, 120, 123, 121, and 507 as applicable, preventive controls, food-safety plans, qualified individuals, supplier verification, FSVP, traceability, sanitary transportation, intentional adulteration, Reportable Food Registry, recall/complaints, USDA sanitation and HACCP under 9 CFR Parts 416 and 417, and state/local food code.

Drug, device, supplement, and cosmetic packs should include drug CGMP, dietary supplement CGMP, medical-device QMSR, MDR, corrections and removals, registration/listing, investigational/premarket requirements, electronic records and signatures, DSCSA, DEA controlled substances, MoCRA cosmetics, and state distributor/wholesaler/pharmacy/device/controlled-substance licensing. When a standard is incorporated by regulation, such as ISO 13485:2016 through FDA QMSR, Compliance Core treats it as binding to the incorporated extent.

## Government Contracting and Vertical Packs

Government-contracting packs activate when a legal entity holds a covered contract, subcontract, grant, or cooperative agreement. Model them at the contract and clause level because applicability often depends on exact clauses incorporated into a specific award. Required families include FAR, agency supplements, DFARS, False Claims Act, Procurement Integrity Act, Anti-Kickback Act, CAS, truthful cost or pricing data, SCLS, Davis-Bacon, Drug-Free Workplace, federal-contractor E-Verify, Section 503, VEVRAA, VETS-4212, EEO/affirmative action clauses, Buy American/domestic preference, supply-chain security, cybersecurity/CUI clauses, 2 CFR Part 200, suspension/debarment, lobbying disclosures, mandatory disclosure, and ethics-program clauses.

Additional industry packs should exist in the catalog but remain disabled until onboarding facts establish applicability: construction, mining, healthcare, financial services, public companies, energy/utilities, agriculture/pesticides, education, alcohol/tobacco, waste transport, public sector, and insurance.

International packs must be separate jurisdiction-specific packs, not translated clones of U.S. rules. Initial jurisdictions should include European Union, United Kingdom, Canada, Mexico, and additional country packs as independent rule families.

## App Binding Rules

Compliance Core owns what must be done and why. Operational apps own the real-world object and execution. RecordArr owns retained evidence. ReportArr owns generated regulatory outputs.

| App | Binding focus |
|---|---|
| NexArr | Privacy, identity, account access, authentication, security safeguards, breach notification, tenant isolation, access logging, sector security overlays |
| StaffArr | Employment, labor, immigration, wage/hour, leave, accommodation, screening, personnel records, monitoring, drug testing, workers compensation, scheduling, driver qualification, occupational licenses, incident/injury reporting |
| TrainArr | Initial/recurrent training, evaluations, competency, authorized-person status, supervisor training, remediation, instructor qualifications, certificates, expirations, training record retention |
| MaintainArr | FMCSA Parts 393 and 396, recalls, inspections, shop safety, LOTO, PPE, HazCom, machine guarding, welding, electrical, lifting, confined space, used oil, waste, refrigerant, spills, tanks, wastewater, air |
| RoutArr | Motor-carrier authority, driver assignment restrictions, HOS/ELD, route/operating restrictions, hazmat routing, size/weight permits, sanitary transportation, brokers/forwarders, insurance/authority status, cross-border |
| SupplyArr | Vendor qualification, sanctions, import/export, anti-bribery, forced labor, government clauses, product/chemical restrictions, supplier verification, origin/domestic content, PO statutory and contractual obligations |
| LoadArr | OSHA warehouse/material handling, powered industrial trucks, hazmat loading/unloading/segregation/storage, food sanitation/temperature/traceability, bonded warehouse/FTZ, fire-code storage, spill response, waste handling, weights and measures, quarantine/recall controls |
| AssurArr | Inspection and acceptance, nonconformance, legally required CAPA, complaints, adverse-event/defect reporting, recall/field correction, incident escalation, supplier corrective action, regulatory hold decisions |
| RecordArr | Record creation, format, signature, integrity, retention, disposition, confidentiality, legal hold, production, audit access, chain of custody, electronic record validation |
| OrdArr | UCC sales, order/shipping rules, taxes/statutory fees, product-sale restrictions, age/license verification, export/sanctions screening, subscriptions, warranty/return/refund/disclosures, traceability |
| CustomArr | Customer privacy, marketing consent, TCPA, CAN-SPAM, TSR, Do-Not-Call, accessibility/accommodation, consumer protection, credit/screening where enabled, complaints, disputes, regulatory-response deadlines |
| LedgArr | Legal entities, registrations, business licenses, tax registrations/returns/payments, payroll taxes, statutory financial reporting, insurance/financial responsibility, benefit obligations, regulated-finance overlays |
| ReportArr | Regulatory forms, filing schemas, reporting windows, recipients, attestations, signer authority, corrections, reports available upon request, audit/inspection packages |
| Field Companion | Field inspection/evidence, electronic signatures, compensable work, geolocation/monitoring/recording/biometric consent, mobile-device restrictions, offline evidence integrity, field safety, notices and acknowledgments |
| STLComplianceSite | Website privacy notices, cookies/tracking, marketing consent, accessibility, online-order disclosures, electronic contracting, terms evidence, children's data, security/breach |
| Platform Reference Data service | Neutral reference identity only, such as jurisdiction codes, NAICS/SOC, UN/NA IDs, CAS, HTS/Schedule B, country/currency, vehicle/equipment taxonomies, unit/package codes |

Once a reference value has legal meaning, such as an HTS classification triggering a tariff or a UN number triggering packaging restrictions, the legal interpretation and obligation belong in Compliance Core.

## Applicability Fact Inventory

Compliance Core must ask for and retain normalized facts such as:

- employer and employee counts by location and period
- legal-entity type and formation jurisdiction
- worksite and employee work locations
- tenant roles such as carrier, broker, shipper, loader, receiver, forwarder, importer, exporter, manufacturer, distributor, retailer, or government contractor
- interstate versus intrastate operations
- vehicle weight, passenger capacity, use, and cargo
- hazardous-material classes and quantities
- facility chemical and fuel storage
- waste streams and generator status
- food, drug, device, chemical, consumer-product, or controlled-product handling
- personal-data categories and data-subject jurisdictions
- marketing channels and consent sources
- consumer versus business customers
- credit offered or reports obtained
- contract clauses and award types
- sites, permits, discharges, emissions, and tanks
- shipment origin, destination, transit jurisdictions, and border crossings
- public-company, healthcare, financial-institution, educational, or public-sector status

Unknown, stale, or conflicting facts should produce reviewable follow-up needs rather than silent not-applicable results.

## Implementation Order

Priority 0 is platform foundation: legal-source, versioning, applicability, obligation, evidence, retention, filing, jurisdiction-overlay, source-provenance, and app-binding models.

Priority 1 is the STL Compliance operational baseline: FMCSA and DOT drug testing, PHMSA hazmat, OSHA federal/state-plan overlays, employment/labor, EPA waste/spill/EPCRA/tank/refrigerant/chemical, privacy/breach/biometrics/monitoring/communications, electronic records/signatures, entity/licensing/tax/carrier registration, UCC/order/warranty/consumer/accessibility.

Priority 2 is supply-chain and quality expansion: import/export, customs, sanctions, anti-bribery, CPSC/NHTSA product safety and recall, food/FSMA/USDA/cold chain, government contracting, state chemical, packaging, EPR, and product-disclosure laws.

Priority 3 is vertical and international expansion: medical device, pharmaceutical, healthcare, construction, mining, aviation, rail, maritime, transit, pipeline, financial services, education, public sector, agriculture, alcohol/tobacco, and country-specific packs.
