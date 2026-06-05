# STLComplianceSite — Scope, Ownership, and Boundaries

## Product purpose

STLComplianceSite is the public-facing marketing, trust, intake, and informational website for STL Compliance and the ARR product ecosystem.

It explains:

- What STL Compliance is
- What Adaptive Risk Reduction means
- What products exist
- How the products connect
- What industries and workflows the suite supports
- Why the suite exists
- How prospects can contact STL Compliance
- What legal/privacy/trust terms apply
- Where to find status/trust/security information

The public site should move away from being MaintainArr-centric and present STL Compliance as a unified business orchestration and compliance suite.

## STLComplianceSite owns

```text
- Public website routes
- Public landing page
- Suite overview content
- Product marketing pages
- Industry/use-case pages
- Feature overview pages
- Founder/origin story content
- Public diagrams/explainers
- Lead inquiry form
- Demo request form
- Contact form
- Newsletter/update signup if used
- Public legal pages
- Privacy policy page
- Cookie disclosure page
- Terms and conditions page
- Trust/security page
- Status page link/surface
- Public SEO metadata
- Public content blocks
- Public call-to-action routing
- Public site analytics events
- Public site content version metadata
```

## STLComplianceSite does not own

```text
- Platform login
- Tenant entitlement
- Authenticated app shell
- Product launch/handoff
- Person master
- Customer master after handoff
- CRM truth after handoff
- Support ticket truth unless explicitly implemented
- Product operational data
- Asset/work-order/inventory/order/route/training/quality/document truth
- Product permissions
- Billing/accounting execution
- Legal document controlled-record truth if RecordArr is used as official record system
```

## External product/system dependencies

```text
NexArr
- Link to login/app launcher
- Product entitlement/authenticated app boundary
- Platform status/auth references if needed

CustomArr
- Lead/customer/prospect handoff if CustomArr is used for CRM/customer relationship tracking

RecordArr
- Official approved legal/trust document copies if controlled record storage is desired
- Public downloadable documents if controlled

ReportArr
- Public-safe marketing metrics only if needed
- Internal site performance/reporting if desired

External systems
- Email delivery
- Form spam protection
- Web analytics
- Status page provider or self-hosted status page
- Domain/DNS/CDN hosting
```

## Core source-of-truth rules

```text
1. STLComplianceSite owns public messaging, not product operations.
2. Public site lead submissions may start on the site but should hand off to CustomArr or an external CRM for long-term relationship tracking.
3. Legal pages should be versioned and optionally mirrored/stored in RecordArr as controlled records.
4. Public site must not expose authenticated product data.
5. Public site must not become a product admin console.
6. Public product claims should align with the end-goal product ownership constitution.
7. The site should explain the suite as STL Compliance, not as MaintainArr plus extras.
8. Calls to action should route to demo/contact/login, not operational screens.
9. Public forms should include spam protection, consent capture, and audit-friendly submission metadata.
10. Status/trust/security pages should be factual and versioned.
```

## Standard site object envelope

```text
SiteObject
- id
- objectNumber
- objectType
- slug
- title
- status
- locale
- version
- createdAt
- createdBy
- updatedAt
- updatedBy
- publishedAt
- archivedAt
- seoMetadata
- contentBlocks
- auditTrail
```

## Object prefixes

```text
PAGE   Public page
BLK    Content block
PROD   Product marketing page
IND    Industry page
USE    Use case page
CTA    Call to action
LEAD   Lead inquiry
DEMO   Demo request
FORM   Form definition
SUB    Form submission
LEGAL  Legal page
TRUST  Trust page
STAT   Status page reference
SEO    SEO metadata
ASSET  Public media asset
EVT    Public analytics event
```

## Public positioning rule

STLComplianceSite should position the suite as:

```text
STL Compliance
- Standards
- Training
- Logistics
- Adaptive Risk Reduction
- A connected operational compliance suite
- A unified business operations layer that leaves financial execution to systems like QuickBooks
```

## Public product lineup

```text
Core/platform
- NexArr
- StaffArr
- TrainArr
- Compliance Core
- RecordArr
- ReportArr
- Field Companion

Execution products
- MaintainArr
- LoadArr
- SupplyArr
- RoutArr
- AssurArr
- CustomArr
- OrdArr
```


---


# STLComplianceSite — Public Page and Content Model

## Public page

A PublicPage is any public route/content page on stlcompliance.com.

```text
PublicPage
- pageId
- pageNumber
- slug
- path
- title
- shortTitle
- metaTitle
- metaDescription
- pageType
  - home
  - suite_overview
  - product
  - industry
  - use_case
  - feature
  - founder_story
  - trust
  - legal
  - contact
  - demo
  - pricing
  - status
  - article
  - landing
  - comparison
  - faq
- status
  - draft
  - review
  - published
  - unpublished
  - archived
- locale
- version
- canonicalUrl
- parentPageRef
- contentBlockRefs
- seoMetadataRef
- openGraphMetadata
- structuredData
- callToActionRefs
- publishedAt
- publishedBy
- updatedAt
- archivedAt
```

## Page status definitions

```text
draft
- Page is being written.

review
- Page is waiting for review.

published
- Page is publicly visible.

unpublished
- Page is not visible but may return later.

archived
- Page is retained for history.
```

## Content block

A ContentBlock is reusable page content.

```text
ContentBlock
- contentBlockId
- blockKey
- pageId
- blockType
  - hero
  - text
  - rich_text
  - feature_grid
  - product_grid
  - card_grid
  - quote
  - founder_note
  - timeline
  - faq
  - comparison_table
  - diagram
  - image
  - video
  - callout
  - stats
  - logo_strip
  - contact_form
  - demo_form
  - legal_text
  - trust_card
  - status_embed
  - cta_band
- title
- eyebrow
- body
- supportingText
- mediaAssetRefs
- linkRefs
- sortOrder
- visibility
  - public
  - hidden
  - draft_only
- createdAt
- updatedAt
```

## Public media asset

```text
PublicMediaAsset
- mediaAssetId
- assetKey
- title
- description
- assetType
  - image
  - logo
  - icon
  - video
  - document
  - diagram
  - screenshot
- filePathOrStorageRef
- altText
- caption
- usageRights
- status
  - draft
  - active
  - archived
- createdAt
- updatedAt
```

## SEO metadata

```text
SeoMetadata
- seoMetadataId
- pageId
- metaTitle
- metaDescription
- canonicalUrl
- robots
  - index_follow
  - noindex_follow
  - noindex_nofollow
- keywords
- openGraphTitle
- openGraphDescription
- openGraphImageRef
- twitterCardType
- structuredDataJson
```

## Call to action

```text
CallToAction
- ctaId
- ctaKey
- label
- description
- ctaType
  - demo_request
  - contact
  - login
  - product_page
  - download
  - learn_more
  - status_page
  - external_link
- targetUrl
- targetPageRef
- style
  - primary
  - secondary
  - subtle
  - danger
- trackingKey
- status
  - active
  - inactive
```

## Navigation item

```text
NavigationItem
- navigationItemId
- label
- path
- itemType
  - page
  - dropdown
  - external
  - cta
- parentNavigationItemId
- sortOrder
- visibility
  - public
  - hidden
- status
```

## Sitemap entry

```text
SitemapEntry
- sitemapEntryId
- pageId
- path
- priority
- changeFrequency
  - always
  - hourly
  - daily
  - weekly
  - monthly
  - yearly
  - never
- include
- lastModifiedAt
```

## Home page content shape

```text
HomePage
- Hero
  - STL Compliance positioning
  - Adaptive Risk Reduction explanation
  - Primary CTA: request demo/contact
  - Secondary CTA: explore suite
- Problem section
  - fragmented SaaS
  - disconnected operations
  - compliance gaps
  - evidence scatter
- Suite overview
  - product ecosystem
  - connected operations
- Product grid
  - NexArr
  - StaffArr
  - TrainArr
  - Compliance Core
  - MaintainArr
  - LoadArr
  - SupplyArr
  - RoutArr
  - RecordArr
  - AssurArr
  - ReportArr
  - Field Companion
  - CustomArr
  - OrdArr
- Cross-product flow examples
- Founder/origin story excerpt
- Trust/security/legal links
- Final CTA
```

## Founder story page shape

```text
FounderStoryPage
- Founder intro
- Compliance awakening
- FMCSA audit experience
- Fragmented enterprise SaaS experience
- PepsiCo-style operational fragmentation example
- MaintainArr origin
- ARR naming evolution
- Adaptive Risk Reduction meaning
- Suite mission
- Practical, grounded closing CTA
```

## Content workflow

```text
1. Page is created as draft.
2. Content blocks are added.
3. SEO metadata is defined.
4. Review occurs.
5. Page is published.
6. Sitemap updates.
7. Analytics begins collecting public events.
8. Page can be revised/versioned later.
```

## Events

```text
site.page.created
site.page.updated
site.page.submitted_for_review
site.page.published
site.page.unpublished
site.page.archived
site.content_block.created
site.content_block.updated
site.media_asset.created
site.navigation.updated
site.sitemap.generated
site.cta.created
site.cta.clicked
```


---


# STLComplianceSite — Product, Industry, and Marketing Model

## Product marketing page

A ProductMarketingPage explains one product in the suite without turning the public site into product documentation.

```text
ProductMarketingPage
- productMarketingPageId
- productKey
  - nexarr
  - staffarr
  - trainarr
  - compliancecore
  - maintainarr
  - loadarr
  - supplyarr
  - routarr
  - customarr
  - ordarr
  - recordarr
  - assurarr
  - reportarr
  - FieldCompanion
- pageId
- productName
- productTagline
- oneLiner
- status
  - draft
  - review
  - published
  - archived
- audience
- painPoints
- capabilityGroups
- crossProductConnections
- workflowExamples
- screenshotsOrMockups
- faqRefs
- callToActionRefs
- seoMetadataRef
```

## Product capability group

```text
ProductCapabilityGroup
- capabilityGroupId
- productMarketingPageId
- title
- description
- capabilityItems
- sortOrder
```

## Product capability item

```text
ProductCapabilityItem
- capabilityItemId
- capabilityGroupId
- title
- description
- relatedProductKeys
- relatedWorkflowRefs
- status
```

## Product connection

ProductConnection explains how one product connects to another.

```text
ProductConnection
- productConnectionId
- sourceProductKey
- targetProductKey
- connectionTitle
- description
- exampleFlow
- sourceOwns
- targetOwns
- publicSummary
- status
```

## Product page recommended one-liners

```text
NexArr
- The secure front door for the suite.

StaffArr
- The people, permissions, and internal location backbone.

TrainArr
- The qualification engine.

Compliance Core
- The rule and requirement brain.

MaintainArr
- The maintenance execution system.

LoadArr
- The WMS and inventory execution system.

SupplyArr
- The supplier and procurement execution system.

RoutArr
- The dispatch and transportation execution system.

CustomArr
- The customer relationship and customer requirement system.

OrdArr
- The order and request coordination system.

RecordArr
- The document, evidence, and retention vault.

AssurArr
- The quality, hold, nonconformance, and CAPA system.

ReportArr
- The reporting and analytics layer.

Field Companion
- The mobile execution interface for field work.
```

## Industry page

An IndustryPage explains how the suite applies to a market/operation type.

```text
IndustryPage
- industryPageId
- pageId
- industryKey
  - fleet
  - warehousing
  - manufacturing
  - transportation
  - maintenance
  - safety_compliance
  - regulated_operations
  - field_service
  - logistics
  - mixed_operations
- title
- description
- painPoints
- solutionSummary
- relevantProductKeys
- workflowExamples
- complianceExamples
- callToActionRefs
- status
```

## Use case page

```text
UseCasePage
- useCasePageId
- pageId
- useCaseKey
  - asset_maintenance
  - training_readiness
  - audit_evidence
  - warehouse_receiving
  - supplier_compliance
  - route_delivery
  - quality_hold
  - customer_order_fulfillment
  - incident_retraining
  - document_control
  - mobile_field_execution
- title
- problemStatement
- solutionSummary
- involvedProductKeys
- stepByStepFlow
- outcomeSummary
- evidenceGenerated
- ctaRefs
- status
```

## Marketing workflow example

```text
MarketingWorkflowExample
- workflowExampleId
- title
- description
- involvedProductKeys
- steps
- sourceOfTruthSummary
- publicDiagramRef
- status
```

## Competitive/comparison page

```text
ComparisonPage
- comparisonPageId
- pageId
- comparisonType
  - fragmented_stack
  - point_solution
  - spreadsheet_manual
  - legacy_system
  - generic
- title
- comparisonRows
- positioningSummary
- callToActionRefs
- status
```

## FAQ item

```text
FaqItem
- faqItemId
- pageId
- question
- answer
- category
  - platform
  - pricing
  - security
  - products
  - compliance
  - implementation
  - data
  - support
- status
  - draft
  - published
  - archived
- sortOrder
```

## Product page template

```text
ProductPageTemplate
- Hero
  - Product name
  - One-liner
  - Practical description
- Problem
  - What breaks without this product
- Capabilities
  - 3 to 6 capability groups
- Cross-product connections
  - What this product owns
  - What it consumes from others
  - What it emits to others
- Workflow example
  - Concrete real-world flow
- Evidence/reporting
  - What records, audit events, and reports it supports
- CTA
  - Request demo/contact/explore suite
```

## Industry page template

```text
IndustryPageTemplate
- Hero
- Industry pain points
- Connected STL Compliance solution
- Relevant product surfaces
- Example workflows
- Compliance/evidence angle
- Why connected suite beats fragmented tools
- CTA
```

## Public suite flow examples

```text
New hire to qualified worker
- StaffArr creates person.
- TrainArr assigns onboarding.
- RecordArr stores evidence.
- ReportArr shows readiness.

Work order needs parts
- MaintainArr creates demand.
- LoadArr checks inventory.
- SupplyArr purchases if needed.
- RecordArr stores evidence.
- ReportArr shows delays.

Receiving with BOL capture
- LoadArr receives.
- Field Companion captures BOL.
- RecordArr scans/OCRs.
- AssurArr handles discrepancy.
- SupplyArr sees supplier impact.

Route delivery proof
- RoutArr executes trip.
- Field Companion captures POD.
- RecordArr stores proof.
- OrdArr closes delivery dependency.
- CustomArr sees customer activity.

Quality hold blocks fulfillment
- AssurArr places hold.
- LoadArr/OrdArr/RoutArr/MaintainArr obey hold.
- RecordArr stores evidence.
- ReportArr shows quality impact.
```

## Marketing content workflow

```text
1. Product/industry/use-case page is drafted.
2. Product ownership wording is checked against suite constitution.
3. Content is reviewed for overclaiming.
4. SEO and CTA metadata are added.
5. Page is published.
6. Analytics events track CTA and engagement.
7. Content is revised as product lineup evolves.
```

## Events

```text
site.product_page.created
site.product_page.updated
site.product_page.published
site.industry_page.created
site.industry_page.published
site.use_case_page.created
site.use_case_page.published
site.workflow_example.created
site.faq.created
site.comparison_page.published
```


---


# STLComplianceSite — Lead Inquiry, Demo Request, and Contact Model

## Lead inquiry

A LeadInquiry is a public form submission from a prospect, customer, partner, or interested party.

```text
LeadInquiry
- leadInquiryId
- inquiryNumber
- submittedAt
- name
- company
- email
- phone
- jobTitle
- inquiryType
  - demo
  - pricing
  - general
  - support
  - partnership
  - press
  - investor
  - vendor
  - other
- interestedProducts
- interestedUseCases
- industry
- companySize
- fleetSize
- warehouseCount
- employeeCount
- message
- sourcePage
- sourceCampaign
- consentFlags
- spamScore
- status
  - new
  - spam
  - needs_review
  - reviewed
  - routed
  - contacted
  - qualified
  - disqualified
  - closed
- routedTo
  - customarr
  - external_crm
  - email
  - support
  - manual
- customarrCustomerRef
- externalCrmRef
- assignedPersonId
- notes
- ipAddress
- userAgent
```

## Demo request

```text
DemoRequest
- demoRequestId
- demoNumber
- leadInquiryRef
- requestedAt
- requestedProducts
- requestedUseCases
- preferredContactMethod
  - email
  - phone
  - video_call
  - no_preference
- preferredTimeWindow
- timezone
- urgency
  - low
  - normal
  - high
- currentTools
- painPoints
- status
  - requested
  - reviewed
  - scheduled
  - completed
  - no_show
  - canceled
  - closed
- scheduledAt
- assignedPersonId
- meetingLink
- followUpNotes
```

## Contact form submission

```text
ContactSubmission
- contactSubmissionId
- submissionNumber
- submittedAt
- name
- email
- phone
- company
- subject
- message
- contactReason
  - sales
  - support
  - billing_reference
  - security
  - privacy
  - partnership
  - general
- status
  - new
  - spam
  - reviewed
  - routed
  - responded
  - closed
- assignedPersonId
- responseSummary
```

## Newsletter/update signup

```text
UpdateSignup
- updateSignupId
- email
- name
- company
- interests
  - product_updates
  - compliance_updates
  - founder_updates
  - launch_updates
  - general
- status
  - subscribed
  - unsubscribed
  - bounced
  - complained
- consentAt
- sourcePage
```

## Form definition

```text
PublicFormDefinition
- formDefinitionId
- formKey
- title
- description
- formType
  - lead
  - demo
  - contact
  - newsletter
  - support
  - privacy_request
- status
  - draft
  - active
  - inactive
  - archived
- fields
- validationRules
- spamProtection
- consentRequirements
- routingRules
- successMessage
- failureMessage
```

## Form field

```text
PublicFormField
- fieldId
- formDefinitionId
- fieldKey
- label
- helpText
- fieldType
  - text
  - email
  - phone
  - textarea
  - select
  - multi_select
  - checkbox
  - hidden
- required
- options
- validationRules
- pii
- sortOrder
```

## Consent flag

```text
ConsentFlag
- consentFlagId
- submissionRef
- consentType
  - contact_me
  - marketing_emails
  - privacy_policy
  - terms
  - cookies
- accepted
- acceptedAt
- consentTextVersion
```

## Spam/risk assessment

```text
SpamAssessment
- spamAssessmentId
- submissionRef
- score
- result
  - pass
  - challenge
  - spam
  - manual_review
- signals
- provider
- assessedAt
```

## Routing rule

```text
InquiryRoutingRule
- routingRuleId
- formType
- condition
- routeTo
  - customarr
  - external_crm
  - email
  - support
  - manual_review
- assignedPersonId
- externalTarget
- status
```

## Lead handoff

```text
LeadHandoff
- leadHandoffId
- leadInquiryId
- destination
  - customarr
  - external_crm
  - email
  - manual
- status
  - pending
  - sent
  - accepted
  - failed
  - canceled
- destinationRef
- sentAt
- acceptedAt
- failureReason
```

## Lead inquiry workflow

```text
1. Visitor submits form.
2. Site validates required fields.
3. Spam/risk assessment runs.
4. Consent flags are recorded.
5. LeadInquiry is created.
6. Routing rule runs.
7. Lead is sent to CustomArr, external CRM, email, or manual review.
8. Internal notification is sent.
9. Public confirmation is shown.
10. Long-term relationship tracking happens outside the public site.
```

## Demo request workflow

```text
1. Visitor submits demo request.
2. DemoRequest and LeadInquiry are created.
3. Request is reviewed.
4. Demo is scheduled manually or via integration.
5. Follow-up notes are tracked in CustomArr/external CRM.
6. Site retains submission/audit metadata.
```

## Privacy/contact request workflow

```text
1. Visitor submits privacy/security/contact request.
2. Submission is classified.
3. Sensitive/legal/privacy requests are routed to responsible reviewer.
4. Response is handled outside public site or through configured workflow.
5. Submission status is closed.
```

## Events

```text
site.lead_inquiry.created
site.lead_inquiry.spam_detected
site.lead_inquiry.reviewed
site.lead_inquiry.routed
site.lead_inquiry.contacted
site.lead_inquiry.closed

site.demo_request.created
site.demo_request.scheduled
site.demo_request.completed
site.demo_request.canceled

site.contact_submission.created
site.contact_submission.routed
site.contact_submission.responded
site.contact_submission.closed

site.update_signup.created
site.update_signup.unsubscribed

site.form.submitted
site.form.spam_assessed
site.lead_handoff.sent
site.lead_handoff.failed
```


---


# STLComplianceSite — Legal, Trust, Security, Privacy, and Status Model

## Legal page

A LegalPage is a versioned public legal document/page.

```text
LegalPage
- legalPageId
- pageId
- legalPageType
  - terms
  - privacy
  - cookies
  - acceptable_use
  - security
  - data_processing
  - subprocessors
  - accessibility
  - disclaimer
- title
- version
- status
  - draft
  - review
  - published
  - superseded
  - archived
- effectiveAt
- publishedAt
- supersededAt
- content
- summaryOfChanges
- recordarrRecordRef
- reviewedBy
- approvedBy
```

## Privacy policy content areas

```text
PrivacyPolicySections
- data collected
- cookies used for login/session behavior
- usage data for bug fixes
- usage data for feature improvements
- usage data for internal audits
- security logs
- form submissions
- analytics
- data sharing
- retention
- user rights
- contact information
```

## Cookie disclosure

```text
CookieDisclosure
- cookieDisclosureId
- legalPageRef
- cookieName
- cookieCategory
  - essential
  - analytics
  - preference
  - security
  - marketing
- purpose
- duration
- provider
- required
- optOutAvailable
```

## Trust page

```text
TrustPage
- trustPageId
- pageId
- title
- status
  - draft
  - published
  - archived
- securitySummary
- dataHandlingSummary
- privacySummary
- complianceSummary
- productBoundarySummary
- statusPageRef
- contactSecurityEmail
- updatedAt
```

## Security statement

```text
SecurityStatement
- securityStatementId
- title
- version
- status
  - draft
  - published
  - archived
- topics
  - encryption
  - authentication
  - authorization
  - audit_logs
  - tenant_isolation
  - backups
  - incident_response
  - service_tokens
  - data_retention
  - vendor_management
- content
- recordarrRecordRef
- publishedAt
```

## Status page reference

The public site may link to or embed a status page such as status.stlcompliance.com.

```text
StatusPageReference
- statusPageReferenceId
- displayName
- url
- statusProvider
  - internal
  - external
  - custom
- status
  - active
  - inactive
- monitoredServices
- lastCheckedAt
```

## Monitored service public entry

```text
PublicMonitoredService
- serviceId
- serviceKey
- displayName
- productKey
- publicDescription
- status
  - operational
  - degraded
  - partial_outage
  - major_outage
  - maintenance
  - unknown
- statusPageRef
```

## Public incident notice

```text
PublicIncidentNotice
- incidentNoticeId
- title
- status
  - investigating
  - identified
  - monitoring
  - resolved
  - maintenance
- severity
  - minor
  - major
  - critical
- affectedServices
- publicSummary
- startedAt
- resolvedAt
- updates
```

## Legal page workflow

```text
1. Legal/trust content is drafted.
2. Content is reviewed.
3. Effective date/version are set.
4. Page is published.
5. Previous version is superseded.
6. Official copy may be stored in RecordArr.
7. Public page displays effective version.
```

## Privacy/cookie workflow

```text
1. Site/product data uses are documented.
2. Cookie categories are defined.
3. Legal page is updated.
4. Cookie disclosure displays user-facing summary.
5. Consent/notice behavior is updated if needed.
```

## Trust/status workflow

```text
1. Trust page is published.
2. Status page reference is configured.
3. Product/service status entries are linked or embedded.
4. Public incident notices are displayed when active.
5. Resolved incidents are archived/history-linked.
```

## Events

```text
site.legal_page.created
site.legal_page.submitted_for_review
site.legal_page.published
site.legal_page.superseded
site.legal_page.archived

site.cookie_disclosure.updated
site.trust_page.published
site.security_statement.published
site.status_page_reference.updated
site.public_incident.created
site.public_incident.updated
site.public_incident.resolved
```


---


# STLComplianceSite — Workflows, Status Logic, Events, and APIs

## Major workflow: publish public page

```text
1. Admin creates page draft.
2. Admin adds content blocks.
3. SEO metadata is defined.
4. Review occurs if required.
5. Page is published.
6. Sitemap updates.
7. Public navigation updates if applicable.
8. Analytics begins tracking page views and CTA clicks.
```

## Major workflow: publish product marketing page

```text
1. Product page is drafted.
2. Product one-liner and positioning are checked against product ownership.
3. Capability groups are added.
4. Cross-product connections are described.
5. Workflow examples are added.
6. CTA is configured.
7. SEO metadata is added.
8. Page is published.
```

## Major workflow: lead/demo inquiry

```text
1. Visitor submits lead/demo form.
2. Validation and spam checks run.
3. Consent flags are recorded.
4. LeadInquiry and/or DemoRequest is created.
5. Routing rule determines destination.
6. Inquiry is sent to CustomArr, external CRM, email, or manual review.
7. Confirmation is shown to visitor.
8. Long-term relationship tracking continues outside public site.
```

## Major workflow: legal/trust update

```text
1. Legal/trust page update is drafted.
2. Review/approval occurs.
3. New version is published.
4. Old version is superseded.
5. Official copy may be stored in RecordArr.
6. Public footer/navigation links continue pointing to current version.
```

## Major workflow: status page link/update

```text
1. Status page reference is configured.
2. Public status/trust page displays status link or embed.
3. Active incidents/maintenance notices are shown if configured.
4. Resolved notices are archived.
```

## Site events

```text
site.page.created
site.page.updated
site.page.submitted_for_review
site.page.published
site.page.unpublished
site.page.archived

site.content_block.created
site.content_block.updated
site.media_asset.created
site.navigation.updated
site.sitemap.generated

site.product_page.created
site.product_page.updated
site.product_page.published
site.industry_page.created
site.industry_page.published
site.use_case_page.created
site.use_case_page.published

site.cta.clicked
site.form.submitted
site.lead_inquiry.created
site.lead_inquiry.routed
site.demo_request.created
site.contact_submission.created

site.legal_page.published
site.legal_page.superseded
site.trust_page.published
site.status_page_reference.updated

site.analytics.page_viewed
site.analytics.cta_clicked
site.analytics.form_started
site.analytics.form_submitted
```

## Public APIs / backend endpoints the site may expose

```text
GET /api/public/pages/{slug}
GET /api/public/navigation
GET /api/public/sitemap
GET /api/public/products
GET /api/public/industries
GET /api/public/use-cases
GET /api/public/legal/{legalPageType}
GET /api/public/trust
GET /api/public/status

POST /api/public/forms/lead
POST /api/public/forms/demo
POST /api/public/forms/contact
POST /api/public/forms/newsletter
POST /api/public/analytics/events
```

## APIs STLComplianceSite may consume

```text
CustomArr
- POST /leads
- POST /customer-activities
- GET /lead-status where applicable

RecordArr
- GET /records/{recordId} for public downloadable controlled records where allowed
- POST /records for published legal/trust copy if desired

ReportArr
- POST /events for internal reporting/analytics if used

NexArr
- Login/app launcher URL
- Public product status/metadata if exposed through safe public endpoint

External
- Email delivery
- Spam protection
- Analytics
- Status page provider
```

## Permission examples

Public visitors do not need authenticated permissions for public pages/forms.

Admin-side permissions may include:

```text
site.pages.read
site.pages.create
site.pages.update
site.pages.publish
site.pages.archive

site.content.manage
site.media.manage
site.navigation.manage
site.seo.manage

site.product_pages.manage
site.industry_pages.manage
site.use_case_pages.manage

site.forms.read
site.forms.manage
site.leads.read
site.leads.route
site.demo_requests.read
site.demo_requests.route

site.legal.read
site.legal.manage
site.legal.publish
site.trust.manage
site.status.manage

site.analytics.read
site.admin
```

## Default admin roles

```text
Site Viewer
- Read drafts/content/admin data where permitted.

Content Editor
- Create/update pages and content blocks.

Publisher
- Publish/unpublish/archive public pages.

Marketing Admin
- Manage product pages, industry pages, CTAs, SEO, and campaigns.

Lead Reviewer
- View, review, route, and close lead/demo/contact submissions.

Legal Publisher
- Manage legal/trust pages and versioned publishing.

Site Admin
- Manage all site settings, forms, navigation, legal pages, and publishing.
```

## Public route map

```text
/
- Home

/products
- Suite product overview

/products/nexarr
/products/staffarr
/products/trainarr
/products/compliance-core
/products/maintainarr
/products/loadarr
/products/supplyarr
/products/routarr
/products/customarr
/products/ordarr
/products/recordarr
/products/assurarr
/products/reportarr
/products/field-companion

/industries
/industries/fleet
/industries/warehousing
/industries/manufacturing
/industries/transportation
/industries/regulated-operations

/use-cases
/use-cases/audit-evidence
/use-cases/work-order-parts
/use-cases/new-hire-readiness
/use-cases/receiving-bol-capture
/use-cases/quality-hold
/use-cases/customer-order-fulfillment

/founder
/trust
/status
/contact
/demo
/legal/terms
/legal/privacy
/legal/cookies
/legal/security
```

## Public UI sections

```text
Header
- Logo
- Products
- Industries
- Use cases
- Founder/Story
- Trust
- Contact
- Login

Footer
- Product links
- Company links
- Legal links
- Status link
- Contact
- Copyright
```

## Site design rules

```text
1. Desktop and mobile are both first-class.
2. Public site should be fast and static-friendly.
3. Product pages should not overpromise current implementation if marked future/end-goal.
4. Public copy should emphasize connected operations, evidence, readiness, and Adaptive Risk Reduction.
5. MaintainArr should be presented as one product in the suite, not the whole suite.
6. CTAs should be clear and repeated naturally.
7. Trust/legal links should be visible.
8. Login should route to NexArr/app, not product-specific auth.
9. The site should avoid exposing internal IDs or operational data.
10. Forms should be short, practical, spam-protected, and consent-aware.
```
