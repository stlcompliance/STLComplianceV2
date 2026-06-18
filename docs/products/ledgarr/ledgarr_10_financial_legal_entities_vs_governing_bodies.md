# LedgArr 10 - Financial Legal Entities vs Governing Bodies

LedgArr FinancialLegalEntity and Compliance Core GoverningBody are different concepts and must not be merged.

FinancialLegalEntity:

- owned by LedgArr
- tenant-owned accounting/reporting entity
- examples: company, subsidiary, branch, operating company, tax entity, consolidated entity, disregarded entity, statutory reporting entity
- used for books, base currency, fiscal calendars, tax registrations, intercompany relationships, GL postings, AP, AR, tax accounting, close, and financial reports

GoverningBody:

- owned by Compliance Core
- regulator, agency, authority, standards body, or rule source
- examples: FMCSA, OSHA, MSHA, EPA, DOT, FDA, state agencies, local authorities, standards bodies
- used for rulepacks, citations, regulatory scopes, compliance vocabularies, evidence meaning, law sources, and rule authority metadata

Non-negotiable rules:

- Do not create a LedgArr model named GoverningBody.
- Do not treat Compliance Core GoverningBody records as LedgArr FinancialLegalEntity records.
- Do not model regulators or statutory authorities as LedgArr FinancialLegalEntity records.
- Do not make Compliance Core own FinancialLegalEntity records.
- A government agency that is also a vendor, customer, or payee is still an external party owned by SupplyArr, CustomArr, or the appropriate owning product. LedgArr references that party financially; it does not make the agency a FinancialLegalEntity.
- TaxJurisdiction in LedgArr is a financial tax calculation/reporting concept, not a Compliance Core GoverningBody.

UI labels should use `Financial Legal Entity`, `Accounting Entity`, or `Company Entity`. Avoid the naked label `Legal Entity` in cross-product/global UI where it could be confused with Compliance Core authority models.
