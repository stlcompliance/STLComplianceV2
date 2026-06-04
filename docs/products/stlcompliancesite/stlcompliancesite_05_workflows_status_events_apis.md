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
