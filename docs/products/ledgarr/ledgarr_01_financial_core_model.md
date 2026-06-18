# LedgArr 01 - Financial Core Model

The financial core model establishes tenant accounting structure before operational packets can post.

Core entities:

- TenantFinancialProfile
- FinancialLegalEntity
- FinancialLegalEntityRelationship
- FinancialLegalEntityRegistration
- FinancialLegalEntityAddressSnapshot
- FiscalCalendar
- FiscalYear
- FiscalPeriod
- Currency
- ExchangeRate
- NumberingSequence
- FinancialCloseRun
- PeriodLockAudit

FinancialLegalEntity represents the tenant's own accounting or reporting entity: company, subsidiary, branch, operating company, tax entity, consolidated entity, disregarded entity, or statutory reporting entity. It is used for books, base currency, tax registrations, fiscal calendars, GL postings, AP, AR, intercompany relationships, consolidation, reports, and close.

FinancialLegalEntity must not represent Compliance Core GoverningBody records. It must not replace StaffArr org units or locations, CustomArr customers, SupplyArr vendors, or government regulators. A government agency can appear as a customer, vendor, or payee only through the product that owns that external party. LedgArr may then reference that party for financial posting.

Server-side rules:

- A tenant may require every financial posting to resolve to one FinancialLegalEntity.
- FinancialLegalEntity values that look like regulators, statutory authorities, or Compliance Core governing bodies must be rejected.
- Closed periods reject normal postings.
- Locked periods reject all postings except an authorized reopen workflow.
- All close, reopen, and lock decisions must write FinancialAuditEvent records.
