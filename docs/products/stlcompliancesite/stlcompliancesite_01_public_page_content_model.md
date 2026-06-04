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
