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
