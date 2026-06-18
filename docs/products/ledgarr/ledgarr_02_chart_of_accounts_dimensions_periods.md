# LedgArr 02 - Chart of Accounts, Dimensions, and Periods

LedgArr owns the chart of accounts, GL account catalog, financial dimensions, and fiscal period control used for posting.

Chart entities:

- ChartOfAccounts
- GLAccount
- GLAccountCategory
- GLAccountType
- GLAccountStatus
- AccountAlias
- AccountMapping

Dimension entities:

- FinancialDimensionType
- FinancialDimensionValue
- FinancialDimensionCombination
- DimensionRequirementRule
- DimensionMappingRule
- SourceDimensionMapping

Period entities:

- FiscalCalendar
- FiscalYear
- FiscalPeriod
- FinancialCloseRun
- PeriodLockAudit

Operational products may provide dimension hints, such as StaffArr location, StaffArr department, MaintainArr asset, RoutArr trip, SupplyArr purchase order, LoadArr movement, OrdArr order, CustomArr customer, or project/job references. LedgArr owns the financial dimension mapping and validation result.

Posting must validate:

- active GL account status
- required dimensions
- valid FinancialLegalEntity when required
- accounting date inside an open fiscal period
- period not closed or locked
- balanced debit and credit totals

Posted financial amounts are immutable. Corrections use reversals or adjustment entries.
