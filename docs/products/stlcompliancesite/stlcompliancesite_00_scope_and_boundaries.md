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
- Platform identity, active tenant membership, and session lifecycle
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
- Authenticated product boundary
- Platform status/auth references if needed

CustomArr
- Lead, prospect, contact, campaign/source, consent, and relationship handoff after public intake
- Duplicate review and conversion to customer/opportunity/case

RecordArr
- Official approved legal/trust document copies if controlled record storage is desired
- Public downloadable documents if controlled

ReportArr
- Public-safe marketing metrics only if needed
- Internal site performance/reporting if desired

External systems
- Optional external CRM connector governed through CustomArr
- Email delivery
- Form spam protection
- Web analytics
- Status page provider or self-hosted status page
- Domain/DNS/CDN hosting
```

## Core source-of-truth rules

```text
1. STLComplianceSite owns public messaging, not product operations.
2. Public site lead submissions begin on the site and hand off durably to CustomArr for lead, prospect, consent, relationship, opportunity, and follow-up truth. Optional external CRM synchronization is a CustomArr integration, not an alternate owner.
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
